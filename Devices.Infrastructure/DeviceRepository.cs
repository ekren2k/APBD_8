using System.Data;
using System.Data.Common;
using Devices.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;


namespace Devices.Infrastructure
{
    public class DeviceRepository : IDeviceRepository
    {
        private readonly string _connectionString;
        private readonly ILogger<DeviceRepository> _logger;

        public DeviceRepository(string connectionString, ILogger<DeviceRepository> logger)
        {
            _connectionString = connectionString;
            _logger = logger;
        }

        public async Task AddDeviceAsync(Device device)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            _logger.LogDebug("Adding new device");

            using var transaction = (SqlTransaction)await connection.BeginTransactionAsync();
            try
            {
                if (device is Embedded embedded)
                {
                    await AddEmbeddedDeviceAsync(connection, transaction, embedded);
                }
                else if (device is PersonalComputer pc)
                {
                    await AddPersonalComputerAsync(connection, transaction, pc);
                }
                else if (device is Smartwatch sw)
                {
                    await AddSmartwatchAsync(connection, transaction, sw);
                }
                else
                {
                    throw new ArgumentException("Invalid device type");
                }

                var getVersionCmd = new SqlCommand(
                    "SELECT RowVersion FROM Device WHERE Id = @Id",
                    connection, transaction);
                getVersionCmd.Parameters.AddWithValue("@Id", device.Id);
                device.RowVersion = (byte[])await getVersionCmd.ExecuteScalarAsync();

                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to add device");
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<List<Device>> GetDevicesAsync()
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var command = new SqlCommand(@"
                SELECT d.Id, d.Name, d.IsEnabled, d.RowVersion, pc.OperationSystem, sw.BatteryPercentage, 
                       e.IpAddress, e.NetworkName
                FROM Device d
                LEFT JOIN PersonalComputer pc ON d.Id = pc.DeviceId
                LEFT JOIN Smartwatch sw ON d.Id = sw.DeviceId
                LEFT JOIN Embedded e ON d.Id = e.DeviceId", connection);

            var reader = await command.ExecuteReaderAsync();
            var devices = new List<Device>();

            while (await reader.ReadAsync())
            {
                Device device = null;
                var deviceId = reader.GetString(reader.GetOrdinal("Id"));
                var deviceName = reader.GetString(reader.GetOrdinal("Name"));
                var isEnabled = reader.GetBoolean(reader.GetOrdinal("IsEnabled"));

                if (reader.IsDBNull(reader.GetOrdinal("OperationSystem")))
                {
                    if (!reader.IsDBNull(reader.GetOrdinal("IpAddress")))
                    {
                        device = new Embedded(
                            deviceId,
                            deviceName,
                            isEnabled,
                            reader.GetString(reader.GetOrdinal("IpAddress")),
                            reader.GetString(reader.GetOrdinal("NetworkName"))
                        );
                    }
                    else if (!reader.IsDBNull(reader.GetOrdinal("BatteryPercentage")))
                    {
                        device = new Smartwatch(
                            deviceId,
                            deviceName,
                            isEnabled,
                            reader.GetInt32(reader.GetOrdinal("BatteryPercentage"))
                        );
                    }
                }
                else
                {
                    device = new PersonalComputer(
                        deviceId,
                        deviceName,
                        isEnabled,
                        reader.GetString(reader.GetOrdinal("OperationSystem"))
                    );
                }

                devices.Add(device);
            }

            return devices;
        }

        public async Task<Device?> GetDeviceByIdAsync(string id)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var command = new SqlCommand(@"
        SELECT d.Id, d.Name, d.IsEnabled, d.RowVersion, pc.OperationSystem, sw.BatteryPercentage, 
               e.IpAddress, e.NetworkName
        FROM Device d
        LEFT JOIN PersonalComputer pc ON d.Id = pc.DeviceId
        LEFT JOIN Smartwatch sw ON d.Id = sw.DeviceId
        LEFT JOIN Embedded e ON d.Id = e.DeviceId
        WHERE d.Id = @Id", connection);

            command.Parameters.AddWithValue("@Id", id);

            var reader = await command.ExecuteReaderAsync();
            if (!await reader.ReadAsync()) return null;

            string name = reader.GetString(reader.GetOrdinal("Name"));
            bool isEnabled = reader.GetBoolean(reader.GetOrdinal("IsEnabled"));
            byte[] rowVersion = (byte[])reader.GetValue("RowVersion");

            Device? device = null;

            if (!reader.IsDBNull(reader.GetOrdinal("OperationSystem")))
            {
                device = new PersonalComputer(
                    id, name, isEnabled,
                    reader.GetString(reader.GetOrdinal("OperationSystem"))
                );
            }
            else if (!reader.IsDBNull(reader.GetOrdinal("BatteryPercentage")))
            {
                device = new Smartwatch(
                    id, name, isEnabled,
                    reader.GetInt32(reader.GetOrdinal("BatteryPercentage"))
                );
            }
            else if (!reader.IsDBNull(reader.GetOrdinal("IpAddress")))
            {
                device = new Embedded(
                    id, name, isEnabled,
                    reader.GetString(reader.GetOrdinal("IpAddress")),
                    reader.GetString(reader.GetOrdinal("NetworkName"))
                );
            }
            
            if (device != null)
                device.RowVersion = rowVersion;
            
            return device;
        }

    public async Task RemoveDeviceByIdAsync(string id)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            using SqlTransaction transaction = connection.BeginTransaction();

            var deleteEmbedded = new SqlCommand("DELETE FROM Embedded WHERE DeviceId = @Id", connection, transaction);
            deleteEmbedded.Parameters.AddWithValue("@Id", id);
            await deleteEmbedded.ExecuteNonQueryAsync();

            var deletePc = new SqlCommand("DELETE FROM PersonalComputer WHERE DeviceId = @Id", connection, transaction);
            deletePc.Parameters.AddWithValue("@Id", id);
            await deletePc.ExecuteNonQueryAsync();

            var deleteSw = new SqlCommand("DELETE FROM Smartwatch WHERE DeviceId = @Id", connection, transaction);
            deleteSw.Parameters.AddWithValue("@Id", id);
            await deleteSw.ExecuteNonQueryAsync();

            var deleteDevice = new SqlCommand("DELETE FROM Device WHERE Id = @Id", connection, transaction);
            deleteDevice.Parameters.AddWithValue("@Id", id);
            await deleteDevice.ExecuteNonQueryAsync();
            
            transaction.Commit();
        }

        public async Task<Device> EditDeviceAsync(Device device)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var transaction = connection.BeginTransaction();

            try
            {
                var command = new SqlCommand(@"
                    UPDATE Device 
                    SET Name = @Name, IsEnabled = @IsEnabled 
                    WHERE Id = @Id AND RowVersion = @RowVersion", connection, transaction);

                command.Parameters.AddWithValue("@Id", device.Id);
                command.Parameters.AddWithValue("@Name", device.Name);
                command.Parameters.AddWithValue("@IsEnabled", device.IsEnabled);
                command.Parameters.AddWithValue("@RowVersion", device.RowVersion);

                int affectedRows = await command.ExecuteNonQueryAsync();
                if (affectedRows == 0)
                    throw new DBConcurrencyException("Device was modified by another user.");
                
                if (device is PersonalComputer pc)
                {
                    var cmd = new SqlCommand(@"
                        UPDATE PersonalComputer 
                        SET OperationSystem = @OperationSystem 
                        WHERE DeviceId = @DeviceId", connection, transaction);
                    cmd.Parameters.AddWithValue("@OperationSystem", pc.OperatingSystem ?? (object) DBNull.Value);
                    cmd.Parameters.AddWithValue("@DeviceId", device.Id);
                    var rowsAffected = await cmd.ExecuteNonQueryAsync();
                    _logger.LogDebug("Stored procedure affected {RowsAffected} rows", rowsAffected);
                }
                else if (device is Smartwatch sw)
                {
                    var cmd = new SqlCommand(@"
                        UPDATE Smartwatch 
                        SET BatteryPercentage = @Battery 
                        WHERE DeviceId = @DeviceId", connection, transaction);
                    cmd.Parameters.AddWithValue("@Battery", sw.BatteryLevel);
                    cmd.Parameters.AddWithValue("@DeviceId", device.Id);
                    await cmd.ExecuteNonQueryAsync();
                }
                else if (device is Embedded emb)
                {
                    var cmd = new SqlCommand(@"
                        UPDATE Embedded 
                        SET IpAddress = @Ip, NetworkName = @Network 
                        WHERE DeviceId = @DeviceId", connection, transaction);
                    cmd.Parameters.AddWithValue("@Ip", emb.IpAddress);
                    cmd.Parameters.AddWithValue("@Network", emb.NetworkName);
                    cmd.Parameters.AddWithValue("@DeviceId", device.Id);
                    await cmd.ExecuteNonQueryAsync();
                }

                var getVersionCmd = new SqlCommand(
                    "SELECT RowVersion FROM Device WHERE Id = @Id", 
                    connection, transaction);
                getVersionCmd.Parameters.AddWithValue("@Id", device.Id);
                device.RowVersion = (byte[])await getVersionCmd.ExecuteScalarAsync();
                await transaction.CommitAsync();


                return device;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        
        private async Task AddEmbeddedDeviceAsync(SqlConnection connection, SqlTransaction transaction,
            Embedded embedded)
        {
            using var command = new SqlCommand("AddEmbedded", connection, transaction)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.AddWithValue("@DeviceId", embedded.Id);
            command.Parameters.AddWithValue("@Name", embedded.Name);
            command.Parameters.AddWithValue("@IsEnabled", embedded.IsEnabled);
            command.Parameters.AddWithValue("@IpAddress", embedded.IpAddress);
            command.Parameters.AddWithValue("@NetworkName", embedded.NetworkName);

            await command.ExecuteNonQueryAsync();
        }
        
        private async Task AddSmartwatchAsync(SqlConnection connection, SqlTransaction transaction,
            Smartwatch smartwatch)
        {
            using var command = new SqlCommand("AddSmartwatch", connection, transaction)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.AddWithValue("@DeviceId", smartwatch.Id);
            command.Parameters.AddWithValue("@Name", smartwatch.Name);
            command.Parameters.AddWithValue("@IsEnabled", smartwatch.IsEnabled);
            command.Parameters.AddWithValue("@BatteryPercentage", smartwatch.BatteryLevel);

            await command.ExecuteNonQueryAsync();
        }

        private async Task AddPersonalComputerAsync(SqlConnection connection, SqlTransaction transaction,
            PersonalComputer pc)
        {
            using var command = new SqlCommand("AddPersonalComputer", connection, transaction)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.AddWithValue("@DeviceId", pc.Id);
            command.Parameters.AddWithValue("@Name", pc.Name);
            command.Parameters.AddWithValue("@IsEnabled", pc.IsEnabled);
            command.Parameters.AddWithValue("@OperationSystem", pc.OperatingSystem ?? (object)DBNull.Value);
            
            
            var rowsAffected = await command.ExecuteNonQueryAsync();
            _logger.LogDebug("AddEmbedded affected {Rows} rows", rowsAffected);
        }
        
    }
    
}

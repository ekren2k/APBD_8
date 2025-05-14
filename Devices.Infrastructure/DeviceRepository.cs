using Devices.Models;
using Microsoft.Data.SqlClient;

namespace Devices.Infrastructure
{
    public class DeviceRepository : IDeviceRepository
    {
        private readonly string _connectionString;

        public DeviceRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task AddDeviceAsync(Device device)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var deviceCommand = new SqlCommand("INSERT INTO Device (Id, Name, IsEnabled) VALUES (@Id, @Name, @IsEnabled)", connection);
            deviceCommand.Parameters.AddWithValue("@Id", device.Id);
            deviceCommand.Parameters.AddWithValue("@Name", device.Name);
            deviceCommand.Parameters.AddWithValue("@IsEnabled", device.IsEnabled);

            await deviceCommand.ExecuteNonQueryAsync();

            if (device is PersonalComputer pc)
            {
                var pcCommand = new SqlCommand("INSERT INTO PersonalComputer (OperationSystem, DeviceId) VALUES (@OperatingSystem, @DeviceId)", connection);
                pcCommand.Parameters.AddWithValue("@OperatingSystem", pc.OperatingSystem ?? (object) DBNull.Value);
                pcCommand.Parameters.AddWithValue("@DeviceId", device.Id);

                await pcCommand.ExecuteNonQueryAsync();
            }
            else if (device is Smartwatch sw)
            {
                var smartwatchCommand = new SqlCommand("INSERT INTO Smartwatch (BatteryPercentage, DeviceId) VALUES (@BatteryPercentage, @DeviceId)", connection);
                smartwatchCommand.Parameters.AddWithValue("@BatteryPercentage", sw.BatteryLevel);
                smartwatchCommand.Parameters.AddWithValue("@DeviceId", device.Id);

                await smartwatchCommand.ExecuteNonQueryAsync();
            }
            else if (device is Embedded embedded)
            {
                var embeddedCommand = new SqlCommand("INSERT INTO Embedded (IpAddress, NetworkName, DeviceId) VALUES (@IpAddress, @NetworkName, @DeviceId)", connection);
                embeddedCommand.Parameters.AddWithValue("@IpAddress", embedded.IpAddress);
                embeddedCommand.Parameters.AddWithValue("@NetworkName", embedded.NetworkName);
                embeddedCommand.Parameters.AddWithValue("@DeviceId", device.Id);

                await embeddedCommand.ExecuteNonQueryAsync();
            }
        }

        public async Task<List<Device>> GetDevicesAsync()
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var command = new SqlCommand(@"
                SELECT d.Id, d.Name, d.IsEnabled, pc.OperationSystem, sw.BatteryPercentage, 
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
        SELECT d.Id, d.Name, d.IsEnabled, pc.OperationSystem, sw.BatteryPercentage, 
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

            if (!reader.IsDBNull(reader.GetOrdinal("OperationSystem")))
            {
                return new PersonalComputer(id, name, isEnabled,
                    reader.GetString(reader.GetOrdinal("OperationSystem")));
            }
            if (!reader.IsDBNull(reader.GetOrdinal("BatteryPercentage")))
            {
                return new Smartwatch(id, name, isEnabled,
                    reader.GetInt32(reader.GetOrdinal("BatteryPercentage")));
            }
            if (!reader.IsDBNull(reader.GetOrdinal("IpAddress")))
            {
                return new Embedded(id, name, isEnabled,
                    reader.GetString(reader.GetOrdinal("IpAddress")),
                    reader.GetString(reader.GetOrdinal("NetworkName")));
            }

            return null;
        }

        public async Task RemoveDeviceByIdAsync(string id)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var deleteEmbedded = new SqlCommand("DELETE FROM Embedded WHERE DeviceId = @Id", connection);
            deleteEmbedded.Parameters.AddWithValue("@Id", id);
            await deleteEmbedded.ExecuteNonQueryAsync();

            var deletePc = new SqlCommand("DELETE FROM PersonalComputer WHERE DeviceId = @Id", connection);
            deletePc.Parameters.AddWithValue("@Id", id);
            await deletePc.ExecuteNonQueryAsync();

            var deleteSw = new SqlCommand("DELETE FROM Smartwatch WHERE DeviceId = @Id", connection);
            deleteSw.Parameters.AddWithValue("@Id", id);
            await deleteSw.ExecuteNonQueryAsync();

            var deleteDevice = new SqlCommand("DELETE FROM Device WHERE Id = @Id", connection);
            deleteDevice.Parameters.AddWithValue("@Id", id);
            await deleteDevice.ExecuteNonQueryAsync();
        }

public async Task EditDeviceAsync(Device device)
{
    using var connection = new SqlConnection(_connectionString);
    await connection.OpenAsync();

    var transaction = connection.BeginTransaction();

    try
    {
        var command = new SqlCommand("UPDATE Device SET Name = @Name, IsEnabled = @IsEnabled WHERE Id = @Id", connection, transaction);
        command.Parameters.AddWithValue("@Id", device.Id);
        command.Parameters.AddWithValue("@Name", device.Name);
        command.Parameters.AddWithValue("@IsEnabled", device.IsEnabled);
        await command.ExecuteNonQueryAsync();

        if (device is PersonalComputer pc)
        {
            var cmd = new SqlCommand("UPDATE PersonalComputer SET OperationSystem = @OperatingSystem WHERE DeviceId = @DeviceId", connection, transaction);
            cmd.Parameters.AddWithValue("@OperatingSystem", pc.OperatingSystem ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@DeviceId", device.Id);
            await cmd.ExecuteNonQueryAsync();
        }
        else if (device is Smartwatch sw)
        {
            var cmd = new SqlCommand("UPDATE Smartwatch SET BatteryPercentage = @Battery WHERE DeviceId = @DeviceId", connection, transaction);
            cmd.Parameters.AddWithValue("@Battery", sw.BatteryLevel);
            cmd.Parameters.AddWithValue("@DeviceId", device.Id);
            await cmd.ExecuteNonQueryAsync();
        }
        else if (device is Embedded emb)
        {
            var cmd = new SqlCommand("UPDATE Embedded SET IpAddress = @Ip, NetworkName = @Network WHERE DeviceId = @DeviceId", connection, transaction);
            cmd.Parameters.AddWithValue("@Ip", emb.IpAddress);
            cmd.Parameters.AddWithValue("@Network", emb.NetworkName);
            cmd.Parameters.AddWithValue("@DeviceId", device.Id);
            await cmd.ExecuteNonQueryAsync();
        }

        await transaction.CommitAsync();
    }
    catch
    {
        await transaction.RollbackAsync();
        throw;
    }
}
    }
}
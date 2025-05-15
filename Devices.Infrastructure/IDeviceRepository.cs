namespace Devices.Models;

public interface IDeviceRepository
{
    Task AddDeviceAsync(Device device);
    Task<List<Device>> GetDevicesAsync();
    Task<Device?> GetDeviceByIdAsync(string id);
    Task RemoveDeviceByIdAsync(string id);
    Task<Device?> EditDeviceAsync(Device device);
}
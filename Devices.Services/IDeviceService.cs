using System.Text.Json;
using Devices.Models;

namespace Devices.Services;

public interface IDeviceService
{
    Task<Device> AddDeviceAsync(DeviceCreateDto dto);
    Task<List<Device>> GetDevicesAsync();
    Task<Device?> GetDeviceByIdAsync(string id);
    Task RemoveDeviceByIdAsync(string id);
    Task<Device> EditDeviceAsync(DeviceCreateDto dto);
}
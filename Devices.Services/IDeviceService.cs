using System.Text.Json;
using Devices.Models;

namespace Devices.Services;

public interface IDeviceService
{
    Task<Device> AddDeviceAsync(JsonElement json);
    Task<List<Device>> GetDevicesAsync();
    Task<Device?> GetDeviceByIdAsync(string id);
    Task RemoveDeviceByIdAsync(string id);
    Task<Device> EditDeviceAsync(JsonElement json);
}
using System.Text.Json;
using Devices.Models;

namespace Devices.Services;

public class DeviceService : IDeviceService
{
    
    private readonly IDeviceRepository _deviceRepository;

    public DeviceService(IDeviceRepository deviceRepository)
    {
        _deviceRepository = deviceRepository;
    }
    
    private Device ParseDevice(JsonElement json)
    {

        if (!json.TryGetProperty("type", out var typeElement))
            throw new ArgumentException("Device must have a type");

        var deviceType = typeElement.GetString();

        if (string.IsNullOrWhiteSpace(deviceType))
            throw new ArgumentException("Device type cannot be null or empty");
        
        switch (deviceType)
        {
            case "PersonalComputer":
                return JsonSerializer.Deserialize<PersonalComputer>(json)!;
        
            case "Smartwatch":
                return JsonSerializer.Deserialize<Smartwatch>(json)!;
        
            case "Embedded":
                return JsonSerializer.Deserialize<Embedded>(json)!;
        
            default:
                throw new ArgumentException($"Unknown device type: {deviceType}");
        }
    }

    public async Task<Device> AddDeviceAsync(JsonElement json)
    {
        var device = ParseDevice(json);
        await _deviceRepository.AddDeviceAsync(device);
        return device;
    }

    public async Task RemoveDeviceByIdAsync(string id)
    {
        await _deviceRepository.RemoveDeviceByIdAsync(id);
    }

    public async Task<Device> EditDeviceAsync(JsonElement json)
    {
        var device = ParseDevice(json);
        await _deviceRepository.EditDeviceAsync(device);
        return device;
    }

    public async Task<List<Device>> GetDevicesAsync()
    {
        return await _deviceRepository.GetDevicesAsync();
    }

    public async Task<Device> GetDeviceByIdAsync(string id)
    {
        return await _deviceRepository.GetDeviceByIdAsync(id);
    }
    
}


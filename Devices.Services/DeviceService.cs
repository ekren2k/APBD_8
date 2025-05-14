using System.Text.Json;
using System.Text.RegularExpressions;
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

    public async Task<Device> AddDeviceAsync(DeviceCreateDto dto)
    {
        Regex ipRegex = new Regex(@"^((25[0-5]|2[0-4][0-9]|1\d{2}|[1-9]?\d)\.){3}(25[0-5]|2[0-4][0-9]|1\d{2}|[1-9]?\d)$");
        if (dto.DeviceType == "Embedded"  && !ipRegex.IsMatch(dto.IpAddress!))
            throw new ArgumentException("Ip address is not in a valid format");    Device device = dto.DeviceType switch
        {
            "PersonalComputer" => new PersonalComputer(
                dto.Id, dto.Name, dto.IsEnabled, dto.OperatingSystem),
            "Smartwatch" => new Smartwatch(
                dto.Id, dto.Name, dto.IsEnabled, dto.BatteryLevel ?? 0),
            "Embedded" => new Embedded(
                dto.Id, dto.Name, dto.IsEnabled, dto.IpAddress!, dto.NetworkName!),
            _ => throw new ArgumentException($"Unknown device type: {dto.DeviceType}")
        };

        await _deviceRepository.AddDeviceAsync(device);
        return device;
    }

    public async Task RemoveDeviceByIdAsync(string id)
    {
        await _deviceRepository.RemoveDeviceByIdAsync(id);
    }

    public async Task<Device> EditDeviceAsync(DeviceCreateDto dto)
    {
        var device = await AddDeviceAsync(dto);
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


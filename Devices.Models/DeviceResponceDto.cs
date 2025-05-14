using System.Text.Json.Serialization;

namespace Devices.Models;

[JsonPolymorphic]
[JsonDerivedType(typeof(PersonalComputerResponse), nameof(PersonalComputer))]
[JsonDerivedType(typeof(SmartwatchResponse), nameof(Smartwatch))]
[JsonDerivedType(typeof(EmbeddedResponse), nameof(Embedded))]
public abstract record DeviceResponseDto(
    string Id,
    string Name,
    bool IsEnabled
);

public record PersonalComputerResponse(
    string Id,
    string Name,
    bool IsEnabled,
    string OperatingSystem
) : DeviceResponseDto(Id, Name, IsEnabled);

public record SmartwatchResponse(
    string Id,
    string Name,
    bool IsEnabled,
    int BatteryLevel
) : DeviceResponseDto(Id, Name, IsEnabled);

public record EmbeddedResponse(
    string Id,
    string Name,
    bool IsEnabled,
    string IpAddress,
    string NetworkName
) : DeviceResponseDto(Id, Name, IsEnabled);
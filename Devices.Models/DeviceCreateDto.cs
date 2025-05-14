using System.ComponentModel.DataAnnotations;

namespace Devices.Models;

public record DeviceCreateDto(
    [Required] string Id,
    [Required] string? DeviceType,  //can be null for updates
    [Required, StringLength(100)] string Name,
    bool IsEnabled,
    string? OperatingSystem,    // PC-only
    int? BatteryLevel,  // Smartwatch-only
    string? IpAddress,  // Embedded-only
    string? NetworkName // Embedded-only
);
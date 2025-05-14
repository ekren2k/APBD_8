


namespace Devices.Models;

public class PersonalComputer : Device
{
    public string? OperatingSystem { get; set; }
    
    
    public PersonalComputer(string id, string name, bool isEnabled, string? operatingSystem) : base(id, name, isEnabled)
    {
        if (!CheckId(id))
        {
            throw new ArgumentException("Invalid ID value. Required format: P-1", id);
        }
        
        OperatingSystem = operatingSystem;
    }

    public override void TurnOn()
    {
        if (OperatingSystem is null)
        {
            throw new EmptySystemException();
        }

        base.TurnOn();
    }

    public override string ToString()
    {
        string enabledStatus = IsEnabled ? "enabled" : "disabled";
        string osStatus = OperatingSystem is null ? "has not OS" : $"has {OperatingSystem}";
        return $"PC {Name} ({Id}) is {enabledStatus} and {osStatus}";
    }

    public override void UpdateDevice(Device newDevice)
    {
        base.UpdateDevice(newDevice);
        if (newDevice is PersonalComputer personalComputer)
        {
            this.OperatingSystem = personalComputer.OperatingSystem;
        }
    }

    private bool CheckId(string id) => id.Contains("P-");
}


using System.Text.RegularExpressions;

namespace Devices.Models;

public class Embedded : Device, INetworkDevice
{
    public string NetworkName { get; set; }
    private string _ipAddress;
    private bool _isConnected = false;

    public string IpAddress
    {
        get => _ipAddress;
        set
        {
            //Regex ipRegex = new Regex("^((25[0-5]|(2[0-4]|1\\d|[1-9]|)\\d)\\.?\\b){4}$");
            Regex ipRegex = new Regex(@"^((25[0-5]|2[0-4][0-9]|1\d{2}|[1-9]?\d)\.){3}(25[0-5]|2[0-4][0-9]|1\d{2}|[1-9]?\d)$");
            if (!ipRegex.IsMatch(value))
            {
                throw new ArgumentException("Wrong IP address format.");
            }
            _ipAddress = value;

        }
    }
    
    /*[JsonConstructor]*/
    public Embedded(string id, string name, bool isEnabled, string ipAddress, string networkName) : base(id, name, isEnabled)
    {
        if (!CheckId(id))
        {
            throw new ArgumentException("Invalid ID value. Required format: E-1", id);
        }

        IpAddress = ipAddress;
        NetworkName = networkName;
    }

    public override void TurnOn()
    {
        Connect();
        base.TurnOn();
    }

    public override void TurnOff()
    {
        _isConnected = false;
        base.TurnOff();
    }

    public override string ToString()
    {
        string enabledStatus = IsEnabled ? "enabled" : "disabled";
        return $"Embedded device {Name} ({Id}) is {enabledStatus} and has IP address {IpAddress}";
    }

    public void Connect()
    {
        if (NetworkName.Contains("MD Ltd."))
        {
            _isConnected = true;
        }
        else
        {
            throw new ConnectionException();
        }
    }
    
    public override void UpdateDevice(Device newDevice)
    {
        base.UpdateDevice(newDevice);
        if (newDevice is Embedded embedded)
        {
            this._ipAddress = embedded.IpAddress;
            this._isConnected = embedded._isConnected;
        }
    }
    
    private bool CheckId(string id) => id.Contains("E-");
}
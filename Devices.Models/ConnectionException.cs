
namespace Devices.Models;

class ConnectionException : Exception
{
    public ConnectionException() : base("Wrong netowrk name.") { }
}
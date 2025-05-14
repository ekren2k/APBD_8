
namespace Devices.Models;

class EmptySystemException : Exception
{
    public EmptySystemException() : base("Operation system is not installed.") { }
}
using Devices.Models;
using Microsoft.AspNetCore.Mvc;

namespace Devices.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DevicesController : ControllerBase
    {
        private readonly IDeviceRepository _deviceRepository;

        public DevicesController(IDeviceRepository deviceRepository)
        {
            _deviceRepository = deviceRepository;
        }

        [HttpGet]
        public async Task<ActionResult<List<Device>>> GetDevices()
        {
            var devices = await _deviceRepository.GetDevicesAsync();
            return Ok(devices);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Device>> GetDeviceById(string id)
        {
            var device = await _deviceRepository.GetDeviceByIdAsync(id);
            if (device == null)
                return NotFound();
            return Ok(device);
        }

        [HttpPost]
        public async Task<ActionResult> AddDevice([FromBody] Device device)
        {
            await _deviceRepository.AddDeviceAsync(device);
            return CreatedAtAction(nameof(GetDeviceById), new { id = device.Id }, device);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> EditDevice(string id, [FromBody] Device device)
        {
            if (id != device.Id)
                return BadRequest();

            await _deviceRepository.EditDeviceAsync(device);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> RemoveDevice(string id)
        {
            await _deviceRepository.RemoveDeviceByIdAsync(id);
            return NoContent();
        }
    }
}
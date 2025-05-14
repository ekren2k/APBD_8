using System.Text.Json;
using Devices.Models;
using Devices.Services;
using Microsoft.AspNetCore.Mvc;

namespace Devices.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DevicesController : ControllerBase
    {
        private readonly IDeviceService _deviceService;

        public DevicesController(IDeviceService deviceService)
        {
            _deviceService = deviceService;
        }

        [HttpGet]
        public async Task<ActionResult<List<Device>>> GetDevices()
        {
            var devices = await _deviceService.GetDevicesAsync();
            return Ok(devices);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Device>> GetDeviceById(string id)
        {
            var device = await _deviceService.GetDeviceByIdAsync(id);
            if (device == null)
                return NotFound();
            return Ok(device);
        }

        [HttpPost]
        public async Task<ActionResult<DeviceResponseDto>> AddDevice([FromBody] DeviceCreateDto dto)
        {
            try
            {
                if (dto.DeviceType == null)
                    return BadRequest("DeviceType is required for creation");

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);
                
                var device = await _deviceService.AddDeviceAsync(dto);
                return CreatedAtAction(nameof(GetDeviceById), new { id = device.Id }, device);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Something went wrong", details = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> EditDevice(string id, [FromBody] DeviceCreateDto dto)
        {
            if (id != dto.Id)
                return BadRequest("ID in route does not match body");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            
            var device = await _deviceService.EditDeviceAsync(dto);
            if (id != device.Id)
                return BadRequest();
            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> RemoveDevice(string id)
        {
            await _deviceService.RemoveDeviceByIdAsync(id);
            return NoContent();
        }
    }
}
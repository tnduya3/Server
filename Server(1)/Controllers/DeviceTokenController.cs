using Microsoft.AspNetCore.Mvc;
using Server_1_.Models;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Server_1_.Services;

namespace Server_1_.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DeviceTokenController : ControllerBase
    {
        private readonly ILogger<DeviceTokenController> _logger;
         private readonly IUserService _userService;

        public DeviceTokenController(ILogger<DeviceTokenController> logger , IUserService userService)
        {
            _logger = logger;
            _userService = userService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterDeviceToken([FromBody] DeviceTokenDto deviceTokenDto)
        {
            if (deviceTokenDto == null || string.IsNullOrEmpty(deviceTokenDto.Token))
            {
                return BadRequest("Invalid device token data.");
            }

            _logger.LogInformation($"Received device token for UserId: {deviceTokenDto.UserId}, Token: {deviceTokenDto.Token}");

            var user = await _userService.GetUserByIdAsync(deviceTokenDto.UserId);
            if (user != null)
            {
                user.DeviceToken = deviceTokenDto.Token; // Thêm cột DeviceToken vào User model
                await _userService.UpdateDeviceTokenAsync(user.UserId, deviceTokenDto.Token);
            }
            //Hoặc lưu vào một bảng DeviceTokens riêng(recommended cho nhiều thiết bị / token cho 1 user)
            //if you have a DeviceToken entity:
            // var newDeviceToken = new DeviceToken { UserId = deviceTokenDto.UserId, Token = deviceTokenDto.Token, CreatedAt = DateTime.UtcNow };
            //_context.DeviceTokens.Add(newDeviceToken);
            //await _context.SaveChangesAsync();

            return Ok(new { Message = "Device token registered successfully." });
        }
    }
}

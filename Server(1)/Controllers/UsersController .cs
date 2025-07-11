﻿using Microsoft.AspNetCore.Mvc;
using Server_1_.Models;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using Server_1_.Services;
using System.ComponentModel.DataAnnotations;
using Swashbuckle.AspNetCore.Annotations;

namespace Server_1_.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        //// POST /api/users/register
        //[HttpPost("register")]
        //public async Task<ActionResult<Users>> Register(RegisterRequest request)
        //{
        //    try
        //    {
        //        var user = await _userService.RegisterUserAsync(request.Username, request.Password);
        //        return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(ex.Message); // Trả về BadRequest với thông báo lỗi
        //    }
        //}

        // GET /api/users/{id}
        [HttpGet("{id}")]
        // Thêm mô tả Swagger cho phương thức này
        public async Task<ActionResult<Users>> GetUser(int id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return Ok(user);
        }

        // GET /api/users
        [HttpGet]
        public async Task<ActionResult<List<Users>>> GetUsers()
        {
            var users = await _userService.GetUsersAsync();
            return Ok(users);
        }

        // PUT /api/users/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, UpdateUserRequest request)
        {
            if (id != request.Id)
            {
                return BadRequest("User ID mismatch");
            }

            var result = await _userService.UpdateUserAsync(id, request.Username);
            if (!result)
            {
                return NotFound();
            }
            return NoContent();
        }

        // PUT /api/users/{id}/token
        [HttpPut("{id}/token")]
        public async Task<IActionResult> UpdateDeviceToken(int id, [FromBody] string deviceToken)
        {
            if (string.IsNullOrEmpty(deviceToken))
            {
                return BadRequest("Device token cannot be empty");
            }
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            user.DeviceToken = deviceToken; // Giả sử bạn đã thêm cột DeviceToken vào User model
            await _userService.UpdateDeviceTokenAsync(user.UserId, deviceToken);
            return NoContent();
        }

        // DELETE /api/users/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var result = await _userService.DeleteUserAsync(id);
            if (!result)
            {
                return NotFound();
            }
            return NoContent();
        }

        // GET /api/users/{id}/avatar
        [HttpGet("{id}/avatar")]
        public async Task<ActionResult<string>> GetUserAvatar(int id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound("User not found");
            }

            if (string.IsNullOrEmpty(user.Avatar))
            {
                return Ok(new { avatar = (string?)null, message = "No avatar set" });
            }

            return Ok(new { avatar = user.Avatar });
        }

        // POST /api/users/{id}/avatar
        [HttpPost("{id}/avatar")]
        public async Task<IActionResult> UpdateUserAvatar(int id, [FromBody] UpdateAvatarRequest request)
        {
            if (string.IsNullOrEmpty(request.Avatar))
            {
                return BadRequest("Avatar URL cannot be empty");
            }

            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound("User not found");
            }

            var result = await _userService.UpdateUserAvatarAsync(id, request.Avatar);
            if (!result)
            {
                return BadRequest("Failed to update avatar");
            }

            return Ok(new { message = "Avatar updated successfully", avatar = request.Avatar });
        }

        // DTO cho các request
        public class RegisterRequest
        {
            [Required]
            public required string Username { get; set; }
            [Required]
            public required string Password { get; set; } // Lưu ý: Không bao giờ truyền password trực tiếp như thế này ở production
        }

        public class UpdateUserRequest
        {
            [Required]
            public int Id { get; set; }
            [Required]
            public required string Username { get; set; }
        }

        public class UpdateAvatarRequest
        {
            [Required]
            public required string Avatar { get; set; }
        }
    }
}
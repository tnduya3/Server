using Microsoft.AspNetCore.Mvc;
using Server_1_.Models;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using Server_1_.Services;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.SignalR;
using Server_1_.Hubs;

namespace Server_1_.Controllers
{
    [ApiController]
    [Route("api/[controller]")]    public class ChatroomsController : ControllerBase
    {
        private readonly IChatroomService _chatroomService;
        private readonly IMessageService _messageService;
        private readonly IUserService _userService;
        private readonly IHubContext<ChatHub> _hubContext;

        public ChatroomsController(
            IChatroomService chatroomService, 
            IMessageService messageService,
            IUserService userService,
            IHubContext<ChatHub> hubContext)
        {
            _chatroomService = chatroomService;
            _messageService = messageService;
            _userService = userService;
            _hubContext = hubContext;
        }

        // GET /api/chatrooms
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ChatRooms>>> GetChatrooms()
        {
            var chatrooms = await _chatroomService.GetChatroomsAsync();
            return Ok(chatrooms);
        }

        // GET /api/chatrooms/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<ChatRooms>> GetChatroom(int id)
        {
            var chatroom = await _chatroomService.GetChatroomByIdAsync(id);
            if (chatroom == null)
            {
                return NotFound();
            }
            return Ok(chatroom);
        }

        // POST /api/chatrooms
        [HttpPost]
        public async Task<ActionResult<ChatRooms>> CreateChatroom(CreateChatroomRequest request)
        {
            try
            {
                var chatroom = await _chatroomService.CreateChatroomAsync(request.Name);
                return CreatedAtAction(nameof(GetChatroom), new { id = chatroom.ChatRoomId }, chatroom);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal Server Error: " + ex.Message);
            }
        }

        // PUT /api/chatrooms/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateChatroom(int id, UpdateChatroomRequest request)
        {
            if (id != request.Id)
            {
                return BadRequest("Chatroom ID mismatch");
            }

            var result = await _chatroomService.UpdateChatroomAsync(id, request.Name);
            if (!result)
            {
                return NotFound();
            }
            return NoContent();
        }

        // DELETE /api/chatrooms/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteChatroom(int id)
        {
            var result = await _chatroomService.DeleteChatroomAsync(id);
            if (!result)
            {
                return NotFound();
            }
            return NoContent();
        }

        // POST /api/chatrooms/{chatroomId}/users/{userId}
        [HttpPost("{chatroomId}/users/{userId}")]
        public async Task<IActionResult> AddUserToChatroom(int chatroomId, int userId)
        {
            var result = await _chatroomService.AddUserToChatroomAsync(chatroomId, userId);
            if (!result)
            {
                return NotFound(); // Hoặc BadRequest nếu user hoặc chatroom không tồn tại
            }
            return Ok();
        }

        // DELETE /api/chatrooms/{chatroomId}/users/{userId}
        [HttpDelete("{chatroomId}/users/{userId}")]
        public async Task<IActionResult> RemoveUserFromChatroom(int chatroomId, int userId)
        {
            var result = await _chatroomService.RemoveUserFromChatroomAsync(chatroomId, userId);
            if (!result)
            {
                return NotFound();
            }
            return NoContent();
        }

        // GET /api/chatrooms/{chatroomId}/users
        [HttpGet("{chatroomId}/users")]
        public async Task<ActionResult<List<Users>>> GetParticipantsInChatroom(int chatroomId)
        {
            var users = await _chatroomService.GetParticipantsInChatroomAsync(chatroomId);
            return Ok(users);
        }

        // GET /api/chatrooms/{chatroomId}/messages
        [HttpGet("{chatroomId}/messages")]
        public async Task<ActionResult<IEnumerable<Messages>>> GetMessagesInChatroom(int chatroomId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var messages = await _messageService.GetMessagesInChatroomAsync(chatroomId, page, pageSize);
            return Ok(messages);
        }

        // API endpoint để thông báo user online trong chatroom
        [HttpPost("{chatroomId}/online")]
        public async Task<IActionResult> NotifyUserOnline(int chatroomId, [FromBody] UserOnlineRequest request)
        {
            try
            {
                var user = await _userService.GetUserByIdAsync(request.UserId);
                if (user == null)
                {
                    return NotFound("User not found");
                }

                // Gửi thông báo qua SignalR
                await _hubContext.Clients.Group(chatroomId.ToString())
                    .SendAsync("UserOnlineInChatroom", new
                    {
                        UserId = request.UserId,
                        Username = user.UserName,
                        ChatroomId = chatroomId,
                        OnlineAt = DateTime.UtcNow
                    });

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal Server Error: " + ex.Message);
            }
        }

        // API endpoint để broadcast thông báo chung trong chatroom
        [HttpPost("{chatroomId}/broadcast")]
        public async Task<IActionResult> BroadcastNotification(int chatroomId, [FromBody] BroadcastRequest request)
        {
            try
            {
                await _hubContext.Clients.Group(chatroomId.ToString())
                    .SendAsync("ChatroomNotification", new
                    {
                        Type = request.Type,
                        Message = request.Message,
                        ChatroomId = chatroomId,
                        Timestamp = DateTime.UtcNow,
                        Data = request.Data
                    });

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal Server Error: " + ex.Message);
            }
        }

        // API endpoint để lấy thống kê chatroom
        [HttpGet("{chatroomId}/stats")]
        public async Task<ActionResult<object>> GetChatroomStats(int chatroomId)
        {
            try
            {
                var chatroom = await _chatroomService.GetChatroomByIdAsync(chatroomId);
                if (chatroom == null)
                {
                    return NotFound();
                }

                var participants = await _chatroomService.GetParticipantsInChatroomAsync(chatroomId);
                var messages = await _messageService.GetMessagesInChatroomAsync(chatroomId, 1, 1);

                return Ok(new
                {
                    ChatroomId = chatroomId,
                    // Name = chatroom.RoomName,
                    ParticipantCount = participants.Count,
                    CreatedAt = chatroom.CreatedAt,
                    // Có thể thêm các thống kê khác như số tin nhắn, hoạt động gần đây...
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal Server Error: " + ex.Message);
            }
        }

        // DTO cho các request
        public class CreateChatroomRequest
        {
            [Required]
            public required string Name { get; set; }
        }

        public class UpdateChatroomRequest
        {
            [Required]
            public int Id { get; set; }
            [Required]
            public required string Name { get; set; }
        }

        // DTO classes for new endpoints
        public class UserOnlineRequest
        {
            [Required]
            public int UserId { get; set; }
        }

        public class BroadcastRequest
        {
            [Required]
            public string Type { get; set; } = string.Empty;
            
            [Required]
            public string Message { get; set; } = string.Empty;
            
            public object? Data { get; set; }
        }
    }
}
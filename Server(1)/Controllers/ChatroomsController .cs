using Microsoft.AspNetCore.Mvc;
using Server_1_.Models; // Đảm bảo namespace của Models được import
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using Server_1_.Services;
using System.ComponentModel.DataAnnotations;

namespace Server_1_.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatroomsController : ControllerBase
    {
        private readonly IChatroomService _chatroomService;
        private readonly IMessageService _messageService; // Inject MessageService để lấy tin nhắn

        public ChatroomsController(IChatroomService chatroomService, IMessageService messageService)
        {
            _chatroomService = chatroomService;
            _messageService = messageService; // Lưu MessageService vào một biến
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
    }
}
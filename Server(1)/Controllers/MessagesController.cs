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
    [Route("api/[controller]")] // Định nghĩa route cho controller, ví dụ: /api/messages
    public class MessagesController : ControllerBase
    {
        private readonly IMessageService _messageService;

        public MessagesController(IMessageService messageService)
        {
            _messageService = messageService;
        }

        // POST /api/messages
        [HttpPost]
        public async Task<ActionResult<Messages>> SendMessage(SendMessageRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Message))
                {
                    return BadRequest("Message content cannot be null or empty."); // Validate the message content
                }

                var message = await _messageService.SendMessageAsync(request.SenderId, request.ChatroomId, request.Message);
                return CreatedAtAction(nameof(GetMessage), new { id = message.MessageId }, message); // Trả về 201 Created với thông tin về tin nhắn đã tạo
            }
            catch (Exception ex)
            {
                // Log lỗi ở đây
                return StatusCode(500, "Internal Server Error: " + ex.Message); // Trả về mã lỗi 500 nếu có lỗi
            }
        }

        // GET /api/messages/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Messages>> GetMessage(int id)
        {
            var message = await _messageService.GetMessageByIdAsync(id);
            if (message == null)
            {
                return NotFound(); // Trả về 404 Not Found nếu không tìm thấy
            }
            return Ok(message);
        }

        // GET /api/chatrooms/{chatroomId}/messages
        [HttpGet("chatrooms/{chatroomId}")]
        public async Task<ActionResult<IEnumerable<Messages>>> GetMessagesInChatroom(int chatroomId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var messages = await _messageService.GetMessagesInChatroomAsync(chatroomId, page, pageSize);
            return Ok(messages);
        }

        // PUT /api/messages/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> EditMessage(int id, EditMessageRequest request)
        {
            if (id != request.Id)
            {
                return BadRequest("Message ID mismatch"); // Trả về 400 Bad Request nếu ID không khớp
            }

            if (string.IsNullOrWhiteSpace(request.Message))
            {
                return BadRequest("Message content cannot be null or empty."); // Validate the message content
            }

            var result = await _messageService.EditMessageAsync(id, request.Message);
            if (!result)
            {
                return NotFound(); // Trả về 404 Not Found nếu không tìm thấy tin nhắn để sửa
            }
            return NoContent(); // Trả về 204 No Content nếu thành công
        }

        // DELETE /api/messages/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMessage(int id)
        {
            var result = await _messageService.DeleteMessageAsync(id);
            if (!result)
            {
                return NotFound(); // Trả về 404 Not Found nếu không tìm thấy tin nhắn để xóa
            }
            return NoContent(); // Trả về 204 No Content nếu thành công
        }

        // Các action khác cho các API endpoint khác
    }

    // Định nghĩa các lớp DTO (Data Transfer Objects) cho request
    public class SendMessageRequest
    {
        [Required]
        public int SenderId { get; set; }
        [Required]
        public int ChatroomId { get; set; }
        [Required]
        public string? Message { get; set; }
    }

    public class EditMessageRequest
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public string? Message { get; set; }
    }
}


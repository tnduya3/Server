using Microsoft.AspNetCore.Mvc;
using Server_1_.Models;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using Server_1_.Services;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.SignalR;
using Server_1_.Hubs;
using System.Linq;

namespace Server_1_.Controllers
{
    [ApiController]
    [Route("api/[controller]")] // Định nghĩa route cho controller, ví dụ: /api/messages
    public class MessagesController : ControllerBase
    {
        private readonly IMessageService _messageService;
        private readonly IUserService _userService;
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly IFirebaseNotificationService _firebaseNotificationService;

        public MessagesController(
            IMessageService messageService, 
            IUserService userService, 
            IHubContext<ChatHub> hubContext,
            IFirebaseNotificationService firebaseNotificationService)
        {
            _messageService = messageService;
            _userService = userService;
            _hubContext = hubContext;
            _firebaseNotificationService = firebaseNotificationService;
        }        
        
        // POST /api/messages
        [HttpPost]
        public async Task<ActionResult<Messages>> SendMessage(SendMessageRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Message))
                {
                    return BadRequest("Message content cannot be null or empty.");
                }

                // 1. Lưu tin nhắn vào database (Firebase notification đã được gửi trong MessageService)
                var message = await _messageService.SendMessageAsync(request.SenderId, request.ChatroomId, request.Message);
                
                // 2. Tạo response object using SenderName from the saved message
                var messageResponse = new
                {
                    MessageId = message.MessageId,
                    SenderId = message.SenderId,
                    SenderUsername = message.SenderName, // Use SenderName from the message model
                    ChatroomId = message.ChatRoomId,
                    Content = message.Message,
                    CreatedAt = message.CreatedAt,
                    MessageType = "text"
                };

                // 3. Gửi tin nhắn qua SignalR đến tất cả users trong chatroom
                await _hubContext.Clients.Group(request.ChatroomId.ToString())
                    .SendAsync("ReceiveMessage", messageResponse);

                // 5. Log thành công (Firebase notification đã được gửi trong MessageService)
                Console.WriteLine($"Message sent successfully - ID: {message.MessageId}, Firebase notifications sent");

                // 6. Trả về response cho API call
                return CreatedAtAction(nameof(GetMessage), new { id = message.MessageId }, messageResponse);
            }
            catch (Exception ex)
            {
                // Log lỗi ở đây
                Console.WriteLine($"Error in SendMessage: {ex.Message}");
                return StatusCode(500, "Internal Server Error: " + ex.Message);
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
        }        // GET /api/chatrooms/{chatroomId}/messages
        [HttpGet("chatrooms/{chatroomId}")]
        public async Task<ActionResult<IEnumerable<object>>> GetMessagesInChatroom(int chatroomId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var messages = await _messageService.GetMessagesInChatroomAsync(chatroomId, page, pageSize);
            
            // Return DTO to include SenderName and avoid potential circular references
            var result = messages.Select(m => new
            {
                m.MessageId,
                m.SenderId,
                m.SenderName, // Include SenderName field
                m.ChatRoomId,
                Content = m.Message,
                m.CreatedAt,
                m.UpdatedAt,
                m.IsDeleted
            });
            
            return Ok(result);
        }
        
        // PUT /api/messages/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> EditMessage(int id, EditMessageRequest request)
        {
            try
            {
                if (id != request.Id)
                {
                    return BadRequest("Message ID mismatch");
                }

                if (string.IsNullOrWhiteSpace(request.Message))
                {
                    return BadRequest("Message content cannot be null or empty.");
                }

                var result = await _messageService.EditMessageAsync(id, request.Message);
                if (!result)
                {
                    return NotFound();
                }

                // Lấy thông tin tin nhắn đã được edit
                var editedMessage = await _messageService.GetMessageByIdAsync(id);
                if (editedMessage != null)
                {
                    // Gửi thông báo qua SignalR về việc tin nhắn đã được chỉnh sửa
                    await _hubContext.Clients.Group(editedMessage.ChatRoomId.ToString())
                        .SendAsync("MessageEdited", new
                        {
                            MessageId = editedMessage.MessageId,
                            NewContent = editedMessage.Message,
                            EditedAt = editedMessage.UpdatedAt,
                            ChatroomId = editedMessage.ChatRoomId
                        });
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal Server Error: " + ex.Message);
            }
        }

        // DELETE /api/messages/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMessage(int id)
        {
            try
            {
                // Lấy thông tin tin nhắn trước khi xóa để có thông tin chatroom
                var messageToDelete = await _messageService.GetMessageByIdAsync(id);
                if (messageToDelete == null)
                {
                    return NotFound();
                }

                var result = await _messageService.DeleteMessageAsync(id);
                if (!result)
                {
                    return NotFound();
                }

                // Gửi thông báo qua SignalR về việc tin nhắn đã được xóa
                await _hubContext.Clients.Group(messageToDelete.ChatRoomId.ToString())
                    .SendAsync("MessageDeleted", new
                    {
                        MessageId = id,
                        ChatroomId = messageToDelete.ChatRoomId,
                        DeletedAt = DateTime.UtcNow
                    });

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal Server Error: " + ex.Message);
            }
        }

        // API endpoint để test Firebase notification
        [HttpPost("test-firebase")]
        public async Task<IActionResult> TestFirebaseNotification([FromBody] TestFirebaseRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.DeviceToken))
                {
                    return BadRequest("Device token is required");
                }

                var notificationData = new Dictionary<string, string>
                {
                    { "type", "test_notification" },
                    { "timestamp", DateTime.UtcNow.ToString() }
                };

                await _firebaseNotificationService.SendNotificationToDeviceAsync(
                    request.DeviceToken,
                    request.Title ?? "Test Notification",
                    request.Body ?? "This is a test notification from chat server",
                    notificationData
                );

                return Ok(new { 
                    Success = true, 
                    Message = "Firebase notification sent successfully",
                    SentAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { 
                    Success = false, 
                    Error = ex.Message 
                });
            }
        }

        // API endpoint để gửi broadcast notification cho tất cả users trong chatroom
        [HttpPost("chatrooms/{chatroomId}/broadcast-notification")]
        public async Task<IActionResult> BroadcastNotificationToChatroom(int chatroomId, [FromBody] BroadcastNotificationRequest request)
        {
            try
            {
                // Lấy tất cả participants trong chatroom
                var participants = await _messageService.GetParticipantsWithTokensAsync(chatroomId);
                
                if (!participants.Any())
                {
                    return NotFound("No participants found in chatroom or no device tokens available");
                }

                var notificationData = new Dictionary<string, string>
                {
                    { "chatroomId", chatroomId.ToString() },
                    { "type", "broadcast_notification" },
                    { "timestamp", DateTime.UtcNow.ToString() }
                };

                var successCount = 0;
                var failureCount = 0;

                foreach (var participant in participants)
                {
                    try
                    {
                        await _firebaseNotificationService.SendNotificationToDeviceAsync(
                            participant.DeviceToken,
                            request.Title ?? "Chatroom Notification",
                            request.Body ?? "You have a new notification",
                            notificationData
                        );
                        successCount++;
                    }
                    catch
                    {
                        failureCount++;
                    }
                }

                // Gửi thông báo qua SignalR
                await _hubContext.Clients.Group(chatroomId.ToString())
                    .SendAsync("BroadcastNotification", new
                    {
                        Title = request.Title,
                        Body = request.Body,
                        ChatroomId = chatroomId,
                        Timestamp = DateTime.UtcNow
                    });

                return Ok(new { 
                    Success = true,
                    ParticipantsNotified = successCount,
                    Failures = failureCount,
                    TotalParticipants = participants.Count(),
                    SentAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { 
                    Success = false, 
                    Error = ex.Message 
                });
            }
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
    
    public class TestFirebaseRequest
    {
        [Required]
        public string DeviceToken { get; set; } = string.Empty;
        public string? Title { get; set; }
        public string? Body { get; set; }
    }

    public class BroadcastNotificationRequest
    {
        [Required]
        public string Title { get; set; } = string.Empty;
        [Required]
        public string Body { get; set; } = string.Empty;
    }
}


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
    [Route("api/[controller]")]    
    
    public class ChatroomsController : ControllerBase
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
        }        // GET /api/chatrooms
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetChatrooms()
        {
            var chatrooms = await _chatroomService.GetChatroomsAsync();
            var result = chatrooms.Select(c => new
            {
                c.ChatRoomId,
                c.Name,
                c.Description,
                c.IsGroup,
                c.IsPrivate,
                c.IsArchived,
                c.CreatedBy,
                CreatorName = c.Creator?.UserName,
                c.CreatedAt,
                c.UpdatedAt,
                c.LastActivity,
                ParticipantCount = c.Participants?.Count ?? 0
            });
            return Ok(result);
        }// GET /api/chatrooms/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetChatroom(int id)
        {
            var chatroom = await _chatroomService.GetChatroomByIdAsync(id);
            if (chatroom == null)
            {
                return NotFound();
            }
            
            // Return a DTO to avoid circular reference
            var result = new
            {
                chatroom.ChatRoomId,
                chatroom.Name,
                chatroom.Description,
                chatroom.IsGroup,
                chatroom.IsPrivate,
                chatroom.IsArchived,
                chatroom.IsDeleted,
                chatroom.CreatedBy,
                CreatorName = chatroom.Creator?.UserName,
                chatroom.CreatedAt,
                chatroom.UpdatedAt,
                chatroom.LastActivity,
                ParticipantCount = chatroom.Participants?.Count ?? 0
            };
            
            return Ok(result);
        }// POST /api/chatrooms
        [HttpPost]
        public async Task<ActionResult<object>> CreateChatroom(CreateChatroomRequest request)
        {
            try
            {
                var chatroom = await _chatroomService.CreateChatroomAsync(
                    request.Name, 
                    request.CreatedBy, 
                    request.IsGroup, 
                    request.Description);
                
                // Return a simple DTO to avoid circular reference
                var result = new
                {
                    chatroom.ChatRoomId,
                    chatroom.Name,
                    chatroom.Description,
                    chatroom.IsGroup,
                    chatroom.IsPrivate,
                    chatroom.CreatedBy,
                    chatroom.CreatedAt,
                    chatroom.UpdatedAt,
                    chatroom.LastActivity
                };
                
                return CreatedAtAction(nameof(GetChatroom), new { id = chatroom.ChatRoomId }, result);
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

            var result = await _chatroomService.UpdateChatroomAsync(
                id, 
                request.Name, 
                request.Description, 
                request.UpdatedBy);
            if (!result)
            {
                return NotFound();
            }
            return NoContent();
        }

        // DELETE /api/chatrooms/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteChatroom(int id, [FromQuery] int deletedBy)
        {
            var result = await _chatroomService.SoftDeleteChatroomAsync(id, deletedBy);
            if (!result)
            {
                return NotFound();
            }
            return NoContent();
        }

        // DELETE /api/chatrooms/{id}/hard
        [HttpDelete("{id}/hard")]
        public async Task<IActionResult> HardDeleteChatroom(int id, [FromQuery] int deletedBy)
        {
            var result = await _chatroomService.DeleteChatroomAsync(id, deletedBy);
            if (!result)
            {
                return NotFound();
            }
            return NoContent();
        }        // GET /api/chatrooms/user/{userId}
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<object>>> GetUserChatrooms(int userId)
        {
            var chatrooms = await _chatroomService.GetUserChatroomsAsync(userId);
            var result = chatrooms.Select(c => new
            {
                c.ChatRoomId,
                c.Name,
                c.Description,
                c.IsGroup,
                c.IsPrivate,
                c.IsArchived,
                c.CreatedBy,
                CreatorName = c.Creator?.UserName,
                c.CreatedAt,
                c.UpdatedAt,
                c.LastActivity
            });
            return Ok(result);
        }

        // GET /api/chatrooms/search
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<object>>> SearchChatrooms([FromQuery] string searchTerm, [FromQuery] int userId)
        {
            if (string.IsNullOrEmpty(searchTerm))
            {
                return BadRequest("Search term is required");
            }
            
            var chatrooms = await _chatroomService.SearchChatroomsAsync(searchTerm, userId);
            var result = chatrooms.Select(c => new
            {
                c.ChatRoomId,
                c.Name,
                c.Description,
                c.IsGroup,
                c.IsPrivate,
                c.IsArchived,
                c.CreatedBy,
                CreatorName = c.Creator?.UserName,
                c.CreatedAt,
                c.UpdatedAt,
                c.LastActivity
            });
            return Ok(result);
        }

        // GET /api/chatrooms/archived/{userId}
        [HttpGet("archived/{userId}")]
        public async Task<ActionResult<IEnumerable<object>>> GetArchivedChatrooms(int userId)
        {
            var chatrooms = await _chatroomService.GetArchivedChatroomsAsync(userId);
            var result = chatrooms.Select(c => new
            {
                c.ChatRoomId,
                c.Name,
                c.Description,
                c.IsGroup,
                c.IsPrivate,
                c.IsArchived,
                c.CreatedBy,
                CreatorName = c.Creator?.UserName,
                c.CreatedAt,
                c.UpdatedAt,
                c.LastActivity
            });
            return Ok(result);
        }

        // POST /api/chatrooms/{chatroomId}/users/{userId}
        [HttpPost("{chatroomId}/users/{userId}")]
        public async Task<IActionResult> AddUserToChatroom(int chatroomId, int userId, [FromBody] AddUserRequest? request = null)
        {
            var result = await _chatroomService.AddUserToChatroomAsync(
                chatroomId, 
                userId, 
                request?.Role ?? "member", 
                request?.AddedBy);
            if (!result)
            {
                return BadRequest("Failed to add user to chatroom");
            }
            return Ok();
        }

        // POST /api/chatrooms/{chatroomId}/users/bulk
        [HttpPost("{chatroomId}/users/bulk")]
        public async Task<IActionResult> AddUsersToChatroom(int chatroomId, [FromBody] AddUsersRequest request)
        {
            var result = await _chatroomService.AddUsersToChatroomAsync(
                chatroomId, 
                request.UserIds, 
                request.Role ?? "member", 
                request.AddedBy);
            if (!result)
            {
                return BadRequest("Failed to add users to chatroom");
            }
            return Ok();
        }

        // DELETE /api/chatrooms/{chatroomId}/users/{userId}
        [HttpDelete("{chatroomId}/users/{userId}")]
        public async Task<IActionResult> RemoveUserFromChatroom(int chatroomId, int userId, [FromQuery] int? removedBy = null)
        {
            var result = await _chatroomService.RemoveUserFromChatroomAsync(chatroomId, userId, removedBy);
            if (!result)
            {
                return NotFound();
            }
            return NoContent();
        }

        // PUT /api/chatrooms/{chatroomId}/users/{userId}/role
        [HttpPut("{chatroomId}/users/{userId}/role")]
        public async Task<IActionResult> UpdateUserRole(int chatroomId, int userId, [FromBody] UpdateRoleRequest request)
        {
            var result = await _chatroomService.UpdateUserRoleAsync(chatroomId, userId, request.Role, request.UpdatedBy);
            if (!result)
            {
                return NotFound();
            }
            return NoContent();
        }        // GET /api/chatrooms/{chatroomId}/users
        [HttpGet("{chatroomId}/users")]
        public async Task<ActionResult<List<object>>> GetParticipantsInChatroom(int chatroomId)
        {
            var users = await _chatroomService.GetParticipantsInChatroomAsync(chatroomId);
            var result = users.Select(u => new
            {
                u.UserId,
                u.UserName,
                u.Email,
                u.IsOnline,
                u.CreatedAt
            }).ToList();
            return Ok(result);
        }

        // GET /api/chatrooms/{chatroomId}/participants
        [HttpGet("{chatroomId}/participants")]
        public async Task<ActionResult<List<ParticipantInfo>>> GetParticipantsWithRoles(int chatroomId)
        {
            var participants = await _chatroomService.GetParticipantsWithRolesAsync(chatroomId);
            return Ok(participants);
        }        // GET /api/chatrooms/{chatroomId}/participants/active
        [HttpGet("{chatroomId}/participants/active")]
        public async Task<ActionResult<List<object>>> GetActiveParticipants(int chatroomId)
        {
            var users = await _chatroomService.GetActiveParticipantsAsync(chatroomId);
            var result = users.Select(u => new
            {
                u.UserId,
                u.UserName,
                u.Email,
                u.IsOnline,
                u.CreatedAt
            }).ToList();
            return Ok(result);
        }

        // GET /api/chatrooms/{chatroomId}/participants/count
        [HttpGet("{chatroomId}/participants/count")]
        public async Task<ActionResult<int>> GetParticipantCount(int chatroomId)
        {
            var count = await _chatroomService.GetParticipantCountAsync(chatroomId);
            return Ok(count);
        }

        // GET /api/chatrooms/{chatroomId}/users/{userId}/role
        [HttpGet("{chatroomId}/users/{userId}/role")]
        public async Task<ActionResult<string>> GetUserRole(int chatroomId, int userId)
        {
            var role = await _chatroomService.GetUserRoleInChatroomAsync(chatroomId, userId);
            if (role == null)
            {
                return NotFound("User not found in chatroom");
            }
            return Ok(role);
        }

        // GET /api/chatrooms/{chatroomId}/users/{userId}/access
        [HttpGet("{chatroomId}/users/{userId}/access")]
        public async Task<ActionResult<bool>> CanUserAccess(int chatroomId, int userId)
        {
            var canAccess = await _chatroomService.CanUserAccessChatroomAsync(chatroomId, userId);
            return Ok(canAccess);
        }

        // POST /api/chatrooms/{chatroomId}/archive
        [HttpPost("{chatroomId}/archive")]
        public async Task<IActionResult> ArchiveChatroom(int chatroomId, [FromBody] ArchiveRequest request)
        {
            var result = await _chatroomService.ArchiveChatroomAsync(chatroomId, request.ArchivedBy);
            if (!result)
            {
                return NotFound();
            }
            return NoContent();
        }

        // POST /api/chatrooms/{chatroomId}/unarchive
        [HttpPost("{chatroomId}/unarchive")]
        public async Task<IActionResult> UnarchiveChatroom(int chatroomId, [FromBody] ArchiveRequest request)
        {
            var result = await _chatroomService.UnarchiveChatroomAsync(chatroomId, request.ArchivedBy);
            if (!result)
            {
                return NotFound();
            }
            return NoContent();
        }        // GET /api/chatrooms/{chatroomId}/messages
        [HttpGet("{chatroomId}/messages")]
        public async Task<ActionResult<IEnumerable<object>>> GetMessagesInChatroom(int chatroomId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var messages = await _messageService.GetMessagesInChatroomAsync(chatroomId, page, pageSize);
            
            // Return DTO to include SenderName and avoid circular references
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
        }        // API endpoint để lấy thống kê chatroom
        [HttpGet("{chatroomId}/stats")]
        public async Task<ActionResult<ChatroomStats>> GetChatroomStats(int chatroomId)
        {
            try
            {
                var stats = await _chatroomService.GetChatroomStatsAsync(chatroomId);
                if (stats.ChatroomId == 0)
                {
                    return NotFound();
                }
                return Ok(stats);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal Server Error: " + ex.Message);
            }
        }

        // GET /api/chatrooms/{chatroomId}/activity
        [HttpGet("{chatroomId}/activity")]
        public async Task<ActionResult<DateTime?>> GetLastActivity(int chatroomId)
        {
            var lastActivity = await _chatroomService.GetLastActivityAsync(chatroomId);
            return Ok(lastActivity);
        }

        // GET /api/chatrooms/{chatroomId}/messages/count
        [HttpGet("{chatroomId}/messages/count")]
        public async Task<ActionResult<int>> GetMessageCount(int chatroomId)
        {
            var count = await _chatroomService.GetMessageCountAsync(chatroomId);
            return Ok(count);
        }        // DTO cho các request
        public class CreateChatroomRequest
        {
            [Required]
            public required string Name { get; set; }
            
            [Required]
            public int CreatedBy { get; set; }
            
            public bool IsGroup { get; set; } = true;
            
            public string? Description { get; set; }
        }

        public class UpdateChatroomRequest
        {
            [Required]
            public int Id { get; set; }
            
            public string? Name { get; set; }
            
            public string? Description { get; set; }
            
            public int? UpdatedBy { get; set; }
        }

        public class AddUserRequest
        {
            public string Role { get; set; } = "member";
            public int? AddedBy { get; set; }
        }

        public class AddUsersRequest
        {
            [Required]
            public required List<int> UserIds { get; set; }
            
            public string Role { get; set; } = "member";
            
            public int? AddedBy { get; set; }
        }

        public class UpdateRoleRequest
        {
            [Required]
            public required string Role { get; set; }
            
            [Required]
            public int UpdatedBy { get; set; }
        }

        public class ArchiveRequest
        {
            [Required]
            public int ArchivedBy { get; set; }
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
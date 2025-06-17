using Microsoft.AspNetCore.Mvc;
using Server_1_.Services;
using Server_1_.Models;

namespace Server_1_.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FriendsController : ControllerBase
    {
        private readonly IFriendService _friendService;
        private readonly IChatroomService _chatroomService;
        private readonly IMessageService _messageService;

        public FriendsController(
            IFriendService friendService, 
            IChatroomService chatroomService,
            IMessageService messageService)
        {
            _friendService = friendService;
            _chatroomService = chatroomService;
            _messageService = messageService;
        }

        [HttpPost("send-request")]
        public async Task<IActionResult> SendFriendRequest([FromBody] FriendRequestDto request)
        {
            var result = await _friendService.SendFriendRequestAsync(request.UserId, request.FriendId);
            
            if (result)
                return Ok(new { message = "Yêu cầu kết bạn đã được gửi thành công" });
            
            return BadRequest(new { message = "Không thể gửi yêu cầu kết bạn" });
        }

        [HttpPost("accept-request")]
        public async Task<IActionResult> AcceptFriendRequest([FromBody] FriendRequestDto request)
        {
            var result = await _friendService.AcceptFriendRequestAsync(request.UserId, request.FriendId);
            
            if (result)
                return Ok(new { message = "Đã chấp nhận yêu cầu kết bạn" });
            
            return BadRequest(new { message = "Không thể chấp nhận yêu cầu kết bạn" });
        }

        [HttpPost("reject-request")]
        public async Task<IActionResult> RejectFriendRequest([FromBody] FriendRequestDto request)
        {
            var result = await _friendService.RejectFriendRequestAsync(request.UserId, request.FriendId);
            
            if (result)
                return Ok(new { message = "Đã từ chối yêu cầu kết bạn" });
            
            return BadRequest(new { message = "Không thể từ chối yêu cầu kết bạn" });
        }

        [HttpPost("block-user")]
        public async Task<IActionResult> BlockUser([FromBody] FriendRequestDto request)
        {
            var result = await _friendService.BlockUserAsync(request.UserId, request.FriendId);
            
            if (result)
                return Ok(new { message = "Đã chặn người dùng" });
            
            return BadRequest(new { message = "Không thể chặn người dùng" });
        }

        [HttpPost("unfriend")]
        public async Task<IActionResult> Unfriend([FromBody] FriendRequestDto request)
        {
            var result = await _friendService.UnfriendAsync(request.UserId, request.FriendId);
            
            if (result)
                return Ok(new { message = "Đã hủy kết bạn" });
            
            return BadRequest(new { message = "Không thể hủy kết bạn" });
        }

        [HttpGet("{userId}/friends")]
        public async Task<IActionResult> GetFriends(int userId)
        {
            var friends = await _friendService.GetFriendsAsync(userId);
            return Ok(friends);
        }

        [HttpGet("{userId}/pending-requests")]
        public async Task<IActionResult> GetPendingRequests(int userId)
        {
            var pendingRequests = await _friendService.GetPendingRequestsAsync(userId);
            return Ok(pendingRequests);
        }

        [HttpGet("{userId}/sent-requests")]
        public async Task<IActionResult> GetSentRequests(int userId)
        {
            var sentRequests = await _friendService.GetSentRequestsAsync(userId);
            return Ok(sentRequests);
        }

        [HttpGet("{userId}/status/{friendId}")]
        public async Task<IActionResult> GetFriendshipStatus(int userId, int friendId)
        {
            var status = await _friendService.GetFriendshipStatusAsync(userId, friendId);
            
            if (status.HasValue)
                return Ok(new { status = status.Value.ToString() });
            
            return Ok(new { status = "None" });
        }        
        
        [HttpGet("{userId}/are-friends/{friendId}")]
        public async Task<IActionResult> AreFriends(int userId, int friendId)
        {
            var areFriends = await _friendService.AreFriendsAsync(userId, friendId);
            return Ok(new { areFriends });
        }

        // New endpoints for direct messaging with friends
        [HttpPost("{userId}/start-chat/{friendId}")]
        public async Task<IActionResult> StartChatWithFriend(int userId, int friendId, [FromBody] StartChatRequest? request = null)
        {
            try
            {
                // Check if users are friends
                var areFriends = await _friendService.AreFriendsAsync(userId, friendId);
                if (!areFriends)
                {
                    return BadRequest(new { message = "You can only start chat with friends" });
                }

                // Create or get existing direct chatroom
                var chatroom = await _chatroomService.CreateDirectChatroomWithFriendAsync(userId, friendId);

                // Send initial message if provided
                if (!string.IsNullOrWhiteSpace(request?.InitialMessage))
                {
                    var message = await _messageService.SendMessageAsync(userId, chatroom.ChatRoomId, request.InitialMessage);
                }

                // Return chatroom information
                var result = new
                {
                    chatroom.ChatRoomId,
                    chatroom.Name,
                    chatroom.Description,
                    chatroom.IsGroup,
                    chatroom.IsPrivate,
                    chatroom.CreatedAt,
                    chatroom.LastActivity,
                    Message = "Direct chat created successfully",
                    HasInitialMessage = !string.IsNullOrWhiteSpace(request?.InitialMessage)
                };

                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", detail = ex.Message });
            }
        }

        [HttpGet("{userId}/direct-chats")]
        public async Task<IActionResult> GetDirectChats(int userId)
        {
            try
            {
                // Get all direct chatrooms for the user
                var userChatrooms = await _chatroomService.GetUserChatroomsAsync(userId);
                var directChats = userChatrooms.Where(c => !c.IsGroup && !c.IsDeleted).ToList();

                var result = directChats.Select(c => new
                {
                    c.ChatRoomId,
                    c.Name,
                    c.Description,
                    c.IsGroup,
                    c.IsPrivate,
                    c.CreatedAt,
                    c.LastActivity,
                    c.UpdatedAt
                });

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", detail = ex.Message });
            }
        }

        [HttpGet("{userId}/check-direct-chat/{friendId}")]
        public async Task<IActionResult> CheckExistingDirectChat(int userId, int friendId)
        {
            try
            {
                // Check if users are friends
                var areFriends = await _friendService.AreFriendsAsync(userId, friendId);
                if (!areFriends)
                {
                    return BadRequest(new { message = "You can only check chat with friends" });
                }

                // Check if direct chatroom exists
                var existingChatroom = await _chatroomService.GetExistingDirectChatroomAsync(userId, friendId);

                if (existingChatroom != null)
                {
                    var result = new
                    {
                        Exists = true,
                        ChatroomId = existingChatroom.ChatRoomId,
                        existingChatroom.Name,
                        existingChatroom.Description,
                        existingChatroom.LastActivity
                    };
                    return Ok(result);
                }

                return Ok(new { Exists = false });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", detail = ex.Message });
            }
        }
    }

    // DTOs for the new endpoints
    public class StartChatRequest
    {
        public string? InitialMessage { get; set; }
    }
}

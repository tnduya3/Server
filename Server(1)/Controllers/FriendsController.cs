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

        public FriendsController(IFriendService friendService)
        {
            _friendService = friendService;
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
    }
}

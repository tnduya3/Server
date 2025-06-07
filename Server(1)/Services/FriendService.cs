using Microsoft.EntityFrameworkCore;
using Server_1_.Data;
using Server_1_.Models;

namespace Server_1_.Services
{
    public class FriendService : IFriendService
    {
        private readonly AppDbContext _context;

        public FriendService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> SendFriendRequestAsync(int userId, int friendId)
        {
            // Kiểm tra nếu user gửi yêu cầu cho chính mình
            if (userId == friendId)
                return false;

            // Kiểm tra nếu đã có quan hệ bạn bè hoặc yêu cầu kết bạn
            var existingFriendship = await _context.Friends
                .FirstOrDefaultAsync(f => 
                    (f.UserId == userId && f.FriendId == friendId) || 
                    (f.UserId == friendId && f.FriendId == userId));

            if (existingFriendship != null)
                return false; // Đã có quan hệ

            // Kiểm tra nếu user và friend có tồn tại
            var userExists = await _context.Users.AnyAsync(u => u.UserId == userId);
            var friendExists = await _context.Users.AnyAsync(u => u.UserId == friendId);

            if (!userExists || !friendExists)
                return false;

            // Tạo yêu cầu kết bạn
            var friendRequest = new Friends
            {
                UserId = userId,
                FriendId = friendId,
                Status = Friends.FriendshipStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            _context.Friends.Add(friendRequest);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AcceptFriendRequestAsync(int userId, int friendId)
        {
            // Tìm yêu cầu kết bạn pending
            var friendRequest = await _context.Friends
                .FirstOrDefaultAsync(f => f.UserId == friendId && f.FriendId == userId && f.Status == Friends.FriendshipStatus.Pending);

            if (friendRequest == null)
                return false;

            // Cập nhật trạng thái thành Accepted
            friendRequest.Status = Friends.FriendshipStatus.Accepted;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RejectFriendRequestAsync(int userId, int friendId)
        {
            // Tìm yêu cầu kết bạn pending
            var friendRequest = await _context.Friends
                .FirstOrDefaultAsync(f => f.UserId == friendId && f.FriendId == userId && f.Status == Friends.FriendshipStatus.Pending);

            if (friendRequest == null)
                return false;

            // Xóa yêu cầu kết bạn
            _context.Friends.Remove(friendRequest);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> BlockUserAsync(int userId, int friendId)
        {
            // Tìm quan hệ hiện tại
            var friendship = await _context.Friends
                .FirstOrDefaultAsync(f => 
                    (f.UserId == userId && f.FriendId == friendId) || 
                    (f.UserId == friendId && f.FriendId == userId));

            if (friendship != null)
            {
                // Nếu đã có quan hệ, cập nhật thành Blocked
                friendship.Status = Friends.FriendshipStatus.Blocked;
                friendship.UserId = userId; // Người block
                friendship.FriendId = friendId; // Người bị block
            }
            else
            {
                // Tạo mới quan hệ Blocked
                friendship = new Friends
                {
                    UserId = userId,
                    FriendId = friendId,
                    Status = Friends.FriendshipStatus.Blocked,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Friends.Add(friendship);
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UnfriendAsync(int userId, int friendId)
        {
            // Tìm quan hệ bạn bè đã được chấp nhận
            var friendship = await _context.Friends
                .FirstOrDefaultAsync(f => 
                    ((f.UserId == userId && f.FriendId == friendId) || 
                     (f.UserId == friendId && f.FriendId == userId)) && 
                    f.Status == Friends.FriendshipStatus.Accepted);

            if (friendship == null)
                return false;

            // Xóa quan hệ bạn bè
            _context.Friends.Remove(friendship);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<Users>> GetFriendsAsync(int userId)
        {
            var friends = await _context.Friends
                .Where(f => (f.UserId == userId || f.FriendId == userId) && f.Status == Friends.FriendshipStatus.Accepted)
                .Select(f => f.UserId == userId ? f.FriendUser : f.User)
                .ToListAsync();

            return friends;
        }

        public async Task<List<Users>> GetPendingRequestsAsync(int userId)
        {
            // Lấy danh sách yêu cầu kết bạn mà user nhận được
            var pendingRequests = await _context.Friends
                .Where(f => f.FriendId == userId && f.Status == Friends.FriendshipStatus.Pending)
                .Select(f => f.User)
                .ToListAsync();

            return pendingRequests;
        }

        public async Task<List<Users>> GetSentRequestsAsync(int userId)
        {
            // Lấy danh sách yêu cầu kết bạn mà user đã gửi
            var sentRequests = await _context.Friends
                .Where(f => f.UserId == userId && f.Status == Friends.FriendshipStatus.Pending)
                .Select(f => f.FriendUser)
                .ToListAsync();

            return sentRequests;
        }

        public async Task<Friends.FriendshipStatus?> GetFriendshipStatusAsync(int userId, int friendId)
        {
            var friendship = await _context.Friends
                .FirstOrDefaultAsync(f => 
                    (f.UserId == userId && f.FriendId == friendId) || 
                    (f.UserId == friendId && f.FriendId == userId));

            return friendship?.Status;
        }

        public async Task<bool> AreFriendsAsync(int userId, int friendId)
        {
            var friendship = await _context.Friends
                .AnyAsync(f => 
                    ((f.UserId == userId && f.FriendId == friendId) || 
                     (f.UserId == friendId && f.FriendId == userId)) && 
                    f.Status == Friends.FriendshipStatus.Accepted);

            return friendship;
        }
    }
}

using Server_1_.Models;

namespace Server_1_.Services
{
    public interface IFriendService
    {
        Task<bool> SendFriendRequestAsync(int userId, int friendId);
        Task<bool> AcceptFriendRequestAsync(int userId, int friendId);
        Task<bool> RejectFriendRequestAsync(int userId, int friendId);
        Task<bool> BlockUserAsync(int userId, int friendId);
        Task<bool> UnfriendAsync(int userId, int friendId);
        Task<List<Users>> GetFriendsAsync(int userId);
        Task<List<Users>> GetPendingRequestsAsync(int userId);
        Task<List<Users>> GetSentRequestsAsync(int userId);
        Task<Friends.FriendshipStatus?> GetFriendshipStatusAsync(int userId, int friendId);
        Task<bool> AreFriendsAsync(int userId, int friendId);
    }
}

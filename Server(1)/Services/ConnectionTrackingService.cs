using System.Collections.Concurrent;

namespace Server_1_.Services
{
    public interface IConnectionTrackingService
    {
        Task UserConnectedAsync(string connectionId, string userId);
        Task UserDisconnectedAsync(string connectionId);
        Task UserJoinedChatroomAsync(string connectionId, string chatroomId);
        Task UserLeftChatroomAsync(string connectionId, string chatroomId);
        List<string> GetConnectedUsers();
        List<string> GetUsersInChatroom(string chatroomId);
        List<string> GetChatroomsForUser(string userId);
        bool IsUserOnline(string userId);
        int GetOnlineUsersCount();
    }

    public class ConnectionTrackingService : IConnectionTrackingService
    {
        // ConnectionId -> UserId mapping
        private readonly ConcurrentDictionary<string, string> _connections = new();
        
        // UserId -> List of ConnectionIds (một user có thể có nhiều kết nối)
        private readonly ConcurrentDictionary<string, HashSet<string>> _userConnections = new();
        
        // ChatroomId -> List of ConnectionIds
        private readonly ConcurrentDictionary<string, HashSet<string>> _chatroomConnections = new();
        
        // ConnectionId -> List of ChatroomIds
        private readonly ConcurrentDictionary<string, HashSet<string>> _connectionChatrooms = new();

        public async Task UserConnectedAsync(string connectionId, string userId)
        {
            _connections[connectionId] = userId;

            // Thêm connection vào user's connections
            _userConnections.AddOrUpdate(userId, 
                new HashSet<string> { connectionId },
                (key, existing) => { existing.Add(connectionId); return existing; });

            await Task.CompletedTask;
        }

        public async Task UserDisconnectedAsync(string connectionId)
        {
            if (_connections.TryRemove(connectionId, out string? userId))
            {
                // Xóa connection khỏi user's connections
                if (_userConnections.TryGetValue(userId, out HashSet<string>? userConnections))
                {
                    userConnections.Remove(connectionId);
                    if (userConnections.Count == 0)
                    {
                        _userConnections.TryRemove(userId, out _);
                    }
                }

                // Xóa user khỏi tất cả chatrooms
                if (_connectionChatrooms.TryRemove(connectionId, out HashSet<string>? chatrooms))
                {
                    foreach (var chatroomId in chatrooms)
                    {
                        if (_chatroomConnections.TryGetValue(chatroomId, out HashSet<string>? chatroomConnections))
                        {
                            chatroomConnections.Remove(connectionId);
                            if (chatroomConnections.Count == 0)
                            {
                                _chatroomConnections.TryRemove(chatroomId, out _);
                            }
                        }
                    }
                }
            }

            await Task.CompletedTask;
        }

        public async Task UserJoinedChatroomAsync(string connectionId, string chatroomId)
        {
            // Thêm connection vào chatroom
            _chatroomConnections.AddOrUpdate(chatroomId,
                new HashSet<string> { connectionId },
                (key, existing) => { existing.Add(connectionId); return existing; });

            // Thêm chatroom vào connection's chatrooms
            _connectionChatrooms.AddOrUpdate(connectionId,
                new HashSet<string> { chatroomId },
                (key, existing) => { existing.Add(chatroomId); return existing; });

            await Task.CompletedTask;
        }

        public async Task UserLeftChatroomAsync(string connectionId, string chatroomId)
        {
            // Xóa connection khỏi chatroom
            if (_chatroomConnections.TryGetValue(chatroomId, out HashSet<string>? chatroomConnections))
            {
                chatroomConnections.Remove(connectionId);
                if (chatroomConnections.Count == 0)
                {
                    _chatroomConnections.TryRemove(chatroomId, out _);
                }
            }

            // Xóa chatroom khỏi connection's chatrooms
            if (_connectionChatrooms.TryGetValue(connectionId, out HashSet<string>? connectionChatrooms))
            {
                connectionChatrooms.Remove(chatroomId);
                if (connectionChatrooms.Count == 0)
                {
                    _connectionChatrooms.TryRemove(connectionId, out _);
                }
            }

            await Task.CompletedTask;
        }

        public List<string> GetConnectedUsers()
        {
            return _userConnections.Keys.ToList();
        }

        public List<string> GetUsersInChatroom(string chatroomId)
        {
            if (!_chatroomConnections.TryGetValue(chatroomId, out HashSet<string>? connections))
            {
                return new List<string>();
            }

            var users = new HashSet<string>();
            foreach (var connectionId in connections)
            {
                if (_connections.TryGetValue(connectionId, out string? userId))
                {
                    users.Add(userId);
                }
            }

            return users.ToList();
        }

        public List<string> GetChatroomsForUser(string userId)
        {
            if (!_userConnections.TryGetValue(userId, out HashSet<string>? connections))
            {
                return new List<string>();
            }

            var chatrooms = new HashSet<string>();
            foreach (var connectionId in connections)
            {
                if (_connectionChatrooms.TryGetValue(connectionId, out HashSet<string>? userChatrooms))
                {
                    foreach (var chatroom in userChatrooms)
                    {
                        chatrooms.Add(chatroom);
                    }
                }
            }

            return chatrooms.ToList();
        }

        public bool IsUserOnline(string userId)
        {
            return _userConnections.ContainsKey(userId);
        }

        public int GetOnlineUsersCount()
        {
            return _userConnections.Count;
        }
    }
}

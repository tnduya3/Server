using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Server_1_.Models;

namespace Server_1_.Services
{    // Interface cho ChatroomService
    public interface IChatroomService
    {
        // Basic CRUD operations
        Task<ChatRooms> CreateChatroomAsync(string name, int createdBy, bool isGroup = true, string? description = null);
        Task<ChatRooms?> GetChatroomByIdAsync(int id);
        Task<List<ChatRooms>> GetChatroomsAsync();
        Task<List<ChatRooms>> GetUserChatroomsAsync(int userId);
        Task<List<ChatRooms>> SearchChatroomsAsync(string searchTerm, int userId);
        Task<bool> UpdateChatroomAsync(int id, string? newName = null, string? description = null, int? updatedBy = null);
        Task<bool> DeleteChatroomAsync(int id, int deletedBy);
        Task<bool> SoftDeleteChatroomAsync(int id, int deletedBy);

        // Participant management
        Task<bool> AddUserToChatroomAsync(int chatroomId, int userId, string role = "member", int? addedBy = null);
        Task<bool> AddUsersToChatroomAsync(int chatroomId, List<int> userIds, string role = "member", int? addedBy = null);
        Task<bool> RemoveUserFromChatroomAsync(int chatroomId, int userId, int? removedBy = null);
        Task<bool> UpdateUserRoleAsync(int chatroomId, int userId, string newRole, int updatedBy);
        Task<List<Users>> GetParticipantsInChatroomAsync(int chatroomId);
        Task<List<ParticipantInfo>> GetParticipantsWithRolesAsync(int chatroomId);
        Task<bool> IsUserParticipantAsync(int chatroomId, int userId);
        Task<string?> GetUserRoleInChatroomAsync(int chatroomId, int userId);
        Task<int> GetParticipantCountAsync(int chatroomId);
        Task<List<Users>> GetActiveParticipantsAsync(int chatroomId);

        // Permission & Security
        Task<bool> CanUserAccessChatroomAsync(int chatroomId, int userId);
        Task<bool> CanUserModifyChatroomAsync(int chatroomId, int userId);
        Task<bool> IsUserAdminOrOwnerAsync(int chatroomId, int userId);

        // Statistics & Analytics
        Task<ChatroomStats> GetChatroomStatsAsync(int chatroomId);
        Task<DateTime?> GetLastActivityAsync(int chatroomId);
        Task<int> GetMessageCountAsync(int chatroomId);        // Advanced features
        Task<bool> ArchiveChatroomAsync(int chatroomId, int archivedBy);
        Task<bool> UnarchiveChatroomAsync(int chatroomId, int unarchivedBy);
        Task<List<ChatRooms>> GetArchivedChatroomsAsync(int userId);
        
        // Direct messaging with friends
        Task<ChatRooms> CreateDirectChatroomWithFriendAsync(int userId, int friendId);
        Task<ChatRooms?> GetExistingDirectChatroomAsync(int userId, int friendId);
        Task<bool> CreateAndStartChatWithFriendAsync(int userId, int friendId, string? initialMessage = null);
    }

    // DTOs for enhanced functionality
    public class ParticipantInfo
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public DateTime JoinedAt { get; set; }
        public DateTime? LeftAt { get; set; }
        public bool IsOnline { get; set; }
    }

    public class ChatroomStats
    {
        public int ChatroomId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int TotalParticipants { get; set; }
        public int ActiveParticipants { get; set; }
        public int TotalMessages { get; set; }
        public DateTime? LastActivity { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
    }
}
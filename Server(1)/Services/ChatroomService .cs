using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Server_1_.Models;
using Server_1_.Data;

namespace Server_1_.Services
{    // Class triển khai IChatroomService
    public class ChatroomService : IChatroomService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ChatroomService> _logger;

        public ChatroomService(AppDbContext context, ILogger<ChatroomService> logger)
        {
            _context = context;
            _logger = logger;
        }        // Basic CRUD operations
        public async Task<ChatRooms> CreateChatroomAsync(string name, int createdBy, bool isGroup = true, string? description = null)
        {
            try
            {
                var newChatroom = new ChatRooms
                {
                    Name = name,
                    Description = description,
                    CreatedBy = createdBy,
                    IsGroup = isGroup,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    LastActivity = DateTime.UtcNow
                };

                _context.Chatrooms.Add(newChatroom);
                await _context.SaveChangesAsync();

                // Automatically add creator as owner
                await AddUserToChatroomAsync(newChatroom.ChatRoomId, createdBy, "owner", createdBy);

                _logger.LogInformation("Chatroom {ChatroomName} created by user {UserId}", name, createdBy);
                return newChatroom;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating chatroom {ChatroomName} by user {UserId}", name, createdBy);
                throw;
            }
        }

        public async Task<ChatRooms?> GetChatroomByIdAsync(int id)
        {
            return await _context.Chatrooms
                .Include(c => c.Creator)
                .Include(c => c.Participants)
                .ThenInclude(p => p.User)
                .FirstOrDefaultAsync(c => c.ChatRoomId == id && !c.IsDeleted);
        }

        public async Task<List<ChatRooms>> GetChatroomsAsync()
        {
            return await _context.Chatrooms
                .Include(c => c.Creator)
                .Where(c => !c.IsDeleted)
                .OrderByDescending(c => c.LastActivity)
                .ToListAsync();
        }

        public async Task<List<ChatRooms>> GetUserChatroomsAsync(int userId)
        {
            return await _context.Participants
                .Where(p => p.UserId == userId && p.LeftAt == null && p.IsActive)
                .Include(p => p.Chatroom)
                .ThenInclude(c => c.Creator)
                .Select(p => p.Chatroom)
                .Where(c => c != null && !c.IsDeleted)
                .OrderByDescending(c => c.LastActivity)
                .ToListAsync();
        }

        public async Task<List<ChatRooms>> SearchChatroomsAsync(string searchTerm, int userId)
        {
            var userChatrooms = await _context.Participants
                .Where(p => p.UserId == userId && p.LeftAt == null && p.IsActive)
                .Select(p => p.ChatroomId)
                .ToListAsync();

            return await _context.Chatrooms
                .Include(c => c.Creator)
                .Where(c => !c.IsDeleted && 
                           userChatrooms.Contains(c.ChatRoomId) &&
                           (c.Name.Contains(searchTerm) || 
                            (c.Description != null && c.Description.Contains(searchTerm))))
                .OrderByDescending(c => c.LastActivity)
                .ToListAsync();
        }

        public async Task<bool> UpdateChatroomAsync(int id, string? newName = null, string? description = null, int? updatedBy = null)
        {
            try
            {
                var chatroom = await _context.Chatrooms.FindAsync(id);
                if (chatroom == null || chatroom.IsDeleted)
                {
                    return false;
                }

                if (!string.IsNullOrEmpty(newName))
                    chatroom.Name = newName;
                
                if (description != null)
                    chatroom.Description = description;

                chatroom.UpdatedAt = DateTime.UtcNow;
                chatroom.UpdatedBy = updatedBy;

                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Chatroom {ChatroomId} updated by user {UserId}", id, updatedBy);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating chatroom {ChatroomId}", id);
                return false;
            }
        }

        public async Task<bool> DeleteChatroomAsync(int id, int deletedBy)
        {
            try
            {
                var chatroom = await _context.Chatrooms.FindAsync(id);
                if (chatroom == null)
                {
                    return false;
                }

                _context.Chatrooms.Remove(chatroom);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Chatroom {ChatroomId} hard deleted by user {UserId}", id, deletedBy);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting chatroom {ChatroomId}", id);
                return false;
            }
        }

        public async Task<bool> SoftDeleteChatroomAsync(int id, int deletedBy)
        {
            try
            {
                var chatroom = await _context.Chatrooms.FindAsync(id);
                if (chatroom == null || chatroom.IsDeleted)
                {
                    return false;
                }

                chatroom.IsDeleted = true;
                chatroom.DeletedBy = deletedBy;
                chatroom.DeletedAt = DateTime.UtcNow;
                chatroom.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Chatroom {ChatroomId} soft deleted by user {UserId}", id, deletedBy);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error soft deleting chatroom {ChatroomId}", id);
                return false;
            }
        }        // Participant management
        public async Task<bool> AddUserToChatroomAsync(int chatroomId, int userId, string role = "member", int? addedBy = null)
        {
            try
            {
                var chatroom = await _context.Chatrooms.FindAsync(chatroomId);
                var user = await _context.Users.FindAsync(userId);
                if (chatroom == null || user == null || chatroom.IsDeleted)
                {
                    return false;
                }

                // Check if user is already a participant
                var existingParticipant = await _context.Participants
                    .FirstOrDefaultAsync(p => p.ChatroomId == chatroomId && p.UserId == userId && p.IsActive);
                
                if (existingParticipant != null)
                {
                    return false; // User already in chatroom
                }

                var participant = new Participants
                {
                    ChatroomId = chatroomId,
                    UserId = userId,
                    Role = role,
                    JoinedAt = DateTime.UtcNow,
                    AddedBy = addedBy,
                    IsActive = true,
                    User = user,
                    Chatroom = chatroom
                };

                _context.Participants.Add(participant);
                
                // Update chatroom last activity
                chatroom.LastActivity = DateTime.UtcNow;
                
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("User {UserId} added to chatroom {ChatroomId} with role {Role}", userId, chatroomId, role);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding user {UserId} to chatroom {ChatroomId}", userId, chatroomId);
                return false;
            }
        }

        public async Task<bool> AddUsersToChatroomAsync(int chatroomId, List<int> userIds, string role = "member", int? addedBy = null)
        {
            try
            {
                var chatroom = await _context.Chatrooms.FindAsync(chatroomId);
                if (chatroom == null || chatroom.IsDeleted)
                {
                    return false;
                }

                var existingParticipants = await _context.Participants
                    .Where(p => p.ChatroomId == chatroomId && userIds.Contains(p.UserId) && p.IsActive)
                    .Select(p => p.UserId)
                    .ToListAsync();

                var newUserIds = userIds.Except(existingParticipants).ToList();
                
                if (!newUserIds.Any())
                {
                    return false; // All users already in chatroom
                }                var users = await _context.Users
                    .Where(u => newUserIds.Contains(u.UserId))
                    .ToListAsync();

                var participants = users.Select(user => new Participants
                {
                    ChatroomId = chatroomId,
                    UserId = user.UserId,
                    Role = role,
                    JoinedAt = DateTime.UtcNow,
                    AddedBy = addedBy,
                    IsActive = true,
                    User = user,
                    Chatroom = chatroom
                }).ToList();

                _context.Participants.AddRange(participants);
                
                // Update chatroom last activity
                chatroom.LastActivity = DateTime.UtcNow;
                
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Added {Count} users to chatroom {ChatroomId}", participants.Count, chatroomId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding users to chatroom {ChatroomId}", chatroomId);
                return false;
            }
        }

        public async Task<bool> RemoveUserFromChatroomAsync(int chatroomId, int userId, int? removedBy = null)
        {
            try
            {
                var participant = await _context.Participants
                    .FirstOrDefaultAsync(p => p.ChatroomId == chatroomId && p.UserId == userId && p.IsActive);

                if (participant == null)
                {
                    return false;
                }

                participant.LeftAt = DateTime.UtcNow;
                participant.RemovedBy = removedBy;
                participant.IsActive = false;

                // Update chatroom last activity
                var chatroom = await _context.Chatrooms.FindAsync(chatroomId);
                if (chatroom != null)
                {
                    chatroom.LastActivity = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();
                
                _logger.LogInformation("User {UserId} removed from chatroom {ChatroomId}", userId, chatroomId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing user {UserId} from chatroom {ChatroomId}", userId, chatroomId);
                return false;
            }
        }

        public async Task<bool> UpdateUserRoleAsync(int chatroomId, int userId, string newRole, int updatedBy)
        {
            try
            {
                var participant = await _context.Participants
                    .FirstOrDefaultAsync(p => p.ChatroomId == chatroomId && p.UserId == userId && p.IsActive);

                if (participant == null)
                {
                    return false;
                }

                participant.Role = newRole;

                // Update chatroom last activity
                var chatroom = await _context.Chatrooms.FindAsync(chatroomId);
                if (chatroom != null)
                {
                    chatroom.LastActivity = DateTime.UtcNow;
                    chatroom.UpdatedBy = updatedBy;
                    chatroom.UpdatedAt = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();
                
                _logger.LogInformation("User {UserId} role updated to {Role} in chatroom {ChatroomId}", userId, newRole, chatroomId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user {UserId} role in chatroom {ChatroomId}", userId, chatroomId);
                return false;
            }
        }

        public async Task<List<Users>> GetParticipantsInChatroomAsync(int chatroomId)
        {
            var participants = await _context.Participants
                .Where(p => p.ChatroomId == chatroomId && p.IsActive)
                .Include(p => p.User)
                .ToListAsync();

            return participants.Select(p => p.User).ToList();
        }

        public async Task<List<ParticipantInfo>> GetParticipantsWithRolesAsync(int chatroomId)
        {
            try
            {
                var participants = await _context.Participants
                    .Where(p => p.ChatroomId == chatroomId && p.IsActive)
                    .Include(p => p.User)
                    .ToListAsync();                return participants.Select(p => new ParticipantInfo
                {
                    UserId = p.UserId,
                    UserName = p.User.UserName ?? string.Empty,
                    Email = p.User.Email ?? string.Empty,
                    Role = p.Role,
                    JoinedAt = p.JoinedAt,
                    LeftAt = p.LeftAt,
                    IsOnline = p.User.IsOnline
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting participants with roles for chatroom {ChatroomId}", chatroomId);
                return new List<ParticipantInfo>();
            }
        }

        public async Task<bool> IsUserParticipantAsync(int chatroomId, int userId)
        {
            return await _context.Participants
                .AnyAsync(p => p.ChatroomId == chatroomId && p.UserId == userId && p.IsActive);
        }

        public async Task<string?> GetUserRoleInChatroomAsync(int chatroomId, int userId)
        {
            var participant = await _context.Participants
                .FirstOrDefaultAsync(p => p.ChatroomId == chatroomId && p.UserId == userId && p.IsActive);
            
            return participant?.Role;
        }

        public async Task<int> GetParticipantCountAsync(int chatroomId)
        {
            return await _context.Participants
                .CountAsync(p => p.ChatroomId == chatroomId && p.IsActive);
        }

        public async Task<List<Users>> GetActiveParticipantsAsync(int chatroomId)
        {
            var participants = await _context.Participants
                .Where(p => p.ChatroomId == chatroomId && p.IsActive)
                .Include(p => p.User)
                .Where(p => p.User.IsOnline)
                .ToListAsync();

            return participants.Select(p => p.User).ToList();
        }

        // Permission & Security
        public async Task<bool> CanUserAccessChatroomAsync(int chatroomId, int userId)
        {
            var chatroom = await _context.Chatrooms.FindAsync(chatroomId);
            if (chatroom == null || chatroom.IsDeleted)
            {
                return false;
            }

            // Public chatrooms can be accessed by anyone
            if (!chatroom.IsPrivate)
            {
                return true;
            }

            // Private chatrooms require membership
            return await IsUserParticipantAsync(chatroomId, userId);
        }

        public async Task<bool> CanUserModifyChatroomAsync(int chatroomId, int userId)
        {
            var role = await GetUserRoleInChatroomAsync(chatroomId, userId);
            return role == "owner" || role == "admin";
        }

        public async Task<bool> IsUserAdminOrOwnerAsync(int chatroomId, int userId)
        {
            var role = await GetUserRoleInChatroomAsync(chatroomId, userId);
            return role == "owner" || role == "admin";
        }

        // Statistics & Analytics
        public async Task<ChatroomStats> GetChatroomStatsAsync(int chatroomId)
        {
            try
            {
                var chatroom = await _context.Chatrooms
                    .Include(c => c.Participants)
                    .FirstOrDefaultAsync(c => c.ChatRoomId == chatroomId);

                if (chatroom == null)
                {
                    return new ChatroomStats();
                }

                var totalParticipants = await GetParticipantCountAsync(chatroomId);
                var activeParticipants = await _context.Participants
                    .Where(p => p.ChatroomId == chatroomId && p.IsActive)
                    .Include(p => p.User)
                    .CountAsync(p => p.User.IsOnline);

                var messageCount = await GetMessageCountAsync(chatroomId);
                var lastActivity = await GetLastActivityAsync(chatroomId);                return new ChatroomStats
                {
                    ChatroomId = chatroomId,
                    Name = chatroom?.Name ?? "Unknown",
                    TotalParticipants = totalParticipants,
                    ActiveParticipants = activeParticipants,
                    TotalMessages = messageCount,
                    LastActivity = lastActivity,
                    CreatedAt = chatroom?.CreatedAt ?? DateTime.UtcNow,
                    IsActive = chatroom != null && !chatroom.IsDeleted
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting stats for chatroom {ChatroomId}", chatroomId);
                return new ChatroomStats();
            }
        }

        public async Task<DateTime?> GetLastActivityAsync(int chatroomId)
        {
            var chatroom = await _context.Chatrooms.FindAsync(chatroomId);
            return chatroom?.LastActivity;
        }        public async Task<int> GetMessageCountAsync(int chatroomId)
        {
            // Assuming you have a Messages table
            return await _context.Messages
                .CountAsync(m => m.ChatRoomId == chatroomId && !m.IsDeleted);
        }

        // Advanced features
        public async Task<bool> ArchiveChatroomAsync(int chatroomId, int archivedBy)
        {
            try
            {
                var chatroom = await _context.Chatrooms.FindAsync(chatroomId);
                if (chatroom == null || chatroom.IsDeleted)
                {
                    return false;
                }

                chatroom.IsArchived = true;
                chatroom.UpdatedBy = archivedBy;
                chatroom.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Chatroom {ChatroomId} archived by user {UserId}", chatroomId, archivedBy);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error archiving chatroom {ChatroomId}", chatroomId);
                return false;
            }
        }

        public async Task<bool> UnarchiveChatroomAsync(int chatroomId, int unarchivedBy)
        {
            try
            {
                var chatroom = await _context.Chatrooms.FindAsync(chatroomId);
                if (chatroom == null || chatroom.IsDeleted)
                {
                    return false;
                }

                chatroom.IsArchived = false;
                chatroom.UpdatedBy = unarchivedBy;
                chatroom.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Chatroom {ChatroomId} unarchived by user {UserId}", chatroomId, unarchivedBy);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unarchiving chatroom {ChatroomId}", chatroomId);
                return false;
            }
        }

        public async Task<List<ChatRooms>> GetArchivedChatroomsAsync(int userId)
        {
            return await _context.Participants
                .Where(p => p.UserId == userId && p.IsActive)
                .Include(p => p.Chatroom)
                .ThenInclude(c => c.Creator)
                .Select(p => p.Chatroom)
                .Where(c => c != null && !c.IsDeleted && c.IsArchived)
                .OrderByDescending(c => c.UpdatedAt)
                .ToListAsync();
        }
    }
}
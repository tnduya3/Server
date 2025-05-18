using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Server_1_.Models;
using Server_1_.Data;

namespace Server_1_.Services
{

    // Class triển khai IChatroomService
    public class ChatroomService : IChatroomService
    {
        private readonly AppDbContext _context;

        public ChatroomService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ChatRooms> CreateChatroomAsync(string name)
        {
            var newChatroom = new ChatRooms
            {
                Name = name,
            };

            _context.Chatrooms.Add(newChatroom);
            await _context.SaveChangesAsync();
            return newChatroom;
        }

        public async Task<ChatRooms> GetChatroomByIdAsync(int id)
        {
            var chatroom = await _context.Chatrooms.FindAsync(id);
            if (chatroom == null)
            {
                throw new InvalidOperationException($"Chatroom with ID {id} not found.");
            }
            return chatroom;
        }

        public async Task<List<ChatRooms>> GetChatroomsAsync()
        {
            return await _context.Chatrooms.ToListAsync();
        }

        public async Task<bool> UpdateChatroomAsync(int id, string newName)
        {
            var chatroom = await _context.Chatrooms.FindAsync(id);
            if (chatroom == null)
            {
                return false;
            }

            chatroom.Name = newName;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteChatroomAsync(int id)
        {
            var chatroom = await _context.Chatrooms.FindAsync(id);
            if (chatroom == null)
            {
                return false;
            }

            _context.Chatrooms.Remove(chatroom);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AddUserToChatroomAsync(int chatroomId, int userId)
        {
            var chatroom = await _context.Chatrooms.FindAsync(chatroomId);
            var user = await _context.Users.FindAsync(userId);
            if (chatroom == null || user == null)
            {
                return false;
            }

            var participant = new Participants
            {
                ChatroomId = chatroomId,
                UserId = userId,
                JoinedAt = DateTime.UtcNow,
                Role = "member", // You can adjust the role logic as needed
                User = user, // Set the required User property
                Chatroom = chatroom // Set the required Chatroom property
            };

            _context.Participants.Add(participant);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveUserFromChatroomAsync(int chatroomId, int userId)
        {
            var participant = await _context.Participants
               .FirstOrDefaultAsync(p => p.ChatroomId == chatroomId && p.UserId == userId);

            if (participant == null)
            {
                return false;
            }

            participant.LeftAt = DateTime.UtcNow; // Đặt thời điểm rời đi thay vì xóa trực tiếp
            _context.Participants.Remove(participant);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<Users>> GetParticipantsInChatroomAsync(int chatroomId)
        {
            var participants = await _context.Participants
                .Where(p => p.ChatroomId == chatroomId && p.LeftAt == null) // Chỉ lấy những người còn ở trong phòng chat
                .Include(p => p.User) // Load thông tin User của participant
                .ToListAsync();

            return participants.Select(p => p.User).ToList(); // Trả về danh sách User
        }
    }
}
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Server_1_.Models;

namespace Server_1_.Services
{
    // Interface cho ChatroomService
    public interface IChatroomService
    {
        Task<ChatRooms> CreateChatroomAsync(string name);
        Task<ChatRooms> GetChatroomByIdAsync(int id);
        Task<List<ChatRooms>> GetChatroomsAsync();
        Task<bool> UpdateChatroomAsync(int id, string newName);
        Task<bool> DeleteChatroomAsync(int id);
        Task<bool> AddUserToChatroomAsync(int chatroomId, int userId);
        Task<bool> RemoveUserFromChatroomAsync(int chatroomId, int userId);
        Task<List<Users>> GetParticipantsInChatroomAsync(int chatroomId);
    }
}
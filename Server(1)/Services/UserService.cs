using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Server_1_.Models;
using Server_1_.Data;

namespace Server_1_.Services
{
    // Class triển khai IUserService
    public class UserService : IUserService
    {
        private readonly AppDbContext _context;

        public UserService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Users> GetUserByIdAsync(int id) // Updated return type to match IUserService
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                throw new InvalidOperationException($"User with ID {id} not found."); // Handle null case explicitly
            }
            return user;
        }

        public async Task<Users> GetUserByUsernameAsync(string username) // Updated return type to match IUserService  
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == username);
            if (user == null)
            {
                throw new InvalidOperationException($"User with username '{username}' not found."); // Handle null case explicitly  
            }
            return user;
        }

        public async Task<List<Users>> GetUsersAsync()
        {
            return await _context.Users.ToListAsync();
        }

        //public async Task<Users> RegisterUserAsync(string username, string password)
        //{
        //    string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

        //    var newUser = new Users
        //    {
        //        UserName = username,
        //        password = hashedPassword,
        //    };

        //    _context.Users.Add(newUser);
        //    await _context.SaveChangesAsync();
        //    return newUser;
        //}

        public async Task<bool> UpdateUserAsync(int id, string newUsername)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return false;
            }

            user.UserName = newUsername;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateDeviceTokenAsync(int userId, string deviceToken)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return false;
            }
            user.DeviceToken = deviceToken;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return false;
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
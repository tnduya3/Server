﻿using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Server_1_.Models;

namespace Server_1_.Services 
{    // Interface cho UserService
    public interface IUserService
    {
        //Task<Users> RegisterUserAsync(string username, string password); // Thêm tham số password
        Task<Users> GetUserByIdAsync(int id);
        Task<Users> GetUserByUsernameAsync(string username);
        Task<Users?> GetUserByEmailAsync(string email);
        Task<Users?> CreateUserAsync(Users user);
        Task<List<Users>> GetUsersAsync();
        Task<bool> UpdateUserAsync(int id, string newUsername); //Thêm các tham số cần thiết
        Task<bool> DeleteUserAsync(int id);
        Task<bool> UpdateDeviceTokenAsync(int userId, string deviceToken);
        Task<bool> UpdateUserAvatarAsync(int userId, string avatar);
        // Các phương thức khác liên quan đến người dùng
    }
}
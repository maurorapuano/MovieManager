using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MovieManager.DTOs;
using MovieManager.Entities;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace MovieManager.AppServices
{
    public class AuthService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthService(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<User> GetUserFromDBAsync(string username)
        {
            try
            {
                User? existingUser = await _context.Users
                    .Include(u => u.Role)
                    .FirstOrDefaultAsync(u => u.Username == username);

                return existingUser;
            }
            catch(Exception ex)
            {
                throw new Exception("Error while getting user information.", ex);
            }
        }

        public async Task<string> CreateUserAsync(User userData)
        {
            try
            {
                _context.Users.Add(userData);
                await _context.SaveChangesAsync();

                return userData.Username;
            }
            catch (Exception ex)
            {
                throw new Exception("Error while creating new user.", ex);
            }
        }
    }
}

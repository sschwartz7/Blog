using Blog.Data;
using Blog.Models;
using Blog.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Blog.Services
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<BlogUser> _userManager;

        public UserService(ApplicationDbContext context, UserManager<BlogUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IEnumerable<BlogUser>> GetAllUsersAsync()
        {
            try
            {
                return await _context.Users
        .Include(u => u.Comments)
        .ToListAsync();
            }
            catch (Exception)
            {

                throw;
            }

        }

        public async Task<BlogUser> GetUserByIdAsync(string? blogUserId)
        {
            if (string.IsNullOrEmpty(blogUserId)) return new BlogUser();
            try
            {
            BlogUser? blogUser = await _context.Users.FindAsync(blogUserId);

            return blogUser ?? new BlogUser();
            }
            catch (Exception)
            {

                throw;
            }


        }

        public async Task UpdateUser(BlogUser? user)
        {
            if (user == null) return;

            try
            {
                _context.Update(user);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public bool UserExists(string userId)
        {
            if(string.IsNullOrEmpty(userId)) return false;
            try
            {
                return (_context.Users?.Any(e => e.Id == userId)).GetValueOrDefault();

            }
            catch (Exception)
            {

                throw;
            }

        }
    }
}

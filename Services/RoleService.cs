using Blog.Data;
using Blog.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Blog.Models;

namespace Blog.Services
{
    public class RoleService : IRoleService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<BlogUser> _userManager;
        public RoleService(ApplicationDbContext context, UserManager<BlogUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<bool> AddUserToRoleAsync(BlogUser? user, string? roleName)
        {
            try
            {
                if (user != null && !string.IsNullOrEmpty(roleName))
                {
                    bool result = (await _userManager.AddToRoleAsync(user, roleName)).Succeeded;
                    return result;
                }
                return false;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<IdentityRole>> GetRolesAsync()
        {
            try
            {
                List<IdentityRole> result = new();
                result = await _context.Roles.ToListAsync();
                return result;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<IEnumerable<string>?> GetUserRolesAsync(BlogUser? user)
        {
            try
            {
                if (user != null)
                {
                    IEnumerable<string> result = await _userManager.GetRolesAsync(user);
                    return result;
                }
                return null;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<BlogUser>> GetUsersInRoleAsync(string? roleName)
        {
            try
            {
                List<BlogUser> users = new();
                if (!string.IsNullOrEmpty(roleName))
                {
                    users = (await _userManager.GetUsersInRoleAsync(roleName)).ToList();
                    return users;
                }
                return users;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<bool> IsUserInRoleAsync(BlogUser? member, string? roleName)
        {
            try
            {
                if (member != null && !string.IsNullOrEmpty(roleName))
                {
                    bool result = await _userManager.IsInRoleAsync(member, roleName);
                    return result;
                }
                return false;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<bool> RemoveUserFromRoleAsync(BlogUser? user, string? roleName)
        {
            try
            {
                if (user != null && !string.IsNullOrEmpty(roleName))
                {
                    bool result = (await _userManager.RemoveFromRoleAsync(user, roleName)).Succeeded;
                    return result;
                }
                return false;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<bool> RemoveUserFromRolesAsync(BlogUser? user, IEnumerable<string>? roleNames)
        {
            try
            {
                if (user != null && roleNames != null)
                {
                    bool result = (await _userManager.RemoveFromRolesAsync(user, roleNames)).Succeeded;
                    return result;
                }
                return false;
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}

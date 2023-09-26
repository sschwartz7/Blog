using Microsoft.AspNetCore.Identity;
using Blog.Models;

namespace Blog.Services.Interfaces
{
    public interface IRoleService
    {
        public Task<bool> AddUserToRoleAsync(BlogUser? user, string? roleName);

        public Task<List<IdentityRole>> GetRolesAsync();

        public Task<IEnumerable<string>?> GetUserRolesAsync(BlogUser? user);

        public Task<List<BlogUser>> GetUsersInRoleAsync(string? roleName);

        public Task<bool> IsUserInRoleAsync(BlogUser? member, string? roleName);

        public Task<bool> RemoveUserFromRoleAsync(BlogUser? user, string? roleName);

        public Task<bool> RemoveUserFromRolesAsync(BlogUser? user, IEnumerable<string>? roleNames);
    }
}

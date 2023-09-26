using Blog.Models;

namespace Blog.Services.Interfaces
{
    public interface IUserService
    {
        public bool UserExists(string userId);
        public Task UpdateUser(BlogUser? user);
        public Task<BlogUser> GetUserByIdAsync(string? blogUserId);
        public Task<IEnumerable<BlogUser>> GetAllUsersAsync();
    }
}

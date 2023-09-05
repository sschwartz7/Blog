using Blog.Models;
using Microsoft.AspNetCore.Mvc;

namespace Blog.Services.Interfaces
{
    public interface IBlogService
    {
        public Task AddBlogPostAsync( BlogPost? blogPost);
        public Task DeleteBlogPostAsync(BlogPost? blogPost);
        public Task UpdateBlogPostAsync(BlogPost? blogPost);
        public Task<BlogPost> GetBlogPostAsync(int? id);
        public Task<BlogPost> GetBlogPostAsync(string? slug);
        public Task PublishBlogPostAsync(BlogPost? blogPost);
        public Task<IEnumerable<BlogPost>> GetBlogPostsAsync();
        public Task<IEnumerable<BlogPost>> GetAllBlogPostsAsync();
        public Task<IEnumerable<BlogPost>> GetDraftBlogPostsAsync();
        public Task<IEnumerable<BlogPost>> GetDeletedBlogPostsAsync();
        public  Task<IEnumerable<Category>> GetCategoriesAsync();
        public Task<IEnumerable<BlogPost>> GetPopularBlogPostsAsync(int? count = null);
        public Task<IEnumerable<Tag>> GetTagsAsync();
        public Task AddTagsToBlogPostAsync(IEnumerable<string>? tags, int? blogPostId);
        public Task<bool> IsTagOnBLogPostAsync(int? tagId, int? blogPostId);
        public Task RemoveAllBlogPostTagsAync(int? blogPostId);
        public IEnumerable<BlogPost> SearchBlogPosts(string? searchString);
        public Task<bool> ValidSlugAsync(string? title, int? blogPostId);
        public IEnumerable<BlogPost> GetBlogPostByCategoryAsync(string? category);
        public IEnumerable<BlogPost> GetBlogPostByTagAsync(string? tag);
        public Task<bool> UserLikedBlogAsync(int? blogPostId, string blogUserId);
        public Task<BlogPost> GetBlogPostPreviewAsync(string? slug);
    }
}

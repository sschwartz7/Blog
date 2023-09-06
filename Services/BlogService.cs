using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Blog.Data;
using Blog.Models;
using Microsoft.AspNetCore.Authorization;
using Blog.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow;
using System.Linq;
using Microsoft.EntityFrameworkCore.Query;
using Humanizer;
using Azure;

namespace Blog.Services
{
    public class BlogService : IBlogService
    {

        private readonly ApplicationDbContext _context;

        public BlogService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddBlogPostAsync(BlogPost? blogPost)
        {
            if (blogPost == null) { return; }
            try
            {
                _context.Add(blogPost);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task AddTagsToBlogPostAsync(IEnumerable<string>? tags, int? blogPostId)
        {
            if (blogPostId == null || tags == null) { return; }
            try
            {

                BlogPost blogPost = await GetBlogPostAsync(blogPostId);
                if (blogPost == null) return;

                foreach (string tagName in tags)
                {
                    if(string.IsNullOrEmpty(tagName.Trim())) continue;

                    Tag? blogTag = await _context.Tags.FirstOrDefaultAsync(t => t.Name!.ToLower().Trim() == tagName.Trim().ToLower());

                    if (blogTag == null)
                    {
                        blogTag = new Tag() { Name = tagName.Trim().Titleize() };
                        await _context.AddAsync(blogTag);
                    }

                    blogPost.Tags.Add(blogTag);

                }
                await _context.SaveChangesAsync();
                return;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task DeleteBlogPostAsync(BlogPost? blogPost)//change this
        {

            if (blogPost == null) { return; }
            try
            {
                blogPost.IsDeleted = true;
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<BlogPost> GetBlogPostAsync(int? id)
        {
            if (id == null) { return new BlogPost(); }
            try
            {
                BlogPost? blogPost = await _context.BlogPosts
                    .Include(b => b.Category).Include(v => v.Tags).Include(c => c.Likes).Include(c => c.Comments).ThenInclude(c => c.Author)
                    .FirstOrDefaultAsync(c => c.Id == id);
                return blogPost!;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<BlogPost> GetBlogPostAsync(string? slug)
        {
            if (string.IsNullOrEmpty(slug)) { return new BlogPost(); }
            try
            {
                BlogPost? blogPost = await _context.BlogPosts
                    .Where(b => b.IsPublished == true && b.IsDeleted == false)
                    .Include(b => b.Category).Include(v => v.Tags).Include(c => c.Likes).Include(c => c.Comments).ThenInclude(c => c.Author)
                    .FirstOrDefaultAsync(c => c.Slug == slug);
                return blogPost!;
            }
            catch (Exception)
            {

                throw;
            }
        }
        public async Task<BlogPost> GetBlogPostPreviewAsync(string? slug)
        {
            if (string.IsNullOrEmpty(slug)) { return new BlogPost(); }
            try
            {
                BlogPost? blogPost = await _context.BlogPosts
                    .Include(b => b.Category).Include(v => v.Tags).Include(c => c.Likes).Include(c => c.Comments).ThenInclude(c => c.Author)
                    .FirstOrDefaultAsync(c => c.Slug == slug);
                return blogPost!;
            }
            catch (Exception)
            {

                throw;
            }
        }
        public async Task<IEnumerable<BlogPost>> GetBlogPostByTagAsync(string? tag)
        {
            try
            {
                IEnumerable<BlogPost> blogPosts = new List<BlogPost>();

                if (string.IsNullOrEmpty(tag))
                {
                    return blogPosts;
                }
                else
                {

                    blogPosts = await _context.BlogPosts
                      .Include(b => b.Category)
                      .Include(b => b.Tags)
                      .Include(b => b.Comments)
                          .ThenInclude(c => c.Author)
                    .Where(b => b.Tags.Any(t => t.Name == tag))
                      .Where(b => b.IsPublished == true && b.IsDeleted == false)
                      .AsNoTracking()
                      .OrderByDescending(b => b.Created)
                      .ToListAsync();

                    return blogPosts;
                }
            }
            catch (Exception)
            {

                throw;
            }
        }


        public async Task<IEnumerable<BlogPost>> GetBlogPostsAsync()
        {
            try
            {
                List<BlogPost> blogPosts = await _context.BlogPosts
                    .Where(b => b.IsPublished == true && b.IsDeleted == false)
                    .Include(b => b.Category).Include(v => v.Tags).Include(c=>c.Likes).Include(c => c.Comments).ThenInclude(c => c.Author).ToListAsync();
                return blogPosts;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<IEnumerable<BlogPost>> GetAllBlogPostsAsync()
        {
            try
            {
                List<BlogPost> blogPosts = await _context.BlogPosts
                    .Include(b => b.Category).Include(v => v.Tags).Include(c => c.Likes).Include(c => c.Comments).ThenInclude(c => c.Author).ToListAsync();
                return blogPosts;
            }
            catch (Exception)
            {

                throw;
            }
        }
        public async Task<IEnumerable<BlogPost>> GetDeletedBlogPostsAsync()
        {
            try
            {
                List<BlogPost> blogPosts = await _context.BlogPosts
                    .Where(b => b.IsDeleted == true)
                    .Include(b => b.Category).Include(v => v.Tags).Include(c => c.Likes).Include(c => c.Comments).ThenInclude(c => c.Author).ToListAsync();
                return blogPosts;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<IEnumerable<BlogPost>> GetDraftBlogPostsAsync()
        {
            try
            {
                List<BlogPost> blogPosts = await _context.BlogPosts
                    .Where(b => b.IsPublished == false && b.IsDeleted == false)
                    .Include(b => b.Category).Include(v => v.Tags).Include(c => c.Likes).Include(c => c.Comments).ThenInclude(c => c.Author).ToListAsync();
                return blogPosts;
            }
            catch (Exception)
            {

                throw;
            }
        }


        public async Task<IEnumerable<Category>> GetCategoriesAsync()
        {
            try
            {
                IEnumerable<Category> categories = await _context.Categories.ToListAsync();
                return categories;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<IEnumerable<BlogPost>> GetPopularBlogPostsAsync(int? count)
        {
            try
            {
                IEnumerable<BlogPost> blogPosts = await _context.BlogPosts
                                                                .Where(b => b.IsPublished == true && b.IsDeleted == false)
                                                                .Include(b => b.Category)
                                                                .Include(b => b.Comments)
                                                                    .ThenInclude(c => c.Author)
                                                                .Include(b => b.Tags)
                                                                .ToListAsync();
                if (count == null)
                {
                    blogPosts = blogPosts.OrderByDescending(b => b.Comments.Count);
                }
                else
                {
                    blogPosts = blogPosts.OrderByDescending(b => b.Comments.Count).Take(count!.Value);
                }
                return blogPosts;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<IEnumerable<Tag>> GetTagsAsync()
        {
            try
            {
                IEnumerable<Tag> tags = await _context.Tags.ToListAsync();
                return tags;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<bool> IsTagOnBLogPostAsync(int? tagId, int? blogPostId)
        {
            if (tagId == null || blogPostId == null) { return false; }
            try
            {
                Tag? tag = await _context.Tags.FirstOrDefaultAsync(t => t.Id == tagId);
                BlogPost? blogPost = await _context.BlogPosts.FirstOrDefaultAsync(b => b.Id == blogPostId);

                if (tag == null || blogPost == null)
                {
                    return blogPost.Tags.Contains(tag);
                }
                return false;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task PublishBlogPostAsync(BlogPost? blogPost)
        {
            if (blogPost == null) { return; }
            try
            {
                blogPost.IsPublished = true;
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task RemoveAllBlogPostTagsAync(int? blogPostId)
        {
            if (blogPostId == null) return;
            try
            {
                BlogPost? blogPost = await GetBlogPostAsync(blogPostId);
                if (blogPost != null)
                {
                    blogPost.Tags.Clear();
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception)
            {

                throw;
            };
        }

        public IEnumerable<BlogPost> SearchBlogPosts(string? searchString)
        {

            try
            {
                IEnumerable<BlogPost> blogPosts = new List<BlogPost>();

                if (string.IsNullOrEmpty(searchString))
                {
                    return blogPosts;
                }
                else
                {
                    searchString = searchString.Trim().ToLower();
                    blogPosts = _context.BlogPosts.
                        Where(b => b.Title!.ToLower().Contains(searchString) ||
                         b.Abstract!.ToLower().Contains(searchString) ||
                         b.Content!.ToLower().Contains(searchString) ||
                         b.Category!.Name!.ToLower().Contains(searchString) ||
                         b.Comments.Any(c => c.Body!.ToLower().Contains(searchString) ||
                            c.Author!.FirstName!.ToLower().Contains(searchString) ||
                            c.Author!.LastName!.ToLower().Contains(searchString)) ||
                         b.Tags.Any(d => d.Name!.ToLower().Contains(searchString))).
                         Include(b => b.Category).Include(v => v.Tags).Include(c => c.Comments).ThenInclude(c => c.Author).
                         Where(b => b.IsPublished == true && b.IsDeleted == false).
                         AsNoTracking().
                         OrderByDescending(b => b.Created).
                         AsEnumerable(); 

                    return blogPosts;
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task UpdateBlogPostAsync(BlogPost? blogPost)
        {

            if (blogPost == null) { return; }
            try
            {
                _context.Update(blogPost);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<bool> UserLikedBlogAsync(int? blogPostId, string blogUserId)
        {
            try
            {
                return await _context.BlogLikes.AnyAsync(bl => bl.BlogPostId == blogPostId && bl.BlogUserId == blogUserId);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<bool> ValidSlugAsync(string? title, int? blogPostId)
        {
            try
            {
                //New BlogPost
                if (blogPostId == null || blogPostId == 0)
                {
                    return !await _context.BlogPosts.AnyAsync(b => b.Slug == title);
                }
                else
                {
                    BlogPost? blogPost = await GetBlogPostAsync(blogPostId);

                    string? oldSlug = blogPost?.Slug;
                    if (!string.Equals(oldSlug, title))
                    {
                        return !await _context.BlogPosts.AnyAsync(b => b.Id != blogPost!.Id && b.Slug == title);
                    }
                }
                return true;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public IEnumerable<BlogPost> GetBlogPostByCategoryAsync(string? category)
        {
            try
            {
                IEnumerable<BlogPost> blogPosts = new List<BlogPost>();

                if (string.IsNullOrEmpty(category))
                {
                    return blogPosts;
                }
                else
                {

                    blogPosts = _context.BlogPosts.
                         Include(b => b.Category).Include(v => v.Tags).Include(c => c.Comments).ThenInclude(c => c.Author).
                         Where(b => b.IsPublished == true && b.IsDeleted == false).
                         Where(v => v.Category!.Name == category).
                         AsNoTracking().
                         OrderByDescending(b => b.Created).
                         AsEnumerable();

                    return blogPosts;
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

    }
}

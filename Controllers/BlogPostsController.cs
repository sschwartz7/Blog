using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Blog.Data;
using Blog.Models;
using Microsoft.AspNetCore.Authorization;
using Blog.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using System.CodeDom;
using Microsoft.VisualStudio.Web.CodeGeneration.EntityFrameworkCore;
using X.PagedList;
using Blog.Helper;
using Blog.Services;
using System.Diagnostics.CodeAnalysis;

namespace Blog.Controllers
{

    public class BlogPostsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<BlogUser> _userManager;
        private readonly IImageService _imageService;
        private readonly IBlogService _blogService;

        public BlogPostsController(ApplicationDbContext context, IImageService imageService, UserManager<BlogUser> userMangager, IBlogService blogService)
        {
            _userManager = userMangager;
            _context = context;
            _imageService = imageService;
            _blogService = blogService;
        }

        public async Task<IActionResult> SearchIndex(string? searchString, int? pageNum)
        {
            int pageSize = 3;
            int page = pageNum ?? 1;

            IPagedList<BlogPost> blogPosts = await _blogService.SearchBlogPosts(searchString).ToPagedListAsync(page, pageSize);

            ViewData["ActionName"] = nameof(SearchIndex);
            ViewData["SearchString"] = searchString;

            return View(nameof(Index), blogPosts);
        }

        public async Task<IActionResult> CategoryFilter(string? category, int? pageNum)
        {
            int pageSize = 3;
            int page = pageNum ?? 1;

            IPagedList<BlogPost> blogPosts = await _blogService.GetBlogPostByCategoryAsync(category).ToPagedListAsync(page, pageSize);

            ViewData["ActionName"] = nameof(CategoryFilter);
            ViewData["CategoryString"] = category;

            return View(nameof(Index), blogPosts);
        }
        public async Task<IActionResult> TagFilter(string? tag, int? pageNum)
        {
            int pageSize = 3;
            int page = pageNum ?? 1;

            IPagedList<BlogPost> blogPosts = await _blogService.GetBlogPostByTagAsync(tag).ToPagedListAsync(page, pageSize);

            ViewData["ActionName"] = nameof(TagFilter);
            ViewData["TagString"] = tag;

            return View(nameof(Index), blogPosts);
        }

        // GET: BlogPosts
        [AllowAnonymous]
        public async Task<IActionResult> Index(int? pageNum)
        {
            int pageSize = 3;
            int page = pageNum ?? 1;

            IPagedList<BlogPost> blogPosts = await (await _blogService.GetBlogPostsAsync()).ToPagedListAsync(page, pageSize);

            ViewData["ActionName"] = nameof(Index);

            return View(blogPosts);
        }
        [AllowAnonymous]
        public async Task<IActionResult> Popular(int? pageNum)
        {
            int pageSize = 3;
            int page = pageNum ?? 1;

            IPagedList<BlogPost> blogPosts = await (await _blogService.GetPopularBlogPostsAsync()).ToPagedListAsync(page, pageSize);

            ViewData["ActionName"] = nameof(Popular);
            ViewData["SearchString"] = "Popular";

            return View(nameof(Index), blogPosts);
        }

        // GET: BlogPosts/Details/5
        [AllowAnonymous]
        public async Task<IActionResult> Details(string? slug)
        {
            if (string.IsNullOrEmpty(slug))
            {
                return NotFound();
            }
            BlogPost? blogPost = await _blogService.GetBlogPostAsync(slug);
            Comment comment = new Comment();
            if (blogPost == null)
            {
                return NotFound();
            }

            return View(blogPost);
        }
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Preview(string? slug)
        {
            if (string.IsNullOrEmpty(slug))
            {
                return NotFound();
            }
            BlogPost? blogPost = await _blogService.GetBlogPostPreviewAsync(slug);
            Comment comment = new Comment();
            if (blogPost == null)
            {
                return NotFound();
            }

            return View(nameof(Details), blogPost);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AuthorArea(int? pageNum)
        {
            int pageSize = 3;
            int page = pageNum ?? 1;

            IPagedList<BlogPost> blogPosts = await (await _blogService.GetBlogPostsAsync()).ToPagedListAsync(page, pageSize);
            ViewData["ActionName"] = nameof(AuthorArea);
            ViewData["actionTitle"] = "Published Posts";

            return View(blogPosts);
        }
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AuthorAreaDrafts(int? pageNum)
        {
            int pageSize = 3;
            int page = pageNum ?? 1;

            IPagedList<BlogPost> blogPosts = await (await _blogService.GetDraftBlogPostsAsync()).ToPagedListAsync(page, pageSize);
            ViewData["ActionName"] = nameof(AuthorAreaDrafts);
            ViewData["actionTitle"] = "Drafts";

            return View(nameof(AuthorArea), blogPosts);
        }
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AuthorAreaDeleted(int? pageNum)
        {
            int pageSize = 3;
            int page = pageNum ?? 1;

            IPagedList<BlogPost> blogPosts = await (await _blogService.GetDeletedBlogPostsAsync()).ToPagedListAsync(page, pageSize);
            ViewData["ActionName"] = nameof(AuthorAreaDeleted);
            ViewData["actionTitle"] = "Deleted Posts";

            return View(nameof(AuthorArea), blogPosts);
        }
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AuthorAreaAll(int? pageNum)
        {
            int pageSize = 3;
            int page = pageNum ?? 1;

            IPagedList<BlogPost> blogPosts = await (await _blogService.GetAllBlogPostsAsync()).ToPagedListAsync(page, pageSize);
            ViewData["ActionName"] = nameof(AuthorAreaAll);
            ViewData["actionTitle"] = "All Posts";

            return View(nameof(AuthorArea), blogPosts);
        }
        // GET: BlogPosts/Create
        [Authorize(Roles = "Admin,Author")]
        public IActionResult Create()
        {
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name");
            return View();
        }

        // POST: BlogPosts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "Admin,Author")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Content,Abstract,IsPublished,CategoryId, ImageFormFile")] BlogPost blogPost, string? stringTags)
        {
            ModelState.Remove("Slug");

            if (ModelState.IsValid)
            {
                string? newSlug = StringHelper.BlogPostSlug(blogPost.Title);
                if (!await _blogService.ValidSlugAsync(newSlug, blogPost.Id))
                {
                    ModelState.AddModelError("Title", "A similar Title is already in use.");
                    ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name");
                    return View(blogPost);
                }
                blogPost.Slug = newSlug;
                //Set Created Date
                blogPost.Created = DateTime.Now;

                if (blogPost.ImageFormFile != null)
                {
                    //Convert file to byte array and assign it to ImageData
                    blogPost.ImageFileData = await _imageService.ConvertFileToByteArrayAsync(blogPost.ImageFormFile);
                    //Assign the imagetype based on chosen file
                    blogPost.ImageFileType = blogPost.ImageFormFile.ContentType;
                }

                await _blogService.AddBlogPostAsync(blogPost);

                if (string.IsNullOrEmpty(stringTags) == false)
                {
                    IEnumerable<string> tags = stringTags.Split(',');
                    await _blogService.AddTagsToBlogPostAsync(tags, blogPost.Id);
                }

                return RedirectToAction(nameof(Index));
            }
            return View(blogPost);
        }

        // GET: BlogPosts/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.BlogPosts == null)
            {
                return NotFound();
            }

            var blogPost = await _context.BlogPosts.FindAsync(id);
            if (blogPost == null)
            {
                return NotFound();
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Description", blogPost.CategoryId);
            return View(blogPost);
        }

        // POST: BlogPosts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Content,Abstract,Created,Updated,Slug,IsPublished,IsDeleted,,ImageFormFile,ImageFileData,ImageFileType,CategoryId")] BlogPost blogPost, string? stringTags)
        {
            if (id != blogPost.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    //add slug update if the title change
                    blogPost.Updated = DateTime.UtcNow;
                    if (blogPost.ImageFormFile != null)
                    {
                        //Convert file to byte array and assign it to ImageData
                        blogPost.ImageFileData = await _imageService.ConvertFileToByteArrayAsync(blogPost.ImageFormFile);
                        //Assign the imagetype based on chosen file
                        blogPost.ImageFileType = blogPost.ImageFormFile.ContentType;
                    }
                    if (string.IsNullOrEmpty(stringTags) == false)
                    {
                        IEnumerable<string> tags = stringTags.Split(',');
                        await _blogService.AddTagsToBlogPostAsync(tags, blogPost.Id);
                    }
                    await _blogService.UpdateBlogPostAsync(blogPost);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BlogPostExists(blogPost.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Description", blogPost.CategoryId);
            return View(blogPost);
        }

       
        // POST: BlogPosts/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteToggle(int? id)
        {
            if (id == null || id == 0) return NotFound();

            BlogPost? blogPost = await _blogService.GetBlogPostAsync(id);
            if (blogPost == null) return NotFound();
            if (blogPost.IsDeleted == false)
            {
                blogPost.IsDeleted = true;
            }
            else
            {
                blogPost.IsDeleted = false;
            }
            await _blogService.UpdateBlogPostAsync(blogPost);
            return RedirectToAction(nameof(AuthorArea));
        }

        private bool BlogPostExists(int id)
        {
            return (_context.BlogPosts?.Any(e => e.Id == id)).GetValueOrDefault();
        }
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PublishToggle(int? id)
        {
            if (id == null || id == 0) return NotFound();

            BlogPost? blogPost = await _blogService.GetBlogPostAsync(id);
            if (blogPost == null) return NotFound(); 
            if(blogPost.IsPublished == false)
            {
                blogPost.IsPublished = true;
            }
            else
            {
                blogPost.IsPublished = false;
            }
            await _blogService.UpdateBlogPostAsync(blogPost);
            return RedirectToAction(nameof(AuthorArea));
        }
        [HttpPost]
        public async Task<IActionResult> LikeBlogPost(int? blogPostId, string? blogUserId)
        {
            // check if user has already liked this blog.
            // 1. get the user
            BlogUser? blogUser = await _context.Users.Include(u => u.BlogLikes).FirstOrDefaultAsync(u => u.Id == blogUserId);
            bool result = false;
            BlogLike? blogLike = new();

            if (blogUser != null && blogPostId != null)
            {          
             BlogPost? blogPost = await _context.BlogPosts.Include(u => u.Likes).FirstOrDefaultAsync(u => u.Id == blogPostId);
                if (blogPost == null) return NotFound();
                // if not add a new BlogLike and set IsLiked = true
                if (!blogUser.BlogLikes.Any(bl => bl.BlogPostId == blogPostId))
                {
                    blogLike.BlogUserId = blogUserId;
                    blogLike.BlogUser = blogUser;
                    blogLike.BlogPostId = blogPostId.Value;
                    blogLike.BlogPost = blogPost;
                    blogLike.IsLiked = true;
                    await _context.BlogLikes.AddAsync(blogLike);
                }else {
                    // if a Like already exists on this BlogPost for this User,
                    blogLike = await _context.BlogLikes.FirstOrDefaultAsync(u => u.BlogUserId == blogUserId && u.BlogPostId == blogPostId);
                    if(blogLike.IsLiked == true)
                    {
                    blogLike.IsLiked = false;
                    }
                    else
                    {
                        blogLike.IsLiked = true;
                    }

                    _context.BlogLikes.Update(blogLike);
                }
                result = blogLike.IsLiked;
                
                await _blogService.UpdateBlogPostAsync(blogPost);
            }
            return Json(new
            {
                isLiked = result,
                count = _context.BlogLikes.Where(bl => bl.BlogPostId == blogPostId && bl.IsLiked == true).Count(),
            });

        }
        [Authorize]
        public async Task<IActionResult> FavoriteBlogs(int? pageNum)
        {
            int pageSize = 3;
            int page = pageNum ?? 1;
            string blogUserId = _userManager.GetUserId(User)!; 
            BlogUser? blogUser = await _context.Users.Include(u => u.BlogLikes).ThenInclude(bl => bl.BlogPost).FirstOrDefaultAsync(u => u.Id == blogUserId);

            IPagedList<BlogPost> blogPosts = blogUser!.BlogLikes!.Select(bl => bl.BlogPost!).ToPagedList(page, pageSize);

            ViewData["ActionName"] = nameof(FavoriteBlogs);
            ViewData["SearchString"] = "Favorites";

            return View(nameof(Index), blogPosts);
        }
    }
}

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

            ViewData["ActionName"] = nameof(SearchIndex);
            ViewData["SearchString"] = category;

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

            ViewData["ActionName"] = nameof(Index);

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
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Content,Abstract,Created,Updated,Slug,IsPublished,IsDeleted,,ImageFormFile,ImageFileData,ImageFileType,CategoryId")] BlogPost blogPost)
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

        // GET: BlogPosts/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.BlogPosts == null)
            {
                return NotFound();
            }

            var blogPost = await _context.BlogPosts
                .Include(b => b.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (blogPost == null)
            {
                return NotFound();
            }

            return View(blogPost);
        }

        // POST: BlogPosts/Delete/5
        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.BlogPosts == null)
            {
                return Problem("Entity set 'ApplicationDbContext.BlogPosts'  is null.");

            }
            var blogPost = await _context.BlogPosts.FindAsync(id);
            if (blogPost != null)
            {
                await _blogService.DeleteBlogPostAsync(blogPost);
            }
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private bool BlogPostExists(int id)
        {
            return (_context.BlogPosts?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}

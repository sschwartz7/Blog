using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Blog.Data;
using Blog.Models;
using Blog.Services.Interfaces;

namespace Blog.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class BlogPostController : ControllerBase
    {
        private readonly IBlogService _blogService;

        public BlogPostController(IBlogService blogService)
        {
            _blogService = blogService;
        }

        // GET: api/BlogPost
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BlogPost>>> GetBlogPosts()
        {
            IEnumerable<BlogPost> blogPosts = (await _blogService.GetBlogPostsAsync()).Take(4);
            return Ok(blogPosts);
        }
    }
}

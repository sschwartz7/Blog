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

        /// <summary>
        /// This endpoint will return the most recent blog post.
        /// The count parameter indicates the number of blog post to return with a maximum of 10.
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        [HttpGet("{count:int}")]
        public async Task<ActionResult<IEnumerable<BlogPost>>> GetBlogPosts(int count)
        {
            if( count > 10)
            {
                count = 10;
            }
            IEnumerable<BlogPost> blogPosts = (await _blogService.GetBlogPostsAsync()).Take(count);

            return Ok(blogPosts);
        }
    }
}

using Blog.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Blog.Data
{
    public class ApplicationDbContext : IdentityDbContext<BlogUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.HasDefaultSchema("blog");
        }
        public virtual DbSet<BlogPost> BlogPosts { get; set; } = default!;
        public virtual DbSet<Tag> Tags { get; set; } = default!;
        public virtual DbSet<Category> Categories { get; set; } = default!;
        public virtual DbSet<Comment> Comments { get; set; } = default!;
        public virtual DbSet<BlogLike> BlogLikes { get; set; } = default!;

    }
}
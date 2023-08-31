using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Blog.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required, StringLength(50, ErrorMessage = "The {0} must be at least {2} and max {1} characters long.", MinimumLength = 2)
            , Display(Name="Category Name")] public string? Name { get; set; }
        [Required, StringLength(50, ErrorMessage = "The {0} must be at least {2} and max {1} characters long.", MinimumLength = 2)]
        public string? Description { get; set; }

        public ICollection<BlogPost> BlogPosts { get; set; } = new HashSet<BlogPost>();

        [NotMapped] public IFormFile? ImageFormFile { get; set; }
        public byte[]? ImageFileData { get; set; }
        public string? ImageFileType { get; set; }
    }
}

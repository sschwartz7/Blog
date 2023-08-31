using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Drawing;

namespace Blog.Models
{

    public class BlogPost
    {
        private DateTime _created;
        private DateTime? _updated;
        public int Id { get; set; }
        [Required]
        [StringLength(300, ErrorMessage = "The {0} must be at least {2} and max {1} characters long.", MinimumLength = 2)]
        public string? Title { get; set; }  

        public string? Content { get; set; }
        [StringLength(600, ErrorMessage = "The {0} must be at most {1} characters long.")]
        public string? Abstract { get; set; }
        public DateTime Created 
        {
            get 
            {
                return _created; 
            }
            set
            {
            _created = value.ToUniversalTime();
            } 
        }
        public DateTime? Updated 
        {
            get => _updated;
            set
            {
                if (value.HasValue)
                {
                    _updated = value.Value.ToUniversalTime();
                }
                else
                {
                    _updated = null;
                }

            }
        }
        [Required]
        public string? Slug { get; set; }
        [Display(Name ="Published")]
        public bool IsPublished { get; set; }
        [Display(Name ="Deleted?")]
        public bool IsDeleted { get; set; }

        [NotMapped]
        public IFormFile? ImageFormFile { get; set; }
        public byte[]? ImageFileData { get; set; }
        public string? ImageFileType { get; set; }
        //Navigation Properties
        public virtual ICollection<Comment> Comments { get; set; } = new HashSet<Comment>();
        public virtual ICollection<Tag> Tags { get; set; } = new HashSet<Tag>();
        public int CategoryId { get; set; }
        public virtual Category? Category { get; set; }

    }
}

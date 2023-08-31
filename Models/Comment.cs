using System.ComponentModel.DataAnnotations;

namespace Blog.Models
{
    public class Comment
    {
        public int Id { get; set; }

        private DateTime _created;
        private DateTime? _updated;
        public DateTime Created
        {
            get => _created;
            set => _created = value.ToUniversalTime();
        }

        public DateTime? Updated 
        {
            get => _updated;
            set => _updated = value.HasValue ? value.Value.ToUniversalTime(): null;
        }

        [StringLength(300, ErrorMessage = "The {0} must be at least {2} and max {1} characters long.", MinimumLength = 2)]
        [Display(Name = "Update Reason")]
        public string? UpdateReason { get; set; }

        [Required]
        [StringLength(5000, ErrorMessage = "The {0} must be at least {2} and max {1} characters long.", MinimumLength = 2)]
        [Display(Name = "Comment")]

        public string? Body { get; set; }

        //Forgien Key
        [Required]
        public string? AuthorId { get; set; }
        public virtual BlogUser? Author { get; set; }
        //glued together because same name with Id       
        public int BlogPostId { get; set; } 
        public virtual BlogPost? BlogPost { get; set; }

        
    }
}

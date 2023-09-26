
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Blog.Models
{
    public class ManagerUserRolesVM
    {
        public BlogUser? BlogUser { get; set; }
        public MultiSelectList? Roles { get; set; }
        public IEnumerable<string>? SelectedRoles { get; set; }
    }
}

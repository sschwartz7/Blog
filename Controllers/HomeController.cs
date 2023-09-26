using Blog.Data;
using Blog.Models;
using Blog.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SQLitePCL;
using System.Diagnostics;

namespace Blog.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<BlogUser> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly IConfiguration _configuration;
        private readonly IRoleService _roleService;
        private readonly IUserService _userService;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context, UserManager<BlogUser> userManager,
            IEmailSender emailSender, IConfiguration configuration, IRoleService roleService, IUserService userService)
        {
            _userManager = userManager;
            _context = context;
            _logger = logger;
            _emailSender = emailSender;
            _configuration = configuration;
            _roleService = roleService;
            _userService = userService;
        }

        public IActionResult Index()
        {
            return View();
        }
        [Authorize]
        public async Task<IActionResult> ContactMe()
        {
            string? blogUserId = _userManager.GetUserId(User);
            if(blogUserId == null)
            {
                return NotFound();
            }
            BlogUser? blogUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == blogUserId);
            return View(blogUser);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ContactMe([Bind("FirstName,LastName,Email")] BlogUser blogUser, string? message)
        {
            string? swalMessage = string.Empty;
            
            if (ModelState.IsValid)
            {
                try
                {
                    string? adminEmail = _configuration["AdminLoginEmail"] ?? Environment.GetEnvironmentVariable("AdminLoginEmail");
                    await _emailSender.SendEmailAsync(adminEmail!, $"Contact Me Message From - {blogUser.FullName}", message!);
                    swalMessage = "Email sent successfully!";
                }
                catch (Exception)
                {

                    throw;
                }

                swalMessage = "Error: Unable to send email.";
            }

            return RedirectToAction("Index", "BlogPosts", new { swalMessage });
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ManagerUserRoles()
        {
            // 1 - Add an instance of the ViewModel as a List (model)
            List<ManagerUserRolesVM> model = new List<ManagerUserRolesVM>();
            // 3 - Get all company users
            IEnumerable<BlogUser> members = await _userService.GetAllUsersAsync();
            // 4 - Loop over the users to populate the ViewModel
            //      - instantiate single ViewModel
            //      - use _rolesService
            //      - Create multiselect
            //      - viewmodel to model
            string? btUserId = _userManager.GetUserId(User);

            foreach (BlogUser member in members)
            {
                if (string.Compare(btUserId, member.Id) != 0)
                {
                    ManagerUserRolesVM viewModel = new();
                    IEnumerable<string>? currentRoles = await _roleService.GetUserRolesAsync(member);

                    viewModel.BlogUser = member;
                    viewModel.Roles = new MultiSelectList(await _roleService.GetRolesAsync(), "Name", "Name", currentRoles);

                    model.Add(viewModel);
                }
            }

            // 5 - Return the model to the View
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ManagerUserRoles(ManagerUserRolesVM viewModel)
        {
            // 1- Get the company Id
            // 2 - Instantiate the BlogUser
            BlogUser? bTUser = (await _userService.GetUserByIdAsync(viewModel.BlogUser.Id));
            // 3 - Get Roles for the User
            IEnumerable<string>? currentRoles = await _roleService.GetUserRolesAsync(bTUser);
            // 4 - Get Selected Role(s) for the User
            string? selectedRole = viewModel.SelectedRoles!.FirstOrDefault();
            // 5 - Remove current role(s) and Add new role
            if (!string.IsNullOrEmpty(selectedRole))
            {
                if (await _roleService.RemoveUserFromRolesAsync(bTUser, currentRoles))
                {
                    await _roleService.AddUserToRoleAsync(bTUser, selectedRole);
                }
            }
            // 6 - Navigate
            return RedirectToAction(nameof(ManagerUserRoles));
        }








    }
}
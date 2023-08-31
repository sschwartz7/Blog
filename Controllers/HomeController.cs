using Blog.Data;
using Blog.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
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
        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context, UserManager<BlogUser> userManager,
            IEmailSender emailSender, IConfiguration configuration)
        {
            _userManager = userManager;
            _context = context;
            _logger = logger;
            _emailSender = emailSender;
            _configuration = configuration;
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
    }
}
using Blog.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.Data;

namespace Blog.Data
{
    public static class DataUtility
    {
        private const string? _adminRole = "Admin";
        private const string? _moderatorRole = "Moderator";
        private const string? _writerRole = "Writer";
        public static string GetConnectionString(IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
            return string.IsNullOrEmpty(databaseUrl) ? connectionString! : BuildConnectionString(databaseUrl);
        }

        public static async Task ManageDataAsync(IServiceProvider svcProvider)
        {
            //Obtaining the necessary services based on the IServiceProvider parameter
            var dbContextSvc = svcProvider.GetRequiredService<ApplicationDbContext>();
            var userManagerSvc = svcProvider.GetRequiredService<UserManager<BlogUser>>();
            var configurationSvc = svcProvider.GetRequiredService<IConfiguration>();
            var roleManagerSvc = svcProvider.GetRequiredService<RoleManager<IdentityRole>>();

            //Align db by checking Migrations
            await dbContextSvc.Database.MigrateAsync();

            //Seed App Roles
            await SeedRolesAsync(roleManagerSvc);
            //Seed User(s)
            await SeedBlogUsersAsync(userManagerSvc, configurationSvc);
        }
        public static string BuildConnectionString(string databaseUrl)
        {
            //Provides an object representation of a uniform resource identifier (URI) and easy access to the parts of the URI.
            var databaseUri = new Uri(databaseUrl);
            var userInfo = databaseUri.UserInfo.Split(':');
            //Provides a simple way to create and manage the contents of connection strings used by the NpgsqlConnection class.
            var builder = new NpgsqlConnectionStringBuilder
            {
                Host = databaseUri.Host,
                Port = databaseUri.Port,
                Username = userInfo[0],
                Password = userInfo[1],
                Database = databaseUri.LocalPath.TrimStart('/'),
                SslMode = SslMode.Prefer,
                TrustServerCertificate = true
            };
            return builder.ToString();

        }
        private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            if (!await roleManager.RoleExistsAsync(_adminRole!))
            {
                await roleManager.CreateAsync(new IdentityRole(_adminRole!));
            }
            if (!await roleManager.RoleExistsAsync(_moderatorRole!))
            {
                await roleManager.CreateAsync(new IdentityRole(_moderatorRole!));
            }
            if (!await roleManager.RoleExistsAsync(_writerRole!))
            {
                await roleManager.CreateAsync(new IdentityRole(_writerRole!));
            }

        }
        private static async Task SeedBlogUsersAsync(UserManager<BlogUser> userManager, IConfiguration configuration)
        {
            string? adminLoginEmail = configuration["AdminLoginEmail"] ?? Environment.GetEnvironmentVariable("AdminLoginEmail");
            string? adminPassword = configuration["AdminPwd"] ?? Environment.GetEnvironmentVariable("AdminPwd");

            string? moderatorLoginEmail = configuration["ModeratorLoginEmail"] ?? Environment.GetEnvironmentVariable("ModeratorLoginEmail");
            string? moderatorPassword = configuration["ModeratorPwd"] ?? Environment.GetEnvironmentVariable("ModeratorPwd");

            string? writerLoginEmail = configuration["WriterLoginEmail"] ?? Environment.GetEnvironmentVariable("WriterLoginEmail");
            string? writerPassword = configuration["WriterPwd"] ?? Environment.GetEnvironmentVariable("WriterPwd");

            try
            {
                //Seed the Admin
                BlogUser? adminUser = new BlogUser()
                {
                    UserName = adminLoginEmail,
                    Email = adminLoginEmail,
                    FirstName = "Simon",
                    LastName = "Schwartz",
                    EmailConfirmed = true
                };

                BlogUser? blogUser = await userManager.FindByEmailAsync(adminLoginEmail!);

                if (blogUser == null)
                {
                    await userManager.CreateAsync(adminUser, adminPassword!);
                    await userManager.AddToRoleAsync(adminUser!, _adminRole!);
                }
                //Seed the Moderator
                BlogUser? moderatorUser = new BlogUser()
                {
                    UserName = moderatorLoginEmail,
                    Email = moderatorLoginEmail,
                    FirstName = "Moderator",
                    LastName = "Raynor",
                    EmailConfirmed = true
                };

                blogUser = await userManager.FindByEmailAsync(moderatorLoginEmail!);

                if (blogUser == null)
                {
                    await userManager.CreateAsync(moderatorUser, moderatorPassword!);
                    await userManager.AddToRoleAsync(moderatorUser!, _moderatorRole!);
                }

                //Seed the Writer
                BlogUser? writerUser = new BlogUser()
                {
                    UserName = writerLoginEmail,
                    Email = writerLoginEmail,
                    FirstName = "Antonio",
                    LastName = "Raynor",
                    EmailConfirmed = true
                };

                blogUser = await userManager.FindByEmailAsync(writerLoginEmail!);

                if (blogUser == null)
                {
                    await userManager.CreateAsync(writerUser, writerPassword!);
                    await userManager.AddToRoleAsync(writerUser!, _writerRole!);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("******************ERROR************");
                Console.WriteLine(ex.ToString());
                Console.WriteLine("Error Seeding Defult Blog users.");
                Console.WriteLine("***********************************");
                throw;
            }

        }

    }
}

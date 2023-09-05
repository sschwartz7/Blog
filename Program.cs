using Blog.Data;
using Blog.Models;
using Blog.Services.Interfaces;
using Blog.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.UI.Services;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = DataUtility.GetConnectionString(builder.Configuration) ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString, o => o.MigrationsHistoryTable(tableName:"BlogMigrationHistory", schema: "blog")));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

//Edit this for custom role mod
builder.Services.AddIdentity<BlogUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddDefaultUI().AddDefaultTokenProviders()
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddMvc();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "MSTB",
        Version = "v1",
        Description = "Getting the latest blog posts from MSTB",
        Contact= new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name= "Simon Schwartz",
            Email = "sschwartz794@gmail.com",
            Url = new Uri("https://schwartzwork.vercel.app/")
        }
    });
    string xmlFileName = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFileName));
});

builder.Services.AddCors(cors =>
{
    cors.AddPolicy("DefaultPolicy", builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());                             
});
                            
//custom services
builder.Services.AddScoped<IImageService, ImageService>();                   
builder.Services.AddScoped<IBlogService, BlogService>();
builder.Services.AddScoped<IEmailSender, EmailService>();


//bind the email settings to the EmailSettings object
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

var app = builder.Build();

app.UseCors("DefaultPolicy");

//Makin services available for dependency injection engine
var scope = app.Services.CreateScope();
await DataUtility.ManageDataAsync(scope.ServiceProvider);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "PublicAPI v1");
    c.InjectStylesheet("/css/swagger.css");
    c.InjectJavascript("/js/swagger.js");

    c.DocumentTitle = "MSTB Documentation";
}); 

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

//Custom BlogPost Details Route
app.MapControllerRoute(
    name: "custom",
    pattern: "Content/{slug}",
    defaults: new {controller = "BlogPosts", action = "Details"}
    );

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=BlogPosts}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();

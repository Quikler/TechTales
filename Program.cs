using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TechTales.Data;
using TechTales.Data.Models;
using TechTales.Helpers.Extensions;
using TechTales.Hubs;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddUserSecrets<Program>();

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services
    .AddDbContext<AppDbContext>(optionsBuilder =>
    {
        var connectionString = builder.Configuration.GetConnectionString(nameof(TechTales));
        var serverVersion = new MySqlServerVersion(new Version(8, 4, 2));
        optionsBuilder.UseMySql(connectionString, serverVersion);
    });

builder.Services.AddAuthentication();
builder.Services.AddAuthorization();

builder.Services
    .AddIdentity<UserEntity, IdentityRole<Guid>>(options =>
    {
        options.Password.RequireDigit = false;
        options.Password.RequiredLength = 8;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireLowercase = false;
        options.Password.RequiredUniqueChars = 0;

        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
        options.Lockout.AllowedForNewUsers = true;
    })
    //.AddRoleManager<RoleManager<IdentityRole<Guid>>>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Authorization/Login";
    options.LogoutPath = "/Authorization/Logout";
    //options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromDays(14);
    options.SlidingExpiration = true;
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddSignalR(options => options.MaximumReceiveMessageSize = 102400000);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var serviceProvider = scope.ServiceProvider;

    await serviceProvider.InitializeRolesAsync();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error/InternalServer");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapHub<CommentHub>("/commentHub");
});

//app.MapHub<CommentHub>("/commentHub");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
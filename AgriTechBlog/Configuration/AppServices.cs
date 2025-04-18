using AgriTechBlog.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using AgriTechBlog.Data.Models;
using AgriTechBlog.WebUI.BusinessManagers.Interfaces;
using AgriTechBlog.WebUI.BusinessManagers;
using AgriTechBlog.Service.Interfaces;
using AgriTechBlog.Service;
using Microsoft.Extensions.FileProviders;
using System.IO;
using Microsoft.AspNetCore.Authorization;
using AgriTechBlog.WebUI.Authorization;
using System;

namespace AgriTechBlog.WebUI.Configuration
{
    public static class AppServices
    {
        public static void AddDefaultServices(this IServiceCollection servicesCollection, IConfiguration configuration)
        {
            servicesCollection.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection")));
            
            servicesCollection.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = false)
                .AddEntityFrameworkStores<ApplicationDbContext>();

            servicesCollection.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Identity/Account/Login";
                options.LogoutPath = "/Identity/Account/Logout";
                options.ExpireTimeSpan = TimeSpan.FromDays(14); // Persistent duration
                options.SlidingExpiration = true;
                options.Cookie.IsEssential = true; // Optional: for GDPR compliance
            });

            servicesCollection.AddControllersWithViews();
            servicesCollection.AddRazorPages();

            servicesCollection.AddSingleton<IFileProvider>(new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot")));
        }

        public static void AddCustomServices(this IServiceCollection servicesCollection)
        {
            servicesCollection.AddScoped<IPostBusinessManager, PostBusinessManager>();
            servicesCollection.AddScoped<IAdminBusinessManager, AdminBusinessManager>();
            servicesCollection.AddScoped<IHomeBusinessManager, HomeBusinessManager>();


            servicesCollection.AddScoped<IPostService, PostService>();
            servicesCollection.AddScoped<IUserService, UserService>();
        }

        public static void AddCustomAuthorization(this IServiceCollection servicesCollection)
        {
            servicesCollection.AddTransient<IAuthorizationHandler, PostAuthorizationHandler>();
        }
    }
}

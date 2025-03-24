using AgriTechBlog.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using AgriTechBlog.Data.Models;

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
            
            servicesCollection.AddControllersWithViews();
            servicesCollection.AddRazorPages();
        }
    }
}

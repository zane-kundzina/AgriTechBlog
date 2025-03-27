using AgriTechBlog.Data.Models;
using AgriTechBlog.Service.Interfaces;
using AgriTechBlog.WebUI.BusinessManagers.Interfaces;
using AgriTechBlog.WebUI.Models.AdminViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.CodeAnalysis.Host;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AgriTechBlog.WebUI.BusinessManagers
{
    public class AdminBusinessManager : IAdminBusinessManager
    {
        private UserManager<ApplicationUser> _userManager;
        private IPostService _postService;

        public AdminBusinessManager(UserManager<ApplicationUser> userManager, IPostService postService)
        {
            _userManager = userManager;
            _postService = postService;
        }

        public async Task<IndexViewModel> GetAdminDashboard(ClaimsPrincipal claimsPrincipal)
        {
            var applicationUser = await _userManager.GetUserAsync(claimsPrincipal);
            return new IndexViewModel
            {
                Posts = _postService.GetPosts(applicationUser)
            };
        }
    }
}

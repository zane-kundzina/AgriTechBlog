using AgriTechBlog.Data.Models;
using AgriTechBlog.Service.Interfaces;
using AgriTechBlog.WebUI.BusinessManagers.Interfaces;
using AgriTechBlog.WebUI.Models.BlogViewModels;
using Microsoft.AspNetCore.Identity;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AgriTechBlog.WebUI.BusinessManagers
{
    public class BlogBusinessManager: IBlogBusinessManager
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IBlogService _blogService;

        public BlogBusinessManager(UserManager<ApplicationUser> userManager, IBlogService blogService) 
        {
            _userManager = userManager;
            _blogService = blogService;
        }

        public async Task<Blog> CreateBlog(CreateBlogViewModel createBlogViewModel, ClaimsPrincipal claimsPrincipal)
        {
            Blog blog = createBlogViewModel.Blog;
            blog.Creator = await _userManager.GetUserAsync(claimsPrincipal);
            blog.CreatedDate = DateTime.Now;

            return await _blogService.Add(blog);
            
        }

    }
}

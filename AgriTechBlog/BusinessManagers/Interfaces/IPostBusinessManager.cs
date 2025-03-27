using AgriTechBlog.Data.Models;
using AgriTechBlog.WebUI.Models.PostViewModels;
using AgriTechBlog.WebUI.Models.HomeViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AgriTechBlog.WebUI.BusinessManagers.Interfaces
{
    public interface IPostBusinessManager
    {
        IndexViewModel GetIndexViewModel(string searchString, int? page);
        Task<Post> CreatePost(CreateViewModel createViewModel, ClaimsPrincipal claimsPrincipal);
        Task<ActionResult<EditViewModel>> UpdatePost(EditViewModel editViewModel, ClaimsPrincipal claimsPrincipal);
        Task<ActionResult<EditViewModel>> GetEditViewModel(int? id, ClaimsPrincipal claimsPrincipal);
    }
}

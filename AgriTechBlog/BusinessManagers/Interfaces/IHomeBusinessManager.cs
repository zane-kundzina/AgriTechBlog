using AgriTechBlog.WebUI.Models.HomeViewModels;
using Microsoft.AspNetCore.Mvc;

namespace AgriTechBlog.WebUI.BusinessManagers.Interfaces
{
    public interface IHomeBusinessManager
    {
        ActionResult<AuthorViewModel> GetAuthorViewModel(string authorId, string searchString, int? page);
    }
}

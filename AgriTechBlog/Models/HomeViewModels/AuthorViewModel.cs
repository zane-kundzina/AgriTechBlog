using AgriTechBlog.Data.Models;
using System.Diagnostics.CodeAnalysis;
using X.PagedList;

namespace AgriTechBlog.WebUI.Models.HomeViewModels
{
    [ExcludeFromCodeCoverage]

    public class AuthorViewModel
    {
        public ApplicationUser Author { get; set; }
        public IPagedList<Post> Posts { get; set; }
        public string SearchString { get; set; }
        public int PageNumber { get; set; }


    }
}

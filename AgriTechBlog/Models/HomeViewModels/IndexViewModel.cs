using AgriTechBlog.Data.Models;
using X.PagedList;


namespace AgriTechBlog.WebUI.Models.HomeViewModels
{
    public class IndexViewModel
    {
        public IPagedList<Post> Posts { get; set; }
        public string SearchString { get; set; }
        public int PageNumber { get; set; }
    }
}

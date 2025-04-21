using AgriTechBlog.Data.Models;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using X.PagedList;


namespace AgriTechBlog.WebUI.Models.HomeViewModels
{
    [ExcludeFromCodeCoverage]

    public class IndexViewModel
    {
        public IPagedList<Post> Posts { get; set; }
        public string SearchString { get; set; }
        public int PageNumber { get; set; }
        public List<Post> LastCommentedPosts { get; set; }
        public List<Post> MostLikedPosts { get; set; } = new List<Post>();

    }
}

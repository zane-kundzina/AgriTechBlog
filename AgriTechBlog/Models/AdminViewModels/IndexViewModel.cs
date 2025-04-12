using AgriTechBlog.Data.Models;
using System.Collections;
using System.Collections.Generic;

namespace AgriTechBlog.WebUI.Models.AdminViewModels
{
    public class IndexViewModel
    {
        public IEnumerable<Post> Posts { get; set; }
        public string SearchString { get; set; }

    }
}

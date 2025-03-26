using AgriTechBlog.Data.Models;
using System.Collections;
using System.Collections.Generic;

namespace AgriTechBlog.WebUI.Models.AdminViewModels
{
    public class IndexViewModel
    {
        public IEnumerable<Blog> Blogs { get; set; }
    }
}

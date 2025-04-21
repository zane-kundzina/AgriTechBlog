using AgriTechBlog.Data.Models;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace AgriTechBlog.WebUI.Models.AdminViewModels
{
    [ExcludeFromCodeCoverage]

    public class IndexViewModel
    {
        public IEnumerable<Post> Posts { get; set; }
        public string SearchString { get; set; }

    }
}

using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Collections.Generic;
using System.Linq;

namespace AgriTechBlog.WebUI.TagHelpers
{
    [HtmlTargetElement(Attributes = "list, count")]

    public class HidePagerHelper : TagHelper
    {
        // If there are less than 20 posts (just for 1 page, it will hide pager - previous/next)

        public IEnumerable<object> List { get; set; }
        public int Count { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (List.Count() <= Count)
                output.SuppressOutput();
        }
    }
}

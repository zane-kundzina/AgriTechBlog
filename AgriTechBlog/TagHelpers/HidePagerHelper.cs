using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Collections.Generic;
using System.Linq;
using X.PagedList;

namespace AgriTechBlog.WebUI.TagHelpers
{
    //[HtmlTargetElement(Attributes = "list, count")]
    [HtmlTargetElement(Attributes = "paged-list")]

    public class HidePagerHelper : TagHelper
    {
        // If there are less than 6 posts (just for 1 page, it will hide pager - previous/next)       

        public IPagedList PagedList { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (PagedList.TotalItemCount <= PagedList.PageSize)
            {
                output.SuppressOutput(); // Hide pager if everything fits on one page
            }
        }
    }
}

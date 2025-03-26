using AgriTechBlog.Data.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AgriTechBlog.Service.Interfaces
{
    public interface IBlogService
    {
        Blog GetBlog(int blogid);
        IEnumerable<Blog> GetBlogs(string searchString);
        IEnumerable<Blog> GetBlogs(ApplicationUser applicationUser);
        Task<Blog> Add(Blog blog);
        Task<Blog> Update(Blog blog);
    }
}

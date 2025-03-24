using AgriTechBlog.Service.Interfaces;
using AgriTechBlog.Data;
using System;
using System.Threading.Tasks;
using AgriTechBlog.Data.Models;

namespace AgriTechBlog.Service
{
    public class BlogService: IBlogService
    {
        private readonly ApplicationDbContext _applicationDbContext;

        public BlogService(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }
        public async Task<Blog> Add(Blog blog)
        {
            _applicationDbContext.Add(blog);
            await _applicationDbContext.SaveChangesAsync();
            return blog;
        }
    }
}

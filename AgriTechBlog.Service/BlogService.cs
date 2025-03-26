using AgriTechBlog.Service.Interfaces;
using AgriTechBlog.Data;
using System;
using System.Threading.Tasks;
using AgriTechBlog.Data.Models;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace AgriTechBlog.Service
{
    public class BlogService: IBlogService
    {
        private readonly ApplicationDbContext _applicationDbContext;

        public BlogService(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }
       
        public Blog GetBlog(int blogid)
        {
            return _applicationDbContext.Blogs.FirstOrDefault(blog  => blog.Id == blogid);
        }

        public IEnumerable<Blog> GetBlogs(ApplicationUser applicationUser)
        {
            return _applicationDbContext.Blogs
                .Include(blog => blog.Creator)
                .Include(blog => blog.Approver)
                .Include(blog => blog.Posts)
                .Where(blog => blog.Creator == applicationUser);
        }

        public async Task<Blog> Add(Blog blog)
        {
            _applicationDbContext.Add(blog);
            await _applicationDbContext.SaveChangesAsync();
            return blog;
        }

        public async Task<Blog> Update(Blog blog)
        {
            _applicationDbContext.Update(blog);
            await _applicationDbContext.SaveChangesAsync();
            return blog;
        }
    }
}

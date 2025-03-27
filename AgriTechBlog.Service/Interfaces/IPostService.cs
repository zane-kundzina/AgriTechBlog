using AgriTechBlog.Data.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AgriTechBlog.Service.Interfaces
{
    public interface IPostService
    {
        Post GetPost(int postid);
        IEnumerable<Post> GetPosts(string searchString);
        IEnumerable<Post> GetPosts(ApplicationUser applicationUser);
        Task<Post> Add(Post post);
        Task<Post> Update(Post post);
    }
}

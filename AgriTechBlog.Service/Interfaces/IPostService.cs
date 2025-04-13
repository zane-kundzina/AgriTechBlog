using AgriTechBlog.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgriTechBlog.Service.Interfaces
{
    public interface IPostService
    {
        Post GetPost(int postid);
        IQueryable<Post> GetPosts(string searchString);
        IEnumerable<Post> GetPosts(ApplicationUser applicationUser);
        Comment GetComment(int commentId);
        Task<Post> Add(Post post);
        Task<Comment> Add(Comment comment);
        Task<Post> Update(Post post);
    }
}

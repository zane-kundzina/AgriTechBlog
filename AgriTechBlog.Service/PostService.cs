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
    public class PostService: IPostService
    {
        private readonly ApplicationDbContext _applicationDbContext;

        public PostService(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }
       
        public Post GetPost(int postid)
        {
            return _applicationDbContext.Posts
                .Include(post => post.Creator)
                .Include(post => post.Reactions)
                .Include(post => post.Comments)
                    .ThenInclude(comment => comment.Author)
                .Include(post => post.Comments)
                    .ThenInclude(comment => comment.Comments)
                        .ThenInclude(reply => reply.Parent)
                .FirstOrDefault(post  => post.Id == postid);
        }

        public IQueryable<Post> GetPosts(string searchString)
        {
            return _applicationDbContext.Posts
                .Include(post => post.Creator)
                .Include(post => post.Comments)
                .Include(post => post.Reactions)
                .Where(post => post.Title.Contains(searchString) || post.Content.Contains(searchString));
        }

        public IEnumerable<Post> GetPosts(ApplicationUser applicationUser)
        {
            return _applicationDbContext.Posts
                .Include(post => post.Creator)
                .Include(post => post.Approver)
                .Include(post => post.Comments)
                .Where(post => post.Creator == applicationUser);
        }

        public Comment GetComment(int commentId)
        {
            return _applicationDbContext.Comments
                .Include(comment => comment.Author)
                .Include(comment => comment.Post)
                .Include(comment => comment.Parent)
                .FirstOrDefault(comment => comment.Id == commentId);
        }

        public async Task<Post> Add(Post post)
        {
            _applicationDbContext.Add(post);
            await _applicationDbContext.SaveChangesAsync();
            return post;
        }

        public async Task<Comment> Add(Comment comment)
        {
            _applicationDbContext.Add(comment);
            await _applicationDbContext.SaveChangesAsync();
            return comment;
        }

        public async Task<Post> Update(Post post)
        {
            _applicationDbContext.Update(post);
            await _applicationDbContext.SaveChangesAsync();
            return post ;
        }
    }
}

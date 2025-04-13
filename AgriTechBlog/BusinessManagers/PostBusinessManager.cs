using AgriTechBlog.Data.Models;
using AgriTechBlog.Service.Interfaces;
using AgriTechBlog.WebUI.Authorization;
using AgriTechBlog.WebUI.BusinessManagers.Interfaces;
using AgriTechBlog.WebUI.Models.PostViewModels;
using AgriTechBlog.WebUI.Models.HomeViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Host;
using PagedList.Core;
using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AgriTechBlog.Service;

namespace AgriTechBlog.WebUI.BusinessManagers
{
    public class PostBusinessManager: IPostBusinessManager
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IPostService _postService;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IAuthorizationService _authorizationService;

        public PostBusinessManager(
            UserManager<ApplicationUser> userManager, 
            IPostService postService, 
            IWebHostEnvironment webHostEnvironment, 
            IAuthorizationService authorizationService) 
        {
            _userManager = userManager;
            _postService = postService;
            _webHostEnvironment = webHostEnvironment;
            _authorizationService = authorizationService;
        }

        public IndexViewModel GetIndexViewModel(string searchString, int? page)
        {
            int pageSize = 6;
            int pageNumber = page ?? 1;
            var posts = _postService.GetPosts(searchString ?? string.Empty)
                .Where(post => post.Published);
            //.Where(blog => blog.Published && blog.Approved);

            return new IndexViewModel
            {
                Posts = new StaticPagedList<Post>(posts.Skip((pageNumber - 1) * pageSize).Take(pageSize), pageNumber, pageSize, posts.Count()),
                SearchString = searchString,
                PageNumber = pageNumber
            };
        }

        public async Task<ActionResult<PostViewModel>> GetPostViewModel(int? id, ClaimsPrincipal claimsPrincipal)
        {
            if (id is null)
                return new BadRequestResult();
            
            var postId = id.Value;

            var post = _postService.GetPost(postId);

            if (post is null)
                return new NotFoundResult();

            if (!post.Published)
            {
                var authorizationResult = await _authorizationService.AuthorizeAsync(claimsPrincipal, post, Operations.Read);

                if (!authorizationResult.Succeeded) return DetermineActionResult(claimsPrincipal);
            }

            return new PostViewModel
            {
                Post = post
            };
        }

        public async Task<Post> CreatePost(CreateViewModel createViewModel, ClaimsPrincipal claimsPrincipal)
        {
            Post post = createViewModel.Post;
            post.Creator = await _userManager.GetUserAsync(claimsPrincipal);
            post.CreatedDate = DateTime.Now;
            post.UpdatedDate = DateTime.Now;

            post = await _postService.Add(post);
            string webRootPath = _webHostEnvironment.WebRootPath;
            string pathToImage =$@"{webRootPath}\UserFiles\Posts\{post.Id}\HeaderImage.jpg";

            EnsureFolder(pathToImage);

            using (var fileStream = new FileStream(pathToImage, FileMode.Create))
            {
                await createViewModel.HeaderImage.CopyToAsync(fileStream);
            }

            return post;            
        }

        public async Task<ActionResult<Comment>> CreateComment(PostViewModel postViewModel, ClaimsPrincipal claimsPrincipal)
        {
            if (postViewModel.Post is null || postViewModel.Post.Id == 0)
                return new BadRequestResult();

            var post = _postService.GetPost(postViewModel.Post.Id);

            if (post is null)
                return new NotFoundResult();

            var comment = postViewModel.Comment;

            comment.Author = await _userManager.GetUserAsync(claimsPrincipal);
            comment.Post = post;
            comment.CreatedOn = DateTime.Now;

            if (comment.Parent != null)
            {
                comment.Parent = _postService.GetComment(comment.Parent.Id);
            }

            return await _postService.Add(comment);
        }

        public async Task<ActionResult<EditViewModel>> UpdatePost(EditViewModel editViewModel, ClaimsPrincipal claimsPrincipal)
        {
            var post = _postService.GetPost(editViewModel.Post.Id);

            if (post is null)
                return new NotFoundResult();

            var authorizationResult = await _authorizationService.AuthorizeAsync(claimsPrincipal, post, Operations.Update);

            if (!authorizationResult.Succeeded) return DetermineActionResult(claimsPrincipal);

            post.Published = editViewModel.Post.Published;
            post.Title = editViewModel.Post.Title;
            post.Content = editViewModel.Post.Content;
            post.UpdatedDate = DateTime.Now;

            if (editViewModel.HeaderImage != null)
            {
                string webRootPath = _webHostEnvironment.WebRootPath;
                string pathToImage = $@"{webRootPath}\UserFiles\Posts\{post.Id}\HeaderImage.jpg";

                EnsureFolder(pathToImage);

                using (var fileStream = new FileStream(pathToImage, FileMode.Create))
                {
                    await editViewModel.HeaderImage.CopyToAsync(fileStream);
                }
            }

            return new EditViewModel
            {
                Post = await _postService.Update(post)
            };
        }

        public async Task<ActionResult<EditViewModel>> GetEditViewModel(int? id, ClaimsPrincipal claimsPrincipal)
        {
            if (id is null)
                return new BadRequestResult();

            var postId = id.Value;
            var post = _postService.GetPost(postId);

            if (post is null)
                return new NotFoundResult();

            var authorizationResult = await _authorizationService.AuthorizeAsync(claimsPrincipal, post, Operations.Update);

            if (!authorizationResult.Succeeded) return DetermineActionResult(claimsPrincipal);                          

            return new EditViewModel
            {
                Post = post
            };
        }

        private ActionResult DetermineActionResult(ClaimsPrincipal claimsPrincipal)
        {
            if (claimsPrincipal.Identity.IsAuthenticated)
            {
                return new ForbidResult();
            }                
            else
            {
                return new ChallengeResult();
            }
                
        }

        private void EnsureFolder(string path)
        {
            string directoryName = Path.GetDirectoryName(path);
            if (directoryName.Length > 0)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
            }
        }

    }
}

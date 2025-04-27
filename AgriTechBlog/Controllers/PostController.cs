using AgriTechBlog.Data;
using AgriTechBlog.Data.Models;
using AgriTechBlog.WebUI.BusinessManagers;
using AgriTechBlog.WebUI.BusinessManagers.Interfaces;
using AgriTechBlog.WebUI.Models.CommentViewModels;
using AgriTechBlog.WebUI.Models.PostViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace AgriTechBlog.WebUI.Controllers
{
    [Authorize]
    public class PostController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IPostBusinessManager _postBusinessManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public PostController(ApplicationDbContext context, IPostBusinessManager postBusinessManager, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _postBusinessManager = postBusinessManager;
            _userManager = userManager;
        }

        [Route("Post/{id}", Name = "PostDetails"), AllowAnonymous]
        public async Task<IActionResult> Index(int? id)
        {
            var actionResult = await _postBusinessManager.GetPostViewModel(id, User);

            if (actionResult.Result is null)
                return View(actionResult.Value);

            return actionResult.Result;
        }

        public IActionResult Create()
        {
            return View(new CreateViewModel());
        }

        public async Task<IActionResult> Edit(int? id)
        {
            var actionResult = await _postBusinessManager.GetEditViewModel(id, User);

            if (actionResult.Result is null)
                return View(actionResult.Value);

            return actionResult.Result;
        }

        [HttpPost]
        public async Task<IActionResult> Add(CreateViewModel createViewModel)
        {
            await _postBusinessManager.CreatePost(createViewModel, User);
            //return RedirectToAction("Create");
            return RedirectToAction("Index", "Admin");
        }

        [HttpPost]
        public async Task<IActionResult> Update(EditViewModel editViewModel)
        {
            var actionResult = await _postBusinessManager.UpdatePost(editViewModel, User);

            if (actionResult.Result is null)
                return RedirectToAction("Edit", new { editViewModel.Post.Id });

            return actionResult.Result;
        }

        [HttpPost]
        public async Task<IActionResult> Comment(PostViewModel postViewModel)
        {
            var comment = await _postBusinessManager.CreateComment(postViewModel, User);

            if (comment == null)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    return BadRequest();

                return RedirectToAction("Index", new { postViewModel.Post.Id });
            }

            // If it's an AJAX request, return JSON
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new
                {
                    success = true,
                    authorName = $"{comment.Author.FirstName} {comment.Author.LastName}",
                    content = comment.Content,
                    createdOn = comment.CreatedOn.ToString("MMMM d, yyyy hh:mm tt")
                });
            }

            // Otherwise (normal classic submit)
            return RedirectToAction("Index", new { id = comment.Post.Id });
        }



        [HttpPost]
        public IActionResult ReactToPost(int postId, bool isLike)
        {
            var userId = _userManager.GetUserId(User);

            var existingReaction = _context.PostReactions
                .FirstOrDefault(r => r.PostId == postId && r.UserId == userId);

            if (existingReaction != null)
            {
                if (existingReaction.IsLike == isLike)
                {
                    _context.PostReactions.Remove(existingReaction); // unreact
                }
                else
                {
                    existingReaction.IsLike = isLike; // change reaction
                    _context.PostReactions.Update(existingReaction);
                }
            }
            else
            {
                var reaction = new PostReaction
                {
                    PostId = postId,
                    UserId = userId,
                    IsLike = isLike
                };

                _context.PostReactions.Add(reaction);
            }

            _context.SaveChanges();
            return RedirectToRoute("PostDetails", new { id = postId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var post = _context.Posts
                .Include(p => p.Comments)
                .Include(p => p.Reactions)
                .FirstOrDefault(p => p.Id == id);

            if (post == null) return NotFound();

            var userId = _userManager.GetUserId(User);
            if (post.CreatorID != userId && !User.IsInRole("Admin"))
                return Forbid();

            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Post deleted successfully!";
            return RedirectToAction("Index", "Admin");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleVisibility(int id)
        {
            var post = await _context.Posts
                .Include(p => p.Creator)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (post == null)
                return NotFound();

            var userId = _userManager.GetUserId(User);

            if (post.Creator?.Id != userId && !User.IsInRole("Admin"))
                return Forbid();

            post.Published = !post.Published;
            _context.Update(post);
            await _context.SaveChangesAsync();

            TempData["Message"] = post.Published
                ? "Post has been made public again!"
                : "Post has been hidden successfully.";

            return RedirectToAction("Index", "Admin");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteComment(int commentId)
        {
            var comment = await _context.Comments
                .Include(c => c.Comments) // include replies
                .FirstOrDefaultAsync(c => c.Id == commentId);

            if (comment == null)
            {
                return NotFound();
            }

            var currentUserId = _userManager.GetUserId(User);
            var postAuthorId = await _context.Posts
                .Where(p => p.Id == comment.PostId)
                .Select(p => p.Creator.Id)
                .FirstOrDefaultAsync();

            if (comment.AuthorId != currentUserId && postAuthorId != currentUserId)
            {
                return Forbid(); // Not authorized
            }

            // First delete all replies
            if (comment.Comments != null && comment.Comments.Any())
            {
                _context.Comments.RemoveRange(comment.Comments);
            }

            // Then delete the parent comment
            _context.Comments.Remove(comment);

            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Post", new { id = comment.PostId });
        }


        [HttpPost]
        public async Task<IActionResult> HideComment(int commentId)
        {
            var comment = await _context.Comments.FindAsync(commentId);
            if (comment == null)
            {
                return NotFound();
            }

            var currentUserId = _userManager.GetUserId(User);
            var postAuthorId = _context.Posts
                .Where(p => p.Id == comment.PostId)
                .Select(p => p.Creator.Id)
                .FirstOrDefault();

            if (postAuthorId != currentUserId)
            {
                return Forbid();
            }            

            // Toggle Hide/Unhide
            comment.IsHidden = !comment.IsHidden;

            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Post", new { id = comment.PostId });
        }

        [HttpPost]
        public async Task<IActionResult> ToggleHideComment(int commentId)
        {
            var comment = await _context.Comments.FindAsync(commentId);
            if (comment == null)
            {
                return NotFound();
            }

            comment.IsHidden = !comment.IsHidden;
            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true,
                isHidden = comment.IsHidden
            });
        }


        [HttpGet]
        public async Task<IActionResult> EditComment(int commentId)
        {
            var comment = await _context.Comments.FindAsync(commentId);
            if (comment == null)
            {
                return NotFound();
            }

            var currentUserId = _userManager.GetUserId(User);
            if (comment.AuthorId != currentUserId)
            {
                return Forbid();
            }

            var model = new EditCommentViewModel
            {
                CommentId = comment.Id,
                Content = comment.Content
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditComment(EditCommentViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var comment = await _context.Comments.FindAsync(model.CommentId);
                if (comment == null)
                {
                    return NotFound();
                }

                return RedirectToAction("Index", "Post", new { id = comment.PostId });
            }

            var existingComment = await _context.Comments.FindAsync(model.CommentId);
            if (existingComment == null)
            {
                return NotFound();
            }

            var currentUserId = _userManager.GetUserId(User);
            if (existingComment.AuthorId != currentUserId)
            {
                return Forbid();
            }

            existingComment.Content = model.Content;
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Post", new { id = existingComment.PostId });
        }



    }
}

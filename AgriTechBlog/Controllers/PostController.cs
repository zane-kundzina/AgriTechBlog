using AgriTechBlog.Data;
using AgriTechBlog.Data.Models;
using AgriTechBlog.WebUI.BusinessManagers;
using AgriTechBlog.WebUI.BusinessManagers.Interfaces;
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
            var actionResult = await _postBusinessManager.CreateComment(postViewModel, User);

            if (actionResult.Result is null)
                return RedirectToAction("Index", new { postViewModel.Post.Id });

            return actionResult.Result;
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
    }
}

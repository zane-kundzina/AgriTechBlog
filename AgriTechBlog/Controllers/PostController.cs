using AgriTechBlog.WebUI.BusinessManagers.Interfaces;
using AgriTechBlog.WebUI.Models.PostViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace AgriTechBlog.WebUI.Controllers
{
    public class PostController : Controller
    {
        private readonly IPostBusinessManager _postBusinessManager;

        public PostController(IPostBusinessManager postBusinessManager)
        {
            _postBusinessManager = postBusinessManager;
        }

        public IActionResult Index()
        {
            return View();
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
            return RedirectToAction("Create");
        }

        [HttpPost]
        public async Task<IActionResult> Update(EditViewModel editViewModel)
        {
            var actionResult = await _postBusinessManager.UpdatePost(editViewModel, User);

            if (actionResult.Result is null)
                return RedirectToAction("Edit", new { editViewModel.Post.Id });

            return actionResult.Result;
        }
    }
}

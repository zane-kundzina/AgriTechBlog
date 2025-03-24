using Microsoft.AspNetCore.Mvc;

namespace AgriTechBlog.WebUI.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}

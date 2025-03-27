using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using AgriTechBlog.Data.Models;
using AgriTechBlog.WebUI.BusinessManagers.Interfaces;

namespace AgriTechBlog.Controllers
{
    public class HomeController : Controller
    {
        private readonly IPostBusinessManager _postBusinessManager;

        public HomeController(IPostBusinessManager postBusinessManager)
        {
            _postBusinessManager = postBusinessManager;
        }

        public IActionResult Index(string searchString, int? page)
        {
            return View(_postBusinessManager.GetIndexViewModel(searchString, page));
        }       

        //[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        //public IActionResult Error()
        //{
        //    return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        //}
    }
}

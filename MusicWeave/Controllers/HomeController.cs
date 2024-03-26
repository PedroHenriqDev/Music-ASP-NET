using Microsoft.AspNetCore.Mvc;
using MusicWeave.Cloud.Classes;
using MusicWeave.Models.ViewModels;
using MusicWeave.Services;
using System.Diagnostics;

namespace MusicWeave.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly MusicService _musicService;

        public HomeController(ILogger<HomeController> logger, MusicService musicService)
        {
            _logger = logger;
            _musicService = musicService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error(string message)
        {
            return View(new ErrorViewModel { Message = message, RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

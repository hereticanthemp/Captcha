using Captcha;
using Demo.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Demo.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ICaptchaFactory _captchaFactory;

        public HomeController(ILogger<HomeController> logger, ICaptchaFactory captchaFactory)
        {
            _logger = logger;
            _captchaFactory = captchaFactory;
        }

        public async Task<IActionResult> Index()
        {
            CaptchaInfo c = await _captchaFactory.CreateAsync(new CaptchaOption
            {
                Type = CaptchaTypes.Numeric | CaptchaTypes.UpperCase,
                FontSize = 20,
                CharCount = 6
            });
            return View(c);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
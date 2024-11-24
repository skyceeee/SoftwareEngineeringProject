using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SOFTWAREPROJECT_2024.Models;

namespace SOFTWAREPROJECT_2024.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        //Giriş sayfası
        public IActionResult Index()
        {   
            return View();
        }

        //About Page
        public IActionResult About(){

            return View();
        }
        //SignIn Page
        public IActionResult SignIn(){

            return View();
        }
        public IActionResult SignUp(){
            return View();
        }
        public IActionResult Profile(){
            return View();
        }

        public IActionResult Reservation(){
            
            return View();
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

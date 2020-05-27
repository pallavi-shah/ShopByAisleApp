using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ShopBy_Aisle.Models;


namespace ShopBy_Aisle.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signinManager;

        public HomeController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signinManager, ILogger<HomeController> logger)
        {
            _logger = logger;
            _userManager = userManager;
            _signinManager = signinManager;
        }


        public IActionResult Index()
        {
            if (_signinManager.IsSignedIn(User))
            {
                return Redirect("/MasterItems/Index");
            }
            else
            {
                return Redirect("/Identity/Account/Login");
            }
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

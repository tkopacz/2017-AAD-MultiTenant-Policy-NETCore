using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TK2017MTAADv2.Models;
using System.Security.Claims;

namespace TK2017MTAADv2.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            var claims = ((ClaimsIdentity)User.Identity).Claims;
            return View((object)claims);
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        [Authorize(Policy = "AdminPolicy")]
        public IActionResult DemoAdminPolicy()
        {
            ViewData["Message"] = "Demo - AdminPolicy";
            return View("Demo");
        }

        [Authorize(Policy = "Admin1Policy")]
        public IActionResult DemoAdmin1Policy()
        {
            ViewData["Message"] = "Demo - Admin1Policy";
            return View("Demo");
        }

        [Authorize(Policy = "AdminPolicyByGuid")]
        public IActionResult DemoAdminPolicyByGuid()
        {
            ViewData["Message"] = "Demo - AdminPolicyByGuid";
            return View("Demo");
        }

        /// <summary>
        /// No Roles Support!
        /// </summary>
        /// <returns></returns>
        ///We didn't setup the roles!
        [Authorize(Roles = "Company Administrator")]
        public IActionResult DemoAdminRoles()
        {
            ViewData["Message"] = "Demo - Admin Roles";

            return View("Demo");
        }


        [AllowAnonymous]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

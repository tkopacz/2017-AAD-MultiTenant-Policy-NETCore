using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace TK2017MTAADv2.Controllers
{
    public class MTController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
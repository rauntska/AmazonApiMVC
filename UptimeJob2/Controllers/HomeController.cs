using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace UptimeJob2.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Message = "HomeTask";

            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Amazon Product Search page.";

            return View();
        } 
    }
}

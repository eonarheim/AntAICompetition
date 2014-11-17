using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AntAICompetition.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Title = "Ant AI Challenge";

            return View();
        }

        public ActionResult Game(int id)
        {
            return View(id);
        }
    }
}

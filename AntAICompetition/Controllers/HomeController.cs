using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AntAICompetition.Server;

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

        [HttpPost]
        public JsonResult KillGame(int id)
        {
            GameManager.Instance.KillGame(id);
            return Json("success");
        }
    }
}

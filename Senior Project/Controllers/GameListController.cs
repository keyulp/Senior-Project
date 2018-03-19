using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Senior_Project.Models;

namespace Senior_Project.Controllers
{
    public class GameListController : Controller
    {
        // GET: GameList
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult List()
        {
            return View();
        }
    }
}
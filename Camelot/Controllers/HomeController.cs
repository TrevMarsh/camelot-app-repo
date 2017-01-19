using Camelot.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Camelot.Controllers
{
    
    public class HomeController : Controller
    {
        private CamelotContext db = new CamelotContext();
        
        [Authorize]
        public ActionResult Program()
        {
            return View();
        }

        public ActionResult GetAllData()
        {
            return PartialView("_SessionList", db.Sessions.Where(s => s.EndTime == null).ToList());
        }

        public ActionResult GetArchiveData()
        {
            return PartialView("_ArchiveList", db.Sessions.Where(s => s.EndTime != null).ToList());
        }

        public ActionResult Chat()
        {
            return View();
        }

        public ActionResult Join()
        {
            return View();
        }

        public ActionResult GetAllActives()
        {
            return PartialView("_ActivesList", db.Sessions.Where(s => s.StartTime != null && s.EndTime == null).ToList());
        }

        [Authorize]
        public ActionResult Archive()
        {
            return View();
        }

        public ActionResult About()
        {
            return View();
        }
    }
}
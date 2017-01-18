using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Camelot.Models;
using System.Web.Security;

namespace Camelot.Controllers
{
    public class AccountController : Controller
    {
        // GET: Account
        public ActionResult Index()
        {
            using (OurDbContext db = new OurDbContext())
            {
                return View(db.userAccount.ToList());
            }
        }

        public ActionResult Register()
        {
            return View();
        }

        //Adding new user
        [HttpPost]
        public ActionResult Register(UserAccount account)
        {
            if (ModelState.IsValid)
            {
                using (OurDbContext db = new OurDbContext())
                {
                    db.userAccount.Add(account);
                    db.SaveChanges(); //required to save changes to db
                }
                ModelState.Clear();
                ViewBag.Message = account.FirstName + " " + account.LastName + " successfully registered.";
            }
            return View();
        }

        //Login
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(UserAccount model, string returnUrl)
        {
            //Lets first check if the Model is valid or not
            if (ModelState.IsValid)
            {
                using (userDbEntities entities = new userDbEntities())
                {
                    string username = model.Username;
                    string password = model.Password;

                    bool userValid = entities.
                }
            }
        }
    }
}
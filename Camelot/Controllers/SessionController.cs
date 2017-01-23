using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Camelot.DAL;
using Camelot.Models;
using Camelot.ViewModels;
using System.Diagnostics;

namespace Camelot.Controllers
{
    public class SessionController : Controller
    {
        private CamelotContext db = new CamelotContext();
        private readonly int RADIUS = 10;
        
        public ActionResult Index()
        {
            return View(db.Sessions.ToList());
        }
        
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Session session = db.Sessions.Find(id);
            if (session == null)
            {
                return HttpNotFound();
            }
            return View(session);
        }
        
        public ActionResult Create()
        {
            return View();
        }

        public ActionResult ArchiveGraph(int id)
        {
            Round round = db.Rounds.Find(id);

            return View(round);
        }

        [HttpPost]
        public JsonResult GetChartArchiveData(int? roundID)
        {
            if (roundID == null || roundID == 0)
            {
                return Json(new { }, JsonRequestBehavior.AllowGet);
            }
            var round = db.Rounds.Find(roundID);
            var part = round.Parts.FirstOrDefault(p => p.IsActive);

            if (part == null)
            {
                return Json(new { }, JsonRequestBehavior.AllowGet);
            }

            var responses = part.Responses.ToList();

            var datasets = responses.Select(r =>
            {
                var point = new
                {
                    x = (int)r.Importance,
                    y = (int)r.Feasability,
                    r = RADIUS
                };

                var points = new[] { point }.ToList();
                return new
                {
                    label = r.Participant,
                    backgroundColor = r.Color,
                    hoverBackgroundColor = r.Color,
                    data = points
                };
            }
            ).ToList();

            db.SaveChanges();

            return Json(datasets, JsonRequestBehavior.AllowGet);
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,Name,StartTime,EndTime")] Session session)
        {
            if (ModelState.IsValid)
            {
                db.Sessions.Add(session);
                db.SaveChanges();
                SessionHub.BroadcastData();
                return RedirectToAction("Index");
            }

            return View(session);
        }
        
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Session session = db.Sessions.Find(id);
            if (session == null)
            {
                return HttpNotFound();
            }
            return View(session);
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,Name,StartTime,EndTime")] Session session)
        {
            if (ModelState.IsValid)
            {
                db.Entry(session).State = EntityState.Modified;
                db.SaveChanges();
                SessionHub.BroadcastData();
                return RedirectToAction("Index");
            }
            return View(session);
        }
        
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Session session = db.Sessions.Find(id);
            if (session == null)
            {
                return HttpNotFound();
            }
            return View(session);
        }
        
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Session session = db.Sessions.Find(id);
            db.Sessions.Remove(session);
            db.SaveChanges();
            SessionHub.BroadcastData();
            return RedirectToAction("Index");
        }
        
        public ActionResult Start(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Session session = db.Sessions.Find(id);
            if (session == null)
            {
                return HttpNotFound();
            }

            var sessionRound = new SessionRound {
                SessionID = id.Value,
                SessionName = session.Name,
                Number = 1
            };

            return View(sessionRound);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Start([Bind(Include = "SessionID,SessionName,Number,Topic")] SessionRound sessionRound)
        {
            if (!String.IsNullOrWhiteSpace(sessionRound.Topic))
            {
                Round round = new Round
                {
                    Number = sessionRound.Number,
                    Topic = sessionRound.Topic,
                    SessionID = sessionRound.SessionID
                };

                db.Rounds.Add(round);
                db.SaveChanges();

                // 1. create a new part for the newly created round and add it too it

                Part part = new Part
                {
                    IsActive = true,
                    Number = 1,
                    RoundID = round.ID
                };

                db.Parts.Add(part);
                db.SaveChanges();
                
                SessionHub.BroadcastTopic(round);

                return RedirectToAction("Plot", "Voting", new { id = round.ID });
            }

            return View(sessionRound);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}

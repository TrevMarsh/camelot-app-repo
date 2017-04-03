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

        [Authorize]
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

        public ActionResult RoundData(int id)
        {
            Session session = db.Sessions.Find(id);
            var roundList = session.Rounds;

            return PartialView("_RoundData", roundList);
        }

        public ActionResult RoundList(int id)
        {
            return View(id);
        }

        [HttpPost]
        public JsonResult GetChartArchiveData(int? roundID)
        {
            if (roundID == null || roundID == 0)
            {
                return Json(new { }, JsonRequestBehavior.AllowGet);
            }
            var round = db.Rounds.Find(roundID);
            var parts = round.Parts;

            if (parts == null)
            {
                return Json(new { }, JsonRequestBehavior.AllowGet);
            }

            System.Collections.ArrayList temp = new System.Collections.ArrayList();

            foreach (Part part in parts)
            {
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
                        label = "Part: " + r.Part.Number + " Participant: " + r.Participant,
                        backgroundColor = r.Color,
                        hoverBackgroundColor = r.Color,
                        borderWidth = 10,
                        data = points
                    };
                }).ToList();

                if (datasets != null && datasets.Count()> 0)
                {
                   
                    temp.AddRange(datasets);
                }
            }


            db.SaveChanges();

            return Json(temp, JsonRequestBehavior.AllowGet);
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

        [HttpGet]
        public ActionResult Start(int? sessionID, string flag)
        {
            if (sessionID == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Session session = db.Sessions.Find(sessionID);
            
            if (session == null)
            {
                return HttpNotFound();
            }

            int roundNumber = 0;
            if (flag == null)
            {
                roundNumber = 1;
            }
            else if( flag.Equals("Next") )
            {
                Round round = session.Rounds.Last();
                roundNumber = round.Number + 1;
            }

            var sessionRound = new SessionRound
            {
                SessionID = sessionID.Value,
                SessionName = session.Name,
                Number = roundNumber
            };

            return View(sessionRound);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Start([Bind(Include = "SessionID,SessionName,Number,Topic")] SessionRound sessionRound)
        {
            if (!String.IsNullOrWhiteSpace(sessionRound.Topic))
            {
                Session session = db.Sessions.Find(sessionRound.SessionID);
                session.Active = true;

                Round round = new Round
                {
                    Number = sessionRound.Number,
                    Topic = sessionRound.Topic,
                    SessionID = sessionRound.SessionID,
                    IsActive = true
                };

                db.Rounds.Add(round);

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

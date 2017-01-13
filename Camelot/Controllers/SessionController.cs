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

        // GET: Session
        public ActionResult Index()
        {
            return View(db.Sessions.ToList());
        }

        // GET: Session/Details/5
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

        // GET: Session/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Session/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
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

        // GET: Session/Edit/5
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

        // POST: Session/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
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

        // GET: Session/Delete/5
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

        // POST: Session/Delete/5
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

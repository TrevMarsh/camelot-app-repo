using Camelot.DAL;
using Camelot.Models;
using Camelot.ViewModels;
using RandomColorGenerator;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace Camelot.Controllers
{
    public class VotingController : Controller
    {
        private CamelotContext db = new CamelotContext();
        private readonly int RADIUS = 10;


        public ActionResult Plot(int id)
        {
            Round round = db.Rounds.Find(id);

            return View(round);
        }

        [HttpPost]
        public JsonResult GetChartData(int? roundID)
        {
            int foo = 0;
            if (roundID == null || roundID == 0)
            {
                return Json(new { }, JsonRequestBehavior.AllowGet);
            }
            var round = db.Rounds.Find(roundID);
            var part = round.Parts.FirstOrDefault(p => p.IsActive);

            if (part == null)
            {
                // this shouldn't happen so need to return a failed graph data here;
                return Json(new { }, JsonRequestBehavior.AllowGet);
            }

            // if the part does exist however get the responses and return the datasets

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
                       hoverBackGroundColor = r.Color,
                       data = points
                   };
               }
            ).ToList();

            db.SaveChanges();

            return Json(datasets, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Cancel(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Round round = db.Rounds.Find(id);
            Session session = round.Session;

            db.Rounds.Remove(round);
            db.SaveChanges();

            return RedirectToAction("Start", "Session", new { id = session.ID });
        }

        public ActionResult Room(JoinParticipant jp)
        {
            if (jp == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            SessionParticipant vm = new SessionParticipant();

            vm.SessionID = jp.SessionID;
            vm.ID = jp.ID;
            vm.Name = jp.Name;

            var session = db.Sessions.Find(vm.SessionID);
            vm.SessionName = session.Name;
            
            var color = RandomColor.GetColor(ColorScheme.Random, Luminosity.Dark);
            var hex = ColorTranslator.ToHtml(color);

            vm.Color = hex;

            return View(vm);
        }

        [HttpGet]
        public ActionResult Register(int id)
        {
            JoinParticipant jp = new JoinParticipant();
            jp.SessionID = id;
            return View(jp);
        }

        [HttpPost]
        public ActionResult Register(JoinParticipant joinParticipant)
        {
            if (joinParticipant.ID == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            if (ModelState.IsValid)
            {
                db.joinParticipants.Add(joinParticipant);
                db.SaveChanges();
            }
            
            return RedirectToAction("Room", "Voting", joinParticipant);
        }


        //public ActionResult Register(int? id)
        //{

        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }

        //    var vm = new SessionParticipant
        //    {
        //        ID = id
        //    };

        //    return View(vm);
        //}

        
        public ActionResult Next(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Session session = db.Sessions.Find(id);
            Round round = db.Rounds.Find(id);
            if (session == null)
            {
                return HttpNotFound();
            }

            var sessionRound = new SessionRound
            {
                SessionID = id.Value,
                SessionName = session.Name,
                Number = round.Number + 1
            };

            return View("Start", "Session", sessionRound);
        }

        [HttpPost]
        public ActionResult Next(SessionRound sessionRound)
        {
            Round round = new Round
            {
                Number = sessionRound.Number + 1
            };

            return RedirectToAction("Start", "Session");

        }

        //Finish the current session
        public RedirectToRouteResult Close(int? id)
        {
            //set the session into archive
            Session session = db.Sessions.Find(id);
            session.EndTime = DateTime.Now;
            session.Active = false;
            db.SaveChanges();
            return RedirectToAction("Program", "Home");
        }

        [HttpPost]
        public ActionResult GetVotingControls(int roundID, string user, string color)
        {

            Round round = db.Rounds.Find(roundID);
            var partID = round.Parts.Where(p => p.IsActive).First().ID;
            var vm = new TopicResponse
            {
                PartID = partID,
                Participant = user,
                Topic = round.Topic,
                Color = color
            };
            return PartialView("_VotingForm", vm);
        }

        public int? CheckSession(int ID)
        {
            var session = db.Sessions.Find(ID);
            Round round = session.Rounds.FirstOrDefault(q => q.SessionID == session.ID && q.IsActive == true);
            if (round == null) return null;
            else return round.ID;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public PartialViewResult Vote([Bind(Include = "PartID,Participant,Topic,Importance,Feasability,Color")] TopicResponse topicResponse)
        {

            Response response = new Response
            {
                Participant = topicResponse.Participant,
                PartID = topicResponse.PartID,
                Importance = topicResponse.Importance,
                Feasability = topicResponse.Feasability,
                Color = topicResponse.Color
            };

            //put your color code here

            /*var color = RandomColor.GetColor(ColorScheme.Random, Luminosity.Light);
            var hex = ColorTranslator.ToHtml(color);
            response.Color = hex;

            if (response.Participant == Response.Participant)
            {
                response.Color = RandomColor
            }*/

            db.Responses.Add(response);
            db.SaveChanges();

            var part = db.Parts.Find(response.PartID);
            
            SessionHub.RespondToTopic(part.RoundID);

            return PartialView("_VoteConfirmation");
        }

        [HttpGet]
        public RedirectToRouteResult Repeat(int ID)
        {
            var currentRound = db.Rounds.FirstOrDefault(r => r.ID == ID);
            var partNumber = currentRound.Parts.Last().ID + 1;
            Part parts = new Part
            {
                IsActive = true,
                Number = partNumber,
                RoundID = ID
            };

            db.Parts.Add(parts);
            db.SaveChanges();
            SessionHub.RepeatTopic();

            return RedirectToAction("Plot", "Voting", new { id = ID });
        }

    }
}
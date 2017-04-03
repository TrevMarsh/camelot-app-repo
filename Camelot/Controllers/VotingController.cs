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
            var parts = round.Parts;
            //var parts = round.Parts.Last();

            if (parts == null)
            {
                // this shouldn't happen so need to return a failed graph data here;
                return Json(new { }, JsonRequestBehavior.AllowGet);
            }

            // if the part does exist however get the responses and return the datasets

            List<Response> responses = new List<Models.Response>();
            foreach (Part part in parts)
            {
                responses.AddRange(part.Responses);
            }

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
                       borderWidth = 10,
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

            return RedirectToAction("Start", "Session", new { sessionID = session.ID });
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

            var color = db.joinParticipants.Find(jp.ID).Color;

            vm.Color = color;

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
                var color = RandomColor.GetColor(ColorScheme.Random, Luminosity.Light);
                //var color = Color.Red;
                var hex = ColorTranslator.ToHtml(color);

                joinParticipant.Color = hex;
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
        public ActionResult GetVotingControls(int roundID, string user, string color, int joinParticipantID)
        {

            Round round = db.Rounds.Find(roundID);
            var partID = round.Parts.Where(p => p.IsActive).First().ID;
            var vm = new TopicResponse
            {
                PartID = partID,
                Participant = user,
                Topic = round.Topic,
                Color = color,
                JoinParticipantID = joinParticipantID
            };
            return PartialView("_VotingForm", vm);
        }

        public int? CheckSession(int ID)
        {
            var session = db.Sessions.Find(ID);
            Round round = null;
            if (session.Rounds != null && session.Rounds.Count() > 0)
                round = session.Rounds.Last(q => q.SessionID == session.ID && q.IsActive == true);

            if (round == null) return null;
            else return round.ID;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public PartialViewResult Vote([Bind(Include = "PartID,Participant,Topic,Importance,Feasability,Color, JoinParticipantID")] TopicResponse topicResponse)
        {

            Response response = new Response
            {
                Participant = topicResponse.Participant,
                PartID = topicResponse.PartID,
                Importance = topicResponse.Importance,
                Feasability = topicResponse.Feasability,
                Color = topicResponse.Color,
                JoinParticipantID = topicResponse.JoinParticipantID
            };

            var part = db.Parts.Find(response.PartID);

            Round round = part.Round;
            response.PartID = round.Parts.Last().ID;
            int parts = round.Parts.Count();

            if (parts > 1)
            {
                System.Drawing.Color color = System.Drawing.ColorTranslator.FromHtml(topicResponse.Color);
                int green = color.G - 8 * parts;
                int blue = color.B;
                int red = color.R - 8 * parts;
                int alpha = color.A - 8 * parts;
                if (green > 255)
                {
                    green = 255;
                }
                if (blue > 255)
                {
                    blue = 255;
                }
                if (red > 255)
                {
                    red = 255;
                }
                
                    response.Color = ColorTranslator.ToHtml(Color.FromArgb(25, red, green, blue));
                
            }


            db.Responses.Add(response);
            db.SaveChanges();

            SessionHub.RespondToTopic(part.RoundID);

            return PartialView("_VoteConfirmation");
        }

        [HttpGet]
        public RedirectToRouteResult Repeat(int ID)
        {
            //var currentRound = db.Rounds.FirstOrDefault(r => r.ID == ID);
            var currentRound = db.Rounds.Find(ID);
            var partNumber = currentRound.Parts.Last().Number + 1;
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
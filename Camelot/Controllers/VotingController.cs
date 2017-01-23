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

        public ActionResult Room(SessionParticipant vm)
        {
            if (vm.ID == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var session = db.Sessions.Find(vm.ID);
            vm.SessionName = session.Name;
            
            var color = RandomColor.GetColor(ColorScheme.Random, Luminosity.Dark);
            var hex = ColorTranslator.ToHtml(color);

            vm.Color = hex;

            return View(vm);
        }

        public ActionResult Register(int? id)
        {

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var vm = new SessionParticipant
            {
                ID = id
            };

            return View(vm);
        }

        public ActionResult Next()
        {
            return RedirectToAction("Start");
        }

        //Finish the current session
        public ActionResult Close()
        {
            return View();
        }

        [HttpPost]
        public ActionResult GetVotingControls(Round round, string user, string color)
        {
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
            db.Responses.Add(response);
            db.SaveChanges();

            var part = db.Parts.Find(response.PartID);
            
            SessionHub.RespondToTopic(part.RoundID);

            return PartialView("_VoteConfirmation");
        }

    }
}
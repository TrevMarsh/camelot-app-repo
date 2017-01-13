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

        public ActionResult Plot(int id)
        {
            Round round = db.Rounds.Find(id);

            return View(round);
        }

        [HttpPost]
        public JsonResult GetChartData(int? roundID)
        {

            if (roundID == null || roundID == 0)
            {
                return Json(new { }, JsonRequestBehavior.AllowGet);
            }
            Round round = db.Rounds.Find(roundID);

            var part = round.Parts.First(p => p.IsActive);

            if(part == null)
            {
                return Json(new { }, JsonRequestBehavior.AllowGet);
            }

            // if the part does exist however get the responses and return the datasets
            
            var responses = part.Responses.ToList();

            // the color of the users should be save for comparison purposes

            int radius = 10; // don't like this loose local x.x

            var datasets = responses.Select( r =>
                {
                    var point = new
                    {
                        x = (int)r.Importance,
                        y = (int)r.Feasability,
                        r = radius
                    };
                    var color = RandomColor.GetColor(ColorScheme.Random, Luminosity.Dark);
                    var hex = ColorTranslator.ToHtml(color);

                    var points = new[] { point }.ToList();
                    return new
                    {
                        label = r.Participant,
                        backgroundColor = hex,
                        hoverBackGroundColor = hex,
                        data = points
                    };
                }    
            ).ToList();
            
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

            Session session = db.Sessions.Find(vm.ID);
            vm.SessionName = session.Name;
            if (session.Rounds.Count != 0)
            {

            }
            // 1. get the session that was joined
            // 2. get the last added round
            // 2a. there is around and display it's stuff on the view on a partial view
            // 2b. there is non wait on the server to post one and refresh the partial view's controls
            ViewBag.User = vm.Name;

            TempData["User"] = vm.Name;

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

        [HttpPost]
        public ActionResult GetVotingControls(Round round, string user)
        {
            var vm = new TopicResponse
            {
                // get the part id of the most active part
                PartID = round.ID,
                Participant = user,
                Topic = round.Topic
            };
            return PartialView("_VotingForm", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public PartialViewResult Vote([Bind(Include = "PartID,Participant,Topic,Importance,Feasability")] TopicResponse topicResponse)
        {
            Response response = new Response
            {
                Participant = topicResponse.Participant,
                PartID = topicResponse.PartID,
                Importance = topicResponse.Importance,
                Feasability = topicResponse.Feasability
            };
            db.Responses.Add(response);
            db.SaveChanges();

            // 3. send signal to server to update graph
            SessionHub.RespondToTopic(response.PartID);

            return PartialView("_VoteConfirmation");
        }

    }
}
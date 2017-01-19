using Camelot.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace Camelot.DAL
{
    public class CamelotInitializer : DropCreateDatabaseIfModelChanges<CamelotContext>
    {
        protected override void Seed(CamelotContext context)
        {
            var sessions = new List<Session>
            {
                new Session { Name="NYE", Active=true, StartTime=DateTime.Parse("2016-12-22") },
                new Session { Name="Become Rich", Active=true, StartTime=DateTime.Parse("2016-12-22") },
                new Session { Name="Graduate", Active=false, StartTime=DateTime.Parse("2010-09-01"), EndTime= DateTime.Parse("2016-02-29") },
                new Session { Name="Project: Camelot", Active=true, StartTime=DateTime.Parse("2016-12-12") },
                new Session { Name="Watch Star Wars", Active=false, StartTime=DateTime.Parse("1977-05-25"), EndTime=DateTime.Parse("1990-03-12") },
                new Session { Name="Tacos Day", Active=true, StartTime=DateTime.Parse("2016-12-12")}
            };

            sessions.ForEach(s => context.Sessions.Add(s));
            context.SaveChanges();
            var rounds = new List<Round>
            {
                new Round { Topic="Being able to afford it.", SessionID=1 },
                new Round { Topic="For the Future.", SessionID=3 },
                new Round { Topic="Learn from it.", SessionID=3 },
                new Round { Topic="Will anyone know?", SessionID=4 },
                new Round { Topic="Is it Criminal.", SessionID=4 },
                new Round { Topic="What about Time.", SessionID=2 },
                new Round { Topic="Relevant with the Current Environment.", SessionID=5 },
                new Round { Topic="Making MONEY.", SessionID=5 },
                new Round { Topic="Will it Matter.", SessionID=3 }
            };
            rounds.ForEach(r => context.Rounds.Add(r));
            context.SaveChanges();

            var responses = new List<Response>
            {
                //new Response { RoundID=1, Participant="Bob", Importance=Score.FIVE, Feasability=Score.FOUR},
                //new Response { RoundID=1, Participant="Jill", Importance=Score.THREE, Feasability=Score.TWO},
                //new Response { RoundID=1, Participant="Andy", Importance=Score.FIVE, Feasability=Score.FIVE},
                //new Response { RoundID=2, Participant="Bob", Importance=Score.FIVE, Feasability=Score.ONE},
                //new Response { RoundID=2, Participant="Jill", Importance=Score.TWO, Feasability=Score.FOUR},
                //new Response { RoundID=2, Participant="Andy", Importance=Score.ONE, Feasability=Score.FIVE},
                //new Response { RoundID=3, Participant="Bob", Importance=Score.FOUR, Feasability=Score.TWO},
                //new Response { RoundID=3, Participant="Jill", Importance=Score.ONE, Feasability=Score.FOUR},
                //new Response { RoundID=3, Participant="Alex", Importance=Score.FIVE, Feasability=Score.TWO},
                //new Response { RoundID=4, Participant="Jay", Importance=Score.FIVE, Feasability=Score.FOUR},
                //new Response { RoundID=4, Participant="Bob", Importance=Score.FIVE, Feasability=Score.FOUR},
                //new Response { RoundID=5, Participant="Bas", Importance=Score.FIVE, Feasability=Score.ONE},
                //new Response { RoundID=5, Participant="Bob", Importance=Score.ONE, Feasability=Score.FOUR},
                //new Response { RoundID=6, Participant="Alice", Importance=Score.THREE, Feasability=Score.THREE},
                //new Response { RoundID=6, Participant="Udgar", Importance=Score.TWO, Feasability=Score.THREE},
                //new Response { RoundID=7, Participant="Jake", Importance=Score.FIVE, Feasability=Score.FOUR},
                //new Response { RoundID=7, Participant="Beck", Importance=Score.TWO, Feasability=Score.FIVE},
                //new Response { RoundID=8, Participant="Bob", Importance=Score.FIVE, Feasability=Score.FOUR},
                //new Response { RoundID=8, Participant="Jay", Importance=Score.FIVE, Feasability=Score.FOUR},
                //new Response { RoundID=9, Participant="Chester Bennington", Importance=Score.ONE, Feasability=Score.TWO},
                //new Response { RoundID=9, Participant="James Hetfield", Importance=Score.ONE, Feasability=Score.ONE},
            };
            responses.ForEach(r => context.Responses.Add(r));
            context.SaveChanges();
        }
    }
}
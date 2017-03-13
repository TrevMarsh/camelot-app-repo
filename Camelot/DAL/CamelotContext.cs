using Camelot.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Web;

namespace Camelot.DAL
{
    public class CamelotContext : DbContext
    {
        public CamelotContext() : base("CamelotContext") { }

        public DbSet<Session> Sessions { get; set; }
        public DbSet<Round> Rounds { get; set; }
        public DbSet<Part> Parts { get; set; }
        public DbSet<Response> Responses { get; set; }
        public DbSet<UserAccount> userAccount { get; set; }
        public DbSet<JoinParticipant> joinParticipants { get; set; }
    }
}
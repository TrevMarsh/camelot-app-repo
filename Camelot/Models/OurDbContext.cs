using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;

namespace Camelot.Models
{
    public class OurDbContext : DbContext
    {
        public DbSet<UserAccount> userAccount { get; set; }

        public System.Data.Entity.DbSet<Camelot.Models.Round> Rounds { get; set; }

        public System.Data.Entity.DbSet<Camelot.Models.Session> Sessions { get; set; }
    }
}
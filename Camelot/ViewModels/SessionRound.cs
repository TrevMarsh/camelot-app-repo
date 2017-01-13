using Camelot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Camelot.ViewModels
{
    public class SessionRound
    {
        public int SessionID { get; set; }
        public string SessionName { get; set; }
        public string Topic { get; set; }
        public int Number { get; set; }
    }
}
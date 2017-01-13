using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace Camelot.Models
{
    [JsonObject(IsReference = true)]
    [DataContract(IsReference = true)]
    public class Round
    {
        public int ID { get; set; }
        public int Number { get; set; }
        public string Topic { get; set; }
        public bool IsActive { get; set; }
        public int SessionID { get; set; }
        

        public virtual Session Session { get; set; }
        public virtual ICollection<Part> Parts { get; set; }

    }
}
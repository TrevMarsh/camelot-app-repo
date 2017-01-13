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
    public class Part
    {
        public int ID { get; set; }
        public int Number { get; set; }
        public bool IsActive { get; set; }
        public int RoundID { get; set; }


        public virtual Round Round { get; set; }
        public virtual ICollection<Response> Responses { get; set; }
    }
}
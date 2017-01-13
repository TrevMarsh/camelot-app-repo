using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Camelot.Models
{
    [JsonObject(IsReference = true)]
    public class Session
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public bool Active { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        
        public virtual ICollection<Round> Rounds { get; set; }
    }
}
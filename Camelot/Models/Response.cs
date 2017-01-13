using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace Camelot.Models
{
    public enum Score
    {
        ONE = 1, TWO, THREE, FOUR, FIVE
    }

    [DataContract(IsReference = true)]
    public class Response
    {
        public int ID { get; set; }
        public string Participant { get; set; }
        public int PartID { get; set; }
        public Score Importance { get; set; }
        public Score Feasability { get; set; }
        
        public virtual Part Part { get; set; }
    }
}
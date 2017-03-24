using Camelot.Models;

namespace Camelot.ViewModels
{
    public class TopicResponse
    {
        public int PartID { get; set; }
        public string Participant { get;  set;}
        public string Topic { get; set; }
        public Score Importance { get; set; }
        public Score Feasability { get; set; }
        public string Color { get; set; }
        public int JoinParticipantID { get; set; }
    }
}
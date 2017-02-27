using System.ComponentModel.DataAnnotations;
using System.Net.Http;

namespace Camelot.ViewModels
{
    public class SessionParticipant
    {
        public int? ID { get; set; }
        public string SessionName { get; set; }

        public string Name { get; set; }
        public string Color { get; set; }
    }
}
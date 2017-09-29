using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BikeSharing.Clients.CogServicesKiosk.Models
{
    public class UserProfile
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime? BirthDate { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public int? PaymentId { get; set; }
        public Guid? FaceProfileId { get; set; }
        public Guid? VoiceProfileId { get; set; }
        public string Skype { get; set; }
        public string Mobile { get; set; }
        public string VoiceSecretPhrase { get; set; }
    }
}

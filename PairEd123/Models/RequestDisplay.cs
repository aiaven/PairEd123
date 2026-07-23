using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PairEd123.Models
{
    public class RequestDisplay
    {
        public int RequestId { get; set; }
        public int TuteeId { get; set; }
        public int TutorId { get; set; }
        public string OtherPartyName { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public DateTime PreferredDate { get; set; }
        public string Status { get; set; } = "Pending";
        public string? RejectionReason { get; set; }
    }
}
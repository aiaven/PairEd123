using System;

namespace PairEd123.Models
{
    public class SessionToReview
    {
        public int SessionId { get; set; }
        public string OtherPartyName { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public DateTime ScheduledTime { get; set; }
        public int TutorId { get; set; }
        public int TuteeId { get; set; }
    }
}
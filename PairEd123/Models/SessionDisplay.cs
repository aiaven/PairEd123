using System;

namespace PairEd123.Models
{
    // A session row joined with the other party's name, for list display.
    public class SessionDisplay
    {
        public int SessionId { get; set; }
        public int RequestId { get; set; }
        public int TutorId { get; set; }
        public int StudentId { get; set; }
        public string Subject { get; set; } = string.Empty;
        public DateTime ScheduledTime { get; set; }
        public string Status { get; set; } = "Upcoming";
        public string? CancellationReason { get; set; }
        public string OtherPartyName { get; set; } = string.Empty;
        public bool IsTutorView { get; set; } // true if the current user is the tutor for this session
    }
}
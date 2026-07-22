namespace PairedAPI.Models
{
    public class Feedback
    {
        public int FeedbackId { get; set; }
        public int SessionId { get; set; }
        public int TuteeId { get; set; }
        public int TutorId { get; set; }
        public int Rating { get; set; }
        public string? Comments { get; set; }
    }
}

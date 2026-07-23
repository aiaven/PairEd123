namespace PairEd123.Models
{
    public class FeedbackDisplay
    {
        public int FeedbackId { get; set; }
        public int TutorId { get; set; }
        public int TuteeId { get; set; }
        public int SessionId { get; set; }
        public int AuthorId { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public string OtherPartyName { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
    }
}
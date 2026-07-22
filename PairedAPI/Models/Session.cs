namespace PairedAPI.Models
{
    public class Session
    {
        public int SessionId { get; set; }
        public int TutorId { get; set; }
        public int StudentId { get; set; }
        public string Subject { get; set; }
        public DateTime ScheduledTime { get; set; }
        public string Status { get; set; }
    }
}

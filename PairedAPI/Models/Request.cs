namespace PairedAPI.Models
{
    public class Request
    {
        public int RequestId { get; set; }
        public int TuteeId { get; set; }
        public int TutorId { get; set; }
        public string Subject { get; set; }
        public DateTime PreferredDate { get; set; }
        public string Status { get; set; }
        public string? RejectionReason { get; set; }
    }
}

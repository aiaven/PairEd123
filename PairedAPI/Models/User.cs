namespace PairedAPI.Models
{
    public class User
    {
        public int UserId { get; set; }       // PK
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string DisplayName { get; set; }
        public string? ProfilePicture { get; set; }
        public bool IsTutor { get; set; }
        public string? Availability { get; set; }
        public string Role { get; set; }      // default "Student"
    }
}

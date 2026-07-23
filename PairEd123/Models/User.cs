using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PairEd123.Models
{
    public class User
    {
        public int UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string? ProfilePicture { get; set; }
        public bool IsTutor { get; set; }
        public string? Availability { get; set; }
        public string? Bio { get; set; }
        public string Role { get; set; } = "Student";
    }
}
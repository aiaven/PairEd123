using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PairEd123.Models
{
    public class TutorSearchResult
    {
        public int UserId { get; set; }
        public string DisplayName { get; set; } = string.Empty;
        public string Skills { get; set; } = string.Empty;
    }
}
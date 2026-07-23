using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace PairEd123.ViewModels
{
    // One row in the tutor's skill picker.
    public partial class SkillPickItem : ObservableObject
    {
        [ObservableProperty] private int skillId;       // catalog skills.SkillId
        [ObservableProperty] private string skillName = string.Empty;
        [ObservableProperty] private bool isSelected;

        // userskills.SkillId (the assignment row's own PK) — null if not currently assigned.
        // NOT the same as SkillId above. Needed by RemoveSkillFromTutorAsync.
        public int? UserSkillRowId { get; set; }
    }
}
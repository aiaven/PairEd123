using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PairEd123.Models;
using PairEd123.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace PairEd123.ViewModels
{
    public partial class AdminSkillsViewModel : ObservableObject
    {
        private readonly SkillsService _skillsService;
        public AdminSkillsViewModel(SkillsService skillsService) => _skillsService = skillsService;
        public ObservableCollection<Skill> Skills { get; } = new();
        [ObservableProperty] private string newSkillName = string.Empty;
        [ObservableProperty] private string statusMessage = string.Empty;
        [ObservableProperty] private bool isBusy;
        public async Task LoadAsync()
        {
            var skills = await _skillsService.GetCatalogAsync();
            Skills.Clear();
            foreach (var s in skills) Skills.Add(s);
        }
        [RelayCommand]
        private async Task AddSkillAsync()
        {
            if (IsBusy || string.IsNullOrWhiteSpace(NewSkillName)) return;
            IsBusy = true;
            try
            {
                var (success, message) = await _skillsService.AddToCatalogAsync(NewSkillName);
                StatusMessage = message;
                if (success)
                {
                    NewSkillName = string.Empty;
                    await LoadAsync();
                }
            }
            finally { IsBusy = false; }
        }
        [RelayCommand]
        private async Task DeleteSkillAsync(Skill skill)
        {
            if (skill is null || IsBusy) return;
            IsBusy = true;
            try
            {
                bool deleted = await _skillsService.DeleteFromCatalogAsync(skill.SkillId);
                if (deleted) Skills.Remove(skill);
            }
            finally { IsBusy = false; }
        }
    }
}
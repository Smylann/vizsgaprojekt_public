using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using MobileVersion.Dtos;
using MobileVersion.Messages;
using MobileVersion.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MobileVersion.ViewModels
{
    public partial class SettingsVM : ViewModelBase
    {
        private readonly consoleClientModel _model;
        private readonly UserVM _userprofile;
        [ObservableProperty] private string _errorMessage;
        [ObservableProperty] private string _eColor;
        public string CurrentPass { get; set; }
        public string NewPass { get; set; }
        public string NewPassConfirm { get; set; }

        public SettingsVM(UserVM user, consoleClientModel model)
        {
            _userprofile = user;
            _model = model;
            _errorMessage = string.Empty;
        }

        [RelayCommand]
        private void Close() => WeakReferenceMessenger.Default.Send(new NavigateToUserMessage(_userprofile.User));

        [RelayCommand]
        private async Task LogOut()
        {
            try
            {
                await _model.LogOut();
                _model.CurrentUser = null;
                WeakReferenceMessenger.Default.Send(new GoBackMessage());
                WeakReferenceMessenger.Default.Send(new UserLoggedOutMessage());
            }
            catch { }
        }
        [RelayCommand]
        private async Task ModifyPass()
        {
            if (NewPass == NewPassConfirm)
            {
                try
                {
                    await _model.ModifyPassword(new ModifyPasswordDTO { currentPassword = CurrentPass, newPassword = NewPass });
                    EColor = "Green";
                    ErrorMessage = "Password changed successfully.";
                }
                catch { }
            }
            else
            {
                EColor = "Red";
                ErrorMessage = "New password and confirmation do not match.";
            }
        }
    }
}

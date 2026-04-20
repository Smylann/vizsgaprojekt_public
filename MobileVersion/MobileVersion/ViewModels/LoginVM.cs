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
    public partial class LoginVM : ViewModelBase
    {
        private readonly consoleClientModel _model;
        public string LoginUsername { get; set; }
        public string LoginPassword { get; set; }
        public string RegUsername { get; set; }
        public string RegEmail { get; set; }
        public string RegPassword { get; set; }
        [ObservableProperty] private bool _isRegisterVisible;
        [ObservableProperty] private string _errorMessage;
        [ObservableProperty] private bool _errorMessageIsVisible;

        [ObservableProperty] private bool _seeLogPass = false;
        [ObservableProperty] private bool _seeRegPass = false;



        // Update constructor to receive the model
        public LoginVM(consoleClientModel model)
        {
            _model = model;
            LoginUsername = string.Empty;
            LoginPassword = string.Empty;
            RegUsername = string.Empty;
            RegEmail = string.Empty;
            RegPassword = string.Empty;
            IsRegisterVisible = false;
            ErrorMessage = string.Empty;
            ErrorMessageIsVisible = false;
        }
        [RelayCommand] private void ShowRegister() => IsRegisterVisible = !IsRegisterVisible;
        [RelayCommand] private void Close() => WeakReferenceMessenger.Default.Send(new GoBackMessage());

        [RelayCommand] private void ToggleLogPass() => SeeLogPass = !SeeLogPass;
        [RelayCommand] private void ToggleRegPass() => SeeRegPass = !SeeRegPass;

        [RelayCommand]
        private async Task Login()
        {
            if (string.IsNullOrEmpty(LoginUsername) || string.IsNullOrEmpty(LoginPassword))
            {
                ErrorMessage = "No input field can remain empty!";
                ErrorMessageIsVisible = true;
                return;
            }
            try
            {
                await _model.LogIn(new LoginDto { username = LoginUsername, password = LoginPassword });
                var datas = await _model.me();
                if (datas != null)
                {
                    _model.CurrentUser = new DisplayAllUserDTO { UserID = datas.ID, Username = datas.UserName, Role = datas.Role };

                    // Trigger refresh in MainViewModel
                    WeakReferenceMessenger.Default.Send(new UserLoggedInMessage());

                    WeakReferenceMessenger.Default.Send(new GoBackMessage());
                }
            }
            catch { ErrorMessage = "Login failed. Please check your credentials."; ErrorMessageIsVisible = true; }
        }

        [RelayCommand]
        private async Task Register()
        {
            if (string.IsNullOrEmpty(RegUsername) || string.IsNullOrEmpty(RegEmail) || string.IsNullOrEmpty(RegPassword))
            {
                ErrorMessage = "No input field can remain empty!";
                ErrorMessageIsVisible = true;
                return;
            }
            try
            {
                await _model.Register(new RegistrationDto { username = RegUsername, email = RegEmail, password = RegPassword });
                await Task.Delay(1000); // Recommendation: use await Task.Delay instead of Task.Delay(...).Wait()
                await _model.LogIn(new LoginDto { username = RegUsername, password = RegPassword });
                var datas = await _model.me();
                if (datas != null)
                {
                    _model.CurrentUser = new DisplayAllUserDTO { UserID = datas.ID, Username = datas.UserName, Role = datas.Role };

                    // Trigger refresh in MainViewModel
                    WeakReferenceMessenger.Default.Send(new UserLoggedInMessage());

                    WeakReferenceMessenger.Default.Send(new GoBackMessage());
                }
            }
            catch { ErrorMessage = "Registration failed. Please try again."; ErrorMessageIsVisible = true; }

        }
    }
}

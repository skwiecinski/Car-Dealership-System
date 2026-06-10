using System;
using System.Net.Mail;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SalonSamochodowy.Entities;
using SalonSamochodowy.Services;

namespace SalonSamochodowy.ViewModels
{
    public partial class LoginViewModel : ObservableObject
    {
        private readonly AuthService _authService;

        public LoginViewModel(AuthService authService)
        {
            _authService = authService;
        }

        [ObservableProperty]
        private string email = "";

        [ObservableProperty]
        private string errorMessage = "";

        [ObservableProperty]
        private bool isLoading;

        public event Action<AppUser>? LoginSucceeded;
        public event Action? ExitRequested;

        [RelayCommand]
        private async Task LoginAsync(object? passwordSource)
        {
            ErrorMessage = "";

            var password = (passwordSource as Wpf.Ui.Controls.PasswordBox)?.Password ?? "";
            var emailTrim = Email?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(emailTrim) || string.IsNullOrWhiteSpace(password))
            {
                ErrorMessage = "Podaj adres e-mail i hasło.";
                return;
            }

            if (!IsValidEmail(emailTrim))
            {
                ErrorMessage = "Podany adres e-mail jest niepoprawny. Przykład: nazwa@domena.pl";
                return;
            }

            IsLoading = true;
            try
            {
                
                await Task.Delay(500);

                var loggedUser = await _authService.LoginAsync(emailTrim, password);
                if (loggedUser == null)
                {
                    ErrorMessage = "Niepoprawny e-mail lub hasło.";
                    return;
                }

                LoginSucceeded?.Invoke(loggedUser);
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private void Exit()
        {
            ExitRequested?.Invoke();
        }

        private static bool IsValidEmail(string email)
        {
            if (!MailAddress.TryCreate(email, out var address))
                return false;

            var domain = address.Host;
            var dotIndex = domain.IndexOf('.');
            return dotIndex > 0 && dotIndex < domain.Length - 1;
        }
    }
}

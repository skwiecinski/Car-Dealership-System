using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Mail;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace SalonSamochodowy.ViewModels
{
    public partial class EditEmployeeViewModel : ObservableObject
    {
        [ObservableProperty] private string firstName = "";
        [ObservableProperty] private string lastName = "";
        [ObservableProperty] private string email = "";
        [ObservableProperty] private string roleName = "";
        [ObservableProperty] private DealershipItem? selectedDealership;
        [ObservableProperty] private string newPassword = "";
        
        [ObservableProperty] private string errorMessage = "";
        [ObservableProperty] private System.Windows.Visibility errorVisibility = System.Windows.Visibility.Collapsed;

        public ObservableCollection<string> AvailableRoles { get; } = new();
        public ObservableCollection<DealershipItem> AvailableDealerships { get; } = new();

        public event Action? SaveSucceeded;
        public event Action? CancelRequested;

        public EditEmployeeViewModel()
        {
        }

        public void LoadData(AccountRow accountRow, System.Collections.Generic.IEnumerable<DealershipItem> dealerships)
        {
            FirstName = accountRow.FullName.Split(' ').FirstOrDefault() ?? "";
            LastName = string.Join(" ", accountRow.FullName.Split(' ').Skip(1));
            Email = accountRow.Email;
            RoleName = accountRow.RoleName;

            AvailableRoles.Add("Sprzedawca");
            AvailableRoles.Add("Serwisant");
            AvailableRoles.Add("Kierownik");
            AvailableRoles.Add("Administrator");

            foreach (var d in dealerships)
            {
                AvailableDealerships.Add(d);
            }

            if (accountRow.DealershipID.HasValue)
            {
                SelectedDealership = AvailableDealerships.FirstOrDefault(d => d.DealershipID == accountRow.DealershipID.Value);
            }
        }

        [RelayCommand]
        private void Save()
        {
            if (string.IsNullOrWhiteSpace(FirstName) || string.IsNullOrWhiteSpace(LastName))
            {
                ShowError("Imię i nazwisko są wymagane.");
                return;
            }

            if (string.IsNullOrWhiteSpace(Email) || !IsValidEmail(Email))
            {
                ShowError("Podaj poprawny adres e-mail.");
                return;
            }

            var isWorkerRole = RoleName == "Sprzedawca" || RoleName == "Serwisant" || RoleName == "Kierownik";
            if (isWorkerRole && SelectedDealership == null)
            {
                ShowError("Wybierz salon dla tego pracownika.");
                return;
            }

            ErrorVisibility = System.Windows.Visibility.Collapsed;
            SaveSucceeded?.Invoke();
        }

        [RelayCommand]
        private void Cancel()
        {
            CancelRequested?.Invoke();
        }

        private void ShowError(string message)
        {
            ErrorMessage = message;
            ErrorVisibility = System.Windows.Visibility.Visible;
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

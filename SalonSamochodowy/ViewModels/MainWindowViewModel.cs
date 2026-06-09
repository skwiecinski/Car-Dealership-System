using System;
using System.Windows;
using System.Windows.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SalonSamochodowy.Entities;
using SalonSamochodowy.Repositories;
using SalonSamochodowy.Views;

namespace SalonSamochodowy.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
        private readonly IUnitOfWork _uow;

        public MainWindowViewModel(IUnitOfWork uow)
        {
            _uow = uow;
            WeakReferenceMessenger.Default.Register(this, (MainWindowViewModel r, SalonSamochodowy.Messages.LanguageChangedMessage m) =>
            {
                r.OnPropertyChanged(nameof(DisplayRoleName));
            });
        }
        public AppUser LoggedInUser { get; private set; }

        [ObservableProperty] private string userFullName = "";
        [ObservableProperty] private string roleName = "";
        public string DisplayRoleName => RoleName == "—" ? "—" : SalonSamochodowy.Services.LocalizationHelper.GetString($"Role_{RoleName}");

        [ObservableProperty] private Visibility dashboardVisibility   = Visibility.Collapsed;
        [ObservableProperty] private Visibility clientsVisibility     = Visibility.Collapsed;
        [ObservableProperty] private Visibility createOrderVisibility = Visibility.Collapsed;
        [ObservableProperty] private Visibility vehiclesVisibility    = Visibility.Collapsed;
        [ObservableProperty] private Visibility servicesVisibility    = Visibility.Collapsed;
        [ObservableProperty] private Visibility adminVisibility       = Visibility.Collapsed;
        [ObservableProperty] private Visibility customerPanelVisibility = Visibility.Collapsed;
        [ObservableProperty] private Visibility salesPanelVisibility    = Visibility.Collapsed;
        [ObservableProperty] private Visibility reportsVisibility       = Visibility.Collapsed;
        [ObservableProperty] private Visibility salesRecordsVisibility = Visibility.Collapsed;

        public Type? StartupPageType { get; private set; }
        public string? AccessDeniedMessage { get; private set; }

        public event Action? LogoutRequested;
        public event Action<string>? ShowProfileRequested;

        public void Initialize(AppUser loggedIn)
        {
            LoggedInUser = loggedIn;
            UserFullName = $"{loggedIn.FirstName} {loggedIn.LastName}";
            RoleName     = loggedIn.Role?.RoleName ?? "—";

            ConfigureForRole(loggedIn.Role?.RoleName);
        }

        private void ConfigureForRole(string? roleName)
        {
            switch (roleName)
            {
                case "Administrator":
                    AdminVisibility = Visibility.Visible;
                    StartupPageType = typeof(AdminPage);
                    break;

                case "Kierownik":
                    DashboardVisibility   = Visibility.Visible;
                    ClientsVisibility     = Visibility.Visible;
                    CreateOrderVisibility = Visibility.Visible;
                    VehiclesVisibility    = Visibility.Visible;
                    ServicesVisibility    = Visibility.Visible;
                    AdminVisibility       = Visibility.Visible;
                    SalesPanelVisibility  = Visibility.Visible;
                    SalesRecordsVisibility = Visibility.Visible;
                    ReportsVisibility     = Visibility.Visible;
                    StartupPageType       = typeof(DashboardPage);
                    break;

                case "Sprzedawca":
                    ClientsVisibility     = Visibility.Visible;
                    CreateOrderVisibility = Visibility.Visible;
                    VehiclesVisibility    = Visibility.Visible;
                    SalesPanelVisibility  = Visibility.Visible;
                    StartupPageType       = typeof(VehiclesPage);
                    break;

                case "Serwisant":
                    ServicesVisibility = Visibility.Visible;
                    SalesRecordsVisibility = Visibility.Visible;
                    StartupPageType    = typeof(ServicesPage);
                    break;

                case "Klient":
                    CustomerPanelVisibility = Visibility.Visible;
                    StartupPageType = typeof(CustomerPanelPage);
                    break;

                default:
                    AccessDeniedMessage = $"Nieznana rola: '{roleName}'. Brak dostępu do systemu.";
                    break;
            }
        }

        [RelayCommand]
        private void Profile()
        {
            var u = LoggedInUser;
            var role = u.Role?.RoleName ?? "—";
            var info = $"Imię: {u.FirstName}\nNazwisko: {u.LastName}\nE-mail: {u.Email}\nRola: {role}";
            ShowProfileRequested?.Invoke(info);
        }

        [RelayCommand]
        private void Logout()
        {
            LogoutRequested?.Invoke();
        }
    }
}

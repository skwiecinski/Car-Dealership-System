using System;
using System.Windows;
using SalonSamochodowy.Entities;
using Wpf.Ui.Controls;

namespace SalonSamochodowy
{
    public partial class MainWindow : FluentWindow
    {
        public AppUser LoggedInUser { get; private set; }

        public MainWindow(AppUser loggedIn)
        {
            InitializeComponent();

            LoggedInUser = loggedIn;

            TxtUserName.Text = $"{loggedIn.FirstName} {loggedIn.LastName}";
            TxtUserRole.Text = loggedIn.Role?.RoleName ?? "—";

            ConfigureForRole(loggedIn.Role?.RoleName);
        }

        private void ConfigureForRole(string? roleName)
        {
            NavDashboard.Visibility   = Visibility.Collapsed;
            NavClients.Visibility     = Visibility.Collapsed;
            NavCreateOrder.Visibility = Visibility.Collapsed;
            NavVehicles.Visibility    = Visibility.Collapsed;
            NavServices.Visibility    = Visibility.Collapsed;

            NavProfile.Visibility = Visibility.Visible;
            NavLogout.Visibility  = Visibility.Visible;
            NavAdmin.Visibility   = Visibility.Collapsed;

            Type? startupPage = null;

            switch (roleName)
            {
                case "Administrator":
                    NavAdmin.Visibility = Visibility.Visible;
                    startupPage = typeof(AdminPage);
                    break;

                case "Kierownik":
                    NavDashboard.Visibility   = Visibility.Visible;
                    NavClients.Visibility     = Visibility.Visible;
                    NavCreateOrder.Visibility = Visibility.Visible;
                    NavVehicles.Visibility    = Visibility.Visible;
                    NavServices.Visibility    = Visibility.Visible;
                    NavAdmin.Visibility       = Visibility.Visible;
                    startupPage = typeof(DashboardPage);
                    break;

                case "Sprzedawca":
                    NavClients.Visibility     = Visibility.Visible;
                    NavCreateOrder.Visibility = Visibility.Visible;
                    NavVehicles.Visibility    = Visibility.Visible;
                    startupPage = typeof(VehiclesPage);
                    break;

                case "Serwisant":
                    NavServices.Visibility = Visibility.Visible;
                    startupPage = typeof(ServicesPage);
                    break;

                case "Klient":
                    System.Windows.MessageBox.Show(
                        "Klienci nie mają dostępu do panelu pracowniczego.",
                        "Brak uprawnień",
                        System.Windows.MessageBoxButton.OK,
                        System.Windows.MessageBoxImage.Warning);
                    Application.Current.Shutdown();
                    return;

                default:
                    System.Windows.MessageBox.Show(
                        $"Nieznana rola: '{roleName}'. Brak dostępu do systemu.",
                        "Brak uprawnień",
                        System.Windows.MessageBoxButton.OK,
                        System.Windows.MessageBoxImage.Error);
                    Application.Current.Shutdown();
                    return;
            }

            if (startupPage != null)
            {
                RootNavigation.Loaded += (s, e) => RootNavigation.Navigate(startupPage);
            }
        }

        private void Menu_Profile_Click(object sender, RoutedEventArgs e)
        {
            var u = LoggedInUser;
            var role = u.Role?.RoleName ?? "—";
            System.Windows.MessageBox.Show(
                $"Imię: {u.FirstName}\nNazwisko: {u.LastName}\nE-mail: {u.Email}\nRola: {role}",
                "Mój profil",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Information);
        }

        private void Menu_Logout_Click(object sender, RoutedEventArgs e)
        {
            var login = new LoginWindow();
            login.Show();
            Close();
        }
    }
}

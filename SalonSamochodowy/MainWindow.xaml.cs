using System;
using System.Windows;
using SalonSamochodowy.Entities;
using Wpf.Ui.Controls;

namespace SalonSamochodowy
{
    public partial class MainWindow : FluentWindow
    {
        // Zalogowany uzytkownik - przekazany z LoginWindow
        public AppUser LoggedInUser { get; private set; }

        public MainWindow(AppUser loggedIn)
        {
            InitializeComponent();

            LoggedInUser = loggedIn;

            // Wypelnienie panelu profilu w topbarze
            TxtUserName.Text = $"{loggedIn.FirstName} {loggedIn.LastName}";
            TxtUserRole.Text = loggedIn.Role?.RoleName ?? "—";

            // Konfiguracja menu + strony startowej w zaleznosci od roli
            ConfigureForRole(loggedIn.Role?.RoleName);
        }

        /// <summary>
        /// Pokazuje/ukrywa pozycje sidebara i wybiera strone startowa
        /// w zaleznosci od roli zalogowanego uzytkownika.
        ///
        /// Mapa uprawnien zgodnie z dokumentem 'spotkanie 01.04.2026.pdf':
        ///   Administrator - tylko Administracja
        ///   Kierownik     - wszystko (dziedziczy Sprzedajacego + raporty/dashboard + Uslugi Serwisowe wglad)
        ///   Sprzedawca    - Klienci, Dodaj zamowienie, Pojazdy (BEZ Dashboardu, BEZ Serwisu, BEZ Administracji)
        ///   Serwisant     - tylko Uslugi Serwisowe
        ///   Klient        - brak dostepu (rola w bazie ale nie pracownik)
        /// </summary>
        private void ConfigureForRole(string? roleName)
        {
            // Domyslnie wszystko ukryte - wlaczymy tylko to co ma byc dostepne
            NavDashboard.Visibility   = Visibility.Collapsed;
            NavClients.Visibility     = Visibility.Collapsed;
            NavCreateOrder.Visibility = Visibility.Collapsed;
            NavVehicles.Visibility    = Visibility.Collapsed;
            NavServices.Visibility    = Visibility.Collapsed;
            NavAdmin.Visibility       = Visibility.Collapsed;

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

            // Nawigacja do strony startowej po zaladowaniu NavigationView
            if (startupPage != null)
            {
                RootNavigation.Loaded += (s, e) => RootNavigation.Navigate(startupPage);
            }
        }

        // --- OBSLUGA OTWIERANIA MENU PROFILU ---
        private void ProfileBtn_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as System.Windows.Controls.Button;
            if (btn != null && btn.ContextMenu != null)
            {
                btn.ContextMenu.PlacementTarget = btn;
                btn.ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
                btn.ContextMenu.IsOpen = true;
            }
        }

        // --- AKCJE W MENU PROFILU ---

        private void Menu_Profile_Click(object sender, RoutedEventArgs e)
        {
            // TODO: osobny widok profilu
            RootNavigation.Navigate(typeof(AdminPage));
        }

        private void Menu_Settings_Click(object sender, RoutedEventArgs e)
        {
            // TODO: osobny widok ustawien
            RootNavigation.Navigate(typeof(AdminPage));
        }

        private void Menu_Logout_Click(object sender, RoutedEventArgs e)
        {
            // Powrot do ekranu logowania
            var login = new LoginWindow();
            login.Show();
            Close();
        }
    }
}

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

            // Domyslna strona po starcie
            RootNavigation.Loaded += (s, e) =>
            {
                RootNavigation.Navigate(typeof(DashboardPage));
            };
        }

        // --- OBSLUGA OTWIERANIA MENU PROFILU (lewy przycisk myszy) ---
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

        // --- AKCJE W MENU ---

        private void Menu_Profile_Click(object sender, RoutedEventArgs e)
        {
            RootNavigation.Navigate(typeof(AdminPage));
        }

        private void Menu_Settings_Click(object sender, RoutedEventArgs e)
        {
            RootNavigation.Navigate(typeof(AdminPage));
        }

        private void Menu_Logout_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.MessageBox.Show("Zostałeś pomyślnie wylogowany.", "Wylogowywanie", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
            Application.Current.Shutdown();
        }
    }
}

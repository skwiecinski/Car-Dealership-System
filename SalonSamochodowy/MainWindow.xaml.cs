using System.Windows;
using Wpf.Ui.Controls;

namespace SalonSamochodowy
{
    public partial class MainWindow : FluentWindow
    {
        public MainWindow()
        {
            InitializeComponent();

            RootNavigation.Loaded += (s, e) =>
            {
                RootNavigation.Navigate(typeof(DashboardPage));
            };
        }

        // --- OBSŁUGA OTWIERANIA MENU ---
        // To wymusza rozwinięcie menu po kliknięciu profilu lewym przyciskiem myszy!
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

        // --- OBSŁUGA AKCJI W MENU ---

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
using System.Net.Mail;
using System.Windows;
using SalonSamochodowy.Services;
using Wpf.Ui.Controls;

namespace SalonSamochodowy
{
    public partial class LoginWindow : FluentWindow
    {
        // Serwis autentykacji (sprawdza email + haslo w bazie)
        private readonly AuthService _authService = new AuthService();

        public LoginWindow()
        {
            InitializeComponent();
        }

        private async void LoginBtn_Click(object sender, RoutedEventArgs e)
        {
            var email    = TxtUsername.Text.Trim();
            var password = TxtPassword.Password;

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                System.Windows.MessageBox.Show(
                    "Podaj adres e-mail i hasło.",
                    "Logowanie",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Warning);
                return;
            }

            if (!IsValidEmail(email))
            {
                System.Windows.MessageBox.Show(
                    "Podany adres e-mail jest niepoprawny. Przykład poprawnego adresu: nazwa@domena.pl",
                    "Logowanie",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Warning);
                TxtUsername.Focus();
                return;
            }

            var loggedUser = await _authService.LoginAsync(email, password);
            if (loggedUser == null)
            {
                System.Windows.MessageBox.Show(
                    "Niepoprawny e-mail lub hasło.",
                    "Logowanie",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Warning);
                TxtUsername.Focus();
                return;
            }

            var main = new MainWindow(loggedUser);
            main.Show();
            Close();
        }

        private void ExitBtn_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private static bool IsValidEmail(string email)
        {
            if (!MailAddress.TryCreate(email, out var address))
                return false;

            // wymuszamy cos@cos.cos bo MailAddress przepuszcza a@b bez kropki w domenie
            var domain = address.Host;
            var dotIndex = domain.IndexOf('.');
            return dotIndex > 0 && dotIndex < domain.Length - 1;
        }
    }
}

using System.Net.Mail;
using System.Windows;
using Wpf.Ui.Controls;

namespace SalonSamochodowy
{
    public partial class LoginWindow : FluentWindow
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            var email = UsernameTextBox.Text.Trim();
            var password = PasswordBox.Password;

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                System.Windows.MessageBox.Show(
                    "Podaj adres e-mail użytkownika i hasło.",
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
                UsernameTextBox.Focus();
                return;
            }

            var main = new MainWindow();
            main.Show();
            Close();
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

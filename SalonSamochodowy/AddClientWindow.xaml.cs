using System.Net.Mail;
using System.Windows;
using Wpf.Ui.Controls;

namespace SalonSamochodowy
{
    public partial class AddClientWindow : FluentWindow
    {
        // Wynik dialogu - wypelniony po kliknieciu Zapisz, null gdy anulowano.
        // ClientsPage moze go odebrac i dodac do swojej listy.
        public ClientModel? Result { get; private set; }

        public AddClientWindow()
        {
            InitializeComponent();
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            var fullName = TxtFullName.Text.Trim();
            var phone    = TxtPhone.Text.Trim();
            var taxId    = TxtTaxId.Text.Trim();
            var email    = TxtEmail.Text.Trim();
            var isCompany = ChkCompany.IsChecked == true;

            // Walidacja podstawowa
            if (string.IsNullOrWhiteSpace(fullName))
            {
                System.Windows.MessageBox.Show("Podaj imię i nazwisko lub nazwę firmy.", "Nowy klient",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                TxtFullName.Focus();
                return;
            }

            if (isCompany && string.IsNullOrWhiteSpace(taxId))
            {
                System.Windows.MessageBox.Show("Dla klienta firmowego NIP jest wymagany.", "Nowy klient",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                TxtTaxId.Focus();
                return;
            }

            if (!string.IsNullOrWhiteSpace(email) && !IsValidEmail(email))
            {
                System.Windows.MessageBox.Show("Podany adres e-mail jest niepoprawny.", "Nowy klient",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                TxtEmail.Focus();
                return;
            }

            Result = new ClientModel
            {
                FullName    = fullName,
                PhoneNumber = phone,
                TaxId       = string.IsNullOrWhiteSpace(taxId) ? "-" : taxId,
                Email       = email,
                IsCompany   = isCompany
            };

            DialogResult = true;
            Close();
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
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

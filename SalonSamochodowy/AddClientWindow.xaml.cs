using System;
using System.Linq;
using System.Net.Mail;
using System.Windows;
using SalonSamochodowy.Entities;
using SalonSamochodowy.Repositories;
using Wpf.Ui.Controls;

namespace SalonSamochodowy
{
    public partial class AddClientWindow : FluentWindow
    {
        public ClientModel? Result { get; private set; }

        public AddClientWindow()
        {
            InitializeComponent();
        }

        private async void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            var fullName  = TxtFullName.Text.Trim();
            var phone     = TxtPhone.Text.Trim();
            var taxId     = TxtTaxId.Text.Trim();
            var email     = TxtEmail.Text.Trim();
            var isCompany = ChkCompany.IsChecked == true;

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

            if (string.IsNullOrWhiteSpace(email))
            {
                System.Windows.MessageBox.Show("E-mail jest wymagany.", "Nowy klient",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                TxtEmail.Focus();
                return;
            }

            if (!IsValidEmail(email))
            {
                System.Windows.MessageBox.Show("Podany adres e-mail jest niepoprawny.", "Nowy klient",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                TxtEmail.Focus();
                return;
            }

            try
            {
                using (var ctx = new AppDbContext())
                using (var uow = new UnitOfWork(ctx))
                {
                    var existingUsers = await uow.AppUsers.FindAsync(u => u.Email == email);
                    if (existingUsers.Any())
                    {
                        System.Windows.MessageBox.Show("Użytkownik z takim adresem e-mail już istnieje.",
                            "Nowy klient", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                        TxtEmail.Focus();
                        return;
                    }

                    var rolesKlient = await uow.AppRoles.FindAsync(r => r.RoleName == "Klient");
                    var roleKlient = rolesKlient.FirstOrDefault();
                    if (roleKlient == null)
                    {
                        System.Windows.MessageBox.Show("Rola 'Klient' nie istnieje w bazie. Zgłoś to backendowi.",
                            "Nowy klient", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                        return;
                    }

                    string firstName, lastName;
                    if (isCompany)
                    {
                        firstName = fullName.Length > 50 ? fullName.Substring(0, 50) : fullName;
                        lastName  = "";
                    }
                    else
                    {
                        var parts = fullName.Split(' ', 2);
                        firstName = parts[0];
                        lastName  = parts.Length > 1 ? parts[1] : "";
                    }

                    var newUser = new AppUser
                    {
                        FirstName    = firstName,
                        LastName     = lastName,
                        Email        = email,
                        PasswordHash = "",
                        RoleID       = roleKlient.RoleID,
                        BirthDate    = DateTime.Today
                    };
                    await uow.AppUsers.AddAsync(newUser);
                    await uow.CompleteAsync();

                    var newClient = new Client
                    {
                        UserID = newUser.UserID,
                        NIP    = string.IsNullOrWhiteSpace(taxId) ? null : taxId,
                        Phone  = string.IsNullOrWhiteSpace(phone) ? "" : phone
                    };
                    await uow.Clients.AddAsync(newClient);
                    await uow.CompleteAsync();

                    Result = new ClientModel
                    {
                        FullName    = fullName,
                        PhoneNumber = phone,
                        TaxId       = string.IsNullOrWhiteSpace(taxId) ? "-" : taxId,
                        Email       = email,
                        IsCompany   = isCompany
                    };
                }

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(
                    $"Nie udało się zapisać klienta do bazy:\n{ex.Message}\n\n{ex.InnerException?.Message}",
                    "Nowy klient",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error);
            }
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

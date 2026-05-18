using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using SalonSamochodowy.Entities;
using SalonSamochodowy.Repositories;

namespace SalonSamochodowy
{
    public partial class AdminPage : Page
    {
        public ObservableCollection<AccountRow> Accounts { get; set; } = new();

        public AdminPage()
        {
            InitializeComponent();
            DataContext = this;

            Loaded += async (s, e) => await LoadAccountsAsync();
        }

        private async Task LoadAccountsAsync()
        {
            try
            {
                using var ctx = new AppDbContext();
                using var uow = new UnitOfWork(ctx);

                Accounts.Clear();
                var users = (await uow.AppUsers.GetAllAsync()).ToList();
                foreach (var u in users.OrderBy(x => x.RoleID).ThenBy(x => x.LastName))
                {
                    var role = await uow.AppRoles.GetByIdAsync(u.RoleID);
                    Accounts.Add(new AccountRow
                    {
                        FullName = $"{u.FirstName} {u.LastName}".Trim(),
                        Email    = u.Email,
                        RoleName = role?.RoleName ?? "—"
                    });
                }
                TxtAccountsCount.Text = $"{Accounts.Count} kont w systemie";
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(
                    $"Nie udało się załadować kont:\n{ex.Message}",
                    "Administracja",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error);
            }
        }

        private async void RegisterEmployee_Click(object sender, RoutedEventArgs e)
        {
            var firstName = TxtFirstName.Text.Trim();
            var lastName  = TxtLastName.Text.Trim();
            var email     = TxtEmail.Text.Trim();
            var password  = TxtPassword.Password;
            var roleName  = (CbRole.SelectedItem as ComboBoxItem)?.Content?.ToString();

            if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
            {
                System.Windows.MessageBox.Show("Podaj imię i nazwisko.", "Administracja",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                TxtFirstName.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(email) || !IsValidEmail(email))
            {
                System.Windows.MessageBox.Show("Podaj poprawny adres e-mail.", "Administracja",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                TxtEmail.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(password) || password.Length < 4)
            {
                System.Windows.MessageBox.Show("Hasło musi mieć minimum 4 znaki.", "Administracja",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                TxtPassword.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(roleName))
            {
                System.Windows.MessageBox.Show("Wybierz rolę.", "Administracja",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return;
            }

            try
            {
                using var ctx = new AppDbContext();
                using var uow = new UnitOfWork(ctx);

                var existing = await uow.AppUsers.FindAsync(u => u.Email == email);
                if (existing.Any())
                {
                    System.Windows.MessageBox.Show("Konto z takim adresem e-mail już istnieje.",
                        "Administracja", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                    TxtEmail.Focus();
                    return;
                }

                var role = (await uow.AppRoles.FindAsync(r => r.RoleName == roleName)).FirstOrDefault();
                if (role == null)
                {
                    System.Windows.MessageBox.Show($"Rola '{roleName}' nie istnieje w bazie.",
                        "Administracja", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                    return;
                }

                var newUser = new AppUser
                {
                    FirstName    = firstName,
                    LastName     = lastName,
                    Email        = email,
                    PasswordHash = password,
                    RoleID       = role.RoleID,
                    BirthDate    = DateTime.Today
                };
                await uow.AppUsers.AddAsync(newUser);
                await uow.CompleteAsync();

                if (roleName == "Sprzedawca" || roleName == "Serwisant" || roleName == "Kierownik")
                {
                    var salon = (await uow.Dealerships.GetAllAsync()).First();
                    await uow.Workers.AddAsync(new Worker
                    {
                        UserID            = newUser.UserID,
                        Payroll           = 0m,
                        EndOfContractDate = DateTime.Today.AddYears(2),
                        DealershipID      = salon.DealershipID
                    });
                    await uow.CompleteAsync();
                }

                System.Windows.MessageBox.Show(
                    $"Konto '{firstName} {lastName}' ({roleName}) zostało utworzone.",
                    "Administracja",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Information);

                TxtFirstName.Clear();
                TxtLastName.Clear();
                TxtEmail.Clear();
                TxtPassword.Password = "";
                CbRole.SelectedIndex = 0;

                await LoadAccountsAsync();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(
                    $"Nie udało się utworzyć konta:\n{ex.Message}\n\n{ex.InnerException?.Message}",
                    "Administracja",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error);
            }
        }

        private async void DeleteEmployee_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Wpf.Ui.Controls.Button btn || btn.Tag is not AccountRow row)
                return;

            if (row.RoleName == "Kierownik")
            {
                System.Windows.MessageBox.Show(
                    "Konto kierownika jest chronione i nie może zostać usunięte.",
                    "Administracja",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Warning);
                return;
            }

            var confirm = System.Windows.MessageBox.Show(
                $"Czy na pewno usunąć konto:\n\n{row.FullName}\n{row.Email}\nRola: {row.RoleName}\n\nOperacji nie można cofnąć.",
                "Usuń pracownika",
                System.Windows.MessageBoxButton.YesNo,
                System.Windows.MessageBoxImage.Warning);
            if (confirm != MessageBoxResult.Yes)
                return;

            try
            {
                using var ctx = new AppDbContext();
                using var uow = new UnitOfWork(ctx);

                var user = (await uow.AppUsers.FindAsync(u => u.Email == row.Email)).FirstOrDefault();
                if (user == null)
                {
                    System.Windows.MessageBox.Show("Konto nie istnieje w bazie.",
                        "Administracja", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                    return;
                }

                var mainWin = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
                if (mainWin?.LoggedInUser != null && mainWin.LoggedInUser.UserID == user.UserID)
                {
                    System.Windows.MessageBox.Show("Nie możesz usunąć własnego konta.",
                        "Administracja", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                    return;
                }

                var workers = await uow.Workers.FindAsync(w => w.UserID == user.UserID);
                foreach (var w in workers) uow.Workers.Delete(w);

                var clients = await uow.Clients.FindAsync(c => c.UserID == user.UserID);
                foreach (var c in clients) uow.Clients.Delete(c);

                uow.AppUsers.Delete(user);
                await uow.CompleteAsync();

                await LoadAccountsAsync();

                System.Windows.MessageBox.Show(
                    $"Konto {row.FullName} zostało usunięte.",
                    "Administracja",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(
                    $"Nie udało się usunąć konta:\n{ex.Message}\n\n{ex.InnerException?.Message}\n\n" +
                    "Możliwa przyczyna: użytkownik ma w bazie powiązane zamówienia lub zlecenia serwisowe.",
                    "Administracja",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error);
            }
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

    public class AccountRow
    {
        public string FullName { get; set; } = "";
        public string Email    { get; set; } = "";
        public string RoleName { get; set; } = "";
    }
}

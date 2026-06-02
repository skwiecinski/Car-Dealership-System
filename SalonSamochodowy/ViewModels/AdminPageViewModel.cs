using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SalonSamochodowy.Entities;
using SalonSamochodowy.Repositories;

namespace SalonSamochodowy.ViewModels
{
    public partial class AdminPageViewModel : ObservableObject
    {
        private readonly IUnitOfWork _uow;

        public AdminPageViewModel(IUnitOfWork uow)
        {
            _uow = uow;
        }
        public ObservableCollection<AccountRow> Accounts { get; } = new();
        public ObservableCollection<DealershipRow> Dealerships { get; } = new();
        public ObservableCollection<DealershipItem> AvailableDealerships { get; } = new();

        [ObservableProperty] private string firstName = "";
        [ObservableProperty] private string lastName = "";
        [ObservableProperty] private string email = "";
        [ObservableProperty] private string roleName = "Sprzedawca";
        [ObservableProperty] private DealershipItem? selectedDealershipForEmployee;
        [ObservableProperty] private string accountsCountText = "";

        [ObservableProperty] private string newSalonName = "";
        [ObservableProperty] private string newSalonAddress = "";
        [ObservableProperty] private string newSalonCity = "";
        [ObservableProperty] private string newSalonOwner = "";
        [ObservableProperty] private string dealershipsCountText = "";

        public event Action<string, MessageBoxImage>? ShowMessage;
        public event Func<string, bool>? ConfirmDelete;
        public event Func<string>? GetPassword;
        public event Action? ClearPassword;
        public event Func<int>? GetLoggedInUserId;

        public async Task LoadAllAsync()
        {
            await LoadDealershipsAsync();
            await LoadAccountsAsync();
        }

        public async Task LoadAccountsAsync()
        {
            try
            {
                var uow = _uow;

                Accounts.Clear();
                var users = (await uow.AppUsers.GetAllAsync()).ToList();
                foreach (var u in users.OrderBy(x => x.RoleID).ThenBy(x => x.LastName))
                {
                    var role = await uow.AppRoles.GetByIdAsync(u.RoleID);
                    var workers = await uow.Workers.FindAsync(w => w.UserID == u.UserID);
                    var worker = workers.FirstOrDefault();
                    Dealership? dealership = null;
                    if (worker != null)
                        dealership = await uow.Dealerships.GetByIdAsync(worker.DealershipID);

                    Accounts.Add(new AccountRow
                    {
                        FullName       = $"{u.FirstName} {u.LastName}".Trim(),
                        Email          = u.Email,
                        RoleName       = role?.RoleName ?? "—",
                        DealershipName = dealership?.Name ?? "—"
                    });
                }
                AccountsCountText = $"{Accounts.Count} kont w systemie";
            }
            catch (Exception ex)
            {
                ShowMessage?.Invoke($"Nie udało się załadować kont:\n{ex.Message}", MessageBoxImage.Error);
            }
        }

        public async Task LoadDealershipsAsync()
        {
            try
            {
                var uow = _uow;

                var prevSelectedId = SelectedDealershipForEmployee?.DealershipID;

                Dealerships.Clear();
                AvailableDealerships.Clear();

                var list = (await uow.Dealerships.GetAllAsync()).ToList();
                foreach (var d in list.OrderBy(d => d.City).ThenBy(d => d.Name))
                {
                    Dealerships.Add(new DealershipRow
                    {
                        DealershipID = d.DealershipID,
                        Name         = d.Name,
                        Address      = d.Address,
                        City         = d.City,
                        Owner        = d.Owner
                    });
                    AvailableDealerships.Add(new DealershipItem
                    {
                        DealershipID = d.DealershipID,
                        Display      = $"{d.Name} ({d.City})"
                    });
                }
                DealershipsCountText = $"{Dealerships.Count} salonów";

                if (prevSelectedId.HasValue)
                    SelectedDealershipForEmployee = AvailableDealerships.FirstOrDefault(x => x.DealershipID == prevSelectedId.Value);

                if (SelectedDealershipForEmployee == null && AvailableDealerships.Count > 0)
                    SelectedDealershipForEmployee = AvailableDealerships.First();
            }
            catch (Exception ex)
            {
                ShowMessage?.Invoke($"Nie udało się załadować salonów:\n{ex.Message}", MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task RegisterEmployeeAsync()
        {
            var fn = FirstName.Trim();
            var ln = LastName.Trim();
            var em = Email.Trim();
            var pw = GetPassword?.Invoke() ?? "";

            if (string.IsNullOrWhiteSpace(fn) || string.IsNullOrWhiteSpace(ln))
            {
                ShowMessage?.Invoke("Podaj imię i nazwisko.", MessageBoxImage.Warning);
                return;
            }
            if (string.IsNullOrWhiteSpace(em) || !IsValidEmail(em))
            {
                ShowMessage?.Invoke("Podaj poprawny adres e-mail.", MessageBoxImage.Warning);
                return;
            }
            if (string.IsNullOrWhiteSpace(pw) || pw.Length < 4)
            {
                ShowMessage?.Invoke("Hasło musi mieć minimum 4 znaki.", MessageBoxImage.Warning);
                return;
            }
            if (string.IsNullOrWhiteSpace(RoleName))
            {
                ShowMessage?.Invoke("Wybierz rolę.", MessageBoxImage.Warning);
                return;
            }

            var isWorkerRole = RoleName == "Sprzedawca" || RoleName == "Serwisant" || RoleName == "Kierownik";
            if (isWorkerRole && SelectedDealershipForEmployee == null)
            {
                ShowMessage?.Invoke("Wybierz salon, do którego pracownik ma zostać przypisany.", MessageBoxImage.Warning);
                return;
            }

            try
            {
                var uow = _uow;

                var existing = await uow.AppUsers.FindAsync(u => u.Email == em);
                if (existing.Any())
                {
                    ShowMessage?.Invoke("Konto z takim adresem e-mail już istnieje.", MessageBoxImage.Warning);
                    return;
                }

                var role = (await uow.AppRoles.FindAsync(r => r.RoleName == RoleName)).FirstOrDefault();
                if (role == null)
                {
                    ShowMessage?.Invoke($"Rola '{RoleName}' nie istnieje w bazie.", MessageBoxImage.Error);
                    return;
                }

                var newUser = new AppUser
                {
                    FirstName    = fn,
                    LastName     = ln,
                    Email        = em,
                    PasswordHash = pw,
                    RoleID       = role.RoleID,
                    BirthDate    = DateTime.Today
                };
                await uow.AppUsers.AddAsync(newUser);
                await uow.CompleteAsync();

                if (isWorkerRole)
                {
                    await uow.Workers.AddAsync(new Worker
                    {
                        UserID            = newUser.UserID,
                        Payroll           = 0m,
                        EndOfContractDate = DateTime.Today.AddYears(2),
                        DealershipID      = SelectedDealershipForEmployee!.DealershipID
                    });
                    await uow.CompleteAsync();
                }

                ShowMessage?.Invoke($"Konto '{fn} {ln}' ({RoleName}) zostało utworzone.", MessageBoxImage.Information);

                FirstName = "";
                LastName = "";
                Email = "";
                RoleName = "Sprzedawca";
                ClearPassword?.Invoke();

                await LoadAccountsAsync();
            }
            catch (Exception ex)
            {
                ShowMessage?.Invoke($"Nie udało się utworzyć konta:\n{ex.Message}\n\n{ex.InnerException?.Message}", MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task DeleteEmployeeAsync(AccountRow? row)
        {
            if (row == null) return;

            if (row.RoleName == "Kierownik")
            {
                ShowMessage?.Invoke("Konto kierownika jest chronione i nie może zostać usunięte.", MessageBoxImage.Warning);
                return;
            }

            var confirmMsg = $"Czy na pewno usunąć konto:\n\n{row.FullName}\n{row.Email}\nRola: {row.RoleName}\nSalon: {row.DealershipName}\n\nOperacji nie można cofnąć.";
            if (ConfirmDelete?.Invoke(confirmMsg) != true) return;

            try
            {
                var uow = _uow;

                var user = (await uow.AppUsers.FindAsync(u => u.Email == row.Email)).FirstOrDefault();
                if (user == null)
                {
                    ShowMessage?.Invoke("Konto nie istnieje w bazie.", MessageBoxImage.Warning);
                    return;
                }

                var loggedId = GetLoggedInUserId?.Invoke() ?? -1;
                if (loggedId == user.UserID)
                {
                    ShowMessage?.Invoke("Nie możesz usunąć własnego konta.", MessageBoxImage.Warning);
                    return;
                }

                var workers = await uow.Workers.FindAsync(w => w.UserID == user.UserID);
                foreach (var w in workers) uow.Workers.Delete(w);

                var clients = await uow.Clients.FindAsync(c => c.UserID == user.UserID);
                foreach (var c in clients) uow.Clients.Delete(c);

                uow.AppUsers.Delete(user);
                await uow.CompleteAsync();

                await LoadAccountsAsync();
                ShowMessage?.Invoke($"Konto {row.FullName} zostało usunięte.", MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                ShowMessage?.Invoke(
                    $"Nie udało się usunąć konta:\n{ex.Message}\n\n{ex.InnerException?.Message}\n\n" +
                    "Możliwa przyczyna: użytkownik ma w bazie powiązane zamówienia lub zlecenia serwisowe.",
                    MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task RegisterDealershipAsync()
        {
            var name    = NewSalonName.Trim();
            var address = NewSalonAddress.Trim();
            var city    = NewSalonCity.Trim();
            var owner   = NewSalonOwner.Trim();

            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(address) ||
                string.IsNullOrWhiteSpace(city) || string.IsNullOrWhiteSpace(owner))
            {
                ShowMessage?.Invoke("Wypełnij wszystkie pola salonu (nazwa, adres, miasto, właściciel).", MessageBoxImage.Warning);
                return;
            }

            try
            {
                var uow = _uow;

                var existing = await uow.Dealerships.FindAsync(d => d.Name == name && d.City == city);
                if (existing.Any())
                {
                    ShowMessage?.Invoke($"Salon '{name}' w mieście '{city}' już istnieje.", MessageBoxImage.Warning);
                    return;
                }

                await uow.Dealerships.AddAsync(new Dealership
                {
                    Name    = name,
                    Address = address,
                    City    = city,
                    Owner   = owner
                });
                await uow.CompleteAsync();

                ShowMessage?.Invoke($"Salon '{name}' ({city}) został dodany.", MessageBoxImage.Information);

                NewSalonName    = "";
                NewSalonAddress = "";
                NewSalonCity    = "";
                NewSalonOwner   = "";

                await LoadDealershipsAsync();
            }
            catch (Exception ex)
            {
                ShowMessage?.Invoke($"Nie udało się dodać salonu:\n{ex.Message}\n\n{ex.InnerException?.Message}", MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task DeleteDealershipAsync(DealershipRow? row)
        {
            if (row == null) return;

            try
            {
                var uow = _uow;

                var workersHere = await uow.Workers.CountAsync(w => w.DealershipID == row.DealershipID);
                if (workersHere > 0)
                {
                    ShowMessage?.Invoke(
                        $"Nie można usunąć salonu '{row.Name}' — przypisanych jest {workersHere} pracowników.\n" +
                        "Najpierw przenieś pracowników do innego salonu.",
                        MessageBoxImage.Warning);
                    return;
                }

                var confirmMsg = $"Czy na pewno usunąć salon:\n\n{row.Name}\n{row.Address}, {row.City}\nWłaściciel: {row.Owner}\n\nOperacji nie można cofnąć.";
                if (ConfirmDelete?.Invoke(confirmMsg) != true) return;

                var dealership = await uow.Dealerships.GetByIdAsync(row.DealershipID);
                if (dealership == null)
                {
                    ShowMessage?.Invoke("Salon nie istnieje w bazie.", MessageBoxImage.Warning);
                    return;
                }

                uow.Dealerships.Delete(dealership);
                await uow.CompleteAsync();

                await LoadDealershipsAsync();
                ShowMessage?.Invoke($"Salon '{row.Name}' został usunięty.", MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                ShowMessage?.Invoke(
                    $"Nie udało się usunąć salonu:\n{ex.Message}\n\n{ex.InnerException?.Message}\n\n" +
                    "Możliwa przyczyna: salon ma powiązane pojazdy lub zamówienia.",
                    MessageBoxImage.Error);
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
        public string FullName       { get; set; } = "";
        public string Email          { get; set; } = "";
        public string RoleName       { get; set; } = "";
        public string DealershipName { get; set; } = "";
    }

    public class DealershipRow
    {
        public int    DealershipID { get; set; }
        public string Name         { get; set; } = "";
        public string Address      { get; set; } = "";
        public string City         { get; set; } = "";
        public string Owner        { get; set; } = "";
    }

    public class DealershipItem
    {
        public int    DealershipID { get; set; }
        public string Display      { get; set; } = "";
        public override string ToString() => Display;
    }
}

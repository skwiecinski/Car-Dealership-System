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
using Microsoft.Extensions.DependencyInjection;

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
        [ObservableProperty] private string roleName = RoleNames.Sprzedawca;
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
            await LoadDictionariesAsync();
        }

        public ObservableCollection<VehicleModel> DictModels { get; } = new();
        public ObservableCollection<Feature> DictFeatures { get; } = new();
        public ObservableCollection<Engine> DictEngines { get; } = new();
        public ObservableCollection<TrimLevel> DictTrims { get; } = new();

        public string[] FeatureCategories { get; } = new[] { "Wygląd", "Akcesoria", "Bezpieczeństwo", "Komfort", "Wnętrze", "Multimedia", "Usługa" };

        [ObservableProperty] private string newModelBrand = "";
        [ObservableProperty] private string newModelName = "";

        [ObservableProperty] private string newFeatureName = "";
        [ObservableProperty] private string newFeatureCategory = "Akcesoria";

        [ObservableProperty] private string newEngineBrand = "";
        [ObservableProperty] private string newEngineName = "";
        [ObservableProperty] private string newEngineSize = "";
        [ObservableProperty] private int? newEnginePower;
        [ObservableProperty] private decimal? newEnginePrice;

        [ObservableProperty] private VehicleModel? newTrimSelectedModel;
        [ObservableProperty] private string newTrimName = "";
        [ObservableProperty] private decimal? newTrimBasePrice;

        public async Task LoadDictionariesAsync()
        {
            try
            {
                var uow = _uow;

                DictModels.Clear();
                var models = await uow.VehicleModels.GetAllAsync();
                foreach (var m in models.OrderBy(x => x.Brand).ThenBy(x => x.ModelName))
                    DictModels.Add(m);

                DictFeatures.Clear();
                var features = await uow.Features.GetAllAsync();
                foreach (var f in features.OrderBy(x => x.Category).ThenBy(x => x.FeatureName))
                    DictFeatures.Add(f);

                DictEngines.Clear();
                var engines = await uow.Engines.GetAllAsync();
                foreach (var e in engines.OrderBy(x => x.Brand).ThenBy(x => x.Power))
                    DictEngines.Add(e);

                DictTrims.Clear();
                var trims = await uow.TrimLevels.GetAllWithIncludesAsync(t => t.Model);
                foreach (var t in trims.OrderBy(x => x.Model.Brand).ThenBy(x => x.Model.ModelName).ThenBy(x => x.BasePrice))
                    DictTrims.Add(t);
            }
            catch (Exception ex)
            {
                ShowMessage?.Invoke($"Nie udało się załadować słowników:\n{ex.Message}", MessageBoxImage.Error);
            }
        }

        public async Task LoadAccountsAsync()
        {
            try
            {
                var uow = _uow;

                Accounts.Clear();
                var users = (await uow.AppUsers.GetAllWithIncludesAsync(u => u.Role)).ToList();
                foreach (var u in users.OrderBy(x => x.RoleID).ThenBy(x => x.LastName))
                {
                    var role = u.Role;
                    var workers = await uow.Workers.FindWithIncludesAsync(w => w.UserID == u.UserID, w => w.Dealership);
                    var worker = workers.FirstOrDefault();
                    Dealership? dealership = worker?.Dealership;

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

            var isWorkerRole = RoleName == RoleNames.Sprzedawca || RoleName == RoleNames.Serwisant || RoleName == RoleNames.Kierownik;
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
                RoleName = RoleNames.Sprzedawca;
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

            if (row.RoleName == RoleNames.Kierownik)
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

        [RelayCommand]
        private async Task AddVehicleModelAsync()
        {
            var brand = NewModelBrand.Trim();
            var name = NewModelName.Trim();
            if (string.IsNullOrWhiteSpace(brand) || string.IsNullOrWhiteSpace(name))
            {
                ShowMessage?.Invoke("Wypełnij markę i nazwę modelu.", MessageBoxImage.Warning);
                return;
            }
            try
            {
                var catalogService = ((App)Application.Current).Services.GetRequiredService<SalonSamochodowy.Services.ICatalogService>();
                await catalogService.CreateVehicleModelAsync(new VehicleModel { Brand = brand, ModelName = name });
                NewModelBrand = "";
                NewModelName = "";
                await LoadDictionariesAsync();
                ShowMessage?.Invoke("Model został dodany.", MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                ShowMessage?.Invoke($"Błąd:\n{ex.Message}", MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task DeleteVehicleModelAsync(VehicleModel? model)
        {
            if (model == null) return;
            if (ConfirmDelete?.Invoke($"Usunąć model {model.Brand} {model.ModelName}?") != true) return;
            try
            {
                var catalogService = ((App)Application.Current).Services.GetRequiredService<SalonSamochodowy.Services.ICatalogService>();
                await catalogService.DeleteVehicleModelAsync(model.ModelID);
                await LoadDictionariesAsync();
                ShowMessage?.Invoke("Model usunięty.", MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                ShowMessage?.Invoke($"Błąd:\n{ex.Message}", MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task AddFeatureAsync()
        {
            var name = NewFeatureName.Trim();
            var cat = NewFeatureCategory;
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(cat))
            {
                ShowMessage?.Invoke("Wypełnij nazwę i kategorię opcji.", MessageBoxImage.Warning);
                return;
            }
            try
            {
                var catalogService = ((App)Application.Current).Services.GetRequiredService<SalonSamochodowy.Services.ICatalogService>();
                await catalogService.CreateFeatureAsync(new Feature { FeatureName = name, Category = cat });
                NewFeatureName = "";
                await LoadDictionariesAsync();
                ShowMessage?.Invoke("Opcja dodana.", MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                ShowMessage?.Invoke($"Błąd:\n{ex.Message}", MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task DeleteFeatureAsync(Feature? feature)
        {
            if (feature == null) return;
            if (ConfirmDelete?.Invoke($"Usunąć opcję {feature.FeatureName}?") != true) return;
            try
            {
                var catalogService = ((App)Application.Current).Services.GetRequiredService<SalonSamochodowy.Services.ICatalogService>();
                await catalogService.DeleteFeatureAsync(feature.FeatureID);
                await LoadDictionariesAsync();
                ShowMessage?.Invoke("Opcja usunięta.", MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                ShowMessage?.Invoke($"Błąd:\n{ex.Message}", MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task AddEngineAsync()
        {
            var brand = NewEngineBrand.Trim();
            var name = NewEngineName.Trim();
            var size = NewEngineSize.Trim();
            if (string.IsNullOrWhiteSpace(brand) || string.IsNullOrWhiteSpace(name) || !NewEnginePower.HasValue || !NewEnginePrice.HasValue)
            {
                ShowMessage?.Invoke("Wypełnij markę, nazwę silnika, moc oraz cenę.", MessageBoxImage.Warning);
                return;
            }
            try
            {
                var catalogService = ((App)Application.Current).Services.GetRequiredService<SalonSamochodowy.Services.ICatalogService>();
                await catalogService.CreateEngineAsync(new Engine 
                { 
                    Brand = brand, EngineName = name, EngineSize = size, 
                    Power = NewEnginePower.Value, Price = NewEnginePrice.Value 
                });
                NewEngineBrand = "";
                NewEngineName = "";
                NewEngineSize = "";
                NewEnginePower = null;
                NewEnginePrice = null;
                await LoadDictionariesAsync();
                ShowMessage?.Invoke("Silnik dodany.", MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                ShowMessage?.Invoke($"Błąd:\n{ex.Message}", MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task DeleteEngineAsync(Engine? engine)
        {
            if (engine == null) return;
            if (ConfirmDelete?.Invoke($"Usunąć silnik {engine.EngineName}?") != true) return;
            try
            {
                var catalogService = ((App)Application.Current).Services.GetRequiredService<SalonSamochodowy.Services.ICatalogService>();
                await catalogService.DeleteEngineAsync(engine.EngineID);
                await LoadDictionariesAsync();
                ShowMessage?.Invoke("Silnik usunięty.", MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                ShowMessage?.Invoke($"Błąd:\n{ex.Message}", MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task AddTrimLevelAsync()
        {
            var name = NewTrimName.Trim();
            if (NewTrimSelectedModel == null || string.IsNullOrWhiteSpace(name) || !NewTrimBasePrice.HasValue)
            {
                ShowMessage?.Invoke("Wybierz model, wpisz nazwę wersji i cenę bazową.", MessageBoxImage.Warning);
                return;
            }
            try
            {
                var catalogService = ((App)Application.Current).Services.GetRequiredService<SalonSamochodowy.Services.ICatalogService>();
                await catalogService.CreateTrimLevelAsync(new TrimLevel 
                { 
                    ModelID = NewTrimSelectedModel.ModelID, 
                    TrimName = name, 
                    BasePrice = NewTrimBasePrice.Value 
                });
                NewTrimName = "";
                NewTrimBasePrice = null;
                await LoadDictionariesAsync();
                ShowMessage?.Invoke("Wersja wyposażenia dodana.", MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                ShowMessage?.Invoke($"Błąd:\n{ex.Message}", MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task DeleteTrimLevelAsync(TrimLevel? trim)
        {
            if (trim == null) return;
            if (ConfirmDelete?.Invoke($"Usunąć wersję {trim.TrimName} dla modelu {trim.Model?.ModelName}?") != true) return;
            try
            {
                var catalogService = ((App)Application.Current).Services.GetRequiredService<SalonSamochodowy.Services.ICatalogService>();
                await catalogService.DeleteTrimLevelAsync(trim.TrimID);
                await LoadDictionariesAsync();
                ShowMessage?.Invoke("Wersja usunięta.", MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                ShowMessage?.Invoke($"Błąd:\n{ex.Message}", MessageBoxImage.Error);
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

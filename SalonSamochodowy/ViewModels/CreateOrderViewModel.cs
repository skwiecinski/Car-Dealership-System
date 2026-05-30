namespace SalonSamochodowy;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SalonSamochodowy.Entities;
using SalonSamochodowy.Repositories;

public partial class CreateOrderViewModel : ObservableObject
{
    [ObservableProperty] private bool czyIstniejacyKlient = true;

    public ObservableCollection<KlientItem> ListaKlientow { get; } = new();
    [ObservableProperty] private KlientItem? wybranyKlient;

    [ObservableProperty] private string nowyImie = "";
    [ObservableProperty] private string nowyNazwisko = "";
    [ObservableProperty] private string nowyEmail = "";
    [ObservableProperty] private string nowyTelefon = "";
    [ObservableProperty] private string nowyNIP = "";

    public ObservableCollection<string> Marki { get; } = new();
    [ObservableProperty] private string? wybranaMarka;

    public ObservableCollection<ModelItem> Modele { get; } = new();
    [ObservableProperty] private ModelItem? wybranyModel;

    public ObservableCollection<TrimItem> Wersje { get; } = new();
    [ObservableProperty] private TrimItem? wybranaWersja;

    public ObservableCollection<EngineItem> Silniki { get; } = new();
    [ObservableProperty] private EngineItem? wybranySilnik;

    [ObservableProperty] private string kolor = "";
    [ObservableProperty] private string vIN = "";
    [ObservableProperty] private bool czyUzywany = false;
    [ObservableProperty] private int przebieg = 0;

    public ObservableCollection<DodatkowaOpcja> DodatkoweOpcje { get; } = new();

    [ObservableProperty] private DateTime dataZamowienia = DateTime.Today;

    public ObservableCollection<SprzedawcaItem> ListaSprzedawcow { get; } = new();
    [ObservableProperty] private SprzedawcaItem? wybranySprzedawca;

    public List<string> FormyPlatnosci { get; } = new() { "Gotówka", "Kredyt", "Leasing" };
    [ObservableProperty] private string? wybranaFormaPlatnosci;

    [ObservableProperty] private decimal cenaFinalna = 0m;

    public List<string> StatusyZamowienia { get; } = new() { "Nowe", "W realizacji", "Zrealizowane", "Anulowane" };
    [ObservableProperty] private string wybranyStatus = "Nowe";

    [ObservableProperty] private string uwagi = "";

    public event Action<string>? ShowInfo;
    public event Action<string>? ShowError;
    public event Action<string>? ShowWarning;

    public ObservableCollection<SerwisantItem> ListaSerwisantow { get; } = new();
    [ObservableProperty] private SerwisantItem? wybranySerwisant;

    public CreateOrderViewModel() { }

    public async Task LoadFromDbAsync()
    {
        try
        {
            using var ctx = new AppDbContext();
            using var uow = new UnitOfWork(ctx);

            ListaKlientow.Clear();
            var allClients = await uow.Clients.GetAllAsync();
            foreach (var c in allClients)
            {
                var user = await uow.AppUsers.GetByIdAsync(c.UserID);
                if (user == null) continue;
                var fullName = string.IsNullOrWhiteSpace(user.LastName)
                    ? user.FirstName
                    : $"{user.FirstName} {user.LastName}";
                ListaKlientow.Add(new KlientItem
                {
                    ClientID = c.ClientID,
                    UserID = user.UserID,
                    FullName = fullName,
                    Email = user.Email
                });
            }

            Marki.Clear();
            var allModels = (await uow.VehicleModels.GetAllAsync()).ToList();
            foreach (var brand in allModels.Select(m => m.Brand).Distinct().OrderBy(b => b))
                Marki.Add(brand);

            Silniki.Clear();

            ListaSprzedawcow.Clear();
            ListaSerwisantow.Clear();

            var rSprzedawca = (await uow.AppRoles.FindAsync(r => r.RoleName == "Sprzedawca")).FirstOrDefault();
            var rKierownik = (await uow.AppRoles.FindAsync(r => r.RoleName == "Kierownik")).FirstOrDefault();
            var rSerwisant = (await uow.AppRoles.FindAsync(r => r.RoleName == "Serwisant")).FirstOrDefault();

            var sprzedawcaId = rSprzedawca?.RoleID ?? -1;
            var kierownikId = rKierownik?.RoleID ?? -1;
            var serwisantId = rSerwisant?.RoleID ?? -1;

            var validUsers = (await uow.AppUsers.FindAsync(
                u => u.RoleID == sprzedawcaId || u.RoleID == kierownikId || u.RoleID == serwisantId)).ToList();
            var userIds = validUsers.Select(u => u.UserID).ToList();
            var validWorkers = await uow.Workers.FindAsync(w => userIds.Contains(w.UserID));

            foreach (var w in validWorkers)
            {
                var user = validUsers.FirstOrDefault(u => u.UserID == w.UserID);
                if (user == null) continue;

                if (user.RoleID == sprzedawcaId || user.RoleID == kierownikId)
                {
                    ListaSprzedawcow.Add(new SprzedawcaItem
                    {
                        WorkerID = w.WorkerID,
                        UserID = user.UserID,
                        FullName = $"{user.FirstName} {user.LastName}".Trim()
                    });
                }
                else if (user.RoleID == serwisantId)
                {
                    ListaSerwisantow.Add(new SerwisantItem
                    {
                        WorkerID = w.WorkerID,
                        UserID = user.UserID,
                        FullName = $"{user.FirstName} {user.LastName}".Trim()
                    });
                }
            }

            DodatkoweOpcje.Clear();
            var features = await uow.Features.GetAllAsync();
            foreach (var f in features.OrderBy(f => f.Category).ThenBy(f => f.FeatureName))
            {
                DodatkoweOpcje.Add(new DodatkowaOpcja
                {
                    FeatureID = f.FeatureID,
                    Nazwa = $"{f.FeatureName} ({f.Category})",
                    Kategoria = f.Category
                });
            }
        }
        catch (Exception ex)
        {
            ShowError?.Invoke($"Nie udało się załadować danych z bazy:\n{ex.Message}\n\n{ex.InnerException?.Message}");
        }
    }

    partial void OnWybranaMarkaChanged(string? value)
    {
        _ = ReloadModelsAndEnginesAsync(value);
    }

    partial void OnWybranyModelChanged(ModelItem? value)
    {
        _ = ReloadTrimsAsync(value);
    }

    private async Task ReloadModelsAndEnginesAsync(string? brand)
    {
        Modele.Clear();
        Wersje.Clear();
        Silniki.Clear();
        if (string.IsNullOrEmpty(brand)) return;

        try
        {
            using var ctx = new AppDbContext();
            using var uow = new UnitOfWork(ctx);

            var models = await uow.VehicleModels.FindAsync(m => m.Brand == brand);
            foreach (var m in models.OrderBy(m => m.ModelName))
                Modele.Add(new ModelItem { ModelID = m.ModelID, Brand = m.Brand, ModelName = m.ModelName });

            var engines = await uow.Engines.FindAsync(en => en.Brand == brand);
            foreach (var en in engines.OrderBy(en => en.Power))
                Silniki.Add(new EngineItem { EngineID = en.EngineID, EngineName = en.EngineName, Power = en.Power, Price = en.Price });
        }
        catch (Exception ex)
        {
            ShowError?.Invoke($"Błąd ładowania modeli/silników: {ex.Message}");
        }
    }

    private async Task ReloadTrimsAsync(ModelItem? model)
    {
        Wersje.Clear();
        if (model == null) return;

        try
        {
            using var ctx = new AppDbContext();
            using var uow = new UnitOfWork(ctx);
            var trims = await uow.TrimLevels.FindAsync(t => t.ModelID == model.ModelID);
            foreach (var t in trims.OrderBy(t => t.BasePrice))
                Wersje.Add(new TrimItem { TrimID = t.TrimID, ModelID = t.ModelID, TrimName = t.TrimName, BasePrice = t.BasePrice });
        }
        catch (Exception ex)
        {
            ShowError?.Invoke($"Błąd ładowania wersji: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        try
        {
            using var ctx = new AppDbContext();
            using var uow = new UnitOfWork(ctx);

            if (WybranaWersja == null || WybranySilnik == null)
            {
                ShowWarning?.Invoke("Wybierz model, wersję i silnik pojazdu.");
                return;
            }
            if (WybranySprzedawca == null)
            {
                ShowWarning?.Invoke("Wybierz sprzedawcę.");
                return;
            }
            if (WybranySerwisant == null)
            {
                ShowWarning?.Invoke("Wybierz serwisanta.");
                return;
            }

            int clientId;
            if (CzyIstniejacyKlient)
            {
                if (WybranyKlient == null)
                {
                    ShowWarning?.Invoke("Wybierz klienta z listy.");
                    return;
                }
                clientId = WybranyKlient.ClientID;
            }
            else
            {
                if (string.IsNullOrWhiteSpace(NowyImie) || string.IsNullOrWhiteSpace(NowyEmail))
                {
                    ShowWarning?.Invoke("Wypełnij imię i e-mail nowego klienta.");
                    return;
                }

                var existing = await uow.AppUsers.FindAsync(u => u.Email == NowyEmail.Trim());
                if (existing.Any())
                {
                    ShowWarning?.Invoke("Użytkownik z takim e-mailem już istnieje. Wybierz go z listy istniejących klientów.");
                    return;
                }

                var roleKlient = (await uow.AppRoles.FindAsync(r => r.RoleName == "Klient")).FirstOrDefault();
                if (roleKlient == null)
                {
                    ShowError?.Invoke("Rola 'Klient' nie istnieje w bazie.");
                    return;
                }

                var newUser = new AppUser
                {
                    FirstName = NowyImie.Trim(),
                    LastName = NowyNazwisko.Trim(),
                    Email = NowyEmail.Trim(),
                    PasswordHash = "",
                    RoleID = roleKlient.RoleID,
                    BirthDate = DateTime.Today
                };
                await uow.AppUsers.AddAsync(newUser);
                await uow.CompleteAsync();

                var newClient = new Client
                {
                    UserID = newUser.UserID,
                    NIP = string.IsNullOrWhiteSpace(NowyNIP) ? null : NowyNIP.Trim(),
                    Phone = string.IsNullOrWhiteSpace(NowyTelefon) ? "" : NowyTelefon.Trim()
                };
                await uow.Clients.AddAsync(newClient);
                await uow.CompleteAsync();

                clientId = newClient.ClientID;
            }

            var salon = (await uow.Dealerships.GetAllAsync()).First();

            var vehicle = new Vehicle
            {
                VIN = string.IsNullOrWhiteSpace(VIN) ? GenerateVin() : VIN.Trim(),
                TrimID = WybranaWersja.TrimID,
                EngineID = WybranySilnik.EngineID,
                Mileage = CzyUzywany ? Przebieg : 0,
                IsUsed = CzyUzywany,
                DealershipID = salon.DealershipID,
                Status = "Zarezerwowany"
            };
            await uow.Vehicles.AddAsync(vehicle);
            await uow.CompleteAsync();

            foreach (var opcja in DodatkoweOpcje.Where(o => o.Zaznaczona))
            {
                await uow.VehicleFeatures.AddAsync(new VehicleFeature
                {
                    VehicleID = vehicle.VehicleID,
                    FeatureID = opcja.FeatureID,
                    PurchasePrice = 0m
                });

                // Status "Oczekujące" — spójny z resztą systemu
                await uow.Jobs.AddAsync(new Job
                {
                    VehicleID = vehicle.VehicleID,
                    FeatureID = opcja.FeatureID,
                    WorkerID = WybranySerwisant.WorkerID,
                    Status = "Oczekujące",
                    CreatedAt = DateTime.Now
                });
            }

            decimal cena = CenaFinalna > 0
                ? CenaFinalna
                : WybranaWersja.BasePrice + WybranySilnik.Price;

            var order = new SalesOrder
            {
                VehicleID = vehicle.VehicleID,
                ClientID = clientId,
                WorkerID = WybranySprzedawca.WorkerID,
                OrderDate = DataZamowienia == default ? DateTime.Now : DataZamowienia,
                FinalPrice = cena,
                Status = WybranyStatus,
                DealershipID = salon.DealershipID
            };
            await uow.SalesOrders.AddAsync(order);
            await uow.CompleteAsync();

            ShowInfo?.Invoke($"Zamówienie zapisane.\nNumer: {order.OrderID}\nCena: {order.FinalPrice:N0} zł");

            ResetForm();
            await LoadFromDbAsync();
        }
        catch (Exception ex)
        {
            ShowError?.Invoke($"Nie udało się zapisać zamówienia:\n{ex.Message}\n\n{ex.InnerException?.Message}");
        }
    }

    [RelayCommand]
    private void Cancel() => ResetForm();

    private void ResetForm()
    {
        WybranyKlient = null;
        NowyImie = NowyNazwisko = NowyEmail = NowyTelefon = NowyNIP = "";
        WybranaMarka = null;
        Modele.Clear();
        Wersje.Clear();
        WybranySilnik = null;
        Kolor = "";
        VIN = "";
        CzyUzywany = false;
        Przebieg = 0;
        foreach (var op in DodatkoweOpcje) op.Zaznaczona = false;
        CenaFinalna = 0m;
        Uwagi = "";
    }

    private static string GenerateVin()
    {
        var rnd = new Random();
        var chars = "ABCDEFGHJKLMNPRSTUVWXYZ0123456789";
        var sb = new System.Text.StringBuilder(17);
        for (int i = 0; i < 17; i++) sb.Append(chars[rnd.Next(chars.Length)]);
        return sb.ToString();
    }
}

public class KlientItem
{
    public int ClientID { get; set; }
    public int UserID { get; set; }
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
    public override string ToString() => string.IsNullOrEmpty(Email) ? FullName : $"{FullName} ({Email})";
}

public class ModelItem
{
    public int ModelID { get; set; }
    public string Brand { get; set; } = "";
    public string ModelName { get; set; } = "";
    public override string ToString() => ModelName;
}

public class TrimItem
{
    public int TrimID { get; set; }
    public int ModelID { get; set; }
    public string TrimName { get; set; } = "";
    public decimal BasePrice { get; set; }
    public override string ToString() => $"{TrimName} ({BasePrice:N0} zł)";
}

public class EngineItem
{
    public int EngineID { get; set; }
    public string EngineName { get; set; } = "";
    public int Power { get; set; }
    public decimal Price { get; set; }
    public override string ToString() =>
        Price > 0 ? $"{EngineName} • {Power} KM (+{Price:N0} zł)" : $"{EngineName} • {Power} KM";
}

public class SprzedawcaItem
{
    public int WorkerID { get; set; }
    public int UserID { get; set; }
    public string FullName { get; set; } = "";
    public override string ToString() => FullName;
}

public class DodatkowaOpcja
{
    public int FeatureID { get; set; }
    public string Nazwa { get; set; } = "";
    public string Kategoria { get; set; } = "";
    public bool Zaznaczona { get; set; }
}

public class SerwisantItem
{
    public int WorkerID { get; set; }
    public int UserID { get; set; }
    public string FullName { get; set; } = "";
    public override string ToString() => FullName;
}
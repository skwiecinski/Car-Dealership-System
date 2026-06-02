namespace SalonSamochodowy;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore.Query.Internal;
using SalonSamochodowy.Entities;
using SalonSamochodowy.Repositories;
using SalonSamochodowy.Services;

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

    private readonly IVehicleService _vehicleService;
    private readonly ICatalogService _catalogService;
    private readonly IOrderService _orderService;

    private readonly IClientService _clientService;
    private readonly IJobService _jobService;
    public CreateOrderViewModel(IVehicleService vehicleService, IOrderService orderService, IClientService clientService, IJobService jobService, ICatalogService catalogService)
    {
        _vehicleService = vehicleService;
        _catalogService = catalogService;
        _orderService = orderService;
        _clientService = clientService;
        _jobService = jobService;
    }

    public async Task LoadFromDbAsync()
    {
        try
        {
            ListaKlientow.Clear();
            var clients = await _clientService.GetAllClientsAsync();
            foreach (var c in clients)
            {
                ListaKlientow.Add(new KlientItem
                {
                    ClientID = c.ClientID,
                    UserID   = c.UserID,
                    FullName = c.FullName,
                    Email    = c.Email
                });
            }

            Marki.Clear();
            var brands = await _catalogService.GetBrandsAsync();
            foreach (var brand in brands)
                Marki.Add(brand);

            Silniki.Clear();
            ListaSprzedawcow.Clear();
            ListaSerwisantow.Clear();
            
            var workers = await _catalogService.GetWorkersByRolesAsync("Sprzedawca", "Kierownik", "Serwisant");
            foreach (var w in workers)
            {
                if (w.RoleName == "Sprzedawca" || w.RoleName == "Kierownik")
                {
                    ListaSprzedawcow.Add(new SprzedawcaItem { WorkerID = w.WorkerID, UserID = w.UserID, FullName = w.FullName });
                }
                else if (w.RoleName == "Serwisant")
                {
                    ListaSerwisantow.Add(new SerwisantItem { WorkerID = w.WorkerID, UserID = w.UserID, FullName = w.FullName });
                }
            }

            DodatkoweOpcje.Clear();
            var features = await _catalogService.GetAllFeaturesAsync();
            foreach (var f in features)
            {
                DodatkoweOpcje.Add(new DodatkowaOpcja
                {
                    FeatureID = f.FeatureID,
                    Nazwa     = $"{f.FeatureName} ({f.Category})",
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
            var models = await _catalogService.GetModelsByBrandAsync(brand);
            foreach (var m in models)
                Modele.Add(new ModelItem { ModelID = m.ModelID, Brand = m.Brand, ModelName = m.ModelName });

            var engines = await _catalogService.GetEnginesByBrandAsync(brand);
            foreach (var en in engines)
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
            var trims = await _catalogService.GetTrimsByModelAsync(model.ModelID);
            foreach (var t in trims)
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
            if(WybranySerwisant == null)
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

                if (await _clientService.EmailExistsAsync(NowyEmail.Trim()))
                {
                    ShowWarning?.Invoke("Użytkownik z takim e-mailem już istnieje. Wybierz go z listy istniejących klientów.");
                    return;
                }

                var newClientDto = await _clientService.CreateClientAsync(NowyImie.Trim(), NowyNazwisko.Trim(), NowyEmail.Trim(), NowyTelefon.Trim(), string.IsNullOrWhiteSpace(NowyNIP) ? null : NowyNIP.Trim());
                clientId = newClientDto.ClientID;
            }

            var salon = await _catalogService.GetMainDealershipAsync();
            if (salon == null) throw new Exception("Brak salonu w bazie.");
            var vehicle = new Vehicle
            {
                VIN          = string.IsNullOrWhiteSpace(VIN) ? GenerateVin() : VIN.Trim(),
                TrimID       = WybranaWersja.TrimID,
                EngineID     = WybranySilnik.EngineID,
                Mileage      = CzyUzywany ? Przebieg : 0,
                IsUsed       = CzyUzywany,
                DealershipID = salon.DealershipID,
                Status       = "Zarezerwowany"
            };
            await _vehicleService.AddVehicleAsync(vehicle);

            var selectedFeatures = DodatkoweOpcje.Where(o => o.Zaznaczona).Select(o => o.FeatureID).ToList();
            if (selectedFeatures.Any())
            {
                await _jobService.AssignFeaturesAndCreateJobsAsync(vehicle.VehicleID, selectedFeatures, WybranySerwisant.WorkerID);
            }

            decimal cena = CenaFinalna > 0 ? CenaFinalna : WybranaWersja.BasePrice + WybranySilnik.Price;
            var order = new SalesOrder
            {
                VehicleID    = vehicle.VehicleID,
                ClientID     = clientId,
                WorkerID     = WybranySprzedawca.WorkerID,
                OrderDate    = DataZamowienia == default ? DateTime.Now : DataZamowienia,
                FinalPrice   = cena,
                Status       = WybranyStatus,
                DealershipID = salon.DealershipID
            };

            /*
             Dodać opcję że gdy przy zamówieniu wybrane zostanie wyposażenie dodatkowe to jednocześnie tworzy się zlecenie serwisowe dla serwisanta.
                Dodać możliwość tworzenia nowych zleceń serwisowych dla serwisanta
             */

            await _orderService.CreateOrderAsync(order);

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
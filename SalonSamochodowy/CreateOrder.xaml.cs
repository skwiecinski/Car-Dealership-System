using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using SalonSamochodowy.Entities;
using SalonSamochodowy.Repositories;

namespace SalonSamochodowy
{
    public partial class CreateOrder : Page
    {
        private readonly CreateOrderViewModel _vm = new CreateOrderViewModel();

        public CreateOrder()
        {
            InitializeComponent();
            DataContext = _vm;

            // Ladowanie danych slownikowych z bazy po wyrenderowaniu strony
            Loaded += async (s, e) => await LoadFromDbAsync();
        }

        /// <summary>
        /// Pobiera z bazy wszystkie dane potrzebne do formularza:
        /// klienci, marki, silniki, sprzedawcy, opcje dodatkowe.
        /// Modele i Wersje sa filtrowane kaskadowo po wybraniu marki/modelu.
        /// </summary>
        private async Task LoadFromDbAsync()
        {
            try
            {
                using var ctx = new AppDbContext();
                using var uow = new UnitOfWork(ctx);

                // --- Klienci (AppUser z rola "Klient" + powiazanie Client) ---
                _vm.ListaKlientow.Clear();
                var allClients = await uow.Clients.GetAllAsync();
                foreach (var c in allClients)
                {
                    var user = await uow.AppUsers.GetByIdAsync(c.UserID);
                    if (user == null) continue;
                    var fullName = string.IsNullOrWhiteSpace(user.LastName)
                        ? user.FirstName
                        : $"{user.FirstName} {user.LastName}";
                    _vm.ListaKlientow.Add(new KlientItem
                    {
                        ClientID = c.ClientID,
                        UserID   = user.UserID,
                        FullName = fullName,
                        Email    = user.Email
                    });
                }

                // --- Marki (DISTINCT z VehicleModels) ---
                _vm.Marki.Clear();
                var allModels = (await uow.VehicleModels.GetAllAsync()).ToList();
                foreach (var brand in allModels.Select(m => m.Brand).Distinct().OrderBy(b => b))
                {
                    _vm.Marki.Add(brand);
                }

                // --- Silniki (wszystkie) ---
                _vm.Silniki.Clear();
                var allEngines = await uow.Engines.GetAllAsync();
                foreach (var en in allEngines.OrderBy(e => e.Power))
                {
                    _vm.Silniki.Add(new EngineItem
                    {
                        EngineID   = en.EngineID,
                        EngineName = en.EngineName,
                        Power      = en.Power,
                        Price      = en.Price
                    });
                }

                // --- Sprzedawcy (Workery z userami o roli "Sprzedawca" lub "Kierownik") ---
                _vm.ListaSprzedawcow.Clear();
                var rSprzedawca = (await uow.AppRoles.FindAsync(r => r.RoleName == "Sprzedawca")).FirstOrDefault();
                var rKierownik  = (await uow.AppRoles.FindAsync(r => r.RoleName == "Kierownik")).FirstOrDefault();
                var workers = await uow.Workers.GetAllAsync();
                foreach (var w in workers)
                {
                    var user = await uow.AppUsers.GetByIdAsync(w.UserID);
                    if (user == null) continue;
                    if (rSprzedawca != null && user.RoleID == rSprzedawca.RoleID
                        || rKierownik != null && user.RoleID == rKierownik.RoleID)
                    {
                        _vm.ListaSprzedawcow.Add(new SprzedawcaItem
                        {
                            WorkerID = w.WorkerID,
                            UserID   = user.UserID,
                            FullName = $"{user.FirstName} {user.LastName}".Trim()
                        });
                    }
                }

                // --- Wyposazenie dodatkowe ---
                _vm.DodatkoweOpcje.Clear();
                var features = await uow.Features.GetAllAsync();
                foreach (var f in features.OrderBy(f => f.Category).ThenBy(f => f.FeatureName))
                {
                    _vm.DodatkoweOpcje.Add(new DodatkowaOpcja
                    {
                        FeatureID = f.FeatureID,
                        Nazwa     = $"{f.FeatureName} ({f.Category})",
                        Kategoria = f.Category
                    });
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(
                    $"Nie udało się załadować danych z bazy:\n{ex.Message}\n\n{ex.InnerException?.Message}",
                    "Dodaj zamówienie",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error);
            }
        }

        // --- Kaskada Marka -> Modele ---
        private async void MarkaChanged(object sender, SelectionChangedEventArgs e)
        {
            _vm.Modele.Clear();
            _vm.Wersje.Clear();
            if (string.IsNullOrEmpty(_vm.WybranaMarka)) return;

            try
            {
                using var ctx = new AppDbContext();
                using var uow = new UnitOfWork(ctx);
                var models = await uow.VehicleModels.FindAsync(m => m.Brand == _vm.WybranaMarka);
                foreach (var m in models.OrderBy(m => m.ModelName))
                {
                    _vm.Modele.Add(new ModelItem
                    {
                        ModelID   = m.ModelID,
                        Brand     = m.Brand,
                        ModelName = m.ModelName
                    });
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Błąd ładowania modeli: {ex.Message}",
                    "Dodaj zamówienie", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        // --- Kaskada Model -> Wersje ---
        private async void ModelChanged(object sender, SelectionChangedEventArgs e)
        {
            _vm.Wersje.Clear();
            if (_vm.WybranyModel == null) return;

            try
            {
                using var ctx = new AppDbContext();
                using var uow = new UnitOfWork(ctx);
                var trims = await uow.TrimLevels.FindAsync(t => t.ModelID == _vm.WybranyModel.ModelID);
                foreach (var t in trims.OrderBy(t => t.BasePrice))
                {
                    _vm.Wersje.Add(new TrimItem
                    {
                        TrimID    = t.TrimID,
                        ModelID   = t.ModelID,
                        TrimName  = t.TrimName,
                        BasePrice = t.BasePrice
                    });
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Błąd ładowania wersji: {ex.Message}",
                    "Dodaj zamówienie", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        // --- Zapis zamowienia do bazy ---
        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using var ctx = new AppDbContext();
                using var uow = new UnitOfWork(ctx);

                // === Walidacja ===
                if (_vm.WybranaWersja == null || _vm.WybranySilnik == null)
                {
                    System.Windows.MessageBox.Show("Wybierz model, wersję i silnik pojazdu.",
                        "Dodaj zamówienie", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                    return;
                }
                if (_vm.WybranySprzedawca == null)
                {
                    System.Windows.MessageBox.Show("Wybierz sprzedawcę.",
                        "Dodaj zamówienie", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                    return;
                }

                // === Klient: istniejacy lub nowy ===
                int clientId;
                if (_vm.CzyIstniejacyKlient)
                {
                    if (_vm.WybranyKlient == null)
                    {
                        System.Windows.MessageBox.Show("Wybierz klienta z listy.",
                            "Dodaj zamówienie", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                        return;
                    }
                    clientId = _vm.WybranyKlient.ClientID;
                }
                else
                {
                    // Nowy klient - walidacja
                    if (string.IsNullOrWhiteSpace(_vm.NowyImie) || string.IsNullOrWhiteSpace(_vm.NowyEmail))
                    {
                        System.Windows.MessageBox.Show("Wypełnij imię i e-mail nowego klienta.",
                            "Dodaj zamówienie", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                        return;
                    }

                    // Sprawdzenie czy e-mail nie jest juz zajety
                    var existing = await uow.AppUsers.FindAsync(u => u.Email == _vm.NowyEmail.Trim());
                    if (existing.Any())
                    {
                        System.Windows.MessageBox.Show("Użytkownik z takim e-mailem już istnieje. Wybierz go z listy istniejących klientów.",
                            "Dodaj zamówienie", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                        return;
                    }

                    // Stworz AppUser z rola Klient
                    var roleKlient = (await uow.AppRoles.FindAsync(r => r.RoleName == "Klient")).FirstOrDefault();
                    if (roleKlient == null)
                    {
                        System.Windows.MessageBox.Show("Rola 'Klient' nie istnieje w bazie.",
                            "Dodaj zamówienie", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                        return;
                    }

                    var newUser = new AppUser
                    {
                        FirstName    = _vm.NowyImie.Trim(),
                        LastName     = _vm.NowyNazwisko.Trim(),
                        Email        = _vm.NowyEmail.Trim(),
                        PasswordHash = "",
                        RoleID       = roleKlient.RoleID,
                        BirthDate    = DateTime.Today
                    };
                    await uow.AppUsers.AddAsync(newUser);
                    await uow.CompleteAsync();

                    var newClient = new Client
                    {
                        UserID = newUser.UserID,
                        NIP    = string.IsNullOrWhiteSpace(_vm.NowyNIP) ? null : _vm.NowyNIP.Trim(),
                        Phone  = string.IsNullOrWhiteSpace(_vm.NowyTelefon) ? "" : _vm.NowyTelefon.Trim()
                    };
                    await uow.Clients.AddAsync(newClient);
                    await uow.CompleteAsync();

                    clientId = newClient.ClientID;
                }

                // === Tworzenie pojazdu (konfiguracja z wybranej wersji + silnika) ===
                var salon = (await uow.Dealerships.GetAllAsync()).First();
                var vehicle = new Vehicle
                {
                    VIN          = string.IsNullOrWhiteSpace(_vm.VIN) ? GenerateVin() : _vm.VIN.Trim(),
                    TrimID       = _vm.WybranaWersja.TrimID,
                    EngineID     = _vm.WybranySilnik.EngineID,
                    Mileage      = _vm.CzyUzywany ? _vm.Przebieg : 0,
                    IsUsed       = _vm.CzyUzywany,
                    DealershipID = salon.DealershipID,
                    Status       = "Zarezerwowany"
                };
                await uow.Vehicles.AddAsync(vehicle);
                await uow.CompleteAsync();

                // === Wyposazenie dodatkowe ===
                foreach (var opcja in _vm.DodatkoweOpcje.Where(o => o.Zaznaczona))
                {
                    await uow.VehicleFeatures.AddAsync(new VehicleFeature
                    {
                        VehicleID      = vehicle.VehicleID,
                        FeatureID      = opcja.FeatureID,
                        PurchasePrice  = 0m // backend wypelni cennik
                    });
                }

                // === Zamowienie ===
                decimal cena = _vm.CenaFinalna > 0 ? _vm.CenaFinalna : _vm.WybranaWersja.BasePrice + _vm.WybranySilnik.Price;
                var order = new SalesOrder
                {
                    VehicleID    = vehicle.VehicleID,
                    ClientID     = clientId,
                    WorkerID     = _vm.WybranySprzedawca.WorkerID,
                    OrderDate    = _vm.DataZamowienia == default ? DateTime.Now : _vm.DataZamowienia,
                    FinalPrice   = cena,
                    Status       = _vm.WybranyStatus,
                    DealershipID = salon.DealershipID
                };
                await uow.SalesOrders.AddAsync(order);
                await uow.CompleteAsync();

                System.Windows.MessageBox.Show(
                    $"Zamówienie zapisane.\nNumer: {order.OrderID}\nCena: {order.FinalPrice:N0} zł",
                    "Dodaj zamówienie",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Information);

                // Reset formularza
                _vm.WybranyKlient = null;
                _vm.NowyImie = _vm.NowyNazwisko = _vm.NowyEmail = _vm.NowyTelefon = _vm.NowyNIP = "";
                _vm.WybranaMarka = null;
                _vm.Modele.Clear();
                _vm.Wersje.Clear();
                _vm.WybranySilnik = null;
                _vm.Kolor = _vm.VIN = "";
                _vm.CzyUzywany = false;
                _vm.Przebieg = 0;
                foreach (var op in _vm.DodatkoweOpcje) op.Zaznaczona = false;
                _vm.CenaFinalna = 0m;
                _vm.Uwagi = "";

                await LoadFromDbAsync();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(
                    $"Nie udało się zapisać zamówienia:\n{ex.Message}\n\n{ex.InnerException?.Message}",
                    "Dodaj zamówienie",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DataContext = new CreateOrderViewModel();
        }

        // Prosty generator placeholdera VIN (17 znakow)
        private static string GenerateVin()
        {
            var rnd = new Random();
            var chars = "ABCDEFGHJKLMNPRSTUVWXYZ0123456789";
            var sb = new System.Text.StringBuilder(17);
            for (int i = 0; i < 17; i++) sb.Append(chars[rnd.Next(chars.Length)]);
            return sb.ToString();
        }
    }
}

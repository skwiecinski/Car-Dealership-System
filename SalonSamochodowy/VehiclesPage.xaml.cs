using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using SalonSamochodowy.Entities;
using SalonSamochodowy.Repositories;

namespace SalonSamochodowy
{
    public partial class VehiclesPage : Page
    {
        public ObservableCollection<VehicleItem> VehicleList { get; set; } = new();

        public VehiclesPage()
        {
            InitializeComponent();
            DataContext = this;

            Loaded += async (s, e) =>
            {
                await LoadFiltersAsync();
                await LoadVehiclesAsync();
            };
        }

        private async Task LoadFiltersAsync()
        {
            try
            {
                using var ctx = new AppDbContext();
                using var uow = new UnitOfWork(ctx);

                CbBrand.Items.Clear();
                CbBrand.Items.Add("Wszystkie marki");
                var models = (await uow.VehicleModels.GetAllAsync()).ToList();
                foreach (var brand in models.Select(m => m.Brand).Distinct().OrderBy(b => b))
                {
                    CbBrand.Items.Add(brand);
                }
                CbBrand.SelectedIndex = 0;

                ReloadModelCombo(models);

                CbEngine.Items.Clear();
                CbEngine.Items.Add("Dowolny");
                var engines = (await uow.Engines.GetAllAsync()).OrderBy(en => en.Power);
                foreach (var en in engines)
                {
                    CbEngine.Items.Add($"{en.EngineName} • {en.Power} KM");
                }
                CbEngine.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(
                    $"Nie udało się załadować filtrów:\n{ex.Message}",
                    "Wyszukiwarka pojazdów",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error);
            }
        }

        private void ReloadModelCombo(System.Collections.Generic.IList<VehicleModel> models)
        {
            CbModel.Items.Clear();
            CbModel.Items.Add("Wszystkie modele");

            var selectedBrand = CbBrand.SelectedItem as string;
            var filtered = !string.IsNullOrEmpty(selectedBrand) && selectedBrand != "Wszystkie marki"
                ? models.Where(m => m.Brand == selectedBrand)
                : models;

            foreach (var m in filtered.OrderBy(m => m.Brand).ThenBy(m => m.ModelName))
            {
                CbModel.Items.Add($"{m.Brand} {m.ModelName}");
            }
            CbModel.SelectedIndex = 0;
        }

        private async void CbBrand_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded) return;
            try
            {
                using var ctx = new AppDbContext();
                using var uow = new UnitOfWork(ctx);
                var models = (await uow.VehicleModels.GetAllAsync()).ToList();
                ReloadModelCombo(models);
            }
            catch { }
        }

        private async Task LoadVehiclesAsync()
        {
            try
            {
                using var ctx = new AppDbContext();
                using var uow = new UnitOfWork(ctx);

                var allVehicles = (await uow.Vehicles.GetAllAsync()).ToList();
                VehicleList.Clear();

                var trims = (await uow.TrimLevels.GetAllAsync()).ToList();
                var models = (await uow.VehicleModels.GetAllAsync()).ToList();
                var engines = (await uow.Engines.GetAllAsync()).ToList();

                var selectedBrand  = CbBrand.SelectedItem as string;
                var selectedModel  = CbModel.SelectedItem as string;
                var selectedEngine = CbEngine.SelectedItem as string;
                var selectedStatus = (CbStatus.SelectedItem as ComboBoxItem)?.Content as string;

                foreach (var v in allVehicles)
                {
                    var trim = trims.FirstOrDefault(t => t.TrimID == v.TrimID);
                    if (trim == null) continue;
                    var model = models.FirstOrDefault(m => m.ModelID == trim.ModelID);
                    if (model == null) continue;
                    var engine = engines.FirstOrDefault(en => en.EngineID == v.EngineID);

                    if (!string.IsNullOrEmpty(selectedBrand) && selectedBrand != "Wszystkie marki"
                        && model.Brand != selectedBrand) continue;

                    if (!string.IsNullOrEmpty(selectedModel) && selectedModel != "Wszystkie modele"
                        && $"{model.Brand} {model.ModelName}" != selectedModel) continue;

                    if (!string.IsNullOrEmpty(selectedEngine) && selectedEngine != "Dowolny"
                        && engine != null && $"{engine.EngineName} • {engine.Power} KM" != selectedEngine) continue;

                    if (!string.IsNullOrEmpty(selectedStatus) && selectedStatus != "Wszystkie"
                        && v.Status != selectedStatus) continue;

                    var (bg, bd, fg) = StatusColors(v.Status);
                    var price = trim.BasePrice + (engine?.Price ?? 0m);

                    VehicleList.Add(new VehicleItem
                    {
                        FullName              = $"{model.Brand} {model.ModelName} {trim.TrimName}",
                        EngineInfo            = engine != null ? $"Silnik: {engine.EngineName} ({engine.Power} KM)" : "Silnik: —",
                        VIN                   = v.VIN,
                        Price                 = $"{price:N0} PLN",
                        Status                = v.Status,
                        StatusBackgroundColor = bg,
                        StatusBorderColor     = bd,
                        StatusTextColor       = fg
                    });
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(
                    $"Nie udało się załadować pojazdów:\n{ex.Message}\n\n{ex.InnerException?.Message}",
                    "Wyszukiwarka pojazdów",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error);
            }
        }

        private async void SearchBtn_Click(object sender, RoutedEventArgs e)
        {
            await LoadVehiclesAsync();
        }

        private static (string bg, string bd, string fg) StatusColors(string status) => status switch
        {
            "Dostępny"      => ("#112C1E", "#2D9A4A", "#44C767"),
            "Zarezerwowany" => ("#332A12", "#D3A125", "#F0B82B"),
            "Sprzedany"     => ("#3D1D1D", "#D34545", "#ED6262"),
            _               => ("#2D3038", "#4F5466", "#8A8D98"),
        };
    }

    public class VehicleItem
    {
        public string FullName { get; set; } = "";
        public string EngineInfo { get; set; } = "";
        public string VIN { get; set; } = "";
        public string Price { get; set; } = "";
        public string Status { get; set; } = "";
        public string StatusTextColor { get; set; } = "";
        public string StatusBackgroundColor { get; set; } = "";
        public string StatusBorderColor { get; set; } = "";
    }
}

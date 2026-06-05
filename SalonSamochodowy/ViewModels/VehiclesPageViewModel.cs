using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SalonSamochodowy.Entities;
using SalonSamochodowy.Repositories;
using SalonSamochodowy.Services;

namespace SalonSamochodowy.ViewModels
{
    public partial class VehiclesPageViewModel : ObservableObject
    {
        private readonly IUnitOfWork _uow;
    private readonly IVehicleService _vehicleService;

        public VehiclesPageViewModel(IUnitOfWork uow, IVehicleService vehicleService)
        {
            _uow = uow;
        _vehicleService = vehicleService;
        }
        public ObservableCollection<string> Brands { get; } = new() { "Wszystkie marki" };
        public ObservableCollection<string> Models { get; } = new() { "Wszystkie modele" };
        public ObservableCollection<string> Engines { get; } = new() { "Dowolny" };

        [ObservableProperty] private string? selectedBrand;
        [ObservableProperty] private string? selectedModel;
        [ObservableProperty] private string? selectedEngine;
        [ObservableProperty] private string? selectedStatus;

        public ObservableCollection<VehicleItem> VehicleList { get; } = new();

        public event Action<string>? LoadFailed;

        public async Task LoadFiltersAsync()
        {
            try
            {
                var uow = _uow;

                var models = (await uow.VehicleModels.GetAllAsync()).ToList();

                Brands.Clear();
                Brands.Add("Wszystkie marki");
                foreach (var b in models.Select(m => m.Brand).Distinct().OrderBy(b => b))
                    Brands.Add(b);
                SelectedBrand = Brands[0];

                ReloadModelCombo(models);

                Engines.Clear();
                Engines.Add("Dowolny");
                var engines = (await uow.Engines.GetAllAsync()).OrderBy(e => e.Power);
                foreach (var en in engines)
                    Engines.Add($"{en.EngineName} • {en.Power} KM");
                SelectedEngine = Engines[0];
            }
            catch (Exception ex)
            {
                LoadFailed?.Invoke($"Nie udało się załadować filtrów:\n{ex.Message}");
            }
        }

        partial void OnSelectedBrandChanged(string? value)
        {
            _ = ReloadModelComboAsync();
        }

        private async Task ReloadModelComboAsync()
        {
            try
            {
                var uow = _uow;
                var models = (await uow.VehicleModels.GetAllAsync()).ToList();
                ReloadModelCombo(models);
            }
            catch { }
        }

        private void ReloadModelCombo(IList<VehicleModel> models)
        {
            Models.Clear();
            Models.Add("Wszystkie modele");

            var filtered = !string.IsNullOrEmpty(SelectedBrand) && SelectedBrand != "Wszystkie marki"
                ? models.Where(m => m.Brand == SelectedBrand)
                : models;

            foreach (var m in filtered.OrderBy(m => m.Brand).ThenBy(m => m.ModelName))
                Models.Add($"{m.Brand} {m.ModelName}");

            SelectedModel = Models[0];
        }

        [RelayCommand]
        private async Task SearchAsync()
        {
            await LoadVehiclesAsync();
        }

        public event Action? OpenAddVehicleRequested;

        [RelayCommand]
        private void OpenAddVehicle() => OpenAddVehicleRequested?.Invoke();

        public event Action<VehicleItem>? EditVehicleRequested;
        [RelayCommand]
        private void EditVehicle(VehicleItem item) => EditVehicleRequested?.Invoke(item);

        public event Action<VehicleItem>? OpenAddJobRequested;
        [RelayCommand]
        private void OpenAddJob(VehicleItem item) => OpenAddJobRequested?.Invoke(item);

        public event Action<VehicleItem>? DeleteVehicleRequested;
        [RelayCommand]
        private void DeleteVehicle(VehicleItem item) => DeleteVehicleRequested?.Invoke(item);

        public async Task LoadVehiclesAsync()
        {
            try
            {
                var uow = _uow;

                IEnumerable<Vehicle> vehicles;
                if (!string.IsNullOrEmpty(SelectedStatus) && SelectedStatus != "Wszystkie")
                {
                    vehicles = await _vehicleService.GetVehiclesByStatusAsync(SelectedStatus);
                }
                else
                {
                    vehicles = await _vehicleService.GetAllVehiclesAsync();
                }

                VehicleList.Clear();

                foreach (var v in vehicles)
                {
                    var trim = await uow.TrimLevels.GetByIdAsync(v.TrimID);
                    if (trim == null) continue;
                    var model = await uow.VehicleModels.GetByIdAsync(trim.ModelID);
                    if (model == null) continue;
                    var engine = await uow.Engines.GetByIdAsync(v.EngineID);

                    if (!string.IsNullOrEmpty(SelectedBrand) && SelectedBrand != "Wszystkie marki"
                        && model.Brand != SelectedBrand) continue;

                    if (!string.IsNullOrEmpty(SelectedModel) && SelectedModel != "Wszystkie modele"
                        && $"{model.Brand} {model.ModelName}" != SelectedModel) continue;

                    if (!string.IsNullOrEmpty(SelectedEngine) && SelectedEngine != "Dowolny"
                        && engine != null && $"{engine.EngineName} • {engine.Power} KM" != SelectedEngine) continue;

                    var (bg, bd, fg) = StatusColors(v.Status);
                    var price = trim.BasePrice + (engine?.Price ?? 0m);

                    var vehicleFeatures = await uow.VehicleFeatures.FindAsync(vf => vf.VehicleID == v.VehicleID);
                    var featureNames = new List<string>();
                    foreach (var vf in vehicleFeatures)
                    {
                        var f = await uow.Features.GetByIdAsync(vf.FeatureID);
                        if (f != null) featureNames.Add(f.FeatureName);
                    }
                    var extraFeatures = featureNames.Any() ? string.Join(", ", featureNames) : "Brak";

                    VehicleList.Add(new VehicleItem
                    {
                        VehicleID             = v.VehicleID,
                        FullName              = $"{model.Brand} {model.ModelName} {trim.TrimName}",
                        EngineInfo            = engine != null ? $"Silnik: {engine.EngineName} ({engine.Power} KM)" : "Silnik: —",
                        VIN                   = v.VIN,
                        Price                 = $"{price:N0} PLN",
                        Status                = v.Status,
                        StatusBackgroundColor = bg,
                        StatusBorderColor     = bd,
                        StatusTextColor       = fg,
                        IsUsed                = v.IsUsed,
                        ExtraFeatures         = extraFeatures
                    });
                }
            }
            catch (Exception ex)
            {
                LoadFailed?.Invoke($"Nie udało się załadować pojazdów:\n{ex.Message}\n\n{ex.InnerException?.Message}");
            }
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
        public int VehicleID                { get; set; }
        public string FullName              { get; set; } = "";
        public string EngineInfo            { get; set; } = "";
        public string VIN                   { get; set; } = "";
        public string Price                 { get; set; } = "";
        public string Status                { get; set; } = "";
        public string StatusTextColor       { get; set; } = "";
        public string StatusBackgroundColor { get; set; } = "";
        public string StatusBorderColor     { get; set; } = "";
        public bool IsUsed                  { get; set; }
        public string ExtraFeatures         { get; set; } = "";
    }
}

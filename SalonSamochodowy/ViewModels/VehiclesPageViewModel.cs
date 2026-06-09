using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
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

            CommunityToolkit.Mvvm.Messaging.WeakReferenceMessenger.Default.Register(this, (VehiclesPageViewModel r, SalonSamochodowy.Messages.DataChangedMessage m) =>
            {
                r.IsLoaded = false;
            });
        }
        public ObservableCollection<string> Brands { get; } = new() { SalonSamochodowy.Services.LocalizationHelper.GetString("Vehicles_AllBrands") };
        public ObservableCollection<string> Models { get; } = new() { SalonSamochodowy.Services.LocalizationHelper.GetString("Vehicles_AllModels") };

        public bool IsLoaded { get; set; } = false;
        public ObservableCollection<string> Engines { get; } = new() { SalonSamochodowy.Services.LocalizationHelper.GetString("Vehicles_Any") };
        public ObservableCollection<string> Colors { get; } = new() { SalonSamochodowy.Services.LocalizationHelper.GetString("Vehicles_Any") };

        [ObservableProperty] private string? selectedBrand;
        [ObservableProperty] private string? selectedModel;
        [ObservableProperty] private string? selectedEngine;
        [ObservableProperty] private string? selectedColor;
        [ObservableProperty] private string? selectedStatus;

        public ObservableCollection<VehicleItem> VehicleList { get; } = new();

        public event Action<string>? LoadFailed;

        public async Task LoadFiltersAsync()
        {
            if (IsLoaded) return;
            try
            {
                var uow = _uow;

                var models = (await uow.VehicleModels.GetAllAsync()).ToList();

                Brands.Clear();
                Brands.Add(SalonSamochodowy.Services.LocalizationHelper.GetString("Vehicles_AllBrands"));
                foreach (var b in models.Select(m => m.Brand).Distinct().OrderBy(b => b))
                    Brands.Add(b);
                SelectedBrand = Brands[0];

                ReloadModelCombo(models);

                Engines.Clear();
                Engines.Add(SalonSamochodowy.Services.LocalizationHelper.GetString("Vehicles_Any"));
                var engines = (await uow.Engines.GetAllAsync()).OrderBy(e => e.Power);
                foreach (var en in engines)
                    Engines.Add(string.Format(SalonSamochodowy.Services.LocalizationHelper.GetString("Vehicles_EngineFormat"), en.EngineName, en.Power));
                SelectedEngine = Engines[0];

                Colors.Clear();
                Colors.Add(SalonSamochodowy.Services.LocalizationHelper.GetString("Vehicles_Any"));
                var colors = (await uow.Features.FindAsync(f => f.Category == "Kolor"))
                    .Select(f => f.FeatureName)
                    .Distinct();
                foreach (var c in colors.OrderBy(c => c))
                    Colors.Add(c);
                SelectedColor = Colors[0];
            }
            catch (Exception ex)
            {
                LoadFailed?.Invoke(string.Format(SalonSamochodowy.Services.LocalizationHelper.GetString("Vehicles_FilterLoadError"), ex.Message));
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
            Models.Add(SalonSamochodowy.Services.LocalizationHelper.GetString("Vehicles_AllModels"));

            var filtered = !string.IsNullOrEmpty(SelectedBrand) && SelectedBrand != SalonSamochodowy.Services.LocalizationHelper.GetString("Vehicles_AllBrands")
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
            if (IsLoaded) return;
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

                    if (!string.IsNullOrEmpty(SelectedBrand) && SelectedBrand != SalonSamochodowy.Services.LocalizationHelper.GetString("Vehicles_AllBrands")
                        && model.Brand != SelectedBrand) continue;

                    if (!string.IsNullOrEmpty(SelectedModel) && SelectedModel != SalonSamochodowy.Services.LocalizationHelper.GetString("Vehicles_AllModels")
                        && $"{model.Brand} {model.ModelName}" != SelectedModel) continue;

                    if (!string.IsNullOrEmpty(SelectedEngine) && SelectedEngine != SalonSamochodowy.Services.LocalizationHelper.GetString("Vehicles_Any")
                        && engine != null && string.Format(SalonSamochodowy.Services.LocalizationHelper.GetString("Vehicles_EngineFormat"), engine.EngineName, engine.Power) != SelectedEngine) continue;

                    var (bg, bd, fg) = StatusColors(v.Status);
                    var price = trim.BasePrice + (engine?.Price ?? 0m);

                    var vehicleFeatures = await uow.VehicleFeatures.FindAsync(vf => vf.VehicleID == v.VehicleID);
                    var featureNames = new List<string>();
                    string colorVal = SalonSamochodowy.Services.LocalizationHelper.GetString("Vehicles_None");
                    foreach (var vf in vehicleFeatures)
                    {
                        var f = await uow.Features.GetByIdAsync(vf.FeatureID);
                        if (f != null)
                        {
                            if (f.Category == "Kolor")
                                colorVal = f.FeatureName;
                            else
                                featureNames.Add(f.FeatureName);
                        }
                    }

                    if (!string.IsNullOrEmpty(SelectedColor) && SelectedColor != SalonSamochodowy.Services.LocalizationHelper.GetString("Vehicles_Any")
                        && colorVal != SelectedColor) continue;

                    var extraFeatures = featureNames.Any() ? string.Join(", ", featureNames) : SalonSamochodowy.Services.LocalizationHelper.GetString("Vehicles_None");

                    string displayStatus = v.Status switch
                    {
                        "Dostępny" => SalonSamochodowy.Services.LocalizationHelper.GetString("Vehicles_StatusAvailable"),
                        "Zarezerwowany" => SalonSamochodowy.Services.LocalizationHelper.GetString("Vehicles_StatusReserved"),
                        "Sprzedany" => SalonSamochodowy.Services.LocalizationHelper.GetString("Vehicles_StatusSold"),
                        _ => v.Status
                    };

                    VehicleList.Add(new VehicleItem
                    {
                        VehicleID             = v.VehicleID,
                        FullName              = $"{model.Brand} {model.ModelName} {trim.TrimName}",
                        EngineInfo            = engine != null ? string.Format(SalonSamochodowy.Services.LocalizationHelper.GetString("Vehicles_EngineFormat"), engine.EngineName, engine.Power) : SalonSamochodowy.Services.LocalizationHelper.GetString("Vehicles_EngineNone"),
                        VIN                   = v.VIN,
                        Price                 = $"{price:N0} PLN",
                        Status                = displayStatus,
                        StatusBackgroundColor = bg,
                        StatusBorderColor     = bd,
                        StatusTextColor       = fg,
                        IsUsed                = v.IsUsed,
                        ExtraFeatures         = extraFeatures,
                        Color                 = colorVal
                    });
                }
                IsLoaded = true;
            }
            catch (Exception ex)
            {
                LoadFailed?.Invoke(string.Format(SalonSamochodowy.Services.LocalizationHelper.GetString("Vehicles_VehiclesLoadError"), ex.Message, ex.InnerException?.Message ?? ""));
            }
        }

        [RelayCommand]
        public async Task RefreshDataAsync()
        {
            IsLoaded = false;
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
        public string Color                 { get; set; } = "";
    }
}

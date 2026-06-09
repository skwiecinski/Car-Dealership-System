using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SalonSamochodowy.Entities;
using SalonSamochodowy.Repositories;

namespace SalonSamochodowy.ViewModels
{
    public partial class AddVehicleViewModel : ObservableObject
    {
        public Vehicle NewVehicle { get; set; }

        public ObservableCollection<VehicleModel> AvailableModels { get; } = new();
        public ObservableCollection<TrimLevel> AvailableTrims { get; } = new();
        public ObservableCollection<Engine> AvailableEngines { get; } = new();
        public ObservableCollection<Feature> AvailableColors { get; } = new();

        private Feature? _selectedColor;
        public Feature? SelectedColor
        {
            get => _selectedColor;
            set => SetProperty(ref _selectedColor, value);
        }

        private List<TrimLevel> _allTrims = new();
        private List<Engine> _allEngines = new();

        // SelectedModel filtruje trims i silniki
        private VehicleModel? _selectedModel;
        public VehicleModel? SelectedModel
        {
            get => _selectedModel;
            set
            {
                if (SetProperty(ref _selectedModel, value))
                {
                    FilterTrimsAndEngines();
                }
            }
        }

        private TrimLevel? _selectedTrim;
        public TrimLevel? SelectedTrim
        {
            get => _selectedTrim;
            set => SetProperty(ref _selectedTrim, value);
        }

        private Engine? _selectedEngine;
        public Engine? SelectedEngine
        {
            get => _selectedEngine;
            set => SetProperty(ref _selectedEngine, value);
        }

        // DealershipID wyznaczane z sesji — nie hardkodowane
        private int _resolvedDealershipId = 1;

        public event Action? CloseRequested;
        public event Action<string>? ShowError;
        public event Action<string>? ShowSuccess;
        public event Action? VehicleAdded;

        public AddVehicleViewModel()
        {
            NewVehicle = new Vehicle { Status = "Dostępny", IsUsed = false };
        }

        public async Task LoadAsync()
        {
            try
            {
                using var ctx = new AppDbContext();
                using var uow = new UnitOfWork(ctx);

                // --- wyznacz salon zalogowanego pracownika ---
                var currentUser = SessionContext.CurrentUser;
                if (currentUser != null)
                {
                    var workers = await uow.Workers.FindAsync(w => w.UserID == currentUser.UserID);
                    var worker = workers.FirstOrDefault();
                    if (worker != null)
                        _resolvedDealershipId = worker.DealershipID;
                    // Administrator nie ma wpisu Worker — zostaje domyślny salon 1
                }

                var models = (await uow.VehicleModels.GetAllAsync()).OrderBy(m => m.Brand).ThenBy(m => m.ModelName);
                var engines = await uow.Engines.GetAllAsync();
                var trims = await uow.TrimLevels.GetAllAsync();

                AvailableModels.Clear();
                foreach (var m in models) AvailableModels.Add(m);

                _allEngines = engines.ToList();
                _allTrims = trims.ToList();

                AvailableTrims.Clear();
                AvailableEngines.Clear();

                var colors = await uow.Features.FindAsync(f => f.Category == "Kolor");
                AvailableColors.Clear();
                foreach (var c in colors.OrderBy(c => c.FeatureName))
                    AvailableColors.Add(c);
            }
            catch (Exception ex)
            {
                ShowError?.Invoke(string.Format(SalonSamochodowy.Services.LocalizationHelper.GetString("Vehicles_DictionaryLoadError"), ex.Message));
            }
        }

        private void FilterTrimsAndEngines()
        {
            AvailableTrims.Clear();
            AvailableEngines.Clear();
            SelectedTrim = null;
            SelectedEngine = null;

            if (SelectedModel == null) return;

            foreach (var t in _allTrims.Where(t => t.ModelID == SelectedModel.ModelID).OrderBy(t => t.BasePrice))
                AvailableTrims.Add(t);

            foreach (var e in _allEngines.Where(e => e.Brand == SelectedModel.Brand).OrderBy(e => e.Power))
                AvailableEngines.Add(e);
        }

        [RelayCommand]
        private async Task SaveAsync()
        {
            if (string.IsNullOrWhiteSpace(NewVehicle.VIN))
            {
                ShowError?.Invoke(SalonSamochodowy.Services.LocalizationHelper.GetString("Vehicles_ValidationVIN"));
                return;
            }
            if (SelectedModel == null || SelectedTrim == null || SelectedEngine == null || SelectedColor == null)
            {
                ShowError?.Invoke(SalonSamochodowy.Services.LocalizationHelper.GetString("Vehicles_ValidationSpecs"));
                return;
            }

            try
            {
                using var ctx = new AppDbContext();
                using var uow = new UnitOfWork(ctx);

                // Sprawdź unikalność VIN
                var existing = await uow.Vehicles.FindAsync(v => v.VIN == NewVehicle.VIN.Trim());
                if (existing.Any())
                {
                    ShowError?.Invoke(string.Format(SalonSamochodowy.Services.LocalizationHelper.GetString("Vehicles_VINExists"), NewVehicle.VIN));
                    return;
                }

                NewVehicle.VIN = NewVehicle.VIN.Trim().ToUpper();
                NewVehicle.EngineID = SelectedEngine.EngineID;
                NewVehicle.TrimID = SelectedTrim.TrimID;
                NewVehicle.DealershipID = _resolvedDealershipId;

                await uow.Vehicles.AddAsync(NewVehicle);

                var isBaseColor = SelectedColor.FeatureName.Contains("bazowy");
                var purchasePrice = isBaseColor ? 0m : 2500m;

                await uow.VehicleFeatures.AddAsync(new VehicleFeature
                {
                    Vehicle = NewVehicle,
                    FeatureID = SelectedColor.FeatureID,
                    PurchasePrice = purchasePrice
                });

                await uow.CompleteAsync();

                ShowSuccess?.Invoke(SalonSamochodowy.Services.LocalizationHelper.GetString("Vehicles_SuccessAdded"));
                VehicleAdded?.Invoke();
                CloseRequested?.Invoke();
            }
            catch (Exception ex)
            {
                ShowError?.Invoke(string.Format(SalonSamochodowy.Services.LocalizationHelper.GetString("Vehicles_SaveError"), ex.Message, ex.InnerException?.Message ?? ""));
            }
        }

        [RelayCommand]
        private void Close() => CloseRequested?.Invoke();
    }
}
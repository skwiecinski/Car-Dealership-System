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
    public partial class EditVehicleViewModel : ObservableObject
    {
        public Vehicle NewVehicle { get; set; }

        public ObservableCollection<VehicleModel> AvailableModels { get; } = new();
        public ObservableCollection<TrimLevel> AvailableTrims { get; } = new();
        public ObservableCollection<Engine> AvailableEngines { get; } = new();

        private List<TrimLevel> _allTrims = new();
        private List<Engine> _allEngines = new();

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

        public event Action? CloseRequested;
        public event Action<string>? ShowError;
        public event Action<string>? ShowSuccess;
        public event Action? VehicleEdited;

        public EditVehicleViewModel(Vehicle vehicleToEdit)
        {
            NewVehicle = new Vehicle
            {
                VehicleID = vehicleToEdit.VehicleID,
                VIN = vehicleToEdit.VIN,
                Mileage = vehicleToEdit.Mileage,
                IsUsed = vehicleToEdit.IsUsed,
                Status = vehicleToEdit.Status,
                TrimID = vehicleToEdit.TrimID,
                EngineID = vehicleToEdit.EngineID,
                DealershipID = vehicleToEdit.DealershipID
            };
        }

        public async Task LoadAsync()
        {
            try
            {
                using var ctx = new AppDbContext();
                using var uow = new UnitOfWork(ctx);

                var models = (await uow.VehicleModels.GetAllAsync()).OrderBy(m => m.Brand).ThenBy(m => m.ModelName);
                var engines = await uow.Engines.GetAllAsync();
                var trims = await uow.TrimLevels.GetAllAsync();

                AvailableModels.Clear();
                foreach (var m in models) AvailableModels.Add(m);

                _allEngines = engines.ToList();
                _allTrims = trims.ToList();

                // Select existing values
                var currentTrim = _allTrims.FirstOrDefault(t => t.TrimID == NewVehicle.TrimID);
                if (currentTrim != null)
                {
                    SelectedModel = AvailableModels.FirstOrDefault(m => m.ModelID == currentTrim.ModelID);
                    SelectedTrim = AvailableTrims.FirstOrDefault(t => t.TrimID == currentTrim.TrimID);
                }
                SelectedEngine = AvailableEngines.FirstOrDefault(e => e.EngineID == NewVehicle.EngineID);
            }
            catch (Exception ex)
            {
                ShowError?.Invoke(string.Format(SalonSamochodowy.Services.LocalizationHelper.GetString("Vehicles_DataLoadError"), ex.Message));
            }
        }

        private void FilterTrimsAndEngines()
        {
            AvailableTrims.Clear();
            AvailableEngines.Clear();

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
            if (SelectedModel == null || SelectedTrim == null || SelectedEngine == null)
            {
                ShowError?.Invoke(SalonSamochodowy.Services.LocalizationHelper.GetString("Vehicles_ValidationSpecsEdit"));
                return;
            }

            try
            {
                using var ctx = new AppDbContext();
                using var uow = new UnitOfWork(ctx);

                var existingVehicle = await uow.Vehicles.GetByIdAsync(NewVehicle.VehicleID);
                if (existingVehicle == null)
                {
                    ShowError?.Invoke(SalonSamochodowy.Services.LocalizationHelper.GetString("Vehicles_NotFound"));
                    return;
                }

                // Check VIN uniqueness only if changed
                if (existingVehicle.VIN != NewVehicle.VIN.Trim().ToUpper())
                {
                    var vinExists = await uow.Vehicles.FindAsync(v => v.VIN == NewVehicle.VIN.Trim());
                    if (vinExists.Any())
                    {
                        ShowError?.Invoke(string.Format(SalonSamochodowy.Services.LocalizationHelper.GetString("Vehicles_VINExists"), NewVehicle.VIN));
                        return;
                    }
                }

                existingVehicle.VIN = NewVehicle.VIN.Trim().ToUpper();
                existingVehicle.EngineID = SelectedEngine.EngineID;
                existingVehicle.TrimID = SelectedTrim.TrimID;
                existingVehicle.Mileage = NewVehicle.Mileage;
                existingVehicle.IsUsed = NewVehicle.IsUsed;

                uow.Vehicles.Update(existingVehicle);
                await uow.CompleteAsync();

                ShowSuccess?.Invoke(SalonSamochodowy.Services.LocalizationHelper.GetString("Vehicles_SuccessEdited"));
                VehicleEdited?.Invoke();
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
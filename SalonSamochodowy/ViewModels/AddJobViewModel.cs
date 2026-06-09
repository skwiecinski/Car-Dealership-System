using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SalonSamochodowy.Entities;
using SalonSamochodowy.Services;
using SalonSamochodowy.Repositories;

namespace SalonSamochodowy.ViewModels
{
    public partial class AddJobViewModel : ObservableObject
    {
        private readonly IJobService _jobService;
        private readonly ICatalogService _catalogService;
        private readonly IVehicleService _vehicleService;
        private readonly IUnitOfWork _uow;
        private int _vehicleId;

        public AddJobViewModel(IJobService jobService, ICatalogService catalogService, IVehicleService vehicleService, IUnitOfWork uow)
        {
            _jobService = jobService;
            _catalogService = catalogService;
            _vehicleService = vehicleService;
            _uow = uow;
        }

        [ObservableProperty] private string vehicleInfo = SalonSamochodowy.Services.LocalizationHelper.GetString("Vehicles_Loading");

        public ObservableCollection<Feature> Features { get; } = new();
        [ObservableProperty] private Feature? selectedFeature;

        public ObservableCollection<WorkerDto> Mechanics { get; } = new();
        [ObservableProperty] private WorkerDto? selectedMechanic;

        public event Action? RequestClose;
        public event Action<string>? ShowError;
        public event Action? JobAdded;

        public async Task InitializeAsync(int vehicleId)
        {
            _vehicleId = vehicleId;
            try
            {
                var v = await _vehicleService.GetVehicleByIdAsync(vehicleId);
                if (v != null)
                {
                    VehicleInfo = $"VIN: {v.VIN}";
                }

                Features.Clear();
                var features = await _catalogService.GetAllFeaturesAsync();
                foreach (var f in features) Features.Add(f);

                Mechanics.Clear();
                var mechanics = await _catalogService.GetWorkersByRolesAsync("Serwisant");
                foreach (var m in mechanics) Mechanics.Add(new WorkerDto { WorkerID = m.WorkerID, FullName = m.FullName });
            }
            catch (Exception ex)
            {
                ShowError?.Invoke(ex.Message);
            }
        }

        [RelayCommand]
        private async Task SaveAsync()
        {
            if (SelectedFeature == null || SelectedMechanic == null)
            {
                ShowError?.Invoke(SalonSamochodowy.Services.LocalizationHelper.GetString("Vehicles_ValidationJob"));
                return;
            }

            try
            {
                await _jobService.AssignFeaturesAndCreateJobsAsync(_vehicleId, new[] { SelectedFeature.FeatureID }, SelectedMechanic.WorkerID);
                JobAdded?.Invoke();
                RequestClose?.Invoke();
            }
            catch (Exception ex)
            {
                ShowError?.Invoke(ex.Message);
            }
        }

        [RelayCommand]
        private void Cancel() => RequestClose?.Invoke();
    }
}

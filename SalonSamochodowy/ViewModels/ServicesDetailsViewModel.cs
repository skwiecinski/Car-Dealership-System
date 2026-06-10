using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SalonSamochodowy.Services;

namespace SalonSamochodowy.ViewModels
{
    public partial class ServicesDetailsViewModel : ObservableObject
    {
        private readonly IJobService _jobService;

        public ServicesDetailsViewModel(IJobService jobService)
        {
            _jobService = jobService;
        }

        private int _jobId;

        
        [ObservableProperty] private string jobCode = "—";
        [ObservableProperty] private string createdAt = "—";
        [ObservableProperty] private string featureName = "—";
        [ObservableProperty] private string featureCategory = "—";

        
        [ObservableProperty] private string vehicleVin = "—";
        [ObservableProperty] private string vehicleModel = "—";
        [ObservableProperty] private string vehicleEngine = "—";
        [ObservableProperty] private string vehicleStatus = "—";

        
        [ObservableProperty] private string workerName = "—";
        [ObservableProperty] private string workerEmail = "—";
        [ObservableProperty] private string dealershipName = "—";

        
        [ObservableProperty] private string currentStatus = "—";
        [ObservableProperty] private string statusBadgeBackground = "#1F2536";
        [ObservableProperty] private string statusBadgeBorder = "#3B82F6";
        [ObservableProperty] private string statusBadgeForeground = "#60A5FA";

        [ObservableProperty] private bool canMoveToPending = false;
        [ObservableProperty] private bool canMoveToInProgress = false;
        [ObservableProperty] private bool canMoveToFinished = false;

        
        public event Action<string>? ShowError;
        public event Action<string>? ShowSuccess;
        public event Action? CloseRequested;
        public event Action? StatusChanged;   

        public void Initialize(int jobId)
        {
            _jobId = jobId;
        }

        public async Task LoadAsync()
        {
            try
            {
                var job = await _jobService.GetJobDetailsAsync(_jobId);
                if (job == null)
                {
                    ShowError?.Invoke(SalonSamochodowy.Services.LocalizationHelper.GetString("ServicesDetails_NotFound"));
                    return;
                }

                JobCode = $"ZLS/{job.CreatedAt:yyyy}/{job.JobID:D4}";
                CreatedAt = job.CreatedAt.ToString("dd.MM.yyyy HH:mm");
                FeatureName = job.FeatureName;
                FeatureCategory = job.FeatureCategory;
                
                VehicleVin = job.VehicleVin;
                VehicleStatus = job.VehicleStatus;
                VehicleModel = job.VehicleModel;
                VehicleEngine = job.VehicleEngine;

                WorkerName = job.WorkerName;
                WorkerEmail = job.WorkerEmail;
                DealershipName = job.DealershipName;

                ApplyStatus(job.Status);
            }
            catch (Exception ex)
            {
                ShowError?.Invoke(string.Format(SalonSamochodowy.Services.LocalizationHelper.GetString("Vehicles_DataLoadError"), ex.Message));
            }
        }

        private void ApplyStatus(string status)
        {
            CurrentStatus = status switch
            {
                "PendingJob" or "Oczekujące" => SalonSamochodowy.Services.LocalizationHelper.GetString("Services_StatusPending"),
                "InProgressJob" or "W trakcie" => SalonSamochodowy.Services.LocalizationHelper.GetString("Services_StatusInProgress"),
                "FinishedJob" or "Zakończone" => SalonSamochodowy.Services.LocalizationHelper.GetString("Services_StatusFinished"),
                _ => status
            };

            var statusPending = SalonSamochodowy.Services.LocalizationHelper.GetString("Services_StatusPending");
            var statusInProgress = SalonSamochodowy.Services.LocalizationHelper.GetString("Services_StatusInProgress");
            var statusFinished = SalonSamochodowy.Services.LocalizationHelper.GetString("Services_StatusFinished");

            if (CurrentStatus == statusPending)
            {
                StatusBadgeBackground = "#332A12";
                StatusBadgeBorder = "#D3A125";
                StatusBadgeForeground = "#F0B82B";
            }
            else if (CurrentStatus == statusInProgress)
            {
                StatusBadgeBackground = "#1A2540";
                StatusBadgeBorder = "#3B82F6";
                StatusBadgeForeground = "#60A5FA";
            }
            else if (CurrentStatus == statusFinished)
            {
                StatusBadgeBackground = "#112C1E";
                StatusBadgeBorder = "#2D9A4A";
                StatusBadgeForeground = "#44C767";
            }
            else
            {
                StatusBadgeBackground = "#1F2536";
                StatusBadgeBorder = "#3B82F6";
                StatusBadgeForeground = "#60A5FA";
            }

            CanMoveToPending = false;
            CanMoveToInProgress = CurrentStatus == statusPending;
            CanMoveToFinished = CurrentStatus == statusInProgress;
        }

        [RelayCommand]
        private async Task SetPendingAsync() => await ChangeStatusAsync("Oczekujące");

        [RelayCommand]
        private async Task SetInProgressAsync() => await ChangeStatusAsync("W trakcie");

        [RelayCommand]
        private async Task SetFinishedAsync() => await ChangeStatusAsync("Zakończone");

        private async Task ChangeStatusAsync(string newStatus)
        {
            try
            {
                await _jobService.ChangeJobStatusAsync(_jobId, newStatus);
                await LoadAsync();
                ShowSuccess?.Invoke(string.Format(SalonSamochodowy.Services.LocalizationHelper.GetString("ServicesDetails_StatusSuccess"), CurrentStatus));
                StatusChanged?.Invoke();
            }
            catch (Exception ex)
            {
                ShowError?.Invoke(string.Format(SalonSamochodowy.Services.LocalizationHelper.GetString("ServicesDetails_StatusError"), ex.Message));
            }
        }

        [RelayCommand]
        private void Close() => CloseRequested?.Invoke();
    }
}

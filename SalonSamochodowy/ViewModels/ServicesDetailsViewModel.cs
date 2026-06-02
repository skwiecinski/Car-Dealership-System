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

        // ── Info o jobbie ──────────────────────────────────────────────
        [ObservableProperty] private string jobCode = "—";
        [ObservableProperty] private string createdAt = "—";
        [ObservableProperty] private string featureName = "—";
        [ObservableProperty] private string featureCategory = "—";

        // ── Pojazd ────────────────────────────────────────────────────
        [ObservableProperty] private string vehicleVin = "—";
        [ObservableProperty] private string vehicleModel = "—";
        [ObservableProperty] private string vehicleEngine = "—";
        [ObservableProperty] private string vehicleStatus = "—";

        // ── Serwisant ─────────────────────────────────────────────────
        [ObservableProperty] private string workerName = "—";
        [ObservableProperty] private string workerEmail = "—";
        [ObservableProperty] private string dealershipName = "—";

        // ── Status joba ───────────────────────────────────────────────
        [ObservableProperty] private string currentStatus = "—";
        [ObservableProperty] private string statusBadgeBackground = "#1F2536";
        [ObservableProperty] private string statusBadgeBorder = "#3B82F6";
        [ObservableProperty] private string statusBadgeForeground = "#60A5FA";

        [ObservableProperty] private bool canMoveToPending = false;
        [ObservableProperty] private bool canMoveToInProgress = false;
        [ObservableProperty] private bool canMoveToFinished = false;

        // ── Eventy ────────────────────────────────────────────────────
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
                    ShowError?.Invoke("Nie znaleziono zlecenia w bazie danych.");
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
                ShowError?.Invoke($"Błąd ładowania danych:\n{ex.Message}");
            }
        }

        private void ApplyStatus(string status)
        {
            CurrentStatus = status switch
            {
                "PendingJob" or "Oczekujące" => "Oczekujące",
                "InProgressJob" or "W trakcie" => "W trakcie",
                "FinishedJob" or "Zakończone" => "Zakończone",
                _ => status
            };

            (StatusBadgeBackground, StatusBadgeBorder, StatusBadgeForeground) = CurrentStatus switch
            {
                "Oczekujące" => ("#332A12", "#D3A125", "#F0B82B"),
                "W trakcie" => ("#1A2540", "#3B82F6", "#60A5FA"),
                "Zakończone" => ("#112C1E", "#2D9A4A", "#44C767"),
                _ => ("#1F2536", "#3B82F6", "#60A5FA"),
            };

            CanMoveToPending = CurrentStatus != "Oczekujące";
            CanMoveToInProgress = CurrentStatus != "W trakcie";
            CanMoveToFinished = CurrentStatus != "Zakończone";
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
                ApplyStatus(newStatus);
                ShowSuccess?.Invoke($"Status zmieniony na: {CurrentStatus}");
                StatusChanged?.Invoke();
            }
            catch (Exception ex)
            {
                ShowError?.Invoke($"Nie udało się zmienić statusu:\n{ex.Message}");
            }
        }

        [RelayCommand]
        private void Close() => CloseRequested?.Invoke();
    }
}

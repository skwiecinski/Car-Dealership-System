using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using SalonSamochodowy.Services;
using SalonSamochodowy.Repositories;

namespace SalonSamochodowy.ViewModels
{
    public partial class ServicesPageViewModel : ObservableObject
    {
        private readonly IJobService _jobService;
        private readonly IUnitOfWork _uow; 

        public ServicesPageViewModel(IUnitOfWork uow, IJobService jobService)
        {
            _uow = uow;
            _jobService = jobService;
            WeakReferenceMessenger.Default.Register(this, (ServicesPageViewModel r, SalonSamochodowy.Messages.LanguageChangedMessage m) =>
            {
                foreach (var j in r.PendingJobs) j.RefreshLocalization();
                foreach (var j in r.InProgressJobs) j.RefreshLocalization();
                foreach (var j in r.FinishedJobs) j.RefreshLocalization();
            });
        }

        public ObservableCollection<ServiceJob> PendingJobs { get; } = new();
        public ObservableCollection<ServiceJob> InProgressJobs { get; } = new();
        public ObservableCollection<ServiceJob> FinishedJobs { get; } = new();

        public event Action<string>? LoadFailed;

        public async Task LoadFromDbAsync()
        {
            var loggedInUser = SessionContext.CurrentUser;

            if (loggedInUser.Role == null)
            {
                loggedInUser.Role = await _uow.AppRoles.GetByIdAsync(loggedInUser.RoleID);
            }

            try
            {
                _uow.ClearTracker();
                var jobs = await _jobService.GetJobsForUserAsync(loggedInUser);

                PendingJobs.Clear();
                InProgressJobs.Clear();
                FinishedJobs.Clear();

                foreach (var job in jobs)
                {
                    var serviceJob = new ServiceJob
                    {
                        JobID = job.JobID,
                        TaskName = job.FeatureName,
                        CarModel = job.VehicleVin,
                        WorkerName = job.WorkerName,
                        Progress = job.Progress
                    };

                    switch (job.Status)
                    {
                        case "Oczekujące":
                        case "PendingJob":
                            PendingJobs.Add(serviceJob);
                            break;
                        case "W trakcie":
                        case "InProgressJob":
                            InProgressJobs.Add(serviceJob);
                            break;
                        case "Zakończone":
                        case "FinishedJob":
                            FinishedJobs.Add(serviceJob);
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                LoadFailed?.Invoke(string.Format(SalonSamochodowy.Services.LocalizationHelper.GetString("Services_LoadError"), ex.Message));
            }
        }
    }

    public class ServiceJob : ObservableObject
    {
        public int JobID { get; set; }
        public string TaskName { get; set; } = "";
        public string DisplayTaskName => SalonSamochodowy.Services.LocalizationHelper.GetString($"Feature_{TaskName.Replace(" ", "_")}");
        public string CarModel { get; set; } = "";
        public string WorkerName { get; set; } = "";
        public int Progress { get; set; }

        public void RefreshLocalization()
        {
            OnPropertyChanged(nameof(DisplayTaskName));
        }
    }
}

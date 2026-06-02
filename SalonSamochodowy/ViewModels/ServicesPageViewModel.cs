using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using SalonSamochodowy.Services;
using SalonSamochodowy.Repositories;

namespace SalonSamochodowy.ViewModels
{
    public partial class ServicesPageViewModel : ObservableObject
    {
        private readonly IJobService _jobService;
        private readonly IUnitOfWork _uow; // Potrzebne do odświeżenia roli w SessionContext

        public ServicesPageViewModel(IUnitOfWork uow, IJobService jobService)
        {
            _uow = uow;
            _jobService = jobService;
        }

        public ObservableCollection<ServiceJob> PendingJobs { get; } = new();
        public ObservableCollection<ServiceJob> InProgressJobs { get; } = new();
        public ObservableCollection<ServiceJob> FinishedJobs { get; } = new();

        public async Task LoadFromDbAsync()
        {
            var loggedInUser = SessionContext.CurrentUser;

            if (loggedInUser.Role == null)
            {
                loggedInUser.Role = await _uow.AppRoles.GetByIdAsync(loggedInUser.RoleID);
            }

            try
            {
                var jobs = await _jobService.GetJobsForUserAsync(loggedInUser);

                PendingJobs.Clear();
                InProgressJobs.Clear();
                FinishedJobs.Clear();

                foreach (var job in jobs)
                {
                    var serviceJob = new ServiceJob
                    {
                        JobID = job.JobID,
                        TaskName = job.CreatedAt.ToString("dd.MM.yyyy"),
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
            }
        }
    }

    public class ServiceJob
    {
        public int JobID { get; set; }
        public string TaskName { get; set; } = "";
        public string CarModel { get; set; } = "";
        public string WorkerName { get; set; } = "";
        public int Progress { get; set; }
    }
}

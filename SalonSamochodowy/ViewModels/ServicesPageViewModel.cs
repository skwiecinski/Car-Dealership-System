using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using SalonSamochodowy.Entities;
using SalonSamochodowy.Repositories;
using SalonSamochodowy.Services;

namespace SalonSamochodowy.ViewModels
{
    public partial class ServicesPageViewModel : ObservableObject
    {
        private readonly IUnitOfWork _uow;
    private readonly IVehicleService _vehicleService;

        public ServicesPageViewModel(IUnitOfWork uow, IVehicleService vehicleService)
        {
            _uow = uow;
        _vehicleService = vehicleService;
        }
        public ObservableCollection<ServiceJob> PendingJobs { get; } = new();
        public ObservableCollection<ServiceJob> InProgressJobs { get; } = new();
        public ObservableCollection<ServiceJob> FinishedJobs { get; } = new();
        public async Task LoadFromDbAsync()
        {
            var loggedInUser = SessionContext.CurrentUser;

            if (loggedInUser.Role == null)
            {
                var uowRole = _uow;
                loggedInUser.Role = await uowRole.AppRoles.GetByIdAsync(loggedInUser.RoleID);
            }

            try
            {
                var uow = _uow;
                IEnumerable<Job> allJobs = Enumerable.Empty<Job>();

                if (loggedInUser.Role?.RoleName == "Kierownik")
                {
                    allJobs = await uow.Jobs.GetAllAsync(); 
                }
                else if (loggedInUser.Role?.RoleName == "Serwisant")
                {
                    var worker = await uow.Workers.FindAsync(w => w.UserID == loggedInUser.UserID);
                    var workerObj = worker.FirstOrDefault();
                    if (workerObj != null)
                        allJobs = await uow.Jobs.FindAsync(j => j.WorkerID == workerObj.WorkerID);
                }

                PendingJobs.Clear();
                InProgressJobs.Clear();
                FinishedJobs.Clear();

                foreach (var job in allJobs)
                {
                    var vehicle = await _vehicleService.GetVehicleByIdAsync(job.VehicleID);
                    var worker = await uow.Workers.GetByIdAsync(job.WorkerID);
                    var workerUser = worker != null ? await uow.AppUsers.GetByIdAsync(worker.UserID) : null;

                    var serviceJob = new ServiceJob
                    {
                        JobID = job.JobID,
                        TaskName = job.CreatedAt.ToString("dd.MM.yyyy"), // tutaj trzeba tuning zajebać
                        CarModel = vehicle?.VIN ?? "Nieznany pojazd",
                        WorkerName = workerUser != null ? $"{workerUser.FirstName} {workerUser.LastName}" : "—",
                        Progress = 0 // klasa servicejob do poprawy
                    };


                    switch (job.Status)
                    {
                        case "Oczekujące" : // jako, że case wykonuje się aż do breaka, można ustawić ich kilka, bo jebaniec będzie jebał w dół, takżę zostaw to huju lepiej dla dobra ogółu
                        case "PendingJob" :
                            PendingJobs.Add(serviceJob);
                            break;
                        case "W trakcie" :
                        case "InProgressJob" :
                            InProgressJobs.Add(serviceJob);
                            break;
                        case "Zakończone":
                        case "FinishedJob" :
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

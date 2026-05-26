using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using SalonSamochodowy.Entities;
using SalonSamochodowy.Repositories;

namespace SalonSamochodowy.ViewModels
{
    public partial class ServicesPageViewModel : ObservableObject
    {
        public ObservableCollection<ServiceJob> PendingJobs { get; } = new();
        public ObservableCollection<ServiceJob> InProgressJobs { get; } = new();
        public ObservableCollection<ServiceJob> FinishedJobs { get; } = new();
        public async Task LoadFromDbAsync()
        {
            var loggedInUser = SessionContext.CurrentUser;

            if (loggedInUser.Role == null)
            {
                using var ctxRole = new AppDbContext();
                using var uowRole = new UnitOfWork(ctxRole);
                loggedInUser.Role = await uowRole.AppRoles.GetByIdAsync(loggedInUser.RoleID);
            }

            try
            {
                using var ctx = new AppDbContext();
                using var uow = new UnitOfWork(ctx);
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
                    // doczytaj nawigacyjne właściwości ręcznie
                    var vehicle = await uow.Vehicles.GetByIdAsync(job.VehicleID);
                    var worker = await uow.Workers.GetByIdAsync(job.WorkerID);
                    var workerUser = worker != null ? await uow.AppUsers.GetByIdAsync(worker.UserID) : null;

                    var serviceJob = new ServiceJob
                    {
                        TaskName = job.CreatedAt.ToString("dd.MM.yyyy"), // tutaj trzeba tuning zajebać
                        CarModel = vehicle?.VIN ?? "Nieznany pojazd",
                        WorkerName = workerUser != null ? $"{workerUser.FirstName} {workerUser.LastName}" : "—",
                        Progress = 0
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
        public string TaskName { get; set; } = "";
        public string CarModel { get; set; } = "";
        public string WorkerName { get; set; } = "";
        public int Progress { get; set; }
    }
}

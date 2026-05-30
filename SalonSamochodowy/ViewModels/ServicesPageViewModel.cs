using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
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

        public event Action<string>? LoadFailed;

        public async Task LoadFromDbAsync()
        {
            var loggedInUser = SessionContext.CurrentUser;
            if (loggedInUser == null) return;

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
                    var workers = await uow.Workers.FindAsync(w => w.UserID == loggedInUser.UserID);
                    var workerObj = workers.FirstOrDefault();
                    if (workerObj != null)
                        allJobs = await uow.Jobs.FindAsync(j => j.WorkerID == workerObj.WorkerID);
                }

                PendingJobs.Clear();
                InProgressJobs.Clear();
                FinishedJobs.Clear();

                foreach (var job in allJobs)
                {
                    var vehicle = await uow.Vehicles.GetByIdAsync(job.VehicleID);
                    var feature = await uow.Features.GetByIdAsync(job.FeatureID);
                    var worker = await uow.Workers.GetByIdAsync(job.WorkerID);
                    var workerUser = worker != null
                        ? await uow.AppUsers.GetByIdAsync(worker.UserID)
                        : null;

                    string carLabel = vehicle?.VIN ?? "Nieznany pojazd";
                    if (vehicle != null)
                    {
                        var trim = await uow.TrimLevels.GetByIdAsync(vehicle.TrimID);
                        var model = trim != null ? await uow.VehicleModels.GetByIdAsync(trim.ModelID) : null;
                        if (model != null && trim != null)
                            carLabel = $"{model.Brand} {model.ModelName} {trim.TrimName}";
                    }

                    var serviceJob = new ServiceJob
                    {
                        JobID = job.JobID,
                        TaskName = feature?.FeatureName ?? $"Zlecenie #{job.JobID}",
                        CarModel = carLabel,
                        WorkerName = workerUser != null
                            ? $"{workerUser.FirstName} {workerUser.LastName}".Trim()
                            : "—",
                        Progress = job.Status == "Zakończone" ? 100
                                 : job.Status == "W trakcie" ? 50
                                 : 0
                    };

                    switch (job.Status)
                    {
                        case "Oczekujące":
                            PendingJobs.Add(serviceJob);
                            break;
                        case "W trakcie":
                            InProgressJobs.Add(serviceJob);
                            break;
                        case "Zakończone":
                            FinishedJobs.Add(serviceJob);
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                LoadFailed?.Invoke($"Nie udało się załadować zleceń serwisowych:\n{ex.Message}");
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
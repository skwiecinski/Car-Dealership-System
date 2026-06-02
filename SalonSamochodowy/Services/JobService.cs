using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SalonSamochodowy.Entities;
using SalonSamochodowy.Repositories;

namespace SalonSamochodowy.Services
{
    public class JobService : IJobService
    {
        private readonly IUnitOfWork _uow;
        private readonly IVehicleService _vehicleService;

        public JobService(IUnitOfWork uow, IVehicleService vehicleService)
        {
            _uow = uow;
            _vehicleService = vehicleService;
        }

        public async Task<IEnumerable<JobDto>> GetJobsForUserAsync(AppUser user)
        {
            IEnumerable<Job> allJobs = Enumerable.Empty<Job>();

            if (user.Role?.RoleName == "Kierownik")
            {
                allJobs = await _uow.Jobs.GetAllAsync();
            }
            else if (user.Role?.RoleName == "Serwisant")
            {
                var worker = (await _uow.Workers.FindAsync(w => w.UserID == user.UserID)).FirstOrDefault();
                if (worker != null)
                {
                    allJobs = await _uow.Jobs.FindAsync(j => j.WorkerID == worker.WorkerID);
                }
            }

            var dtos = new List<JobDto>();
            foreach (var job in allJobs)
            {
                var vehicle = await _vehicleService.GetVehicleByIdAsync(job.VehicleID);
                var worker = await _uow.Workers.GetByIdAsync(job.WorkerID);
                var workerUser = worker != null ? await _uow.AppUsers.GetByIdAsync(worker.UserID) : null;

                dtos.Add(new JobDto
                {
                    JobID = job.JobID,
                    CreatedAt = job.CreatedAt,
                    VehicleVin = vehicle?.VIN ?? "Nieznany pojazd",
                    WorkerName = workerUser != null ? $"{workerUser.FirstName} {workerUser.LastName}" : "—",
                    Status = job.Status,
                    Progress = 0
                });
            }
            return dtos;
        }

        public async Task<JobDetailsDto?> GetJobDetailsAsync(int jobId)
        {
            var job = await _uow.Jobs.GetByIdAsync(jobId);
            if (job == null) return null;

            var feature = await _uow.Features.GetByIdAsync(job.FeatureID);
            var vehicle = await _vehicleService.GetVehicleByIdAsync(job.VehicleID);
            
            string vehicleModel = "—";
            string vehicleEngine = "—";

            if (vehicle != null)
            {
                var trim = await _uow.TrimLevels.GetByIdAsync(vehicle.TrimID);
                var model = trim != null ? await _uow.VehicleModels.GetByIdAsync(trim.ModelID) : null;
                vehicleModel = model != null && trim != null ? $"{model.Brand} {model.ModelName} {trim.TrimName}" : "—";

                var engine = await _uow.Engines.GetByIdAsync(vehicle.EngineID);
                vehicleEngine = engine != null ? $"{engine.EngineName} • {engine.Power} KM" : "—";
            }

            var worker = await _uow.Workers.GetByIdAsync(job.WorkerID);
            var workerUser = worker != null ? await _uow.AppUsers.GetByIdAsync(worker.UserID) : null;
            var dealership = worker != null ? await _uow.Dealerships.GetByIdAsync(worker.DealershipID) : null;

            return new JobDetailsDto
            {
                JobID = job.JobID,
                CreatedAt = job.CreatedAt,
                VehicleVin = vehicle?.VIN ?? "—",
                WorkerName = workerUser != null ? $"{workerUser.FirstName} {workerUser.LastName}".Trim() : "—",
                Status = job.Status,
                Progress = 0,
                FeatureName = feature?.FeatureName ?? "—",
                FeatureCategory = feature?.Category ?? "—",
                VehicleStatus = vehicle?.Status ?? "—",
                VehicleModel = vehicleModel,
                VehicleEngine = vehicleEngine,
                WorkerEmail = workerUser?.Email ?? "—",
                DealershipName = dealership != null ? $"{dealership.Name} ({dealership.City})" : "—"
            };
        }

        public async Task ChangeJobStatusAsync(int jobId, string newStatus)
        {
            var job = await _uow.Jobs.GetByIdAsync(jobId);
            if (job == null) throw new Exception("Nie znaleziono zlecenia.");

            job.Status = newStatus;
            _uow.Jobs.Update(job);
            await _uow.CompleteAsync();
        }

        public async Task AssignFeaturesAndCreateJobsAsync(int vehicleId, IEnumerable<int> featureIds, int workerId)
        {
            foreach (var featureId in featureIds)
            {
                await _uow.VehicleFeatures.AddAsync(new VehicleFeature
                {
                    VehicleID = vehicleId,
                    FeatureID = featureId,
                    PurchasePrice = 0m
                });

                await _uow.Jobs.AddAsync(new Job
                {
                    VehicleID = vehicleId,
                    FeatureID = featureId,
                    WorkerID = workerId,
                    Status = "Oczekujące",
                    CreatedAt = DateTime.Now
                });
            }
            await _uow.CompleteAsync();
        }

        public async Task<int> GetActiveJobsCountAsync()
        {
            return await _uow.Jobs.CountAsync(j => j.Status == "Oczekujące" || j.Status == "W trakcie" || j.Status == "PendingJob" || j.Status == "InProgressJob");
        }
    }
}

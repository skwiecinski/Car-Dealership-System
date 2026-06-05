using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SalonSamochodowy.Entities;

namespace SalonSamochodowy.Services
{
    public class JobDto
    {
        public int JobID { get; set; }
        public DateTime CreatedAt { get; set; }
        public string VehicleVin { get; set; } = "";
        public string WorkerName { get; set; } = "";
        public string Status { get; set; } = "";
        public int Progress { get; set; }
        public string FeatureName { get; set; } = "";
    }

    public class JobDetailsDto : JobDto
    {
        public string FeatureCategory { get; set; } = "";
        public string VehicleStatus { get; set; } = "";
        public string VehicleModel { get; set; } = "";
        public string VehicleEngine { get; set; } = "";
        public string WorkerEmail { get; set; } = "";
        public string DealershipName { get; set; } = "";
    }

    public interface IJobService
    {
        Task<IEnumerable<JobDto>> GetJobsForUserAsync(AppUser user);
        Task<JobDetailsDto?> GetJobDetailsAsync(int jobId);
        Task ChangeJobStatusAsync(int jobId, string newStatus);
        Task AssignFeaturesAndCreateJobsAsync(int vehicleId, IEnumerable<int> featureIds, int workerId);
        Task<int> GetActiveJobsCountAsync();
    }
}

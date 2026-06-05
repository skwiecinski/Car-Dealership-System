using System.Collections.Generic;
using System.Threading.Tasks;
using SalonSamochodowy.Entities;

namespace SalonSamochodowy.Services
{
    public class WorkerDto
    {
        public int WorkerID { get; set; }
        public int UserID { get; set; }
        public string FullName { get; set; } = "";
        public string RoleName { get; set; } = "";
    }

    public interface ICatalogService
    {
        Task<IEnumerable<string>> GetBrandsAsync();
        Task<IEnumerable<VehicleModel>> GetModelsByBrandAsync(string brand);
        Task<IEnumerable<Engine>> GetEnginesByBrandAsync(string brand);
        Task<IEnumerable<TrimLevel>> GetTrimsByModelAsync(int modelId);
        Task<IEnumerable<Feature>> GetAllFeaturesAsync();
        Task<Dealership?> GetMainDealershipAsync();

        Task<IEnumerable<WorkerDto>> GetWorkersByRolesAsync(params string[] roleNames);
        Task<WorkerDto?> GetWorkerByIdAsync(int workerId);

        Task<TrimLevel?> GetTrimByIdAsync(int trimId);
        Task<VehicleModel?> GetModelByIdAsync(int modelId);

        Task CreateVehicleModelAsync(VehicleModel model);
        Task DeleteVehicleModelAsync(int modelId);
        
        Task CreateFeatureAsync(Feature feature);
        Task DeleteFeatureAsync(int featureId);
        
        Task CreateEngineAsync(Engine engine);
        Task DeleteEngineAsync(int engineId);
        
        Task CreateTrimLevelAsync(TrimLevel trim);
        Task DeleteTrimLevelAsync(int trimId);
    }
}

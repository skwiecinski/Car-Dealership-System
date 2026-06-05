using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SalonSamochodowy.Entities;
using SalonSamochodowy.Repositories;

namespace SalonSamochodowy.Services
{
    public class CatalogService : ICatalogService
    {
        private readonly IUnitOfWork _uow;

        public CatalogService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<IEnumerable<string>> GetBrandsAsync()
        {
            var models = await _uow.VehicleModels.GetAllAsync();
            return models.Select(m => m.Brand).Distinct().OrderBy(b => b);
        }

        public async Task<IEnumerable<VehicleModel>> GetModelsByBrandAsync(string brand)
        {
            var models = await _uow.VehicleModels.FindAsync(m => m.Brand == brand);
            return models.OrderBy(m => m.ModelName);
        }

        public async Task<IEnumerable<Engine>> GetEnginesByBrandAsync(string brand)
        {
            var engines = await _uow.Engines.FindAsync(e => e.Brand == brand);
            return engines.OrderBy(e => e.Power);
        }

        public async Task<IEnumerable<TrimLevel>> GetTrimsByModelAsync(int modelId)
        {
            var trims = await _uow.TrimLevels.FindAsync(t => t.ModelID == modelId);
            return trims.OrderBy(t => t.BasePrice);
        }

        public async Task<IEnumerable<Feature>> GetAllFeaturesAsync()
        {
            var features = await _uow.Features.GetAllAsync();
            return features.OrderBy(f => f.Category).ThenBy(f => f.FeatureName);
        }

        public async Task<Dealership?> GetMainDealershipAsync()
        {
            var dealerships = await _uow.Dealerships.GetAllAsync();
            return dealerships.FirstOrDefault();
        }

        public async Task<IEnumerable<WorkerDto>> GetWorkersByRolesAsync(params string[] roleNames)
        {
            var roles = new List<AppRole>();
            foreach (var rn in roleNames)
            {
                var r = (await _uow.AppRoles.FindAsync(ar => ar.RoleName == rn)).FirstOrDefault();
                if (r != null) roles.Add(r);
            }

            var roleIds = roles.Select(r => r.RoleID).ToList();
            if (!roleIds.Any()) return Enumerable.Empty<WorkerDto>();

            var validUsers = (await _uow.AppUsers.FindAsync(u => roleIds.Contains(u.RoleID))).ToList();
            var userIds = validUsers.Select(u => u.UserID).ToList();

            var validWorkers = await _uow.Workers.FindAsync(w => userIds.Contains(w.UserID));

            var result = new List<WorkerDto>();
            foreach (var w in validWorkers)
            {
                var user = validUsers.FirstOrDefault(u => u.UserID == w.UserID);
                if (user == null) continue;
                
                var role = roles.FirstOrDefault(r => r.RoleID == user.RoleID);

                result.Add(new WorkerDto
                {
                    WorkerID = w.WorkerID,
                    UserID = user.UserID,
                    FullName = $"{user.FirstName} {user.LastName}".Trim(),
                    RoleName = role?.RoleName ?? ""
                });
            }
            return result;
        }

        public async Task<WorkerDto?> GetWorkerByIdAsync(int workerId)
        {
            var worker = await _uow.Workers.GetByIdAsync(workerId);
            if (worker == null) return null;

            var user = await _uow.AppUsers.GetByIdAsync(worker.UserID);
            if (user == null) return null;

            var role = await _uow.AppRoles.GetByIdAsync(user.RoleID);

            return new WorkerDto
            {
                WorkerID = worker.WorkerID,
                UserID = user.UserID,
                FullName = $"{user.FirstName} {user.LastName}".Trim(),
                RoleName = role?.RoleName ?? ""
            };
        }

        public async Task<TrimLevel?> GetTrimByIdAsync(int trimId)
        {
            return await _uow.TrimLevels.GetByIdAsync(trimId);
        }

        public async Task<VehicleModel?> GetModelByIdAsync(int modelId)
        {
            return await _uow.VehicleModels.GetByIdAsync(modelId);
        }

        public async Task CreateVehicleModelAsync(VehicleModel model)
        {
            await _uow.VehicleModels.AddAsync(model);
            await _uow.CompleteAsync();
        }

        public async Task DeleteVehicleModelAsync(int modelId)
        {
            var model = await _uow.VehicleModels.GetByIdAsync(modelId);
            if (model == null) return;

            var trims = await _uow.TrimLevels.FindAsync(t => t.ModelID == modelId);
            if (trims.Any())
                throw new InvalidOperationException("Nie można usunąć modelu, ponieważ posiada przypisane wersje wyposażenia.");

            _uow.VehicleModels.Delete(model);
            await _uow.CompleteAsync();
        }

        public async Task CreateFeatureAsync(Feature feature)
        {
            await _uow.Features.AddAsync(feature);
            await _uow.CompleteAsync();
        }

        public async Task DeleteFeatureAsync(int featureId)
        {
            var feature = await _uow.Features.GetByIdAsync(featureId);
            if (feature == null) return;

            var vehicleFeatures = await _uow.VehicleFeatures.FindAsync(vf => vf.FeatureID == featureId);
            if (vehicleFeatures.Any())
                throw new InvalidOperationException("Nie można usunąć opcji/usługi, ponieważ jest przypisana do pojazdu.");

            var jobs = await _uow.Jobs.FindAsync(j => j.FeatureID == featureId);
            if (jobs.Any())
                throw new InvalidOperationException("Nie można usunąć opcji/usługi, ponieważ istnieją powiązane z nią zlecenia serwisowe.");

            _uow.Features.Delete(feature);
            await _uow.CompleteAsync();
        }

        public async Task CreateEngineAsync(Engine engine)
        {
            await _uow.Engines.AddAsync(engine);
            await _uow.CompleteAsync();
        }

        public async Task DeleteEngineAsync(int engineId)
        {
            var engine = await _uow.Engines.GetByIdAsync(engineId);
            if (engine == null) return;

            var vehicles = await _uow.Vehicles.FindAsync(v => v.EngineID == engineId);
            if (vehicles.Any())
                throw new InvalidOperationException("Nie można usunąć silnika, ponieważ jest on używany przez pojazdy w bazie.");

            _uow.Engines.Delete(engine);
            await _uow.CompleteAsync();
        }

        public async Task CreateTrimLevelAsync(TrimLevel trim)
        {
            await _uow.TrimLevels.AddAsync(trim);
            await _uow.CompleteAsync();
        }

        public async Task DeleteTrimLevelAsync(int trimId)
        {
            var trim = await _uow.TrimLevels.GetByIdAsync(trimId);
            if (trim == null) return;

            var vehicles = await _uow.Vehicles.FindAsync(v => v.TrimID == trimId);
            if (vehicles.Any())
                throw new InvalidOperationException("Nie można usunąć wersji wyposażenia, ponieważ jest przypisana do pojazdów.");

            _uow.TrimLevels.Delete(trim);
            await _uow.CompleteAsync();
        }
    }
}

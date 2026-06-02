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
    }
}

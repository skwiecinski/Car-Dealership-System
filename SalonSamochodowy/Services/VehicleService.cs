using System.Collections.Generic;
using System.Threading.Tasks;
using SalonSamochodowy.Entities;
using SalonSamochodowy.Repositories;

namespace SalonSamochodowy.Services
{
    public class VehicleService : IVehicleService
    {
        private readonly IUnitOfWork _uow;

        public VehicleService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<IEnumerable<Vehicle>> GetAvailableVehiclesAsync()
        {
            return await _uow.Vehicles.FindAsync(v => v.Status == "Dostępny");
        }

        public async Task AddVehicleAsync(Vehicle vehicle)
        {
            await _uow.Vehicles.AddAsync(vehicle);
            await _uow.CompleteAsync();
        }

        public async Task<int> GetAvailableVehiclesCountAsync()
        {
            return await _uow.Vehicles.CountAsync(v => v.Status == "Dostępny");
        }

        public async Task<Vehicle?> GetVehicleByIdAsync(int id)
        {
            return await _uow.Vehicles.GetByIdAsync(id);
        }

        public async Task<IEnumerable<Vehicle>> GetVehiclesByStatusAsync(string status)
        {
            return await _uow.Vehicles.FindAsync(v => v.Status == status);
        }

        public async Task<IEnumerable<Vehicle>> GetAllVehiclesAsync()
        {
            return await _uow.Vehicles.GetAllAsync();
        }

        public async Task UpdateVehicleAsync(Vehicle vehicle)
        {
            _uow.Vehicles.Update(vehicle);
            await _uow.CompleteAsync();
        }

        public async Task DeleteVehicleAsync(Vehicle vehicle)
        {
            _uow.Vehicles.Delete(vehicle);
            await _uow.CompleteAsync();
        }

        public async Task AddVehicleFeatureAsync(VehicleFeature vf)
        {
            await _uow.VehicleFeatures.AddAsync(vf);
            await _uow.CompleteAsync();
        }
    }
}

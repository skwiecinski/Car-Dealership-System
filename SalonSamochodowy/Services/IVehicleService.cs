using System.Collections.Generic;
using System.Threading.Tasks;
using SalonSamochodowy.Entities;

namespace SalonSamochodowy.Services
{
    public interface IVehicleService
    {
        Task<IEnumerable<Vehicle>> GetAvailableVehiclesAsync();
        Task AddVehicleAsync(Vehicle vehicle);
        Task<int> GetAvailableVehiclesCountAsync();
        Task<Vehicle?> GetVehicleByIdAsync(int id);
        Task<IEnumerable<Vehicle>> GetVehiclesByStatusAsync(string status);
        Task<IEnumerable<Vehicle>> GetAllVehiclesAsync();
        Task UpdateVehicleAsync(Vehicle vehicle);
        Task DeleteVehicleAsync(Vehicle vehicle);
        Task AddVehicleFeatureAsync(VehicleFeature vf);
    }
}

using System;
using System.Threading.Tasks;
using SalonSamochodowy.Entities;

namespace SalonSamochodowy.Repositories
{
    /**
     * Interface for the Unit of Work pattern, managing multiple repositories and database transactions.
     * 
     * https://medium.com/@differentiate.function/understanding-unit-of-work-in-c-and-its-implementation-with-entity-framework-core-613552995237
     * https://www.netmentor.es/entrada/en/unit-of-work
     * https://learn.microsoft.com/en-us/aspnet/mvc/overview/older-versions/getting-started-with-ef-5-using-mvc-4/implementing-the-repository-and-unit-of-work-patterns-in-an-asp-net-mvc-application
     */
    public interface IUnitOfWork : IDisposable
    {
        IRepository<AppRole> AppRoles { get; }
        IRepository<AppUser> AppUsers { get; }
        IRepository<Client> Clients { get; }
        IRepository<Dealership> Dealerships { get; }
        IRepository<Engine> Engines { get; }
        IRepository<Feature> Features { get; }
        IRepository<Job> Jobs { get; }
        IRepository<SalesOrder> SalesOrders { get; }
        IRepository<TrimFeature> TrimFeatures { get; }
        IRepository<TrimLevel> TrimLevels { get; }
        IRepository<Vehicle> Vehicles { get; }
        IRepository<VehicleFeature> VehicleFeatures { get; }
        IRepository<VehicleModel> VehicleModels { get; }
        IRepository<Worker> Workers { get; }

        /**
         * Saves all changes made in this context to the database.
         * 
         * @return the number of state entries written to the database.
         */
        Task<int> CompleteAsync();

        /**
         * Clears the EF Core Change Tracker so that subsequent queries will fetch fresh data from the database.
         */
        void ClearTracker();
    }
}
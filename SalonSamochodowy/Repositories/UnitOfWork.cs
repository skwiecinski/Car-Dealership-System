using System.Threading.Tasks;
using SalonSamochodowy.Entities;

namespace SalonSamochodowy.Repositories
{
    /**
     * Implementation of the Unit of Work pattern, coordinating the work of multiple repositories.
     */
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;

        private IRepository<AppRole>? _appRoles;
        private IRepository<AppUser>? _appUsers;
        private IRepository<Client>? _clients;
        private IRepository<Dealership>? _dealerships;
        private IRepository<Engine>? _engines;
        private IRepository<Feature>? _features;
        private IRepository<Job>? _jobs;
        private IRepository<SalesOrder>? _salesOrders;
        private IRepository<TrimFeature>? _trimFeatures;
        private IRepository<TrimLevel>? _trimLevels;
        private IRepository<Vehicle>? _vehicles;
        private IRepository<VehicleFeature>? _vehicleFeatures;
        private IRepository<VehicleModel>? _vehicleModels;
        private IRepository<Worker>? _workers;

        /**
         * Initializes a new instance of the UnitOfWork class.
         * 
         * @param context the application database context.
         */
        public UnitOfWork(AppDbContext context)
        {
            _context = context;
        }

        public IRepository<AppRole> AppRoles => _appRoles ??= new Repository<AppRole>(_context);
        public IRepository<AppUser> AppUsers => _appUsers ??= new Repository<AppUser>(_context);
        public IRepository<Client> Clients => _clients ??= new Repository<Client>(_context);
        public IRepository<Dealership> Dealerships => _dealerships ??= new Repository<Dealership>(_context);
        public IRepository<Engine> Engines => _engines ??= new Repository<Engine>(_context);
        public IRepository<Feature> Features => _features ??= new Repository<Feature>(_context);
        public IRepository<Job> Jobs => _jobs ??= new Repository<Job>(_context);
        public IRepository<SalesOrder> SalesOrders => _salesOrders ??= new Repository<SalesOrder>(_context);
        public IRepository<TrimFeature> TrimFeatures => _trimFeatures ??= new Repository<TrimFeature>(_context);
        public IRepository<TrimLevel> TrimLevels => _trimLevels ??= new Repository<TrimLevel>(_context);
        public IRepository<Vehicle> Vehicles => _vehicles ??= new Repository<Vehicle>(_context);
        public IRepository<VehicleFeature> VehicleFeatures => _vehicleFeatures ??= new Repository<VehicleFeature>(_context);
        public IRepository<VehicleModel> VehicleModels => _vehicleModels ??= new Repository<VehicleModel>(_context);
        public IRepository<Worker> Workers => _workers ??= new Repository<Worker>(_context);

        /**
         * Saves all changes made in this context to the database.
         * 
         * @return the number of state entries written to the database.
         */
        public async Task<int> CompleteAsync()
        {
            return await _context.SaveChangesAsync();
        }

        /**
         * Disposes the database context.
         */
        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
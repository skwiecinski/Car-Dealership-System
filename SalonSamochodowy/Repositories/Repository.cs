using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SalonSamochodowy.Entities;

namespace SalonSamochodowy.Repositories
{
    /**
     * Generic implementation of the IRepository interface using Entity Framework Core.
     * 
     * @param T the database entity type.
     */
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly AppDbContext _context;
        protected readonly DbSet<T> _dbSet;

        /**
         * Initializes a new instance of the Repository class.
         * 
         * @param context the application database context.
         */
        public Repository(AppDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        /**
         * Retrieves all records of a given entity from the database.
         * 
         * @return a collection of all items.
         */
        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        /**
         * Retrieves a single record based on its identifier.
         * 
         * @param id the primary key of the record.
         * @return the found entity object or null if it does not exist.
         */
        public async Task<T?> GetByIdAsync(int id)
        {
            return await _dbSet.FindAsync(id);
        }

        /**
         * Adds a new record to the database context.
         * 
         * @param entity the object to add.
         */
        public async Task AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
        }

        /**
         * Finds records that match the specified condition.
         * 
         * @param predicate the condition to filter the records.
         * @return a collection of items matching the condition.
         */
        public async Task<IEnumerable<T>> FindAsync(System.Linq.Expressions.Expression<System.Func<T, bool>> predicate)
        {
            return await _dbSet.Where(predicate).ToListAsync();
        }

        /**
         * Retrieves records sorted in descending order by the given key, taking only N latest items.
         * Filtering and sorting are performed in the database (not in memory).
         */
        public async Task<IEnumerable<T>> GetTopOrderedDescAsync<TKey>(System.Linq.Expressions.Expression<System.Func<T, TKey>> orderByDesc, int take)
        {
            return await _dbSet.OrderByDescending(orderByDesc).Take(take).ToListAsync();
        }

        /**
         * Counts records matching the specified condition (filtering performed in the database).
         */
        public async Task<int> CountAsync(System.Linq.Expressions.Expression<System.Func<T, bool>> predicate)
        {
            return await _dbSet.CountAsync(predicate);
        }

        /**
         * Updates an existing record in the database context.
         *
         * @param entity the object with updated data.
         */
        public void Update(T entity)
        {
            _dbSet.Update(entity);
        }

        /**
         * Removes a record from the database context.
         * 
         * @param entity the object to remove.
         */
        public void Delete(T entity)
        {
            _dbSet.Remove(entity);
        }
    }
}
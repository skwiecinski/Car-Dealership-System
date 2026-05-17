using System.Collections.Generic;
using System.Threading.Tasks;

namespace SalonSamochodowy.Repositories
{
    /**
     * Generic repository interface defining basic CRUD operations for an entity.
     * 
     * https://www.geeksforgeeks.org/system-design/repository-design-pattern/
     * https://martinfowler.com/eaaCatalog/repository.html
     * https://learn.microsoft.com/en-us/aspnet/mvc/overview/older-versions/getting-started-with-ef-5-using-mvc-4/implementing-the-repository-and-unit-of-work-patterns-in-an-asp-net-mvc-application
     * 
     * @param T the database entity type (e.g., Vehicle, Client).
     */
    public interface IRepository<T> where T : class
    {
        /**
         * Retrieves all records of a given entity from the database.
         * 
         * @return a collection of all items.
         */
        Task<IEnumerable<T>> GetAllAsync();

        /**
         * Retrieves a single record based on its identifier.
         * @param id the primary key of the record.
         * @return the found entity object or null if it does not exist.
         */
        Task<T?> GetByIdAsync(int id);

        /**
         * Adds a new record to the database context (requires a subsequent save).
         * 
         * @param entity the object to add.
         */
        Task AddAsync(T entity);

        /**
         * Finds records that match the specified condition.
         * 
         * @param predicate the condition to filter the records.
         * @return a collection of items matching the condition.
         */
        Task<IEnumerable<T>> FindAsync(System.Linq.Expressions.Expression<System.Func<T, bool>> predicate);

        /**
         * Updates an existing record in the database context.
         * 
         * @param entity the object with updated data.
         */
        void Update(T entity);

        /**
         * Removes a record from the database context.
         * 
         * @param entity the object to remove.
         */
        void Delete(T entity);
    }
}
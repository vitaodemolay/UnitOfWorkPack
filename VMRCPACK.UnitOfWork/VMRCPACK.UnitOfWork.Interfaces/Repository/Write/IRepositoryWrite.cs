using System.Threading.Tasks;

namespace VMRCPACK.UnitOfWork.Interfaces.Repository
{

    /// <summary>
    /// Interface of Repository Pattern implemetation basics methods for create, update and remove 
    /// </summary>
    /// <typeparam name="T">Type of Entity object</typeparam>
    public interface IRepositoryWrite<T> where T : class
    {
        /// <summary>
        /// Create Method
        /// </summary>
        /// <param name="entity">Entity object instance</param>
        void Add(T entity);

        /// <summary>
        /// Update Method
        /// </summary>
        /// <param name="entity">Entity object instance</param>
        void Update(T entity);

        /// <summary>
        /// Remove Method
        /// </summary>
        /// <param name="entity">Entity object instance</param>
        void Remove(T entity);
    }


    /// <summary>
    /// Interface of Repository Pattern implemetation basics async methods for create, update and remove 
    /// </summary>
    /// <typeparam name="T">Type of Entity object</typeparam>
    public interface IRepositoryWriteAsync<T> where T : class
    {
        /// <summary>
        /// Async Create Method
        /// </summary>
        /// <param name="entity">Entity object instance</param>
        Task AddAsync(T entity);

        /// <summary>
        /// Async Update Method
        /// </summary>
        /// <param name="entity">Entity object instance</param>
        Task UpdateAsync(T entity);

        /// <summary>
        /// Async Remove Method
        /// </summary>
        /// <param name="entity">Entity object instance</param>
        Task RemoveAsync(T entity);
    }
}

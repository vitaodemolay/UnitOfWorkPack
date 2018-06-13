using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace VMRCPACK.UnitOfWork.Interfaces.Repository
{
    /// <summary>
    /// Interface of Repository Pattern implemetation Extended search data methods  
    /// </summary>
    /// <typeparam name="T">Type of Entity object</typeparam>
    public interface IRepositoryReadOnlyExtended<T> where T : class
    {
        /// <summary>
        /// Method for Reload entity object with update data on database
        /// </summary>
        /// <param name="entity">Entity object instance</param>
        void Reload(ref T entity);

        /// <summary>
        /// Method for Count of registers
        /// </summary>
        /// <param name="filter">Object Filter to apply on count</param>
        /// <returns></returns>
        int Count(Expression<Func<T, bool>> filter = null);

        /// <summary>
        /// Method for verify exists of registers
        /// </summary>
        /// <param name="filter"> Object Filter to apply on verify</param>
        /// <returns></returns>
        bool Exists(Expression<Func<T, bool>> filter);
    }


    /// <summary>
    /// Interface of Repository Pattern implemetation Extended search data async methods  
    /// </summary>
    /// <typeparam name="T">Type of Entity object</typeparam>
    public interface IRepositoryReadOnlyExtendedAsync<T> where T : class
    {
        /// <summary>
        /// Async Method for Count of registers
        /// </summary>
        /// <param name="filter">Object Filter to apply on count</param>
        /// <returns></returns>
        Task<int> CountAsync(Expression<Func<T, bool>> filter = null);

        /// <summary>
        /// Async Method for verify exists of registers
        /// </summary>
        /// <param name="filter"> Object Filter to apply on verify</param>
        /// <returns></returns>
        Task<bool> ExistsAsync(Expression<Func<T, bool>> filter);
    }
}

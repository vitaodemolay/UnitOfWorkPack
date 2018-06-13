using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace VMRCPACK.UnitOfWork.Interfaces.Repository
{
    /// <summary>
    /// Interface of Repository Pattern implemetation basics search data methods  
    /// </summary>
    /// <typeparam name="T">Type of Entity object</typeparam>
    public interface IRepositoryReadOnly<T> where T : class
    {
        /// <summary>
        /// Search method of a single object
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        T Find(Expression<Func<T, bool>> filter = null);

        /// <summary>
        /// Search method of many objects
        /// </summary>
        /// <param name="filter">Object Filter to apply on search</param>
        /// <returns></returns>
        IList<T> FindAll(Expression<Func<T, bool>> filter = null);
    }


    /// <summary>
    /// Interface of Repository Pattern implemetation basics search data  async methods  
    /// </summary>
    /// <typeparam name="T">Type of Entity object</typeparam>
    public interface IRepositoryReadOnlyAsync<T> where T : class
    {
        /// <summary>
        /// Async Search method of a single object
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        Task<T> FindAsync(Expression<Func<T, bool>> filter = null);

        /// <summary>
        /// Async Search method of many objects
        /// </summary>
        /// <param name="filter">Object Filter to apply on search</param>
        /// <returns></returns>
        Task<List<T>> FindAllAsync(Expression<Func<T, bool>> filter = null);
    }
}

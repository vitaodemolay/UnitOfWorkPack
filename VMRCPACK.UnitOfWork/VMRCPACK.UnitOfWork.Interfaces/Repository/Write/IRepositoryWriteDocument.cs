using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace VMRCPACK.UnitOfWork.Interfaces.Repository
{
    /// <summary>
    /// Interface of Repository Pattern implemetation basics methods for create, update and remove 
    /// </summary>
    /// <typeparam name="T">Type of Entity object</typeparam>
    public interface IRepositoryWriteDocument<T> where T : class
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
        /// <param name="filter">Filter for locate object to update</param>
        void Update(Expression<Func<T, bool>> filter, T entity);

        /// <summary>
        /// Remove Method
        /// </summary>
        /// <param name="filter">>Filter for locate object to delete</param>
        void Remove(Expression<Func<T, bool>> filter);
    }

    /// <summary>
    /// Interface of Repository Pattern implemetation basics async methods for create, update and remove 
    /// </summary>
    /// <typeparam name="T">Type of Entity object</typeparam>
    public interface IRepositoryWriteDocumentAsync<T> where T : class
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
        /// <param name="filter">Filter for locate object to update</param>
        Task UpdateAsync(Expression<Func<T, bool>> filter, T entity);

        /// <summary>
        /// Remove Method
        /// </summary>
        /// <param name="filter">>Filter for locate object to delete</param>
        Task RemoveAsync(Expression<Func<T, bool>> filter);
    }
}

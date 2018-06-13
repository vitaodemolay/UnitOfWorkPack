using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VMRCPACK.UnitOfWork.Interfaces.support;

namespace VMRCPACK.UnitOfWork.Interfaces.Repository
{
    /// <summary>
    /// Interface of Repository Pattern implemetation TSQL basic and pagination search data methods  
    /// </summary>
    /// <typeparam name="T">Type of Entity object</typeparam>
    public interface IRepositoryReadOnlyTSQL<T> where T : class
    {
        /// <summary>
        /// Search method of a single object
        /// </summary>
        /// <param name="sql">Sql script for filter data</param>
        /// <param name="param">Parameters for script sql</param>
        /// <returns></returns>
        T Find(string sql, IDictionary<string, Object> param = null);

        /// <summary>
        /// Search method of many objects
        /// </summary>
        /// <param name="sql">Sql script for filter data</param>
        /// <param name="param">Parameters for script sql</param>
        /// <returns></returns>
        IList<T> FindAll(string sql, IDictionary<string, Object> param = null);

        /// <summary>
        /// Search method of many objects pagination
        /// </summary>
        /// <param name="pageIndex">Index of page</param>
        /// <param name="pageSize">Elements number per page</param>
        /// <param name="fieldOrderBy">Field name for apply ordenation</param>
        /// <param name="sql">Sql script for filter data</param>
        /// <param name="desc">Ordenation type (desc ordenation? True or false)</param>
        /// <param name="param">Parameters for script sql</param>
        /// <returns></returns>
        IPagination<T> FindAll(int pageIndex, int pageSize, string fieldOrderBy, string sql, bool desc = false, IDictionary<string, Object> param = null);
    }


    /// <summary>
    /// Interface of Repository Pattern implemetation TSQL basic and pagination search data async methods  
    /// </summary>
    /// <typeparam name="T">Type of Entity object</typeparam>
    public interface IRepositoryReadOnlyTSQLAsync<T> where T : class
    {
        /// <summary>
        /// Async Search method of a single object
        /// </summary>
        /// <param name="sql">Sql script for filter data</param>
        /// <param name="param">Parameters for script sql</param>
        /// <returns></returns>
        Task<T> FindAsync(string sql, IDictionary<string, Object> param = null);

        /// <summary>
        /// Async Search method of many objects
        /// </summary>
        /// <param name="sql">Sql script for filter data</param>
        /// <param name="param">Parameters for script sql</param>
        /// <returns></returns>
        Task<IList<T>> FindAllAsync(string sql, IDictionary<string, Object> param = null);

        /// <summary>
        /// Async Search method of many objects pagination
        /// </summary>
        /// <param name="pageIndex">Index of page</param>
        /// <param name="pageSize">Elements number per page</param>
        /// <param name="fieldOrderBy">Field name for apply ordenation</param>
        /// <param name="sql">Sql script for filter data</param>
        /// <param name="desc">Ordenation type (desc ordenation? True or false)</param>
        /// <param name="param">Parameters for script sql</param>
        /// <returns></returns>
        Task<IPagination<T>> FindAllAsync(int pageIndex, int pageSize, string fieldOrderBy, string sql, bool desc = false, IDictionary<string, Object> param = null);
    }
}

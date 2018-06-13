using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace VMRCPACK.UnitOfWork.Interfaces.Repository
{
    /// <summary>
    /// Interface of Repository Pattern implemetation TSQL extended search data methods  
    /// </summary>
    /// <typeparam name="T">Type of Entity object</typeparam>
    public interface IRepositoryReadOnlyExtendedTSQL<T> where T : class
    {
        /// <summary>
        /// Method for Count of registers
        /// </summary>
        /// <param name="sql">Sql script for filter data</param>
        /// <param name="param">Parameters for script sql</param>
        /// <returns></returns>
        int Count(string sql, IDictionary<string, Object> param = null);

        /// <summary>
        /// Method for verify exists of registers
        /// </summary>
        /// <param name="sql">Sql script for filter data</param>
        /// <param name="param">Parameters for script sql</param>
        /// <returns></returns>
        bool Exists(string sql, IDictionary<string, Object> param = null);
    }


    /// <summary>
    /// Interface of Repository Pattern implemetation TSQL extended search data async methods  
    /// </summary>
    /// <typeparam name="T">Type of Entity object</typeparam>
    public interface IRepositoryReadOnlyExtendedTSQLAsync<T> where T : class
    {
        /// <summary>
        /// Async Method for Count of registers
        /// </summary>
        /// <param name="sql">Sql script for filter data</param>
        /// <param name="param">Parameters for script sql</param>
        /// <returns></returns>
        Task<int> CountAsync(string sql, IDictionary<string, Object> param = null);

        /// <summary>
        /// Async Method for verify exists of registers
        /// </summary>
        /// <param name="sql">Sql script for filter data</param>
        /// <param name="param">Parameters for script sql</param>
        /// <returns></returns>
        Task<bool> ExistsAsync(string sql, IDictionary<string, Object> param = null);
    }
}

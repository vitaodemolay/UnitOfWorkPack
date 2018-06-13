using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace VMRCPACK.UnitOfWork.Interfaces.Repository
{
    /// <summary>
    /// Interface of Repository Pattern implemetation TSQL Version for basics changes methods  
    /// </summary>
    /// <typeparam name="T">Type of Entity object</typeparam>
    public interface IRepositoryWriteTSQL<T> where T : class
    {
        /// <summary>
        /// Execution method for to apply changes on database 
        /// </summary>
        /// <param name="sql">Script sql</param>
        /// <param name="param">parameters dictionary</param>
        /// <param name="numberOfRowsChanges">number of affect rows</param>
        void execute(string sql, IDictionary<string, Object> param = null, int numberOfRowsChanges = 1);
    }

    /// <summary>
    /// Interface of Repository Pattern implemetation TSQL Version for basics changes methods async 
    /// </summary>
    /// <typeparam name="T">Type of Entity object</typeparam>
    public interface IRepositoryWriteTSQLAsync<T> where T : class
    {
        /// <summary>
        /// Async Execution method for to apply changes on database
        /// </summary>
        /// <param name="sql">Script sql</param>
        /// <param name="param">parameters dictionary</param>
        /// <param name="numberOfRowsChanges">number of affect rows</param>
        Task executeAsync(string sql, IDictionary<string, Object> param = null, int numberOfRowsChanges = 1);
    }
}

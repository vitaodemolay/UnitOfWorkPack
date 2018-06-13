using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using VMRCPACK.UnitOfWork.Interfaces.support;

namespace VMRCPACK.UnitOfWork.Interfaces.Repository
{

    /// <summary>
    /// Interface of Repository Pattern implemetation pagination search data methods  
    /// </summary>
    /// <typeparam name="T">Type of Entity object</typeparam>
    public interface IRepositoryReadOnlyPagination<T> where T : class
    {
        /// <summary>
        /// Search method of many objects pagination
        /// </summary>
        /// <param name="pageIndex">Index of page</param>
        /// <param name="pageSize">Elements number per page</param>
        /// <param name="fieldOrderBy">Field name for apply ordenation</param>
        /// <param name="desc">Ordenation type (desc ordenation? True or false)</param>
        /// <param name="filter">Object Filter to apply on search</param>
        /// <returns></returns>
        IPagination<T> FindAll(int pageIndex, int pageSize, string fieldOrderBy, bool desc = false, Expression<Func<T, bool>> filter = null);
    }


    /// <summary>
    /// Interface of Repository Pattern implemetation pagination search data async methods  
    /// </summary>
    /// <typeparam name="T">Type of Entity object</typeparam>
    public interface IRepositoryReadOnlyPaginationAsync<T> where T : class
    {
        /// <summary>
        /// Async Search method of many objects pagination
        /// </summary>
        /// <param name="pageIndex">Index of page</param>
        /// <param name="pageSize">Elements number per page</param>
        /// <param name="fieldOrderBy">Field name for apply ordenation</param>
        /// <param name="desc">Ordenation type (desc ordenation? True or false)</param>
        /// <param name="filter">Object Filter to apply on search</param>
        /// <returns></returns>
        Task<IPagination<T>> FindAllAsync(int pageIndex, int pageSize, string fieldOrderBy, bool desc = false, Expression<Func<T, bool>> filter = null);
    }
}

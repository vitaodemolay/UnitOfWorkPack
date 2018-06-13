using System.Linq;

namespace VMRCPACK.UnitOfWork.Interfaces.Repository
{
    /// <summary>
    /// Interface of Repository Pattern implemetation Queryable object to apply search data methods  
    /// </summary>
    /// <typeparam name="T">Type of Entity object</typeparam>
    public interface IRepositoryReadOnlyQueryable<T> where T : class
    {
        /// <summary>
        /// Queryable object
        /// </summary>
        IQueryable<T> Query { get; }
    }
}

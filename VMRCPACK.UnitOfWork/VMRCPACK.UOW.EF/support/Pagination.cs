using System.Collections.Generic;
using System.Runtime.Serialization;
using VMRCPACK.UnitOfWork.Interfaces.support;

namespace VMRCPACK.UOW.EF.support
{
    /// <summary>
    /// Implementation class of Pagination object for aux on search with pagination method
    /// </summary>
    /// <typeparam name="TEntity">Type of result object on Pagination</typeparam>
    [DataContract()]
    public class Pagination<TEntity> : IPagination<TEntity>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="collectionData">Collection of Object of Search method</param>
        /// <param name="totalRegister">Total of Registries of Search method</param>
        public Pagination(IList<TEntity> collectionData, int totalRegister)
        {
            this.Collection = collectionData;
            this.TotalRegisters = totalRegister;
        }

        /// <summary>
        /// Total of Registries of Search method
        /// </summary>
        [DataMember()]
        public int TotalRegisters { get;  }

        /// <summary>
        /// Collection of Object of Search method
        /// </summary>
        [DataMember()]
        public IList<TEntity> Collection { get; }
    }
}

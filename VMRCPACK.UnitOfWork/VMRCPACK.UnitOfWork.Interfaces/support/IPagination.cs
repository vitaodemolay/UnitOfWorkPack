using System.Collections.Generic;

namespace VMRCPACK.UnitOfWork.Interfaces.support
{
    /// <summary>
    /// Interface of Pagination object for aux on search with pagination method
    /// </summary>
    /// <typeparam name="T"> Type of result object on Pagination </typeparam>
    public interface IPagination<T>
    {
        /// <summary>
        /// Total of Registries of Search method
        /// </summary>
        int TotalRegisters { get;  }

        /// <summary>
        /// Collection of Object of Search method
        /// </summary>
        IList<T> Collection { get;  }
    }
}

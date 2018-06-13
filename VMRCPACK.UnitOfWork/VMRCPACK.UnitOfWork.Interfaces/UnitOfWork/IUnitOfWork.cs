using System;
using System.Threading.Tasks;

namespace VMRCPACK.UnitOfWork.Interfaces.UnitOfWork
{
    /// <summary>
    /// Interface of Unit of Work object 
    /// </summary>
    /// <typeparam name="TContext"> Type of Context object of the database</typeparam>
    public interface IUnitOfWork<TContext> : IDisposable 
    {
        /// <summary>
        /// Instance of Context object of the database
        /// </summary>
        TContext dbContext { get; }

        /// <summary>
        /// Persistation Method for apply changes on the database
        /// </summary>
        void saveChange();
    }


    /// <summary>
    /// Interface of Unit of Work object with async methods
    /// </summary>
    /// <typeparam name="TContext"> Type of Context object of the database</typeparam>
    public interface IUnitOfWorkAsync<TContext> : IDisposable where TContext : class
    {
        /// <summary>
        /// Instance of Context object of the database
        /// </summary>
        TContext dbContext { get; }

        /// <summary>
        /// Persistation Method async for apply changes on the database
        /// </summary>
        Task saveChangeAsync();
    }

    /// <summary>
    /// Interface of Unit of Work object for document database
    /// </summary>
    /// <typeparam name="TContext"> Type of Context object of the database</typeparam>
    public interface IUnitOfWorkDocument<TContext> : IDisposable where TContext : class
    {
        /// <summary>
        /// Instance of Context object of the database
        /// </summary>
        TContext context { get; }
    }
}

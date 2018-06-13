using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using VMRCPACK.UnitOfWork.Interfaces.Repository;
using VMRCPACK.UnitOfWork.Interfaces.support;
using VMRCPACK.UnitOfWork.Interfaces.UnitOfWork;
using VMRCPACK.UOW.EF.support;

namespace VMRCPACK.UOW.EF.Repositories
{
    /// <summary>
    /// Class of Repository Pattern implemetation with any implementation interfaces
    /// </summary>
    /// <typeparam name="TEntity">Type of Entity object</typeparam>
    /// <typeparam name="TContext">Type of Context object of the database</typeparam>
    public abstract class RepositoryEF<TEntity, TContext> : IRepositoryReadOnlyQueryable<TEntity>,
                                                            IRepositoryReadOnly<TEntity>,
                                                            IRepositoryReadOnlyAsync<TEntity>,
                                                            IRepositoryReadOnlyExtended<TEntity>,
                                                            IRepositoryReadOnlyExtendedAsync<TEntity>,
                                                            IRepositoryReadOnlyPaginationExtended<TEntity>,
                                                            IRepositoryReadOnlyPaginationExtendedAsync<TEntity>,
                                                            IRepositoryWrite<TEntity>,
                                                            IRepositoryWriteAsync<TEntity>
        where TEntity : class
        where TContext : DbContext
    {

        #region Private Objects
        private IUnitOfWork<TContext> unitOfWorkef { get; set; }
        private DbSet<TEntity> dbSet { get { return this.unitOfWorkef.dbContext.Set<TEntity>(); } }
        #endregion

        
        #region Constructor
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="unitOfWork">UnitOfWork Instace object</param>
        public RepositoryEF(IUnitOfWork<TContext> unitOfWork)
        {
            if (unitOfWork == null)
                throw new NullReferenceException("Unit of Work object is Null.");

            this.unitOfWorkef = unitOfWork;
        }
        #endregion

        #region ReadOnly

        /* implemetation Queryable object to apply search data methods  */
        #region ReadOnly Queryable

        /// <summary>
        /// Queryable object
        /// </summary>        
        public virtual IQueryable<TEntity> Query => this.dbSet;
        #endregion

        /* implemetation basics search data methods */
        #region ReadOnly Basic

        /// <summary>
        /// Search method of a single object
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public virtual TEntity Find(Expression<Func<TEntity, bool>> filter = null)
        {
            return this.dbSet.SingleOrDefault(filter);
        }

        /// <summary>
        /// Search method of many objects
        /// </summary>
        /// <param name="filter">Object Filter to apply on search</param>
        /// <returns></returns>
        public virtual IList<TEntity> FindAll(Expression<Func<TEntity, bool>> filter = null)
        {
            if (filter != null)
                return this.dbSet.Where(filter).ToList();

            return this.dbSet.ToList();
        }

        #endregion

        /* implemetation basics search data  async methods */
        #region ReadOnly Basic Async

        /// <summary>
        /// Async Search method of a single object
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public virtual Task<TEntity> FindAsync(Expression<Func<TEntity, bool>> filter = null)
        {
            return this.dbSet.SingleOrDefaultAsync(filter);
        }

        /// <summary>
        /// Async Search method of many objects
        /// </summary>
        /// <param name="filter">Object Filter to apply on search</param>
        /// <returns></returns>
        public virtual Task<List<TEntity>> FindAllAsync(Expression<Func<TEntity, bool>> filter = null)
        {
            if (filter != null)
                return this.dbSet.Where(filter).ToListAsync();

            return this.dbSet.ToListAsync();
        }

        #endregion

        /* implemetation Extended search data methods  */
        #region ReadOnly Extended

        /// <summary>
        /// Method for Reload entity object with update data on database
        /// </summary>
        /// <param name="entity">Entity object instance</param>
        public virtual void Reload(ref TEntity entity)
        {
            this.unitOfWorkef.dbContext.Entry(entity).Reload();
        }

        /// <summary>
        /// Method for Count of registers
        /// </summary>
        /// <param name="filter">Object Filter to apply on count</param>
        /// <returns></returns>
        public virtual int Count(Expression<Func<TEntity, bool>> filter = null)
        {
            if (filter != null)
                return this.dbSet.Where(filter).AsNoTracking().Count();

            return this.dbSet.AsNoTracking().Count();
        }

        /// <summary>
        /// Method for verify exists of registers
        /// </summary>
        /// <param name="filter"> Object Filter to apply on verify</param>
        /// <returns></returns>
        public virtual bool Exists(Expression<Func<TEntity, bool>> filter)
        {
            return this.dbSet.Any(filter);
        }
        #endregion

        /* implemetation Extended search data async methods  */
        #region ReadOnly Extended Async

        /// <summary>
        /// Async Method for Count of registers
        /// </summary>
        /// <param name="filter">Object Filter to apply on count</param>
        /// <returns></returns>
        public virtual Task<int> CountAsync(Expression<Func<TEntity, bool>> filter = null)
        {
            if (filter != null)
                return this.dbSet.Where(filter).AsNoTracking().CountAsync();

            return this.dbSet.AsNoTracking().CountAsync();
        }

        /// <summary>
        /// Async Method for verify exists of registers
        /// </summary>
        /// <param name="filter"> Object Filter to apply on verify</param>
        /// <returns></returns>
        public virtual Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> filter)
        {
            return this.dbSet.AnyAsync(filter);
        }

        #endregion

        /* implemetation pagination extended search data methods  */
        #region ReadOnly Pagination Extended

        /// <summary>
        /// Search method of many objects pagination
        /// </summary>
        /// <param name="pageIndex">Index of page</param>
        /// <param name="pageSize">Elements number per page</param>
        /// <param name="fieldOrderBy">Field select expression for apply ordenation</param>
        /// <param name="desc">Ordenation type (desc ordenation? True or false)</param>
        /// <param name="filter">Object Filter to apply on search</param>
        /// <returns></returns>
        public virtual IPagination<TEntity> FindAll<TKey>(int pageIndex, int pageSize, Expression<Func<TEntity, TKey>> fieldOrderBy, bool desc = false, Expression<Func<TEntity, bool>> filter = null)
        {
            var Tot = this.Count(filter);

            IQueryable<TEntity> collection = null;
            if (filter != null)
                collection = this.dbSet.Where(filter);
            else
                collection = this.dbSet;

            return new Pagination<TEntity>(desc ? collection.OrderByDescending(fieldOrderBy).Skip(pageIndex * pageSize).Take(pageSize).ToList() :
                                                        collection.OrderBy(fieldOrderBy).Skip(pageIndex * pageSize).Take(pageSize).ToList(),
                                                 Tot);
        }
        #endregion

        /* implemetation pagination extended search data async methods */
        #region ReadOnly Pagination Extended Async

        /// <summary>
        /// Async Search method of many objects pagination
        /// </summary>
        /// <param name="pageIndex">Index of page</param>
        /// <param name="pageSize">Elements number per page</param>
        /// <param name="fieldOrderBy">Field select expression for apply ordenation</param>
        /// <param name="desc">Ordenation type (desc ordenation? True or false)</param>
        /// <param name="filter">Object Filter to apply on search</param>
        /// <returns></returns>
        public virtual Task<IPagination<TEntity>> FindAllAsync<TKey>(int pageIndex, int pageSize, Expression<Func<TEntity, TKey>> fieldOrderBy, bool desc = false, Expression<Func<TEntity, bool>> filter = null)
        {
            var Tot = this.Count(filter);
            IQueryable<TEntity> collection = null;

            if (filter != null)
                collection = this.dbSet.Where(filter);
            else
                collection = this.dbSet;

            IPagination<TEntity> result = new Pagination<TEntity>(desc ? collection.OrderByDescending(fieldOrderBy).Skip(pageIndex * pageSize).Take(pageSize).ToList() :
                                                        collection.OrderBy(fieldOrderBy).Skip(pageIndex * pageSize).Take(pageSize).ToList(),
                                                 Tot);

            return Task.FromResult(result);
        }
        #endregion

        #endregion

        #region Write

        /* implemetation basics methods for create, update and remove  */
        #region Write sync

        /// <summary>
        /// Create Method
        /// </summary>
        /// <param name="entity">Entity object instance</param>
        public virtual void Add(TEntity entity)
        {
            this.dbSet.Add(entity);
        }

        /// <summary>
        /// Update Method
        /// </summary>
        /// <param name="entity">Entity object instance</param>
        public virtual void Update(TEntity entity)
        {
            this.dbSet.Update(entity);
        }

        /// <summary>
        /// Remove Method
        /// </summary>
        /// <param name="entity">Entity object instance</param>
        public virtual void Remove(TEntity entity)
        {
            this.dbSet.Remove(entity);
        }
        #endregion

        /* implemetation basics async methods for create, update and remove */
        #region Write Async

        /// <summary>
        /// Async Create Method
        /// </summary>
        /// <param name="entity">Entity object instance</param>
        public virtual Task AddAsync(TEntity entity)
        {
            return this.dbSet.AddAsync(entity);
        }

        /// <summary>
        /// Async Update Method
        /// </summary>
        /// <param name="entity">Entity object instance</param>
        public virtual Task UpdateAsync(TEntity entity)
        {
            return this.UpdateAsync(entity);
        }

        /// <summary>
        /// Async Remove Method
        /// </summary>
        /// <param name="entity">Entity object instance</param>
        public virtual Task RemoveAsync(TEntity entity)
        {
            return this.RemoveAsync(entity);
        }
        #endregion

        #endregion

    }
}

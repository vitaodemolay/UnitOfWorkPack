using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using VMRCPACK.UnitOfWork.Interfaces.Repository;
using VMRCPACK.UnitOfWork.Interfaces.support;
using VMRCPACK.UnitOfWork.Interfaces.UnitOfWork;
using VMRCPACK.UOW.Mongo.support;
using VMRCPACK.UOW.Mongo.Uow;

namespace VMRCPACK.UOW.Mongo.Repositories
{
    public abstract class RepositoryMongo<TEntity, TContext> : IRepositoryReadOnly<TEntity>,
                                                               IRepositoryReadOnlyAsync<TEntity>,
                                                               IRepositoryReadOnlyExtended<TEntity>,
                                                               IRepositoryReadOnlyExtendedAsync<TEntity>,
                                                               IRepositoryReadOnlyPaginationExtended<TEntity>,
                                                               IRepositoryReadOnlyPaginationExtendedAsync<TEntity>,
                                                               IRepositoryReadOnlyQueryable<TEntity>,
                                                               IRepositoryWriteDocument<TEntity>,
                                                               IRepositoryWriteDocumentAsync<TEntity>
        where TEntity : class
        where TContext : MongoContext
    {

        #region Private Objects
        private IUnitOfWorkDocument<TContext> unitOfWorkMongo { get; set; }
        private string collectionName { get; }
        private KeyValuePair<string, Type> idFieldMetadata { get; }
        private IMongoCollection<TEntity> DbSet
        {
            get
            {
                var dbMongo = (this.unitOfWorkMongo as UnitOfWorkMongo<TContext>).GetDatabaseConnection();
                return dbMongo.GetCollection<TEntity>(this.collectionName);
            }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="unitOfWork">UnitOfWork Instace object</param>
        public RepositoryMongo(IUnitOfWorkDocument<TContext> unitOfWork, string collectionName, KeyValuePair<string, Type> IdFieldMetadata)
        {
            if (unitOfWork == null) throw new NullReferenceException("Unit of Work object is Null.");
            this.unitOfWorkMongo = unitOfWork;

            if (string.IsNullOrEmpty(collectionName)) throw new NullReferenceException("Collection name is not informed");
            this.collectionName = collectionName;

            if (string.IsNullOrEmpty(IdFieldMetadata.Key) || IdFieldMetadata.Value == null) throw new NullReferenceException("IdField Metadata is not corretaly informed");
            this.idFieldMetadata = IdFieldMetadata;
        }
        #endregion

        #region Queryable
        public virtual IQueryable<TEntity> Query => this.DbSet.AsQueryable();
        #endregion

        #region ReadOnly
        public virtual TEntity Find(Expression<Func<TEntity, bool>> filter = null)
        {
            try
            {
                return this.DbSet.Find(filter).FirstOrDefault();
            }
            catch (ArgumentNullException ex)
            {
                if (ex.Message.Contains("Value cannot be null"))
                    return null;
                throw ex;
            }
        }

        public virtual IList<TEntity> FindAll(Expression<Func<TEntity, bool>> filter = null)
        {
            try
            {
                return this.DbSet.Find(filter).ToList();
            }
            catch (ArgumentNullException ex)
            {
                if (ex.Message.Contains("Value cannot be null"))
                    return null;
                throw ex;
            }
        }
        #endregion

        #region ReadOnly Async
        public virtual async Task<TEntity> FindAsync(Expression<Func<TEntity, bool>> filter = null)
        {
            try
            {
                return await (await this.DbSet.FindAsync(filter)).FirstOrDefaultAsync();
            }
            catch (ArgumentNullException ex)
            {
                if (ex.Message.Contains("Value cannot be null"))
                    return null;
                throw ex;
            }
        }

        public virtual async Task<List<TEntity>> FindAllAsync(Expression<Func<TEntity, bool>> filter = null)
        {
            try
            {
                return await (await this.DbSet.FindAsync(filter)).ToListAsync();
            }
            catch (ArgumentNullException ex)
            {
                if (ex.Message.Contains("Value cannot be null"))
                    return null;
                throw ex;
            }
        }
        #endregion

        #region ReadOnly Extended
        public virtual void Reload(ref TEntity entity)
        {
            ParameterExpression pe = Expression.Parameter(entity.GetType(), "entity");
            var prop = Expression.PropertyOrField(pe, this.idFieldMetadata.Key);
            Expression exp = Expression.Equal(prop, Expression.Constant(entity.GetType().GetProperty(this.idFieldMetadata.Key).GetValue(entity, null)));

            var filter = Expression.Lambda<Func<TEntity, bool>>(exp, pe);

            entity = this.Find(filter);
        }

        public virtual int Count(Expression<Func<TEntity, bool>> filter = null)
        {
            return (int)this.DbSet.Count(filter);
        }

        public virtual bool Exists(Expression<Func<TEntity, bool>> filter)
        {
            return this.DbSet.Count(filter) > 0;
        }
        #endregion

        #region ReadOnly Extended Async
        public virtual async Task<int> CountAsync(Expression<Func<TEntity, bool>> filter = null)
        {
            return (int)await this.DbSet.CountAsync(filter);
        }

        public virtual async Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> filter)
        {
            return (await this.DbSet.CountAsync(filter)) > 0;
        }
        #endregion

        #region ReadOnly Pagination
        public virtual IPagination<TEntity> FindAll<TKey>(int pageIndex, int pageSize, Expression<Func<TEntity, TKey>> fieldOrderBy, bool desc = false, Expression<Func<TEntity, bool>> filter = null)
        {
            var TotalItens = this.DbSet.Count(filter);
            IQueryable<TEntity> collection;
            if (filter != null)
                collection = this.Query.Where(filter);
            else
                collection = this.Query;

            return new Pagination<TEntity>(desc ? collection.OrderByDescending(fieldOrderBy).Skip(pageIndex * pageSize).Take(pageSize).ToList() :
                                                       collection.OrderBy(fieldOrderBy).Skip(pageIndex * pageSize).Take(pageSize).ToList(),
                                                (int)TotalItens);
        }
        #endregion

        #region ReadOnly Pagination Async
        public virtual async Task<IPagination<TEntity>> FindAllAsync<TKey>(int pageIndex, int pageSize, Expression<Func<TEntity, TKey>> fieldOrderBy, bool desc = false, Expression<Func<TEntity, bool>> filter = null)
        {
            var TotalItens = await this.DbSet.CountAsync(filter);
            IQueryable<TEntity> collection;
            if (filter != null)
                collection = this.Query.Where(filter);
            else
                collection = this.Query;

            return new Pagination<TEntity>(desc ? collection.OrderByDescending(fieldOrderBy).Skip(pageIndex * pageSize).Take(pageSize).ToList() :
                                                       collection.OrderBy(fieldOrderBy).Skip(pageIndex * pageSize).Take(pageSize).ToList(),
                                                (int)TotalItens);
        }
        #endregion

        #region Write
        public virtual void Add(TEntity entity)
        {
            this.DbSet.InsertOne(entity);
        }

        public virtual void Update(Expression<Func<TEntity, bool>> filter, TEntity entity)
        {
            this.DbSet.ReplaceOne(filter, entity);
        }

        public virtual void Remove(Expression<Func<TEntity, bool>> filter)
        {
            this.DbSet.DeleteOne(filter);
        }
        #endregion

        #region Write Async
        public virtual Task AddAsync(TEntity entity)
        {
            return this.DbSet.InsertOneAsync(entity);
        }

        public virtual Task UpdateAsync(Expression<Func<TEntity, bool>> filter, TEntity entity)
        {
            return this.DbSet.ReplaceOneAsync(filter, entity);
        }

        public virtual Task RemoveAsync(Expression<Func<TEntity, bool>> filter)
        {
            return this.DbSet.DeleteOneAsync(filter);
        }
        #endregion
    }
}

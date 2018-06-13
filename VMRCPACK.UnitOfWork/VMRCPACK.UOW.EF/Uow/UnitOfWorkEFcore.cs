using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using VMRCPACK.UnitOfWork.Interfaces.UnitOfWork;
using VMRCPACK.UOW.EF.support;

namespace VMRCPACK.UOW.EF.Uow
{
    /// <summary>
    /// Unit of Work class is implementation of IUnitOfWork and IUnitOfWorkAsync
    /// </summary>
    /// <typeparam name="TContext">Type of Context object of the database</typeparam>
    public class UnitOfWorkEFcore<TContext> : IUnitOfWork<TContext>, 
                                              IUnitOfWorkAsync<TContext> 
        where TContext : DbContext
    {
        private TContext _dbContext;

        /// <summary>
        /// Instance of Context object of the database
        /// </summary>
        public virtual TContext dbContext { get
            {
                if (!DbContextCoreFactory<TContext>.IsInit)
                    DbContextCoreFactory<TContext>.Initialize(this.ContextBuilder);

                if(this._dbContext == null)
                    this._dbContext = DbContextCoreFactory<TContext>.CreateDbContextInstance();

                try
                {
                    if (this._dbContext.Database == null)
                        throw new Exception();
                }
                catch (Exception)
                {
                    this._dbContext = DbContextCoreFactory<TContext>.CreateDbContextInstance();
                }

                return this._dbContext;
            }
        }
    
        private DbContextOptionsBuilder<TContext> ContextBuilder { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="ContextBuilder">DbContext Option Builder</param>
        public UnitOfWorkEFcore(DbContextOptionsBuilder<TContext> ContextBuilder)
        {
            this.ContextBuilder = ContextBuilder;
        }


        /// <summary>
        /// Persistation Method for apply changes on the database
        /// </summary>
        public virtual void saveChange()
        {
            int _ret = 0;
            var context = this.dbContext as DbContext;
            using (var trans = context.Database.BeginTransaction())
            {
                try
                {
                    _ret = context.SaveChanges();
                    trans.Commit();
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    throw ex;
                }
            }
        }

        /// <summary>
        /// Persistation Method async for apply changes on the database
        /// </summary>
        /// <returns></returns>
        public async Task saveChangeAsync()
        {
            int _ret = 0;
            var context = this.dbContext as DbContext;
            using (var trans = context.Database.BeginTransaction())
            {
                try
                {
                    _ret = await context.SaveChangesAsync();
                    trans.Commit();
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    throw ex;
                }
            }
        }

        /// <summary>
        /// Dispose method
        /// </summary>
        public void Dispose()
        {
            this.dbContext.Dispose();
            GC.Collect();
        }

        
    }
}

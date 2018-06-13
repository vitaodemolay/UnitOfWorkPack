using System;
using System.Data;
using VMRCPACK.UnitOfWork.Interfaces.UnitOfWork;
using VMRCPACK.UOW.Dapper.support;

namespace VMRCPACK.UOW.Dapper.Uow
{
    public class UnitOfWorkDapper<TContext> : IUnitOfWork<TContext> where TContext : ConnectionContext
    {
        public virtual IDbTransaction transaction { get; private set; }
        private bool disposed;

        public virtual TContext dbContext { get; }

        private IDbConnection Connection => this.dbContext.Connection;
        
        public UnitOfWorkDapper(TContext context)
        {
            this.dbContext = context;
            this.Connection.Open();
            this.transaction = this.Connection.BeginTransaction();
        }

        public virtual void Dispose()
        {
            if (this.transaction != null)
            {
                this.transaction.Dispose();
                this.transaction = null;
            }
            if (this.Connection != null)
            {
                this.Connection.Dispose();
                this.dbContext.Dispose();
            }

            GC.SuppressFinalize(this);
        }

        public virtual void saveChange()
        {
            try
            {
                this.transaction.Commit();
            }
            catch (Exception ex)
            {
                this.transaction.Rollback();
                throw ex;
            }
            finally
            {
                this.transaction.Dispose();
                this.transaction = this.transaction = this.Connection.BeginTransaction();
            }
        }

        public virtual void RollbackTransaction()
        {
            try
            {
                this.transaction.Rollback();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                this.transaction.Dispose();
                this.transaction = this.transaction = this.Connection.BeginTransaction();
            }
        }
    }
}

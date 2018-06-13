using MongoDB.Driver;
using System;
using VMRCPACK.UnitOfWork.Interfaces.UnitOfWork;
using VMRCPACK.UOW.Mongo.support;

namespace VMRCPACK.UOW.Mongo.Uow
{
    public class UnitOfWorkMongo<TContext> : IUnitOfWorkDocument<TContext> where TContext : MongoContext
    {
        public virtual TContext context { get; }

        public Func<IMongoDatabase> GetDatabaseConnection;

        private IMongoDatabase getDataBase()
        {
            return this.context.GetDatabaseConnection;
        }

        public UnitOfWorkMongo(TContext context)
        {
            this.context = context;
            this.GetDatabaseConnection = getDataBase;
        }

        public UnitOfWorkMongo(string connectionString, string databaseName)
        {
            this.context = (TContext)Activator.CreateInstance(typeof(TContext), new object[] { connectionString, databaseName });
            this.GetDatabaseConnection = getDataBase;
        }
        public virtual void Dispose()
        {
            this.context.Dispose();
        }
    }
}

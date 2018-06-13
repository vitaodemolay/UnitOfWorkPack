using MongoDB.Driver;
using System;

namespace VMRCPACK.UOW.Mongo.support
{
    public abstract class MongoContext : IDisposable
    {
        private string connectionstring { get; }
        private string databaseName { get; }
        public abstract void OnModelBuilder();

        private MongoClient client { get; set; }

        public MongoContext(string connectionstring, string databasename)
        {
            this.connectionstring = connectionstring;
            this.databaseName = databasename;
        }

        private void CreateClient()
        {
            this.CloseClient();
            this.OnModelBuilder();
            this.client = new MongoClient(this.connectionstring);

        }

        public virtual void CloseClient()
        {
            this.client = null;
        }

        public virtual IMongoDatabase GetDatabaseConnection
        {
            get
            {
                if (this.client == null)
                    this.CreateClient();

                return this.client.GetDatabase(this.databaseName);

            }
        }



        public void Dispose()
        {
            this.CloseClient();
        }
    }
}

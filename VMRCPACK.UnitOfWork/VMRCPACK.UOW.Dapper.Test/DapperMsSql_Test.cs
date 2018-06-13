using Microsoft.Data.Sqlite;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Dynamic;
using VMRCPACK.UnitOfWork.Interfaces.Repository;
using VMRCPACK.UnitOfWork.Interfaces.UnitOfWork;
using VMRCPACK.UOW.Dapper.Repositories;
using VMRCPACK.UOW.Dapper.support;
using VMRCPACK.UOW.Dapper.Uow;

namespace VMRCPACK.UOW.Dapper.Test
{
    [TestClass]
    public class DapperMsSql_Test
    {

        private const string connectionString = @"Data Source=.\sqlexpress;Initial Catalog=dbVMRCTest;Integrated Security=True";
        private const string tablename = "Car";

        //Entity Classes for tests

        internal class Car
        {
            public int id { get; set; }
            public string name { get; set; }
        }

        internal abstract class SqliteBase : IDisposable
        {
            public Func<ConnectionContext> GetContext;

            private ConnectionContext _GetConnectionContext()
            {
                return this.ContextInstance;
            }
            private string _ConnectionString;
            public ConnectionContext ContextInstance { get; }

            protected SqliteBase()
            {
                GetContext = _GetConnectionContext;

                _InitConnectionString();

                this.ContextInstance = new ConnectionContext(GetConnection(), false);
            }

            private void _InitConnectionString()
            {
                SqliteConnectionStringBuilder builder = new SqliteConnectionStringBuilder
                {
                    Mode = SqliteOpenMode.Memory,
                };

                _ConnectionString = builder.ConnectionString;

            }

            private DbConnection GetConnection()
            {
                try
                {
                    DbConnection dbConnection = SqliteFactory.Instance.CreateConnection();
                    dbConnection.ConnectionString = _ConnectionString;
                    dbConnection.Open();

                    return dbConnection;
                }

                catch (Exception ex)
                {
                    throw new Exception("Error opening database connection.", ex);
                }
            }

            /// <summary>
            /// Creates a table in the SQL database if it does not exist
            /// </summary>
            /// <param name="tableName">The name of the table</param>
            /// <param name="columns">Comma separated column names</param>
            protected void CreateTable(string tableName, string columns)
            {
                using (ConnectionContext context = GetContext())
                {
                    using (DbCommand dbCommand = context.Connection.CreateCommand() as DbCommand)
                    {
                        dbCommand.CommandText = $"CREATE TABLE IF NOT EXISTS {tablename} ({columns})";
                        dbCommand.ExecuteNonQuery();
                    }
                }
            }

            public void Dispose()
            {
                this._GetConnectionContext().Dispose();
            }
        }

        internal class CarBaseCreator : SqliteBase
        {
            public void Intialize()
            {
                CreateTable(tablename, "id INTEGER PRIMARY KEY, name VARCHAR(255)");
            }

            public bool teste()
            {
                using (ConnectionContext context = GetContext())
                {
                    using (DbCommand dbCommand = context.Connection.CreateCommand() as DbCommand)
                    {
                        dbCommand.CommandText = $"Select 1 from {tablename}";
                        var t = dbCommand.ExecuteScalar();
                        return true;
                    }
                    
                }
            }
        }

        internal interface IInternalRepository<TEntity> : IRepositoryReadOnly<TEntity>,
                                                                IRepositoryReadOnlyTSQL<TEntity>,
                                                                IRepositoryReadOnlyExtended<TEntity>,
                                                                IRepositoryReadOnlyExtendedTSQL<TEntity>,
                                                                IRepositoryReadOnlyPagination<TEntity>,
                                                                IRepositoryWriteTSQL<TEntity>
            where TEntity : class
        {

        }

        internal class InternalRepository<TEntity, TContext> : RepositoryDapper<TEntity, TContext>, IInternalRepository<TEntity>
             where TEntity : class
             where TContext : ConnectionContext
        {
            public InternalRepository(IUnitOfWork<TContext> unitOfWork, string tablename, Dictionary<string, Type> fieldIdName)
                : base(unitOfWork, tablename, fieldIdName)
            {
            }
        }

        private static CarBaseCreator ContextCreator { get; set; }
        private static IUnitOfWork<ConnectionContext> myUow { get; set; }
        private InternalRepository<Car, ConnectionContext> MyCarsRepository { get; set; }

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            ContextCreator = new CarBaseCreator();
            ContextCreator.Intialize();
            ContextCreator.teste();
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            ContextCreator.Dispose();
        }



        [TestInitialize]
        public void testInit()
        {
            //SqlConnection connection = new SqlConnection(connectionString);
            //var context = new ConnectionContext(connection, true
            ContextCreator.Intialize();
            var context = ContextCreator.GetContext();


            myUow = new UnitOfWorkDapper<ConnectionContext>(context);
            Dictionary<string, Type> fieldIdName = new Dictionary<string, Type>();
            fieldIdName.Add("id", typeof(int));
            this.MyCarsRepository = new InternalRepository<Car, ConnectionContext>(myUow, tablename, fieldIdName);
        }

        [TestCleanup]
        public void TestCleanUp()
        {
            myUow.Dispose();
            this.MyCarsRepository = null;
        }

        private string sql { get { return $"INSERT INTO {tablename} VALUES(@id, @name)"; } }
        private string[] carnames = { "Honda Civic", "Fiat Toro", "Toyota Corola", "Volkswagen Golf", "Hyundai Santa Fé", "Chevrolet Camaro", "Jeep Renagate", "Audi A5", "Peugeot 307", "Ford Fusion" };
        public void TestInsert10Cars()
        {
            for (int i = 0; i < 10; i++)
            {
                IDictionary<string, object> param = new ExpandoObject();
                param.Add("@id", i + 1);
                param.Add("@name", carnames[i]);
                this.MyCarsRepository.execute(sql, param);
            }
            myUow.saveChange();
        }

        public void TestClearCars()
        {
            //Delete
            var sql = $"DELETE FROM {tablename}";
            this.MyCarsRepository.execute(sql, null, 10);
            myUow.saveChange();

            //Select N
            Assert.AreEqual(0, this.MyCarsRepository.FindAll().Count);
        }

        [TestMethod]
        public void SimpleCrud()
        {
            //Insert
            var sql = $"INSERT INTO {tablename} VALUES(@id, @name)";
            IDictionary<string, object> param = new ExpandoObject();
            param.Add("@id", 1);
            param.Add("@name", "Honda Civic");
            this.MyCarsRepository.execute(sql, param);
            myUow.saveChange();


            //Select one
            var car = this.MyCarsRepository.Find(f => f.id == 1);
            Assert.IsNotNull(car);
            Assert.AreEqual("Honda Civic", car.name);

            //Update
            sql = $"UPDATE {tablename} SET name = @name WHERE id = @id";
            param = new ExpandoObject();
            param.Add("@id", 1);
            param.Add("@name", "Fiat Uno");
            this.MyCarsRepository.execute(sql, param);
            myUow.saveChange();

            //Select one
            car = null;
            car = this.MyCarsRepository.Find(f => f.id == 1);
            Assert.IsNotNull(car);
            Assert.AreEqual("Fiat Uno", car.name);

            //Select N
            Assert.AreEqual(1, this.MyCarsRepository.FindAll().Count);

            //Delete
            sql = $"DELETE FROM {tablename} WHERE id = @id";
            param = new ExpandoObject();
            param.Add("@id", 1);
            this.MyCarsRepository.execute(sql, param);
            myUow.saveChange();

            //Select N
            Assert.AreEqual(0, this.MyCarsRepository.FindAll().Count);
        }

        [TestMethod]
        public void Test_RepositoryReadOnly()
        {
            //Insert
            TestInsert10Cars();
            
            //Select a Randon index between 0 and 9 and get name selected
            int ind = new Random().Next(0, 9);
            string carname = carnames[ind];

            //Create Repository ReadOnly
            Dictionary<string, Type> fieldIdName = new Dictionary<string, Type>();
            fieldIdName.Add("id", typeof(int));
            IRepositoryReadOnly<Car> RoRepository = new InternalRepository<Car, ConnectionContext>(myUow, tablename, fieldIdName);
            
            //Test Find method
            var Car_ind = RoRepository.Find(f => f.name == carname);
            Assert.IsNotNull(Car_ind);
            Assert.AreEqual(true, Car_ind.id == ind + 1 && Car_ind.name == carname);

            //Test FindAll
            var Cars = RoRepository.FindAll();
            Assert.IsNotNull(Cars);
            Assert.AreEqual(10, Cars.Count);

            //Delete
            TestClearCars();
        }

        [TestMethod]
        public void Test_RepositoryReadOnlyTSQL()
        {
            //Insert
            TestInsert10Cars();
            try
            {
                //Select a Randon index between 0 and 9 and get name selected
                int ind = new Random().Next(0, 9);
                string carname = carnames[ind];

                //Create Repository ReadOnly
                Dictionary<string, Type> fieldIdName = new Dictionary<string, Type>();
                fieldIdName.Add("id", typeof(int));
                IRepositoryReadOnlyTSQL<Car> RoRepository = new InternalRepository<Car, ConnectionContext>(myUow, tablename, fieldIdName);


                //Test Find method
                var sql = $"Select * from {tablename} where name = @name";
                IDictionary<string, object> paramz = new ExpandoObject();
                paramz.Add("@name", carnames[ind]);

                var Car_ind = RoRepository.Find(sql, paramz);
                Assert.IsNotNull(Car_ind);
                Assert.AreEqual(true, Car_ind.id == ind + 1 && Car_ind.name == carname);

                //Test FindAll
                sql = $"Select * from {tablename}";
                var Cars = RoRepository.FindAll(sql, null);
                Assert.IsNotNull(Cars);
                Assert.AreEqual(10, Cars.Count);

                //Test FindAll Pagination
                var CarPag1 = RoRepository.FindAll(1, 5, "name", sql, false);
                Assert.IsNotNull(CarPag1);
                Assert.AreEqual(10, CarPag1.TotalRegisters);
                Assert.AreEqual(5, CarPag1.Collection.Count);
            }
            catch (Exception ex)
            {

                throw ex;
            }
            finally
            {
                //Delete
                TestClearCars();
            }
        }

        [TestMethod]
        public void Test_RepositoryReadOnlyExtended()
        {
            //Insert
            TestInsert10Cars();

            //Select a Randon index between 0 and 9 and get name selected
            int ind = new Random().Next(0, 9);
            string carname = carnames[ind];

            //Create Repository ReadOnly
            Dictionary<string, Type> fieldIdName = new Dictionary<string, Type>();
            fieldIdName.Add("id", typeof(int));
            IRepositoryReadOnlyExtended<Car> RoRepository = new InternalRepository<Car, ConnectionContext>(myUow, tablename, fieldIdName);

            try
            {
                //Reload
                Car Car_ind = new Car { id = ind + 1 };
                RoRepository.Reload(ref Car_ind);

                Assert.AreEqual(carname, Car_ind.name);

                //Count
                Assert.AreEqual(10, RoRepository.Count());

                //Exists
                Assert.AreEqual(true, RoRepository.Exists(f => f.name == carname));
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                //Delete
                TestClearCars();
            }
          
        }

        [TestMethod]
        public void Test_RepositoryReadOnlyExtendedTSQL()
        {
            //Insert
            TestInsert10Cars();

            //Select a Randon index between 0 and 9 and get name selected
            int ind = new Random().Next(0, 9);
            string carname = carnames[ind];

            //Create Repository ReadOnly
            Dictionary<string, Type> fieldIdName = new Dictionary<string, Type>();
            fieldIdName.Add("id", typeof(int));
            IRepositoryReadOnlyExtendedTSQL<Car> RoRepository = new InternalRepository<Car, ConnectionContext>(myUow, tablename, fieldIdName);

            try
            {

                //Count
                Assert.AreEqual(10, RoRepository.Count($"SELECT * FROM {tablename}"));

                //Exists
                Assert.AreEqual(true, RoRepository.Exists($"SELECT * FROM {tablename} WHERE name = '{carname}'"));
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                //Delete
                TestClearCars();
            }
        }

        [TestMethod]
        public void Test_RepositoryReadOnlyPagination()
        {
            //Insert
            TestInsert10Cars();

            //Select a Randon index between 0 and 9 and get name selected
            int ind = new Random().Next(0, 9);
            string carname = carnames[ind];

            //Create Repository ReadOnly
            Dictionary<string, Type> fieldIdName = new Dictionary<string, Type>();
            fieldIdName.Add("id", typeof(int));
            IRepositoryReadOnlyPagination<Car> RoRepository = new InternalRepository<Car, ConnectionContext>(myUow, tablename, fieldIdName);

            try
            {
                //Pagination
                var CarsPag2 = RoRepository.FindAll(2, 5, "name", false, f => f.name != carname);
                Assert.IsNotNull(CarsPag2);
                Assert.AreEqual(9, CarsPag2.TotalRegisters);
                Assert.AreEqual(true, CarsPag2.Collection.Count <= 5);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                //Delete
                TestClearCars();
            }
        }
    }
}
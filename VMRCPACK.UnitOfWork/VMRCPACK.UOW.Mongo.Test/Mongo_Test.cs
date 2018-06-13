using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Bson.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using VMRCPACK.UnitOfWork.Interfaces.Repository;
using VMRCPACK.UnitOfWork.Interfaces.UnitOfWork;
using VMRCPACK.UOW.Mongo.Repositories;
using VMRCPACK.UOW.Mongo.support;
using VMRCPACK.UOW.Mongo.Uow;

namespace VMRCPACK.UOW.Mongo.Test
{
    [TestClass]
    public class Mongo_Test
    {
        private const string mongoConnectionString = "mongodb://user:password@yourserver:port";
        private const string mongoDatabaseName = "TestVMRCPACK";

        //Entity Classes for tests
        internal class Assembler
        {
            public int id { get; set; }
            public string name { get; set; }
            public virtual IList<Car> cars { get; set; }

        }

        internal class Car
        {
            public int id { get; set; }
            public string name { get; set; }
        }

        internal class MyContext : MongoContext
        {
            public MyContext(string connectionstring, string databasename)
                : base(connectionstring, databasename)
            {
            }

            public override void OnModelBuilder()
            {
                if (!BsonClassMap.IsClassMapRegistered(typeof(Assembler)))
                    BsonClassMap.RegisterClassMap<Assembler>(model =>
                    {
                        model.AutoMap();
                        model.SetIdMember(model.GetMemberMap(m => m.id));
                        model.GetMemberMap(m => m.cars).SetIgnoreIfNull(true);
                    });


                if (!BsonClassMap.IsClassMapRegistered(typeof(Car)))
                    BsonClassMap.RegisterClassMap<Car>(model =>
                    {
                        model.AutoMap();
                        model.SetIdMember(model.GetMemberMap(m => m.id));
                    });
            }
        }


        internal class MyRepository<TEntity, TContext> : RepositoryMongo<TEntity, TContext>
            where TEntity : class
            where TContext : MongoContext
        {
            public MyRepository(IUnitOfWorkDocument<TContext> unitOfWork, string collectionName, KeyValuePair<string, Type> IdFieldMetadata)
                : base(unitOfWork, collectionName, IdFieldMetadata)
            {
            }
        }

        private UnitOfWorkMongo<MyContext> uow { get; set; }
        private MyRepository<Assembler, MyContext> RepositoryAssembler { get; set; }

        [TestInitialize]
        public void TestInit()
        {
            this.uow = new UnitOfWorkMongo<MyContext>(new MyContext(mongoConnectionString, mongoDatabaseName));
            this.RepositoryAssembler = new MyRepository<Assembler, MyContext>(this.uow, "Assemblers", new KeyValuePair<string, Type>("id", typeof(int)));
        }

        [TestCleanup]
        public void TestCleanup()
        {
            this.uow.Dispose();
            this.uow = null;
            this.RepositoryAssembler = null;
        }

        private string[] assmblers = { "Honda", "Fiat", "Toyota", "Volkswagen", "Hyundai", "Chevrolet", "Jeep", "Audi", "Peugeot", "Ford" };

        private void TestInsert10Assember()
        {
            for (int i = 0; i < 10; i++)
            {
                var asb = new Assembler
                {
                    id = i + 1,
                    name = assmblers[i],
                    cars = new List<Car>(),
                };
                for (int j = 1; j < 4; j++)
                {
                    asb.cars.Add(new Car
                    {
                        id = j,
                        name = $"Car {j}",
                    });
                }
                this.RepositoryAssembler.Add(asb);
            }
        }

        private void TestClearDb()
        {
            for (int i = 0; i < 10; i++)
            {
                this.RepositoryAssembler.Remove(f => f.id == i+1);
            }
        }

        [TestMethod]
        public void SimpleCrud()
        {
            var honda = new Assembler
            {
                id = 1,
                name = "Honda",
            };

            this.RepositoryAssembler.Add(honda);

            Assert.IsNotNull(this.RepositoryAssembler.Find(f => f.id == honda.id));

            honda.cars = new List<Car>();
            honda.cars.Add(new Car { id = 1, name = "Honda Fit" });
            honda.cars.Add(new Car { id = 2, name = "Honda Civic" });

            this.RepositoryAssembler.Update(f => f.id == honda.id, honda);

            Assert.AreEqual(2, this.RepositoryAssembler.Find(f => f.id == honda.id).cars.Count);

            this.RepositoryAssembler.Remove(f => f.id == honda.id);

            Assert.AreEqual(true, (this.RepositoryAssembler.FindAll() == null || this.RepositoryAssembler.FindAll().Count == 0));
        }


        [TestMethod]
        public void Test_RepositorReadOnly()
        {
            //Insert
            TestInsert10Assember();

            //Select a Randon index between 0 and 9 and get name selected
            int ind = new Random().Next(0, 9);
            string assembler = assmblers[ind];

            //Create Repository
            IRepositoryReadOnly<Assembler> RoRepository = new MyRepository<Assembler, MyContext>(this.uow, "Assemblers", new KeyValuePair<string, Type>("id", typeof(int)));

            //Test Find method
            var asb = RoRepository.Find(f => f.name == assembler);
            Assert.IsNotNull(asb);
            Assert.AreEqual(true, asb.id == ind + 1 && asb.name == assembler);

            //Test FindAll
            var asbs = RoRepository.FindAll(f => 1 == 1);
            Assert.IsNotNull(asbs);
            Assert.AreEqual(10, asbs.Count);

            //Delete
            TestClearDb();
        }

        [TestMethod]
        public void Test_RepositorReadOnlyAsync()
        {
            //Insert
            TestInsert10Assember();

            //Select a Randon index between 0 and 9 and get name selected
            int ind = new Random().Next(0, 9);
            string assembler = assmblers[ind];

            //Create Repository
            IRepositoryReadOnlyAsync<Assembler> RoRepository = new MyRepository<Assembler, MyContext>(this.uow, "Assemblers", new KeyValuePair<string, Type>("id", typeof(int)));

            //Test Find method
            var asb = RoRepository.FindAsync(f => f.name == assembler).Result;
            Assert.IsNotNull(asb);
            Assert.AreEqual(true, asb.id == ind + 1 && asb.name == assembler);

            //Test FindAll
            var asbs = RoRepository.FindAllAsync(f => 1 == 1).Result;
            Assert.IsNotNull(asbs);
            Assert.AreEqual(10, asbs.Count);

            //Delete
            TestClearDb();
        }

        [TestMethod]
        public void Test_RepositorReadOnlyExtended()
        {
            //Insert
            TestInsert10Assember();

            //Select a Randon index between 0 and 9 and get name selected
            int ind = new Random().Next(0, 9);
            string assembler = assmblers[ind];

            //Create Repository
            IRepositoryReadOnlyExtended<Assembler> RoRepository = new MyRepository<Assembler, MyContext>(this.uow, "Assemblers", new KeyValuePair<string, Type>("id", typeof(int)));

            //Test Count method
            Assert.AreEqual(10, RoRepository.Count(f => 1 == 1));

            //Test FindAll
            Assert.IsTrue(RoRepository.Exists(f => f.name == assembler));

            //Test Reload
            Assembler asb = new Assembler { id = ind + 1 };
            RoRepository.Reload(ref asb);
            Assert.AreEqual(assembler, asb.name);

            //Delete
            TestClearDb();
        }

        [TestMethod]
        public void Test_RepositorReadOnlyExtendedAsync()
        {
            //Insert
            TestInsert10Assember();

            //Select a Randon index between 0 and 9 and get name selected
            int ind = new Random().Next(0, 9);
            string assembler = assmblers[ind];

            //Create Repository
            IRepositoryReadOnlyExtendedAsync<Assembler> RoRepository = new MyRepository<Assembler, MyContext>(this.uow, "Assemblers", new KeyValuePair<string, Type>("id", typeof(int)));

            //Test Count method
            Assert.AreEqual(10, RoRepository.CountAsync(f => 1 == 1).Result);

            //Test FindAll
            Assert.IsTrue(RoRepository.ExistsAsync(f => f.name == assembler).Result);

            //Delete
            TestClearDb();
        }

        [TestMethod]
        public void Test_RepositorReadOnlyPagination()
        {
            //Insert
            TestInsert10Assember();

            //Select a Randon index between 0 and 9 and get name selected
            int ind = new Random().Next(0, 9);
            string assembler = assmblers[ind];

            //Create Repository
            IRepositoryReadOnlyPaginationExtended<Assembler> RoRepository = new MyRepository<Assembler, MyContext>(this.uow, "Assemblers", new KeyValuePair<string, Type>("id", typeof(int)));

            //Test FindAll method
            var asb = RoRepository.FindAll(1, 5, f => f.name, false, f => 1 == 1);
            Assert.IsNotNull(asb);
            Assert.AreEqual(10, asb.TotalRegisters);
            Assert.AreEqual(5, asb.Collection.Count);

            //Delete
            TestClearDb();
        }

        [TestMethod]
        public void Test_RepositorReadOnlyPaginationAsync()
        {
            //Insert
            TestInsert10Assember();

            //Select a Randon index between 0 and 9 and get name selected
            int ind = new Random().Next(0, 9);
            string assembler = assmblers[ind];

            //Create Repository
            IRepositoryReadOnlyPaginationExtendedAsync<Assembler> RoRepository = new MyRepository<Assembler, MyContext>(this.uow, "Assemblers", new KeyValuePair<string, Type>("id", typeof(int)));

            //Test FindAll method
            var asb = RoRepository.FindAllAsync(1, 5, f => f.name, false, f => 1 == 1).Result;
            Assert.IsNotNull(asb);
            Assert.AreEqual(10, asb.TotalRegisters);
            Assert.AreEqual(5, asb.Collection.Count);

            //Delete
            TestClearDb();
        }

        [TestMethod]
        public void Test_RepositorQueryable()
        {
            //Insert
            TestInsert10Assember();

            //Select a Randon index between 0 and 9 and get name selected
            int ind = new Random().Next(0, 9);
            string assembler = assmblers[ind];

            //Create Repository
            IRepositoryReadOnlyQueryable<Assembler> RoRepository = new MyRepository<Assembler, MyContext>(this.uow, "Assemblers", new KeyValuePair<string, Type>("id", typeof(int)));

            //Test Find method
            var asb = RoRepository.Query.Where(f => f.name == assembler).FirstOrDefault();
            Assert.IsNotNull(asb);
            Assert.AreEqual(true, asb.id == ind + 1 && asb.name == assembler);

            //Delete
            TestClearDb();
        }
    }
}

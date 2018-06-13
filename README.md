# Unit Of Work Pack

Unit of Work Pattern Pack help you implement your uow and Respository code. 
It supports EF Core, Dapper and MongoDb.
It have a separeted project with Interfaces for you easily apply Dependecy injection in your projects.


## Getting Started

1.  Installation library
    You need to install / Add this library in your project application. 
    
    * [VMRCPACK.UOW.Interfaces](https://www.nuget.org/packages/VMRCPACK.UOW.Interfaces/) - Nuget Pack with Interface project. Used for Dependency Injection.
    * [VMRCPACK.UOW.EF](https://www.nuget.org/packages/VMRCPACK.UOW.EF/) - Nuget Pack with implementation project for EF Core.
    * [VMRCPACK.UOW.Dapper](https://www.nuget.org/packages/VMRCPACK.UOW.Dapper/) - Nuget Pack with implementation project for Dapper and Slapper mapper.
    * [VMRCPACK.UOW.Mongo](https://www.nuget.org/packages/VMRCPACK.UOW.Mongo/) - Nuget Pack with implementation project for MongoDB
    
2.  Coding
    On sequency a little code fragment for you starting coding with this library:
    
    
    
    ### EF Core sample

```csharp
        //Entity Classes for tests
        internal class Car
        {
            public int id { get; set; }
            public string name { get; set; }
        }


        //DbContext Implementation on your code
        class Context : DbContext
        {
            public Context(DbContextOptions Optios)
                : base(Optios) { }

            public DbSet<Car> Cars { get; set; }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                base.OnModelCreating(modelBuilder);

                modelBuilder.Entity<Car>()
                    .HasKey(f => f.id);
                   
            }

            public override void Dispose()
            {
                base.Dispose();
            }
        }
        
        ///Internal Interface that implement the interfaces that you want
        internal interface ITest<TEntity> : IRepositoryReadOnly<TEntity>,
                                            IRepositoryReadOnlyExtended<TEntity>,
                                            IRepositoryWrite<TEntity> where TEntity : class
        {

        }

        ///Internal Class that inherety the Repository and implement the Internal Interface
        internal class MyRepository<TEntity, TContext> : RepositoryEF<TEntity, TContext>, ITest<TEntity>
            where TEntity : class
            where TContext : DbContext
        {
            public MyRepository(IUnitOfWork<TContext> unitOfWork)
                : base(unitOfWork)
            {

            }

        }
        
        //.......
        //Test for creation cars using only unitofwork 
        [TestMethod]
        public void Test_UnitOfWork_Success()
        {
            vmuow.UnitOfWorkEFcore<Context> uow = new vmuow.UnitOfWorkEFcore<Context>(new DbContextOptionsBuilder<Context>()
                                                                                .UseInMemoryDatabase("Teste"));
                                                                                
            ITest<Car> RepCar = new MyRepository<Car, Context>(uow);

            using (var ctx = uow.dbContext)
            {
                RepCar.Add(new Car { id = 1, name = "Honda Civic" });
                uow.SaveChanges();
            }

            IList<Car> cars = null;
            using(var ctx = uow.dbContext)
            {
                 cars = RepCar.FindAll().ToList();
            }

            Assert.AreEqual(1, cars.Count);


            var carTst = new Car
            {
                id = 1,
                name = "Toyota Corola"
            };

            using (var ctx = uow.dbContext)
            {
                RepCar.Update(carTst);
                uow.SaveChanges();
            }

        }

```

    ### Dapper sample
    
    ```csharp
    ///Internal Class that inherety the Repository and implement the Internal Interface
    internal class InternalRepository<TEntity, TContext> : RepositoryDapper<TEntity, TContext>,  ITest<TEntity>
             where TEntity : class
             where TContext : ConnectionContext
        {
            public InternalRepository(IUnitOfWork<TContext> unitOfWork, string tablename, Dictionary<string, Type> fieldIdName)
                : base(unitOfWork, tablename, fieldIdName)
            {
            }
        }
        
        //......... 
        [TestMethod]
        public void TestInsert10Cars()
        {
            string sql { get { return $"INSERT INTO {tablename} VALUES(@id, @name)"; } }
            
            IDictionary<string, object> param = new ExpandoObject();
            param.Add("@id", i + 1);
            param.Add("@name", carnames[i]);
            this.MyCarsRepository.execute(sql, param);
            uow.saveChange();
        }
    
    ```
    
    

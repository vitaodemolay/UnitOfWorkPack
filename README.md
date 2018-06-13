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
        
        //.......
        //Test for creation cars using only unitofwork 
        [TestMethod]
        public void Test_UnitOfWork_Success()
        {
            vmuow.UnitOfWorkEFcore<Context> uow = new vmuow.UnitOfWorkEFcore<Context>(new DbContextOptionsBuilder<Context>()
                                                                                .UseInMemoryDatabase("Teste"));

            using (var ctx = uow.dbContext)
            {
                ctx.Cars.Add(new Car { id = 1, name = "Honda Civic" });
                ctx.SaveChanges();
            }

            IList<Car> cars = null;
            using(var ctx = uow.dbContext)
            {
                 cars = ctx.Cars.ToList();
            }

            Assert.AreEqual(1, cars.Count);


            var carTst = new Car
            {
                id = 1,
                name = "Toyota Corola"
            };

            using (var ctx = uow.dbContext)
            {
                ctx.Cars.Update(carTst);
                ctx.SaveChanges();
            }

            cars = null;
            using (var ctx = uow.dbContext)
            {
                cars = ctx.Cars.ToList();
            }

            Assert.AreEqual(1, cars.Count);
            Assert.AreEqual("Toyota Corola", cars.First().name);
        }

```

using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using VMRCPACK.UnitOfWork.Interfaces.Repository;
using VMRCPACK.UnitOfWork.Interfaces.UnitOfWork;
using VMRCPACK.UOW.EF.Repositories;
using vmuow = VMRCPACK.UOW.EF.Uow;

namespace VMRCPACK.UOW.EF.Test
{
    [TestClass]
    public class Repositories_EF_Test
    {
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
            public int? assemblerId { get; set; }
            public virtual Assembler assembler { get; set; }
        }


        //DbContext Implementation on your code
        internal class Context : DbContext
        {
            public Context(DbContextOptions Optios)
                : base(Optios) { }


            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                base.OnModelCreating(modelBuilder);

                modelBuilder.Entity<Car>()
                    .HasKey(f => f.id);

                modelBuilder.Entity<Assembler>()
                    .HasKey(f => f.id);

                modelBuilder.Entity<Assembler>()
                    .HasMany(f => f.cars).WithOne(f => f.assembler).HasForeignKey(f => f.assemblerId).IsRequired(false);

            }

            public override void Dispose()
            {
                base.Dispose();
            }
        }


        //Test for creation Assemblers
        [TestMethod]
        public void RepositoryWrite_Assembler_Success()
        {
            vmuow.UnitOfWorkEFcore<Context> uow = new vmuow.UnitOfWorkEFcore<Context>(new DbContextOptionsBuilder<Context>()
                                                                                .UseInMemoryDatabase("Teste"));

            ITest<Assembler> RepAssembler = new MyRepository<Assembler, Context>(uow);

            using (var ctx = uow.dbContext)
            {
                RepAssembler.Add(new Assembler {
                     id = 1,
                     name = "Honda"
                });

                RepAssembler.Add(new Assembler
                {
                    id = 2,
                    name = "Toyota"
                });

                RepAssembler.Add(new Assembler
                {
                    id = 3,
                    name = "Fiat"
                });

                ctx.SaveChanges();
            }

            IList<Assembler> Asseblers = null;
            using (var ctx = uow.dbContext)
            {
                Asseblers = RepAssembler.FindAll();
            }

            Assert.AreEqual(3, Asseblers.Count);
            uow.Dispose();
        }


        //Test for creation cars and use Assemblers created on the previous test 
        [TestMethod]
        public void RepositoryWrite_car_Success()
        {
            RepositoryWrite_Assembler_Success();

            vmuow.UnitOfWorkEFcore<Context> uow = new vmuow.UnitOfWorkEFcore<Context>(new DbContextOptionsBuilder<Context>()
                                                                                .UseInMemoryDatabase("Teste"));


            ITest<Car> RepCar = new MyRepository<Car, Context>(uow);
            ITest<Assembler> RepAssembler = new MyRepository<Assembler, Context>(uow);

            using (var ctx = uow.dbContext)
            {
                var _assembler = RepAssembler.Find(f => f.name == "Honda");

                Assert.IsNotNull(_assembler);

                var car = new Car
                {
                    id = 1,
                    name = "Honda Civic",
                    assembler = _assembler,
                };

                RepCar.Add(car);
                ctx.SaveChanges();
            }

            IList<Assembler> assemblers = null;
            IList<Car> cars = null;
            using (var ctx = uow.dbContext)
            {
                assemblers = RepAssembler.FindAll();
                cars = RepCar.FindAll();
            }

            Assert.AreEqual(3, assemblers.Count);
            Assert.AreEqual(1, cars.Count);

           

            var carTst = new Car
            {
                id = 1,
                name = "Toyota Corola",
                assembler = assemblers.FirstOrDefault(f=> f.name == "Toyota")
            };

            Assert.IsNotNull(carTst.assembler);

            using (var ctx = uow.dbContext)
            {
                RepCar.Update(carTst);
                ctx.SaveChanges();
            }

            cars = null;
            assemblers = null;

            using (var ctx = uow.dbContext)
            {
                assemblers = RepAssembler.FindAll();
                cars = RepCar.FindAll();
            }


            Assert.AreEqual(3, assemblers.Count);
            Assert.AreEqual(1, cars.Count);
            Assert.AreEqual("Toyota Corola", cars.First().name);
        }
    }
}

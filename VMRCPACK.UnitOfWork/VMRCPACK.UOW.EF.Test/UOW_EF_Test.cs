using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using vmuow = VMRCPACK.UOW.EF.Uow;

namespace VMRCPACK.UOW.EF.Test
{
    [TestClass]
    public class UOW_EF_Test
    {
        //Entity Classes for tests
        internal class Car
        {
            public int id { get; set; }
            public string name { get; set; }
        }



        //DbContext Implementation on your code
        internal class Context : DbContext
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
    }
}

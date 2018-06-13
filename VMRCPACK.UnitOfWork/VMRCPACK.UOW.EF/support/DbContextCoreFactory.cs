using Microsoft.EntityFrameworkCore;
using System;

namespace VMRCPACK.UOW.EF.support
{
    /// <summary>
    /// Factory class for create a instance of dbcontext
    /// </summary>
    /// <typeparam name="TContext">Type of Context object of the database</typeparam>
    public static class DbContextCoreFactory<TContext> where TContext : DbContext
    {
        /// <summary>
        /// Method for create a DbContext Instance
        /// </summary>
        /// <returns></returns>
        public static TContext CreateDbContextInstance()
        {
            if (IsInit)
                return (TContext)Activator.CreateInstance(typeof(TContext), new object[] { Options });

            throw new NullReferenceException("DbContext Core Factory is not initialized");
        }

        /// <summary>
        /// Property for verify initialize status
        /// </summary>
        public static bool IsInit { get; private set; }

        /// <summary>
        /// DbContext Options Property
        /// </summary>
        public static DbContextOptions<TContext> Options { get; private set; }

        /// <summary>
        /// Initalize method
        /// </summary>
        /// <param name="builder">DbContext Option Builder object</param>
        public static void Initialize(DbContextOptionsBuilder<TContext> builder)
        {
            Options = builder.Options;
            IsInit = true;
        }
    }
}

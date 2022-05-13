using Kloon.EmployeePerformance.DataAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kloon.EmployeePerformance.DataAccess
{
    public sealed class UnitOfWork<TContext> : IUnitOfWork<TContext>
            where TContext : DbContext
    {
        /// <summary>
        /// The DbContext
        /// </summary>
        public DbContext dbContext;

        /// <summary>
        /// The repositories
        /// </summary>
        private Dictionary<Type, object> _repositories;

        /// <summary>
        /// Initializes a new instance of the <see cref="UnitOfWork" /> class.
        /// </summary>
        /// <param name="contextFactory">The context factory.</param>
        public UnitOfWork(IContextFactory contextFactory)
        {
            dbContext = contextFactory.GetDbContext<TContext>();
        }

        /// <summary>
        /// Gets the repository.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <returns></returns>
        public IEntityRepository<TEntity> GetRepository<TEntity>()
            where TEntity : class, new()
        {
            if (_repositories == null)
            {
                _repositories = new Dictionary<Type, object>();
            }

            var type = typeof(TEntity);
            if (!_repositories.ContainsKey(type))
            {
                _repositories[type] = new EntityRepository<TEntity>(dbContext);
            }

            return (IEntityRepository<TEntity>)_repositories[type];
        }

        /// <summary>
        /// Gets the customized repository.
        /// </summary>
        /// <typeparam name="TRepository">The type of the customized repository.</typeparam>
        /// <returns></returns>
        public TRepository GetCustomizedRepository<TRepository>()
            where TRepository : class
        {
            if (_repositories == null)
            {
                _repositories = new Dictionary<Type, object>();
            }

            var type = typeof(TRepository);
            if (!_repositories.ContainsKey(type))
            {
                _repositories[type] = Activator.CreateInstance(typeof(TRepository), dbContext);
            }

            return (TRepository)_repositories[type];
        }


        /// <returns>The number of objects in an Added, Modified, or Deleted state</returns>
        /// <summary>
        /// Disposes the current object
        /// </summary>
        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(obj: this);
        }

        /// <summary>
        /// Disposes all external resources.
        /// </summary>
        /// <param name="disposing">The dispose indicator.</param>
        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (dbContext != null)
                {
                    dbContext.Dispose();
                    dbContext = null;
                }
            }
        }

        public int Save()
        {
            return dbContext.SaveChanges();
        }

        public void BeginTransaction()
        {
            dbContext.Database.BeginTransaction();
        }
        public void CommitTransaction()
        {
            dbContext.Database.CommitTransaction();
        }
        public void RollbackTransaction()
        {
            dbContext.Database.RollbackTransaction();
        }
    }
}

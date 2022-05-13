using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kloon.EmployeePerformance.DataAccess
{
    public interface IUnitOfWork<TContext> : IDisposable
        where TContext : DbContext
    {
        /// <summary>
        /// Gets the repository.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <returns>Repository</returns>
        IEntityRepository<TEntity> GetRepository<TEntity>()
            where TEntity : class, new();

        /// <summary>
        /// Gets the customized repository.
        /// </summary>
        /// <typeparam name="TRepository">The type of the customized repository.</typeparam>
        /// <returns></returns>
        TRepository GetCustomizedRepository<TRepository>()
            where TRepository : class;

        /// <summary>
        /// Saves all pending changes
        /// </summary>
        /// <returns>The number of objects in an Added, Modified, or Deleted state</returns>
        int Save();
        void BeginTransaction();
        void CommitTransaction();
        void RollbackTransaction();
    }
}

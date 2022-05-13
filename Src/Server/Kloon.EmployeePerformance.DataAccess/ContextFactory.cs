using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;

namespace Kloon.EmployeePerformance.DataAccess
{
    public abstract class ContextFactory : IContextFactory
    {
        protected readonly ConnectionSettings _connectionSettings;
        private static ConcurrentDictionary<string, DbContextOptions> _options = new ConcurrentDictionary<string, DbContextOptions>();

        public ContextFactory(IOptions<ConnectionSettings> options)
        {
            _connectionSettings = options.Value;
        }

        private DbContext _dbContext;

        public DbContext GetDbContext<TContext>() where TContext : DbContext
        {
            if (_dbContext == null)
            {
                var options = BuildDbContextOption<TContext>();

                _dbContext = (DbContext)Activator.CreateInstance(typeof(TContext), options);
            }

            return _dbContext;
        }

        protected virtual DbContextOptions BuildDbContextOption<TContext>()
            where TContext : DbContext
        {
            var connectionString = GetConnectionString();
            if (_options.TryGetValue(connectionString, out var option))
            {
                return option;
            }

            var contextOptionsBuilder = new DbContextOptionsBuilder<TContext>();

            contextOptionsBuilder.UseSqlServer(connectionString);

            var options = contextOptionsBuilder.Options;
            _options.TryAdd(connectionString, options);

            return options;
        }

        protected abstract string GetConnectionString();

        public static TDbContext CreateDbContext<TDbContext>(string connectionString)
            where TDbContext : DbContext
        {
            var contextOptionsBuilder = new DbContextOptionsBuilder<TDbContext>();

            contextOptionsBuilder.UseSqlServer(connectionString);

            var dbContext = (TDbContext)Activator.CreateInstance(typeof(TDbContext), contextOptionsBuilder.Options);
            return dbContext;
        }
    }
}

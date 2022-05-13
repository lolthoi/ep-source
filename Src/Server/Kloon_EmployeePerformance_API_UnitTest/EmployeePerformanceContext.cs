using Kloon.EmployeePerformance.DataAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kloon_EmployeePerformance_UnitTest
{
    public class EmployeePerformanceContextFactory : ContextFactory
    {
        public EmployeePerformanceContextFactory(
        IOptions<ConnectionSettings> options)
        : base(options)
        {
        }
        protected override string GetConnectionString()
        {
            return _connectionSettings.ConnectionStrings["EmployeePerformanceContext"];
        }

        protected override DbContextOptions BuildDbContextOption<TContext>()
        {
            var builder = new DbContextOptionsBuilder<TContext>();
            builder.UseInMemoryDatabase("APITest");
            builder.ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning));
            var options = builder.Options;
            return options;
        }
    }
}

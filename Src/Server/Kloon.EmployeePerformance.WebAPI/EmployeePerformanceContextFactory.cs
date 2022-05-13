using Kloon.EmployeePerformance.DataAccess;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Kloon.EmployeePerformance.WebAPI
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
    }
}

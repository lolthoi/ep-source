using Kloon.EmployeePerformance.DataAccess;
using Kloon.EmployeePerformance.Logic.Caches;
using Microsoft.Extensions.Logging;
using System;

namespace Kloon.EmployeePerformance.Logic.Services.Base
{
    public interface ICommonService
    {
        IServiceProvider ServiceProvider { get; }
        IUnitOfWork<EmployeePerformanceContext> DbContext { get; }
        CacheProvider Cache { get; }
        ILogger Logger { get; }
    }
    public class CommonService : ICommonService
    {
        private readonly IServiceProvider _serviceProvider;
        IServiceProvider ICommonService.ServiceProvider { get { return _serviceProvider; } }

        private readonly IUnitOfWork<EmployeePerformanceContext> _dbContext;

        IUnitOfWork<EmployeePerformanceContext> ICommonService.DbContext { get { return _dbContext; } }

        private readonly CacheProvider _cacheProvider;
        CacheProvider ICommonService.Cache { get { return _cacheProvider; } }

        private readonly ILogger _logger;
        ILogger ICommonService.Logger { get { return _logger; } }

        public CommonService(
            IServiceProvider serviceProvider,
            IUnitOfWork<EmployeePerformanceContext> dbContext,
            CacheProvider cacheProvider,
            ILogger logger
        )
        {
            _serviceProvider = serviceProvider;
            _dbContext = dbContext;
            _cacheProvider = cacheProvider;
            _logger = logger;
        }
    }
}

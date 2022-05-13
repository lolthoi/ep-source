using Kloon.EmployeePerformance.DataAccess;
using Kloon.EmployeePerformance.Logic.Caches;
using Kloon.EmployeePerformance.Models.User;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kloon.EmployeePerformance.Logic.Services.Base
{
    public interface IAuthenLogicService<T> : ICommonService
    {
        LogicResult Start();
    }
    public class AuthenLogicService<T> : CommonService, IAuthenLogicService<T>
    {
        private readonly IIdentityService _identityService;

        public AuthenLogicService(
            IIdentityService identityService,
            IServiceProvider serviceProvider,
            IUnitOfWork<EmployeePerformanceContext> dbContext,
            CacheProvider cacheProvider,
            ILoggerFactory loggerFactory
        ) : base(serviceProvider, dbContext, cacheProvider, loggerFactory.CreateLogger<T>())
        {
            _identityService = identityService;
        }
        public LogicResult Start()
        {
            var currentUser = GetCurrentUser();
            return new LogicResult(currentUser, this);
        }


        private LoginUserModel.LoginUser GetCurrentUser()
        {
            var currentUser = _identityService.GetCurrentUser();
            return currentUser.User;
        }
    }
}

using Kloon.EmployeePerformance.Logic.Caches;
using Kloon.EmployeePerformance.Logic.Services;
using Kloon.EmployeePerformance.Logic.Services.Base;
using Microsoft.Extensions.DependencyInjection;
using Kloon.EmployeePerformance.Models.Common;
using Kloon.EmployeePerformance.Models.User;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Kloon_EmployeePerformance_UnitTest
{
    public class FakeIdentityApiService : IIdentityService
    {
        private readonly ConcurrentDictionary<string, TestResult> _testResults = new ConcurrentDictionary<string, TestResult>();
        private CacheProvider _cache;
        private readonly ITestService _testLogic;

        public FakeIdentityApiService(CacheProvider cache, ITestService testService)
        {
            _cache = cache;
            _testLogic = testService;
        }

        public LoginUserModel GetCurrentUser()
        {
            return _testLogic.GetLoginUser();
        }
    }

    public interface ITestService
    {
        LoginUserModel GetLoginUser();

        TestResult StartLoginWithAdmin();

        TestResult StartLoginWithUser(string username, string password);
    }

    public class TestService: ITestService
    {
        private readonly ConcurrentDictionary<string, TestResult> _testResults = new ConcurrentDictionary<string, TestResult>();
        private TestResult _lastTestResult;
        private readonly AccountSetting _accountSetting;
        private readonly IAuthenticationService _authenService;
        private readonly CacheProvider _cache;

        public TestService(CacheProvider cache, IOptions<AccountSetting> options)
        {
            _accountSetting = options.Value;
            _authenService = Startup.Instance.ServiceProvider.GetService<IAuthenticationService>();
            _cache = cache;
        }

        public LoginUserModel GetLoginUser()
        {
            return _lastTestResult.DataParam.CurrentUser;
        }

        public TestResult StartLoginWithAdmin()
        {
            return Login(_accountSetting.Admin.UserName, _accountSetting.Admin.Password);
        }

        public TestResult StartLoginWithUser(string username, string password)
        {
            return Login(username, password);
        }

        private TestResult Login(string username, string password)
        {
            if (_testResults.TryGetValue(username, out TestResult testResult))
            {
                _lastTestResult = testResult;
                return testResult;
            }

            var user = _authenService.Login(username, password);

            if (user.Error != null)
            {
                throw new System.Exception(user.Error.Message);
            }

            var lstProject = _cache.Users.GetProjects(user.Data.Id);

            var loginUser = new LoginUserModel
            {
                Key = "test",
                User = new LoginUserModel.LoginUser
                {
                    Id = user.Data.Id,
                    Email = user.Data.Email,
                    FirstName = user.Data.FirstName,
                    LastName = user.Data.LastName,
                    PositionId = user.Data.PositionId,
                    Role = user.Data.Role,
                    ProjectRole = lstProject?.Select(t => new ProjectRoleModel
                    {
                        ProjectId = t.Id,
                        ProjectRoleId = (ProjectRoles)t.ProjectRoleId
                    }).ToList()
                }
            };

            var result = new TestResult(this, loginUser);
            _lastTestResult = result;
            _testResults.TryAdd(username, result);
            return result;
        }
    }

    public class TestResult
    {

        private readonly ITestService _testService;
        private bool isSuccessTest = true;
        public readonly DataParam DataParam;

        public TestResult(ITestService testService, LoginUserModel currentUser)
        {
            _testService = testService;
            DataParam = new DataParam(currentUser);
        }


        public TestResult ThenImplementTest(Action<DataParam> action)
        {
            try
            {
                action.Invoke(DataParam);
            }
            catch
            {
                isSuccessTest = false;
                return this;
                //Assert.Fail();
            }

            return this;
        }
        public ITestService ThenCleanDataTest(Action<DataParam> cleanAction)
        {
            try
            {
                if (DataParam.CleanData.HasValue)
                {
                    cleanAction.Invoke(DataParam);
                }
            }
            catch
            {
                isSuccessTest = false;
            }

            if (!isSuccessTest)
            {
                Assert.Fail();
            }
            return _testService;
        }
    }

    public interface IClean
    {
        void Add(string key, object data);

        T Get<T>(string key);

        bool HasValue { get; }
    }

    public class DataParam
    {
        public LoginUserModel CurrentUser { get; private set; }

        public IClean CleanData { get; private set; }

        public DataParam(LoginUserModel currentData)
        {
            CurrentUser = currentData;
            CleanData = new Clean();
        }

        protected class Clean : IClean
        {
            private Dictionary<string, object> _dics = new Dictionary<string, object>();

            public void Add(string key, object data)
            {
                _dics[key] = data;
            }

            public T Get<T>(string key)
            {
                if (_dics.TryGetValue(key, out object data))
                {
                    return (T)data;
                }
                return default(T);
            }

            public bool HasValue { get { return _dics.Count > 0; } }
        }
    }
}

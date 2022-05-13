using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kloon_EmployeePerformance_UnitTest
{
    [TestClass()]
    public abstract class TestBase
    {
        protected AccountSetting _accountSetting;
        protected IServiceScope _scope;
        protected ITestService _testService;

        #region Variables and Constructors

        public TestBase()
        {
            var startup = Startup.Instance;
            _accountSetting = startup.Options.Value;
            _scope = startup.ServiceProvider.CreateScope();
            _testService = _scope.ServiceProvider.GetService<ITestService>();
        }

        #endregion

        protected abstract void InitServices();

        protected abstract void InitEnvirontment();

        protected abstract void CleanEnvirontment();

        [TestInitialize]
        public void TestInitialize()
        {
            InitServices();
            InitEnvirontment();
        }

        [TestCleanup]
        public void TestCleanUp()
        {
            CleanEnvirontment();
        }

    }
}

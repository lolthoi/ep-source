using Kloon.EmployeePerformance.DataAccess;
using Kloon.EmployeePerformance.DataAccess.Extentions;
using Kloon.EmployeePerformance.Logic.Common;
using Kloon.EmployeePerformance.Logic.Services;
using Kloon.EmployeePerformance.Logic.Services.Base;
using Kloon.EmployeePerformance.WebAPI;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kloon_EmployeePerformance_UnitTest
{
    public class Startup
    {
        private static Startup _instance;
        public ServiceProvider ServiceProvider { get; private set; }
        public IOptions<AccountSetting> Options { get; private set; }

        public static Startup Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new Startup();
                }
                return _instance;
            }
        }

        private Startup()
        {
            var configuration = InitConfiguration();
            IServiceCollection services = new ServiceCollection();

            services.AddControllers(options =>
                options.Filters.Add(new HttpResponseExceptionFilter()));

            services.Configure<ConnectionSettings>(options =>
            {
                configuration.GetSection(nameof(ConnectionSettings)).Bind(options);
                options.ConnectionStrings = new Dictionary<string, string>();
                var connValue = configuration.GetConnectionString("EmployeePerformanceContext");
                options.ConnectionStrings["EmployeePerformanceContext"] = connValue;
            });

            services.Configure<AccountSetting>(options =>
            {
                configuration.GetSection("AccountSettings").Bind(options);
            });

            services.AddCustomizedDbContextFactory<EmployeePerformanceContextFactory>();

            services.AddCustomizedService<FakeIdentityApiService>();
            services.AddScoped<IProjectService, ProjectService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IProjectUserService, ProjectUserService>();
            services.AddScoped<IPositionService, PositionService>();
            services.AddScoped<IAuthenticationService, AuthenticationService>();
            services.AddScoped<ITestService, TestService>();
            services.AddScoped<IMailService, MailService>();
            services.AddScoped<IQuarterEvaluationService, QuarterEvaluationService>();
            services.AddScoped<IUserQuarterEvaluationService, UserQuarterEvaluationService>();
            services.AddScoped<ICriteriaStoreService, CriteriaStoreService>();
            services.AddScoped<IEvaluationTemplateService, EvaluationTemplateService>();
            services.AddScoped<IActivityGroupService, ActivityGroupService>();
            services.AddScoped<IActivityService, ActivityService>();
            services.AddScoped<IActivityGroupUserService, ActivityGroupUserService>();
            services.AddScoped<IWorkSpaceSettingService, WorkSpaceSettingService>();
            services.AddScoped<ITimeSheetRecordService, TimeSheetRecordService>();
            services.AddScoped<ITimeSheetReportDetailService, TimeSheetReportDetailService>();



            services.AddSingleton<ILoggerFactory, LoggerFactory>();
            //services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddHttpContextAccessor();

            ServiceProvider = services.BuildServiceProvider();
            Options = ServiceProvider.GetService<IOptions<AccountSetting>>();

            EmployeePerformanceDataInitializer.Initialize(ServiceProvider);
        }

        private IConfiguration InitConfiguration()
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();
            return config;
        }
    }
}

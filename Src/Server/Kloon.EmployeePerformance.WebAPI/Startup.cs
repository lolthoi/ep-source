using Kloon.EmployeePerformance.DataAccess;
using Kloon.EmployeePerformance.DataAccess.Extentions;
using Kloon.EmployeePerformance.Logic.Common;
using Kloon.EmployeePerformance.Logic.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Apis.Auth.AspNetCore3;
using Microsoft.EntityFrameworkCore;

namespace Kloon.EmployeePerformance.WebAPI
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        public ServiceProvider ServiceProvider { get; private set; }


        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddControllers(options =>
                options.Filters.Add(new HttpResponseExceptionFilter()));
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "EmployeePerformance.WebAPI", Version = "v1" });

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Enter 'Bearer' [space] and then your valid token in the text input below.\r\n\r\nExample: \"Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9\"",
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                  {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    new string[] {}
                  }
                });
            });

            #region Authentication
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = context =>
                    {
                        return Task.CompletedTask;
                    },
                    OnAuthenticationFailed = context =>
                    {
                        return Task.CompletedTask;
                    }
                };
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = Configuration["JwtIssuer"],
                    ValidAudience = Configuration["JwtAudience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["JwtSecurityKey"])),
                    ClockSkew = TimeSpan.Zero
                };
            })
            .AddGoogleOpenIdConnect(options =>
            {
                options.ClientId = "440844154305-sviqj84c9r10foqfl8ngff6tr0dd73in.apps.googleusercontent.com";
                options.ClientSecret = "iFMjsP88XkUstOFbaDhUq1w_";
            });
            #endregion Authentication

            services.ReadConnectionSettingConfig(Configuration, "EmployeePerformanceContext");
            services.AddCustomizedDbContextFactory<EmployeePerformanceContextFactory>();

            services.AddCustomizedService<IdentityApiService>();

            services.AddScoped<IProjectService, ProjectService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IProjectUserService, ProjectUserService>();
            services.AddScoped<IPositionService, PositionService>();
            services.AddScoped<IAuthenticationService, AuthenticationService>();
            services.Configure<MailSetting>(Configuration.GetSection("MailSetting"));
            services.AddScoped<IMailService, MailService>();
            services.AddScoped<IQuarterEvaluationService, QuarterEvaluationService>();
            services.AddScoped<IUserQuarterEvaluationService, UserQuarterEvaluationService>();
            services.AddScoped<IEvaluationTemplateService, EvaluationTemplateService>();
            services.AddScoped<IWorkSpaceSettingService, WorkSpaceSettingService>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddHttpContextAccessor();

            ServiceProvider = services.BuildServiceProvider();

            EmployeePerformanceDataInitializer.Initialize(ServiceProvider);
            services.AddScoped<ICriteriaStoreService, CriteriaStoreService>();
            services.AddScoped<IActivityGroupService, ActivityGroupService>();
            services.AddScoped<IActivityService, ActivityService>();
            services.AddScoped<IActivityGroupUserService, ActivityGroupUserService>();
            services.AddScoped<ITimeSheetRecordService, TimeSheetRecordService>();
            services.AddScoped<ITimeSheetReportDetailService, TimeSheetReportDetailService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "EmployeePerformance.WebAPI v1"));
            }


            app.UseHttpsRedirection();

            app.UseCors(builder => builder.AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod());

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}

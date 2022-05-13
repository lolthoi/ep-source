using Kloon.EmployeePerformance.DataAccess.Domain;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kloon.EmployeePerformance.DataAccess
{
    public class EmployeePerformanceDataInitializer
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetService<IUnitOfWork<EmployeePerformanceContext>>();
                InitializeRethinkBaseData(context);

            }
        }

        private static void InitializeRethinkBaseData(IUnitOfWork<EmployeePerformanceContext> context)
        {
            var createDate = DateTime.UtcNow;

            var positions = context.GetRepository<Position>();
            if (!positions.Query().Any())
            {
                var data = new List<Position>
                {
                    new Position
                    {
                        Name = "CEO",
                        CreatedBy = 1,
                        CreatedDate = createDate
                    },
                    new Position
                    {
                        Name = "Management Board",
                        CreatedBy = 1,
                        CreatedDate = createDate
                    },
                    new Position
                    {
                        Name = "HR and Admin Team Leader",
                        CreatedBy = 1,
                        CreatedDate = createDate
                    },
                    new Position
                    {
                        Name = "Office Assistant",
                        CreatedBy = 1,
                        CreatedDate = createDate
                    },
                    new Position
                    {
                        Name = "Project Leader",
                        CreatedBy = 1,
                        CreatedDate = createDate
                    },
                    new Position
                    {
                        Name = "Team Leader",
                        CreatedBy = 1,
                        CreatedDate = createDate
                    },
                    new Position
                    {
                        Name = "Intern Tester",
                        CreatedBy = 1,
                        CreatedDate = createDate
                    },
                    new Position
                    {
                        Name = "Junior Tester",
                        CreatedBy = 1,
                        CreatedDate = createDate
                    },
                    new Position
                    {
                        Name = "Middle Tester",
                        CreatedBy = 1,
                        CreatedDate = createDate
                    },
                    new Position
                    {
                        Name = "Senior Tester",
                        CreatedBy = 1,
                        CreatedDate = createDate
                    },
                    new Position
                    {
                        Name = "Intern Developer",
                        CreatedBy = 1,
                        CreatedDate = createDate
                    },
                    new Position
                    {
                        Name = "Junior Developer",
                        CreatedBy = 1,
                        CreatedDate = createDate
                    },
                    new Position
                    {
                        Name = "Middle Developer",
                        CreatedBy = 1,
                        CreatedDate = createDate
                    },
                    new Position
                    {
                        Name = "Senior Developer",
                        CreatedBy = 1,
                        CreatedDate = createDate
                    },
                    new Position
                    {
                        Name = "IT",
                        CreatedBy = 1,
                        CreatedDate = createDate
                    },
                    new Position
                    {
                        Name = "Intern Designer",
                        CreatedBy = 1,
                        CreatedDate = createDate
                    },
                    new Position
                    {
                        Name = "Junior Designer",
                        CreatedBy = 1,
                        CreatedDate = createDate
                    },
                    new Position
                    {
                        Name = "Middle Designer",
                        CreatedBy = 1,
                        CreatedDate = createDate
                    },
                    new Position
                    {
                        Name = "Senior Designer",
                        CreatedBy = 1,
                        CreatedDate = createDate
                    },
                    new Position
                    {
                        Name = "Intern QA",
                        CreatedBy = 1,
                        CreatedDate = createDate
                    },
                    new Position
                    {
                        Name = "Junior QA",
                        CreatedBy = 1,
                        CreatedDate = createDate
                    },
                    new Position
                    {
                        Name = "Middle QA",
                        CreatedBy = 1,
                        CreatedDate = createDate
                    },
                    new Position
                    {
                        Name = "Senior QA",
                        CreatedBy = 1,
                        CreatedDate = createDate
                    },
                    new Position
                    {
                        Name = "Language Assistant",
                        CreatedBy = 1,
                        CreatedDate = createDate
                    },
                    new Position
                    {
                        Name = "Customer Support",
                        CreatedBy = 1,
                        CreatedDate = createDate
                    },
                };
                positions.InsertRange(data);
            }

            var users = context.GetRepository<User>();
            if (!users.Query().Any())
            {
                var data = new List<User>
                {
                    new User
                    {
                        Email = "admin@kloon.vn",
                        FirstName = "Admin",
                        LastName = "Admin",
                        DoB = new DateTime(1980,1,1),
                        PhoneNo = "123456789",
                        RoleId = 1,
                        PositionId = 1,
                        Sex = 1,
                        PasswordHash = "26EFBFBDEFBFBDEFBFBDEFBFBD5FEFBFBD76EFBFBDEFBFBD4774365754EFBFBDEFBFBDEFBFBD5EEFBFBD5B05EFBFBD620373384313EFBFBD22EFBFBD", //123456
                        PasswordSalt ="84b32f39-a6d5-4d5a-908c-538fea22b3d9",
                        CreatedDate = createDate,
                        CreatedBy = 1,
                        Status = true
                    },
                    new User
                    {
                        Email = "user@kloon.vn",
                        FirstName = "Admin",
                        LastName = "Admin",
                        DoB = new DateTime(1980,1,1),
                        PhoneNo = "123456789",
                        RoleId = 2,
                        PositionId = 1,
                        Sex = 1,
                        PasswordHash = "26EFBFBDEFBFBDEFBFBDEFBFBD5FEFBFBD76EFBFBDEFBFBD4774365754EFBFBDEFBFBDEFBFBD5EEFBFBD5B05EFBFBD620373384313EFBFBD22EFBFBD", //123456
                        PasswordSalt ="84b32f39-a6d5-4d5a-908c-538fea22b3d9",
                        CreatedDate = createDate,
                        CreatedBy = 1,
                        Status = true
                    }
                };
                users.InsertRange(data);
            }

            var appSettings = context.GetRepository<AppSetting>();
            if (!appSettings.Query().Any())
            {
                var settings = new List<AppSetting>
                    {
                        new AppSetting
                        {
                            Id = "DATABASEVERSION",
                            Value = "1.0.0.0",
                            Status = true,
                        },
                        new AppSetting
                        {
                            Id = "ISLOCKTIMESHEET",
                            Value = "false",
                            Status = true,
                        },new AppSetting
                        {
                            Id = "LOCKAFTER",
                            Value = "0",
                            Status = true,
                        },new AppSetting
                        {
                            Id = "LOCKVALUEBYDATE",
                            Value = "1",
                            Status = true,
                        }
                    };
                appSettings.InsertRange(settings);
            }
        }
    }
}

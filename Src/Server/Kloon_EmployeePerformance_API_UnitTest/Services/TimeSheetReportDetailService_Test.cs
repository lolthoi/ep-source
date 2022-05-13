using Kloon.EmployeePerformance.DataAccess;
using Kloon.EmployeePerformance.DataAccess.Domain;
using Kloon.EmployeePerformance.Logic.Caches;
using Kloon.EmployeePerformance.Logic.Services;
using Kloon.EmployeePerformance.Models.Common;
using Kloon.EmployeePerformance.Models.Project;
using Kloon.EmployeePerformance.Models.TimeSheet.TimeSheetRecord;
using Kloon.EmployeePerformance.Models.TimeSheet.TimeSheetReport;
using Kloon.EmployeePerformance.Models.User;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kloon_EmployeePerformance_UnitTest.Services
{
    [TestClass]
    public class TimeSheetReportDetailService_Test : TestBase
    {
        private CacheProvider _cache;
        private readonly Random _rand = new Random();
        private ITimeSheetReportDetailService _timeSheetReportDetailService;
        private IProjectService _projectService;
        private IProjectUserService _projectUserService;
        private IUserService _userService;

        private IUnitOfWork<EmployeePerformanceContext> _dbContext;
        private IEntityRepository<User> _users;
        private IEntityRepository<Project> _projects;
        private IEntityRepository<ProjectUser> _projectUsers;
        private IEntityRepository<TSActivityGroup> _tSActivityGroups;
        private IEntityRepository<TSActivity> _tSActivities;
        private IEntityRepository<TSRecord> _tsRecords;
        private IEntityRepository<ActivityGroupUser> _activityGroupUsers;

        private UserModel _userModel;
        private UserModel _userModel2;
        private UserModel _userModel3;
        private UserModel _userModel4;
        private ProjectModel _projectModel;
        private ProjectUserModel _projectUserModel;

        private TSRecord _tSRecord;
        private TSActivityGroup _tSActivityGroup;
        private TSActivity _tSActivity;
        private ActivityGroupUser _activityGroupUser;

        private TimeSheetReportDetailRouter _timeSheetReportDetailRouter;
        private FilterParamUserRecord _filterParamUserRecord;
        protected override void InitServices()
        {
            _cache = _scope.ServiceProvider.GetService<CacheProvider>();
            _timeSheetReportDetailService = _scope.ServiceProvider.GetService<ITimeSheetReportDetailService>();
            _userService = _scope.ServiceProvider.GetService<IUserService>();
            _projectService = _scope.ServiceProvider.GetService<IProjectService>();
            _projectUserService = _scope.ServiceProvider.GetService<IProjectUserService>();

            _dbContext = _scope.ServiceProvider.GetService<IUnitOfWork<EmployeePerformanceContext>>();
            _users = _dbContext.GetRepository<User>();
            _projects = _dbContext.GetRepository<Project>();
            _projectUsers = _dbContext.GetRepository<ProjectUser>();
            _tSActivityGroups = _dbContext.GetRepository<TSActivityGroup>();
            _tSActivities = _dbContext.GetRepository<TSActivity>();
            _tsRecords = _dbContext.GetRepository<TSRecord>();
            _activityGroupUsers = _dbContext.GetRepository<ActivityGroupUser>();

        }
        protected override void InitEnvirontment()
        {
            _testService.StartLoginWithAdmin()
                .ThenImplementTest(param =>
                {
                    _userModel = InitUserModel();
                    _userModel = _userService.Create(_userModel).Data;

                    _userModel2 = InitUserModel();
                    _userModel2 = _userService.Create(_userModel2).Data;

                    _userModel3 = InitUserModel();
                    _userModel3 = _userService.Create(_userModel3).Data;

                    _projectModel = InitProjectModel();
                    _projectModel = _projectService.Create(_projectModel).Data;

                    _projectUserModel = _projectUserService.Create(_projectModel.Id, _userModel2.Id).Data;

                    _tSActivityGroup = InitTSActivityGroup();
                    _tSActivityGroup.ProjectId = _projectModel.Id;
                    _tSActivityGroup.Name = _projectModel.Name;
                    _tSActivityGroup.Description = _projectModel.Description;
                    _tSActivityGroups.Add(_tSActivityGroup);

                    _tSActivity = InitTSActivity(_tSActivityGroup);
                    _tSActivities.Add(_tSActivity);
                    _dbContext.Save();

                    _tSRecord = InitTSRecord(_tSActivity.Id, _userModel2.Id);
                    _tsRecords.Add(_tSRecord);

                    _activityGroupUser = InitActivityGroupUser(_tSActivityGroup.Id, _userModel3.Id);
                    _activityGroupUsers.Add(_activityGroupUser);
                    _dbContext.Save();

                    _cache.Users.Clear();
                    _cache.Projects.Clear();
                    _cache.TSActivityGroups.Clear();
                    _cache.TSActivities.Clear();
                });
        }
        protected override void CleanEnvirontment()
        {
            _testService.StartLoginWithAdmin()
                .ThenImplementTest(param =>
                {
                    var projectUser = _projectUsers.Query(x => x.Id == _projectUserModel.Id).FirstOrDefault();
                    if (projectUser != null)
                    {
                        _projectUsers.Delete(projectUser);
                    }

                    var project = _projects.Query(x => x.Id == _projectModel.Id).FirstOrDefault();
                    if (project != null)
                    {
                        _projects.Delete(project);
                    }

                    var user = _users.Query(x => x.Id == _userModel.Id).FirstOrDefault();
                    if (user != null)
                    {
                        _users.Delete(user);
                    }
                    var user2 = _users.Query(x => x.Id == _userModel2.Id).FirstOrDefault();
                    if (user2 != null)
                    {
                        _users.Delete(user2);
                    }

                    var user3 = _users.Query(x => x.Id == _userModel3.Id).FirstOrDefault();
                    if (user3 != null)
                    {
                        _users.Delete(user3);
                    }

                    _tSActivityGroups.Delete(_tSActivityGroup);
                    _tSActivities.Delete(_tSActivity);
                    _tsRecords.Delete(_tSRecord);

                    _dbContext.Save();

                    _cache.Users.Clear();
                    _cache.Projects.Clear();
                    _cache.TSActivityGroups.Clear();
                    _cache.TSActivities.Clear();
                });
        }

        #region GetAll
        [TestMethod]
        public void AdminRoleGetAll_ValidData_Success()
        {
            _testService.StartLoginWithAdmin()
                .ThenImplementTest(param =>
                {

                    //Arrange
                    _timeSheetReportDetailRouter = InitTimeSheetReportDetailRouter();

                    //Act
                    var result = _timeSheetReportDetailService.GetAllRecordPaging(_timeSheetReportDetailRouter);

                    //Assert
                    Assert.IsNotNull(result.Data);
                    Assert.IsNull(result.Error);
                    Assert.AreEqual(1, result.Data.Count);
                    Assert.AreEqual(_timeSheetReportDetailRouter.Page, result.Data.Page);
                    Assert.AreEqual(1, result.Data.TotalPages);
                    TimeSheetReportDetailModel actual = result.Data.Items.ToList()[0];

                    Assert.AreEqual(_userModel2.FirstName, actual.FirstName);
                    Assert.AreEqual(_userModel2.LastName, actual.LastName);
                    Assert.AreEqual(_userModel2.Id, actual.UserId);

                    Assert.AreEqual(_tSActivityGroup.Id, actual.TSAcitivityGroupId);
                    Assert.AreEqual(_tSActivityGroup.Name, actual.TSAcitivityGroupName);
                    Assert.AreEqual(_tSActivity.Id, actual.TSActivityId);
                    Assert.AreEqual(_tSActivity.Name, actual.TSActivityName);
                    Assert.AreEqual(_tSRecord.Id, actual.TimeSheetRecordId);
                    Assert.AreEqual(_tSRecord.Name, actual.TimeSheetRecordName);
                    Assert.AreEqual(_tSRecord.StartTime, actual.StartDate);
                    Assert.AreEqual(_tSRecord.EndTime, actual.EndDate);

                })
                .ThenCleanDataTest(param =>
                {
                });
        }

        [TestMethod]
        public void UserRoleGetAll_ValidData_Success()
        {
            _testService.StartLoginWithUser(_userModel3.Email, "123456")
                .ThenImplementTest(param =>
                {

                    //Arrange
                    _timeSheetReportDetailRouter = InitTimeSheetReportDetailRouter();

                    //Act
                    var result = _timeSheetReportDetailService.GetAllRecordPaging(_timeSheetReportDetailRouter);

                    //Assert

                    Assert.IsNotNull(result.Data);
                    Assert.IsNull(result.Error);
                    Assert.AreEqual(1, result.Data.Count);
                    Assert.AreEqual(_timeSheetReportDetailRouter.Page, result.Data.Page);
                    Assert.AreEqual(1, result.Data.TotalPages);
                    TimeSheetReportDetailModel actual = result.Data.Items.ToList()[0];

                    Assert.AreEqual(_userModel2.FirstName, actual.FirstName);
                    Assert.AreEqual(_userModel2.LastName, actual.LastName);
                    Assert.AreEqual(_userModel2.Id, actual.UserId);

                    Assert.AreEqual(_tSActivityGroup.Id, actual.TSAcitivityGroupId);
                    Assert.AreEqual(_tSActivityGroup.Name, actual.TSAcitivityGroupName);
                    Assert.AreEqual(_tSActivity.Id, actual.TSActivityId);
                    Assert.AreEqual(_tSActivity.Name, actual.TSActivityName);
                    Assert.AreEqual(_tSRecord.Id, actual.TimeSheetRecordId);
                    Assert.AreEqual(_tSRecord.Name, actual.TimeSheetRecordName);
                    Assert.AreEqual(_tSRecord.StartTime, actual.StartDate);
                    Assert.AreEqual(_tSRecord.EndTime, actual.EndDate);

                })
                .ThenCleanDataTest(param =>
                {
                });
        }

        [TestMethod]
        public void UserRoleGetAll_NoPerMission_Fail()
        {
            _testService.StartLoginWithUser(_userModel.Email, "123456")
                .ThenImplementTest(param =>
                {
                    var model = new TimeSheetReportDetailRouter();
                    var actual = _timeSheetReportDetailService.GetAllRecordPaging(model);

                    Assert.IsNotNull(actual.Error);
                    Assert.IsNull(actual.Data);
                    Assert.AreEqual(ErrorType.NO_ROLE, actual.Error.Type);
                    Assert.AreEqual("User dont have permission with this activity", actual.Error.Message);
                })
                .ThenCleanDataTest(param =>
                {

                });
        }
        #endregion

        #region Init

        private UserModel InitUserModel()
        {
            return new UserModel
            {
                Email = "Email" + rand() + "@kloon.vn",
                FirstName = "Firstname " + rand(),
                LastName = "Lastname " + rand(),
                PositionId = new Random().Next(1, 3),
                Sex = (SexEnum)new Random().Next(1, 2),
                DoB = DateTime.Today.AddDays(-new Random().Next(20 * 635)),
                PhoneNo = "0" + rand(),
                RoleId = (int)Roles.USER,
                Status = true
            };
        }

        public ProjectModel InitProjectModel()
        {
            return new ProjectModel()
            {
                Name = "Name " + rand(),
                Status = (ProjectStatusEnum)randProjectRandom(),
                Description = "Create Description " + rand(),
                StartDate = DateTime.Now
            };
        }

        private TimeSheetReportDetailRouter InitTimeSheetReportDetailRouter()
        {
            return new TimeSheetReportDetailRouter()
            {
                Page = 1,
                PageSize = 10,
                StartDate = DateTime.Now.AddDays(-7),
                EndDate = DateTime.Now,
                TaskIds = new List<Guid>(),
                TSAcitivityGroupIds = new List<Guid>(),
                UserIds = new List<int>()
            };
        }

        public TimeSheetRecordModel InitTimeSheetRecordModel(int userId, Guid activityId)
        {
            return new TimeSheetRecordModel()
            {
                UserId = userId,
                Name = "Task Name" + rand(),
                TaskId = "Some Task for test" + rand(),
                BacklogId = "SomeBacklog" + rand(),
                TSActivityId = activityId,
                StartTime = DateTime.Now.AddMinutes(-10),
                EndTime = DateTime.Now,
            };
        }

        private TSActivityGroup InitTSActivityGroup()
        {
            return new TSActivityGroup()
            {
                CreatedBy = 1,
                CreatedDate = DateTime.Now
            };
        }

        private TSActivity InitTSActivity(TSActivityGroup tSActivityGroup)
        {
            return new TSActivity()
            {
                TSActivityGroup = tSActivityGroup,
                Id = Guid.NewGuid(),
                Description = "Description Activity" + rand(),
                Name = "Acitivity test Name" + rand(),
            };
        }

        private TSRecord InitTSRecord(Guid tSActivityId, int userId)
        {
            return new TSRecord()
            {
                Name = "random TSRecord" + rand(),
                BacklogId = "haha" + rand(),
                TaskId = "haha" + rand(),
                StartTime = DateTime.Now.AddMinutes(-10),
                EndTime = DateTime.Now,
                TSActivityId = tSActivityId,
                UserId = userId,
                CreatedBy = 1,
                CreatedDate = DateTime.Now
            };
        }

        private ActivityGroupUser InitActivityGroupUser(Guid tSAcitivityGroupId, int userId)
        {
            return new ActivityGroupUser()
            {
                Id = new Guid(),
                TSActivityGroupId = tSAcitivityGroupId,
                UserId = userId,
                Role = 1,
                CreatedBy = 1,
                CreatedDate = DateTime.Now
            };
        }

        #endregion

        #region Random func

        private int rand()
        {
            return _rand.Next(1, 1000000);
        }

        private int randProjectRandom()
        {
            return _rand.Next(1, 3);
        }

        #endregion
    }
}


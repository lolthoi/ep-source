using Kloon.EmployeePerformance.DataAccess;
using Kloon.EmployeePerformance.DataAccess.Domain;
using Kloon.EmployeePerformance.Logic.Caches;
using Kloon.EmployeePerformance.Logic.Services;
using Kloon.EmployeePerformance.Models.Common;
using Kloon.EmployeePerformance.Models.User;
using Kloon.EmployeePerformance.Models.WorkSpaceSetting;
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

    public class WorkSpaceSettingService_Test : TestBase
    {
        private readonly Random _rand = new Random();

        private IWorkSpaceSettingService _workSpaceSettingService;
        private IUserService _userService;
        private IUnitOfWork<EmployeePerformanceContext> _dbContext;
        private CacheProvider _cache;

        private IEntityRepository<AppSetting> _appSettings;
        private IEntityRepository<User> _users;

        private UserModel _userModel;
        private WorkSpaceSettingModel _workSpaceSettingModel;
        private TimeSpan _ticks;
        protected override void InitServices()
        {
            _workSpaceSettingService = _scope.ServiceProvider.GetService<IWorkSpaceSettingService>();
            _userService = _scope.ServiceProvider.GetService<IUserService>();
            _dbContext = _scope.ServiceProvider.GetService<IUnitOfWork<EmployeePerformanceContext>>();
            _cache = _scope.ServiceProvider.GetService<CacheProvider>();

            _appSettings = _dbContext.GetRepository<AppSetting>();
            _users = _dbContext.GetRepository<User>();
        }

        protected override void InitEnvirontment()
        {
            _testService.StartLoginWithAdmin()
               .ThenImplementTest(param =>
               {

                   _userModel = InitUserModel();
                   _userModel = _userService.Create(_userModel).Data;


                   AppSetting isLockTimeSheetDelete = _appSettings.Query(x => x.Id == WorkSpaceSettingCommon.ISLOCKTIMESHEET).FirstOrDefault();
                   if (isLockTimeSheetDelete != null)
                   {
                       _appSettings.Delete(isLockTimeSheetDelete);
                   }
                   AppSetting lockafterDelete = _appSettings.Query(x => x.Id == WorkSpaceSettingCommon.LOCKAFTER).FirstOrDefault();
                   if (lockafterDelete != null)
                   {
                       _appSettings.Delete(lockafterDelete);
                   }
                   AppSetting lockVulueByDateDelete = _appSettings.Query(x => x.Id == WorkSpaceSettingCommon.LOCKVALUEBYDATE).FirstOrDefault();

                   if (lockVulueByDateDelete != null)
                   {
                       _appSettings.Delete(lockVulueByDateDelete);
                   }
                   _dbContext.Save();
                   _cache.WorkSpaceSetting.Clear();

                   _workSpaceSettingModel = InitWorkSpaceSettingModel();

                   AppSetting isLockTimeSheet = new AppSetting()
                   {
                       Id = WorkSpaceSettingCommon.ISLOCKTIMESHEET,
                       Value = _workSpaceSettingModel.IsLockTimeSheet,
                       Status = true
                   };
                   _ticks = TimeSpan.Parse(_workSpaceSettingModel.LockAfter);
                   AppSetting lockAfter = new AppSetting()
                   {
                       Id = WorkSpaceSettingCommon.LOCKAFTER,
                       Value = _ticks.Ticks.ToString(),
                       Status = true
                   };
                   AppSetting lockValueByDate = new AppSetting()
                   {
                       Id = WorkSpaceSettingCommon.LOCKVALUEBYDATE,
                       Value = _workSpaceSettingModel.LockValueByDate,
                       Status = true
                   };

                   List<AppSetting> appSettings = new List<AppSetting>()
                    {
                        isLockTimeSheet,
                        lockAfter,
                        lockValueByDate
                    }; 

                   foreach (var item in appSettings)
                   {
                       _appSettings.Add(item);
                   }
                   _dbContext.Save();
                   _cache.WorkSpaceSetting.Clear();
               });
        }

        protected override void CleanEnvirontment()
        {
            _testService.StartLoginWithAdmin()
                .ThenImplementTest(param =>
                {

                    var user = _users.Query(x => x.Id == _userModel.Id).FirstOrDefault();
                    if (user != null)
                    {
                        _users.Delete(user);
                    }

                    var appSettings = _appSettings.Query().ToList();
                    if (appSettings.Count > 0)
                    {
                        foreach (var item in appSettings)
                        {
                            _appSettings.Delete(item);
                        }
                    }
                    _dbContext.Save();
                    _cache.WorkSpaceSetting.Clear();
                });
        }

        #region Get

        [TestMethod]
        public void AdminRoleGetAll_Valid_Success()
        {
            _testService.StartLoginWithAdmin()
                .ThenImplementTest(param =>
                {

                    var actual = _workSpaceSettingService.GetAllWorkSpaceSettings();
                    Assert.IsNull(actual.Error);
                    Assert.IsNotNull(actual.Data);

                    Assert.AreEqual(_workSpaceSettingModel.IsLockTimeSheet, actual.Data.IsLockTimeSheet);

                    Assert.AreEqual(_ticks.ToString(@"hh\:mm\:ss"), actual.Data.LockAfter);
                    Assert.AreEqual(_workSpaceSettingModel.LockValueByDate, actual.Data.LockValueByDate);

                })
                .ThenCleanDataTest(param =>
                {
                    
                });
        }


        [TestMethod]
        public void UserRoleGetAll_Valid_Success()
        {
            _testService.StartLoginWithUser(_userModel.Email, "123456")
                .ThenImplementTest(param =>
                {
                    var actual = _workSpaceSettingService.GetAllWorkSpaceSettings();
                    Assert.IsNull(actual.Error);
                    Assert.IsNotNull(actual.Data);

                    Assert.AreEqual(_workSpaceSettingModel.IsLockTimeSheet, actual.Data.IsLockTimeSheet);
                    Assert.AreEqual(_ticks.ToString(@"hh\:mm\:ss"), actual.Data.LockAfter);
                    Assert.AreEqual(_workSpaceSettingModel.LockValueByDate, actual.Data.LockValueByDate);

                })
                .ThenCleanDataTest(param =>
                {
                });
        }

        #endregion

        #region Edit

        [TestMethod]
        public void AdminRoleEdit_ValidModel_Success()
        {
            _testService.StartLoginWithAdmin()
                .ThenImplementTest(param =>
                {

                    var model2 = InitWorkSpaceSettingModel();
                    var tick2 = TimeSpan.Parse(model2.LockAfter);
                    WorkSpaceSettingModel model = new WorkSpaceSettingModel()
                    {
                        IsLockTimeSheet = model2.IsLockTimeSheet,
                        LockValueByDate = model2.LockValueByDate,
                        LockAfter = model2.LockAfter
                    };

                    var actual = _workSpaceSettingService.Edit(model);
                    Assert.IsNotNull(actual.Data);
                    Assert.IsNull(actual.Error);

                    Assert.AreEqual(model2.IsLockTimeSheet, actual.Data.IsLockTimeSheet);
                    Assert.AreEqual(($"{tick2.Hours}:{tick2.Minutes}").ToString(), actual.Data.LockAfter);
                    Assert.AreEqual(model2.LockValueByDate, actual.Data.LockValueByDate);

                })
                .ThenCleanDataTest(param =>
                {
                   
                });
        }

        [TestMethod]
        public void AdminRoleEdit_InvalidModel_Fail()
        {
            _testService.StartLoginWithAdmin()
                .ThenImplementTest(param =>
                {
                    
                    WorkSpaceSettingModel nullModel = null;

                    WorkSpaceSettingModel stringValueIsRequired = new WorkSpaceSettingModel()
                    {
                        IsLockTimeSheet = true.ToString(),
                        LockAfter = "",
                        LockValueByDate = ""
                    };
                    

                    Dictionary<string, WorkSpaceSettingModel> invalidModels = new Dictionary<string, WorkSpaceSettingModel>();
                    invalidModels.Add(nameof(nullModel), nullModel);
                    invalidModels.Add(nameof(stringValueIsRequired), stringValueIsRequired);

                    foreach (var invalidModel in invalidModels)
                    {
                        var actualModel = _workSpaceSettingService.Edit(invalidModel.Value);
                        Assert.IsNotNull(actualModel);
                        Assert.IsNull(actualModel.Data);

                        switch (invalidModel.Key)
                        {
                            case nameof(nullModel):
                                Assert.AreEqual(ErrorType.BAD_REQUEST, actualModel.Error.Type);
                                Assert.AreEqual("Please fill in the required fields", actualModel.Error.Message);
                                break;

                            case nameof(stringValueIsRequired):
                                Assert.AreEqual(ErrorType.BAD_REQUEST, actualModel.Error.Type);
                                Assert.AreEqual("All Values are not allowed to be null", actualModel.Error.Message);
                                break;
                           
                            default:
                                Assert.Fail();
                                break;
                        }
                    }
                })
                .ThenCleanDataTest(param =>
                {
                });
        }

        [TestMethod]
        public void UserRoleEdit_NoPermission_Fail()
        {
            _testService.StartLoginWithUser(_userModel.Email, "123456")
                .ThenImplementTest(param =>
                {
                    //Arrange

                    //Act
                    var actual = _workSpaceSettingService.Edit(new WorkSpaceSettingModel());

                    //Assert

                    Assert.IsNotNull(actual.Error);
                    Assert.IsNull(actual.Data);
                    Assert.AreEqual(ErrorType.NO_ROLE, actual.Error.Type);
                    Assert.AreEqual("No Role", actual.Error.Message);
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

        private WorkSpaceSettingModel InitWorkSpaceSettingModel()
        {
            return new WorkSpaceSettingModel()
            {
                IsLockTimeSheet = _rand.Next(1, 2) == 1 ? true.ToString() : false.ToString(),
                LockAfter = timeSpanRandom(),

                LockValueByDate = _rand.Next(1, 16).ToString(),
            };
        }


        #endregion

        #region Random func
        private int rand()
        {
            return _rand.Next(1, 1000000);
        }
        private string timeSpanRandom()
        {
            return $"{_rand.Next(1, 24)}:{_rand.Next(1, 60)}";
        }
        #endregion
    }
}

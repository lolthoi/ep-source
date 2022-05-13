using Kloon.EmployeePerformance.Logic.Caches;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kloon.EmployeePerformance.Models.TimeSheet.TimeSheetRecord;
using Kloon.EmployeePerformance.Logic.Services;
using Kloon.EmployeePerformance.Models.User;
using Kloon.EmployeePerformance.Models.Project;
using Kloon.EmployeePerformance.Models.Common;
using Kloon.EmployeePerformance.DataAccess;
using Kloon.EmployeePerformance.DataAccess.Domain;
using Kloon.EmployeePerformance.Models.WorkSpaceSetting;

namespace Kloon_EmployeePerformance_UnitTest.Services
{
    [TestClass]
    public class TimeSheetRecordService_Test : TestBase
    {
        #region VARIABLE AND INIT

        private CacheProvider _cache;
        private IUserService _userService;
        private IProjectService _projectService;
        private IProjectUserService _projectUserService;
        private ITimeSheetRecordService _tsRecordService;

        private IUnitOfWork<EmployeePerformanceContext> _dbContext;
        private IEntityRepository<TSActivityGroup> _tsActivityGroupRespository;
        private IEntityRepository<TSActivity> _tsActivityRespository;
        private IEntityRepository<TSRecord> _tsRecordRespository;
        private IEntityRepository<AppSetting> _appSettingRespository;


        private UserModel _randomUser;
        private UserModel _randomUser2;
        private ProjectModel _randomProject;
        private ProjectUserModel _randomProjectUser;
        private TSRecord _randomTSRecord;
        private TSActivityGroup _randomTSActivityGroup;
        private TSActivity _randomTSActivity;

        private bool _isLockTimeSheet;
        private TimeSpan _lockAfter;
        private int _lockValueByDate;
        private readonly Random _random = new Random();

        protected override void CleanEnvirontment()
        {
            _testService
                .StartLoginWithAdmin()
                .ThenImplementTest(param => {
                    _userService.Delete(_randomUser.Id);
                    _userService.Delete(_randomUser2.Id);
                    _projectService.Delete(_randomProject.Id);
                    _projectUserService.Delete(_randomProject.Id,_randomProjectUser.Id);
                    _tsActivityGroupRespository.Delete(_randomTSActivityGroup);
                    _tsActivityRespository.Delete(_randomTSActivity);
                    _tsRecordRespository.Delete(_randomTSRecord);
                    _dbContext.Save();
                });
        }

        protected override void InitEnvirontment()
        {
            _testService
                .StartLoginWithAdmin()
                .ThenImplementTest(param =>
                {
                    InitData();
                });
        }

        protected override void InitServices()
        {
            _cache = _scope.ServiceProvider.GetService<CacheProvider>();
            _userService = _scope.ServiceProvider.GetService<IUserService>();
            _tsRecordService = _scope.ServiceProvider.GetService<ITimeSheetRecordService>();
            _projectService = _scope.ServiceProvider.GetService<IProjectService>();
            _projectUserService = _scope.ServiceProvider.GetService<IProjectUserService>();
            _dbContext = _scope.ServiceProvider.GetService<IUnitOfWork<EmployeePerformanceContext>>();
            _tsActivityGroupRespository = _dbContext.GetRepository<TSActivityGroup>();
            _tsActivityRespository = _dbContext.GetRepository<TSActivity>();
            _tsRecordRespository = _dbContext.GetRepository<TSRecord>();
            _appSettingRespository = _dbContext.GetRepository<AppSetting>();
        }

        #endregion

        #region GET

        public void AdminRoleGetAll_ValidData_Success()
        {
            _testService.StartLoginWithAdmin()
                .ThenImplementTest(param =>
                {
                    //Arrange
                    int page = 1, pageSize = 10;

                    //Act
                    var result = _tsRecordService.GetAll("", page, pageSize);

                    //Assert

                    Assert.IsNotNull(result.Data);
                    Assert.IsNull(result.Error);
                    Assert.AreEqual(1, result.Data.Count);
                    Assert.AreEqual(page, result.Data.Page);
                    Assert.AreEqual(pageSize, result.Data.TotalPages);
                })
                .ThenCleanDataTest(param =>
                {
                });
        }

        public void UserRoleGetAll_ValidData_Success()
        {
            _testService.StartLoginWithUser(_randomUser.Email, "123456")
                .ThenImplementTest(param =>
                {
                    //Arrange
                    int page = 1, pageSize = 10;

                    //Act
                    var result = _tsRecordService.GetAll("", page, pageSize);

                    //Assert

                    Assert.IsNotNull(result.Data);
                    Assert.IsNull(result.Error);
                    Assert.AreEqual(1, result.Data.Count);
                    Assert.AreEqual(page, result.Data.Page);
                    Assert.AreEqual(pageSize, result.Data.TotalPages);
                })
                .ThenCleanDataTest(param =>
                {
                });
        }

        #endregion GET


        #region CREATE

        [TestMethod]
        public void AdminRoleCreate_ValidData_Success()
        {
            _testService.StartLoginWithAdmin()
                .ThenImplementTest(param =>
                {
                    //Arrange
                    var model = PrepareValidRecordModel(_randomUser.Id, _randomTSActivity.Id);

                    //Act
                    var actual = _tsRecordService.Create(model);
                    param.CleanData.Add("CreateModel", actual.Data);

                    //Assert
                    Assert.IsNotNull(actual.Data);

                    var expectedModel = _tsRecordRespository.Query().Where(t => t.Id == actual.Data.Id).FirstOrDefault();

                    Assert.IsNotNull(expectedModel);
                    Assert.AreEqual(expectedModel.Id, actual.Data.Id);
                    Assert.AreEqual(expectedModel.Name, actual.Data.Name);
                    Assert.AreEqual(expectedModel.BacklogId, actual.Data.BacklogId);
                    Assert.AreEqual(expectedModel.TaskId, actual.Data.TaskId);
                    Assert.AreEqual(expectedModel.StartTime, actual.Data.StartTime);
                    Assert.AreEqual(expectedModel.EndTime, actual.Data.EndTime);
                    Assert.AreEqual(expectedModel.UserId, actual.Data.UserId);

                })
                .ThenCleanDataTest(param =>
                {
                    var createModel = param.CleanData.Get<TimeSheetRecordModel>("CreateModel");
                    if (createModel != null)
                    {
                        _tsRecordService.Delete(createModel.Id);
                    }
                });
        }

        [TestMethod]
        public void UserRoleCreate_ValidData_Success()
        {
            _testService.StartLoginWithUser(_randomUser.Email, "123456")
               .ThenImplementTest(param =>
               {
                   //Arrange
                   var model = PrepareValidRecordModel(_randomUser.Id, _randomTSActivity.Id);

                   //Act
                   var actual = _tsRecordService.Create(model);
                   param.CleanData.Add("CreateModel", actual.Data);

                   //Assert
                   Assert.IsNotNull(actual.Data);

                   var expectedModel = _tsRecordRespository.Query().Where(t => t.Id == actual.Data.Id).FirstOrDefault();

                   Assert.IsNotNull(expectedModel);
                   Assert.AreEqual(expectedModel.Id, actual.Data.Id);
                   Assert.AreEqual(expectedModel.Name, actual.Data.Name);
                   Assert.AreEqual(expectedModel.BacklogId, actual.Data.BacklogId);
                   Assert.AreEqual(expectedModel.TaskId, actual.Data.TaskId);
                   Assert.AreEqual(expectedModel.StartTime, actual.Data.StartTime);
                   Assert.AreEqual(expectedModel.EndTime, actual.Data.EndTime);
                   Assert.AreEqual(expectedModel.UserId, actual.Data.UserId);

               })
               .ThenCleanDataTest(param =>
               {
                   var createModel = param.CleanData.Get<TimeSheetRecordModel>("CreateModel");
                   if (createModel != null)
                   {
                       _tsRecordService.Delete(createModel.Id);
                   }
               });
        }

        [TestMethod]
        public void AdminRoleCreate_InvalidData_Fail()
        {
            _testService.StartLoginWithAdmin()
                .ThenImplementTest(param =>
                {
                    //Arrange 

                    TimeSheetRecordModel nullModel = null;

                    TimeSheetRecordModel invalidNullName = PrepareValidRecordModel(_randomUser.Id, _randomTSActivity.Id);
                    invalidNullName.Name = "";

                    TimeSheetRecordModel invalidNameMaxLength = PrepareValidRecordModel(_randomUser.Id, _randomTSActivity.Id);
                    invalidNameMaxLength.Name = "max50charmax50charmax50charmax50charmax50charmax50charmax50char";

                    TimeSheetRecordModel invalidTSActivityIdModel = PrepareValidRecordModel(_randomUser.Id, _randomTSActivity.Id);
                    invalidTSActivityIdModel.TSActivityId = Guid.NewGuid();

                    TimeSheetRecordModel userNotInProjectModel = PrepareValidRecordModel(_randomUser2.Id, _randomTSActivity.Id);

                    TimeSheetRecordModel endTimeSmallerThanStartTimeModel = PrepareValidRecordModel(_randomUser.Id, _randomTSActivity.Id);
                    endTimeSmallerThanStartTimeModel.EndTime = DateTime.Now.AddMinutes(-10);
                    endTimeSmallerThanStartTimeModel.StartTime = DateTime.Now;

                    Dictionary<string, TimeSheetRecordModel> invalidModels = new Dictionary<string, TimeSheetRecordModel>();
                    //invalidModels.Add(nameof(nullModel), nullModel);
                    invalidModels.Add(nameof(invalidNullName), invalidNullName);
                    invalidModels.Add(nameof(invalidTSActivityIdModel), invalidTSActivityIdModel);
                    invalidModels.Add(nameof(userNotInProjectModel), userNotInProjectModel);
                    invalidModels.Add(nameof(endTimeSmallerThanStartTimeModel), endTimeSmallerThanStartTimeModel);
                    invalidModels.Add(nameof(invalidNameMaxLength), invalidNameMaxLength);

                    //Act and Assert
                    foreach (var invalidModel in invalidModels)
                    {
                        var actualModel = _tsRecordService.Create(invalidModel.Value);

                        Assert.IsNotNull(actualModel);
                        Assert.IsNull(actualModel.Data);

                        switch (invalidModel.Key)
                        {
                            case nameof(nullModel):
                                Assert.AreEqual(ErrorType.BAD_REQUEST, actualModel.Error.Type);
                                Assert.AreEqual("Please fill in the required fields", actualModel.Error.Message);
                                break;
                            case nameof(invalidNullName):
                                Assert.AreEqual(ErrorType.BAD_REQUEST, actualModel.Error.Type);
                                Assert.AreEqual("INVALID_MODEL_NAME_NULL", actualModel.Error.Message);
                                break;
                            case nameof(invalidNameMaxLength):
                                Assert.AreEqual(ErrorType.BAD_REQUEST, actualModel.Error.Type);
                                Assert.AreEqual("INVALID_MODEL_TASK_NAME_MAX_LENGTH", actualModel.Error.Message);
                                break;
                            case nameof(invalidTSActivityIdModel):
                                Assert.AreEqual(ErrorType.NO_ROLE, actualModel.Error.Type);
                                Assert.AreEqual("User dont have permission with this activity", actualModel.Error.Message);
                                break;
                            case nameof(userNotInProjectModel):
                                Assert.AreEqual(ErrorType.NO_ROLE, actualModel.Error.Type);
                                Assert.AreEqual("User dont have permission with this activity", actualModel.Error.Message);
                                break;
                            case nameof(endTimeSmallerThanStartTimeModel):
                                Assert.AreEqual(ErrorType.BAD_REQUEST, actualModel.Error.Type);
                                Assert.AreEqual("Task StartTime is greater than EndTime. Please check again", actualModel.Error.Message);
                                break;
                        }
                    }
                })
                .ThenCleanDataTest(param =>
                {
                    var createModel = param.CleanData.Get<UserModel>("CreateModelRandomUser2");
                    if (createModel != null)
                    {
                        _userService.Delete(createModel.Id);
                    }
                });
        }

        [TestMethod]
        public void UserRoleCreate_InvalidData_Fail()
        {
            _testService.StartLoginWithUser(_randomUser.Email, "123456")
                .ThenImplementTest(param =>
                {
                    //Arrange 

                    TimeSheetRecordModel nullModel = null;

                    TimeSheetRecordModel invalidNullName = PrepareValidRecordModel(_randomUser.Id, _randomTSActivity.Id);
                    invalidNullName.Name = "";

                    TimeSheetRecordModel invalidNameMaxLength = PrepareValidRecordModel(_randomUser.Id, _randomTSActivity.Id);
                    invalidNameMaxLength.Name = "max50charmax50charmax50charmax50charmax50charmax50charmax50char";

                    TimeSheetRecordModel invalidTSActivityIdModel = PrepareValidRecordModel(_randomUser.Id, _randomTSActivity.Id);
                    invalidTSActivityIdModel.TSActivityId = Guid.NewGuid();

                    TimeSheetRecordModel userNotInProjectModel = PrepareValidRecordModel(_randomUser2.Id, _randomTSActivity.Id);

                    TimeSheetRecordModel endTimeSmallerThanStartTimeModel = PrepareValidRecordModel(_randomUser.Id, _randomTSActivity.Id);
                    endTimeSmallerThanStartTimeModel.EndTime = DateTime.Now.AddMinutes(-10);
                    endTimeSmallerThanStartTimeModel.StartTime = DateTime.Now;

                    TimeSheetRecordModel invalidUserIdModel = PrepareValidRecordModel(999999, _randomTSActivity.Id);

                    Dictionary<string, TimeSheetRecordModel> invalidModels = new Dictionary<string, TimeSheetRecordModel>();
                    //invalidModels.Add(nameof(nullModel), nullModel);
                    invalidModels.Add(nameof(invalidNullName), invalidNullName);
                    invalidModels.Add(nameof(invalidNameMaxLength), invalidNameMaxLength);
                    invalidModels.Add(nameof(invalidTSActivityIdModel), invalidTSActivityIdModel);
                    invalidModels.Add(nameof(userNotInProjectModel), userNotInProjectModel);
                    invalidModels.Add(nameof(endTimeSmallerThanStartTimeModel), endTimeSmallerThanStartTimeModel);
                    invalidModels.Add(nameof(invalidUserIdModel), invalidUserIdModel);

                    //Act and Assert
                    foreach (var invalidModel in invalidModels)
                    {
                        var actualModel = _tsRecordService.Create(invalidModel.Value);

                        Assert.IsNotNull(actualModel);
                        Assert.IsNull(actualModel.Data);

                        switch (invalidModel.Key)
                        {
                            case nameof(nullModel):
                                Assert.AreEqual(ErrorType.BAD_REQUEST, actualModel.Error.Type);
                                Assert.AreEqual("Please fill in the required fields", actualModel.Error.Message);
                                break;
                            case nameof(invalidNullName):
                                Assert.AreEqual(ErrorType.BAD_REQUEST, actualModel.Error.Type);
                                Assert.AreEqual("INVALID_MODEL_NAME_NULL", actualModel.Error.Message);
                                break;
                            case nameof(invalidNameMaxLength):
                                Assert.AreEqual(ErrorType.BAD_REQUEST, actualModel.Error.Type);
                                Assert.AreEqual("INVALID_MODEL_TASK_NAME_MAX_LENGTH", actualModel.Error.Message);
                                break;
                            case nameof(invalidTSActivityIdModel):
                                Assert.AreEqual(ErrorType.NO_ROLE, actualModel.Error.Type);
                                Assert.AreEqual("User dont have permission with this activity", actualModel.Error.Message);
                                break;
                            case nameof(userNotInProjectModel):
                                Assert.AreEqual(ErrorType.NO_ROLE, actualModel.Error.Type);
                                Assert.AreEqual("No Role", actualModel.Error.Message);
                                break;
                            case nameof(endTimeSmallerThanStartTimeModel):
                                Assert.AreEqual(ErrorType.BAD_REQUEST, actualModel.Error.Type);
                                Assert.AreEqual("Task StartTime is greater than EndTime. Please check again", actualModel.Error.Message);
                                break;
                            case nameof(invalidUserIdModel):
                                Assert.AreEqual(ErrorType.NO_ROLE, actualModel.Error.Type);
                                Assert.AreEqual("No Role", actualModel.Error.Message);
                                break;
                        }
                    }
                })
                .ThenCleanDataTest(param =>
                {
                    var createModel = param.CleanData.Get<UserModel>("CreateModelRandomUser2");
                    if (createModel != null)
                    {
                        _userService.Delete(createModel.Id);
                    }
                });
        }

        [TestMethod]
        public void UserRoleCreate_StartDateSmallerThanLockDate_Fail()
        {
            _testService.StartLoginWithUser(_randomUser.Email, "123456")
                .ThenImplementTest(param =>
                {
                    //Arrange

                    var model = PrepareValidRecordModel(_randomUser.Id, _randomTSActivity.Id);
                    DateTime lockDate = DateTime.UtcNow.AddDays(-_lockValueByDate - 1);
                    DateTime outDateStartDateTime = new DateTime(lockDate.Year, lockDate.Month, lockDate.Day, (int)_lockAfter.TotalHours, (int)_lockAfter.TotalMinutes, 0);
                    model.StartTime = outDateStartDateTime;
                    model.EndTime = outDateStartDateTime.AddHours(2);

                    //Act
                    var actual = _tsRecordService.Create(model);
                    param.CleanData.Add("CreateModel", actual.Data);

                    //Assert
                    Assert.IsNotNull(actual.Error);
                    Assert.AreEqual(ErrorType.BAD_REQUEST, actual.Error.Type);
                    Assert.AreEqual("TIMESHEET_LOCKED", actual.Error.Message);
                })
                .ThenCleanDataTest(param =>
                {
                    var createModel = param.CleanData.Get<TimeSheetRecordModel>("CreateModel");
                    if (createModel != null)
                    {
                        _tsRecordService.Delete(createModel.Id);
                    }
                });
        }

        #endregion CREATE

        #region EDIT

        [TestMethod]
        public void AdminRoleEdit_ValidData_Success()
        {
            _testService.StartLoginWithAdmin()
                .ThenImplementTest(param =>
                {
                    //Arrange
                    var model = PrepareValidRecordModel(_randomUser.Id, _randomTSActivity.Id);
                    var actualCreate = _tsRecordService.Create(model);
                    param.CleanData.Add("CreateModel", actualCreate.Data);
                    Assert.IsNotNull(actualCreate.Data);

                    //Act
                    var updateModel = PrepareValidRecordModel(_randomUser.Id, _randomTSActivity.Id);
                    updateModel.Id = actualCreate.Data.Id;
                    var actual = _tsRecordService.Update(updateModel);

                    //Assert
                    Assert.IsNotNull(actual.Data);

                    var expectedModel = _tsRecordRespository.Query().Where(t => t.Id == actual.Data.Id).FirstOrDefault();

                    Assert.IsNotNull(expectedModel);
                    Assert.AreEqual(expectedModel.Id, actual.Data.Id);
                    Assert.AreEqual(expectedModel.Name, actual.Data.Name);
                    Assert.AreEqual(expectedModel.BacklogId, actual.Data.BacklogId);
                    Assert.AreEqual(expectedModel.TaskId, actual.Data.TaskId);
                    Assert.AreEqual(expectedModel.StartTime, actual.Data.StartTime);
                    Assert.AreEqual(expectedModel.EndTime, actual.Data.EndTime);
                    Assert.AreEqual(expectedModel.UserId, actual.Data.UserId);

                })
                .ThenCleanDataTest(param =>
                {
                    var createModel = param.CleanData.Get<TimeSheetRecordModel>("CreateModel");
                    if (createModel != null)
                    {
                        _tsRecordService.Delete(createModel.Id);
                    }
                });
        }

        [TestMethod]
        public void UserRoleEdit_ValidData_Success()
        {
            _testService.StartLoginWithUser(_randomUser.Email, "123456")
                .ThenImplementTest(param =>
                {
                    //Arrange
                    var model = PrepareValidRecordModel(_randomUser.Id, _randomTSActivity.Id);
                    var actualCreate = _tsRecordService.Create(model);
                    param.CleanData.Add("CreateModel", actualCreate.Data);
                    Assert.IsNotNull(actualCreate.Data);

                    //Act
                    var updateModel = PrepareValidRecordModel(_randomUser.Id, _randomTSActivity.Id);
                    updateModel.Id = actualCreate.Data.Id;
                    var actual = _tsRecordService.Update(updateModel);

                    //Assert
                    Assert.IsNotNull(actual.Data);

                    var expectedModel = _tsRecordRespository.Query().Where(t => t.Id == actual.Data.Id).FirstOrDefault();

                    Assert.IsNotNull(expectedModel);
                    Assert.AreEqual(expectedModel.Id, actual.Data.Id);
                    Assert.AreEqual(expectedModel.Name, actual.Data.Name);
                    Assert.AreEqual(expectedModel.BacklogId, actual.Data.BacklogId);
                    Assert.AreEqual(expectedModel.TaskId, actual.Data.TaskId);
                    Assert.AreEqual(expectedModel.StartTime, actual.Data.StartTime);
                    Assert.AreEqual(expectedModel.EndTime, actual.Data.EndTime);
                    Assert.AreEqual(expectedModel.UserId, actual.Data.UserId);

                })
                .ThenCleanDataTest(param =>
                {
                    var createModel = param.CleanData.Get<TimeSheetRecordModel>("CreateModel");
                    if (createModel != null)
                    {
                        _tsRecordService.Delete(createModel.Id);
                    }
                });
        }

        [TestMethod]
        public void AdminRoleEdit_InvalidDate_Fail()
        {
            _testService.StartLoginWithAdmin()
                .ThenImplementTest(param =>
                {
                    //Arrange 

                    TimeSheetRecordModel nullModel = null;

                    TimeSheetRecordModel invalidNullName = PrepareValidRecordModel(_randomUser.Id, _randomTSActivity.Id);
                    invalidNullName.Name = "";
                    invalidNullName.Id = _randomTSRecord.Id;

                    TimeSheetRecordModel invalidRecordId = PrepareValidRecordModel(_randomUser.Id, _randomTSActivity.Id);
                    invalidRecordId.Id = Guid.NewGuid();

                    TimeSheetRecordModel invalidNameMaxLength = PrepareValidRecordModel(_randomUser.Id, _randomTSActivity.Id);
                    invalidNameMaxLength.Name = "max50charmax50charmax50charmax50charmax50charmax50charmax50char";

                    TimeSheetRecordModel invalidTSActivityIdModel = PrepareValidRecordModel(_randomUser.Id, _randomTSActivity.Id);
                    invalidTSActivityIdModel.TSActivityId = Guid.NewGuid();
                    invalidTSActivityIdModel.Id = _randomTSRecord.Id;

                    TimeSheetRecordModel userNotInProjectModel = PrepareValidRecordModel(_randomUser2.Id, _randomTSActivity.Id);
                    userNotInProjectModel.Id = _randomTSRecord.Id;

                    TimeSheetRecordModel endTimeSmallerThanStartTimeModel = PrepareValidRecordModel(_randomUser.Id, _randomTSActivity.Id);
                    endTimeSmallerThanStartTimeModel.EndTime = DateTime.Now.AddMinutes(-10);
                    endTimeSmallerThanStartTimeModel.StartTime = DateTime.Now;
                    endTimeSmallerThanStartTimeModel.Id = _randomTSRecord.Id;

                    TimeSheetRecordModel invalidUserIdModel = PrepareValidRecordModel(999999, _randomTSActivity.Id);

                    Dictionary<string, TimeSheetRecordModel> invalidModels = new Dictionary<string, TimeSheetRecordModel>();
                    //invalidModels.Add(nameof(nullModel), nullModel);
                    invalidModels.Add(nameof(invalidNullName), invalidNullName);
                    invalidModels.Add(nameof(invalidNameMaxLength), invalidNameMaxLength);
                    invalidModels.Add(nameof(invalidTSActivityIdModel), invalidTSActivityIdModel);
                    invalidModels.Add(nameof(userNotInProjectModel), userNotInProjectModel);
                    invalidModels.Add(nameof(endTimeSmallerThanStartTimeModel), endTimeSmallerThanStartTimeModel);
                    invalidModels.Add(nameof(invalidUserIdModel), invalidUserIdModel);
                    invalidModels.Add(nameof(invalidRecordId), invalidRecordId);

                    //Act and Assert
                    foreach (var invalidModel in invalidModels)
                    {
                        var actualModel = _tsRecordService.Update(invalidModel.Value);

                        Assert.IsNotNull(actualModel);
                        Assert.IsNull(actualModel.Data);

                        switch (invalidModel.Key)
                        {
                            case nameof(nullModel):
                                Assert.AreEqual(ErrorType.BAD_REQUEST, actualModel.Error.Type);
                                Assert.AreEqual("Please fill in the required fields", actualModel.Error.Message);
                                break;
                            case nameof(invalidNullName):
                                Assert.AreEqual(ErrorType.BAD_REQUEST, actualModel.Error.Type);
                                Assert.AreEqual("INVALID_MODEL_NAME_NULL", actualModel.Error.Message);
                                break;
                            case nameof(invalidNameMaxLength):
                                Assert.AreEqual(ErrorType.BAD_REQUEST, actualModel.Error.Type);
                                Assert.AreEqual("INVALID_MODEL_TASK_NAME_MAX_LENGTH", actualModel.Error.Message);
                                break;
                            case nameof(invalidTSActivityIdModel):
                                Assert.AreEqual(ErrorType.NO_ROLE, actualModel.Error.Type);
                                Assert.AreEqual("User dont have permission with this activity", actualModel.Error.Message);
                                break;
                            case nameof(userNotInProjectModel):
                                Assert.AreEqual(ErrorType.NO_ROLE, actualModel.Error.Type);
                                Assert.AreEqual("User dont have permission with this activity", actualModel.Error.Message);
                                break;
                            case nameof(endTimeSmallerThanStartTimeModel):
                                Assert.AreEqual(ErrorType.BAD_REQUEST, actualModel.Error.Type);
                                Assert.AreEqual("Task StartTime is greater than EndTime. Please check again", actualModel.Error.Message);
                                break;
                            case nameof(invalidUserIdModel):
                                Assert.AreEqual(ErrorType.NO_ROLE, actualModel.Error.Type);
                                Assert.AreEqual("User dont have permission with this activity", actualModel.Error.Message);
                                break;
                            case nameof(invalidRecordId):
                                Assert.AreEqual(ErrorType.NOT_EXIST, actualModel.Error.Type);
                                Assert.AreEqual("INVALID_RECORD_NOT_FOUND.", actualModel.Error.Message);
                                break;
                        }
                    }
                })
                .ThenCleanDataTest(param =>
                {
                    var createModel = param.CleanData.Get<UserModel>("CreateModelRandomUser2");
                    if (createModel != null)
                    {
                        _userService.Delete(createModel.Id);
                    }
                });
        }

        [TestMethod]
        public void UserRoleEdit_InvalidDate_Fail()
        {
            _testService.StartLoginWithUser(_randomUser.Email, "123456")
                .ThenImplementTest(param =>
                {
                    //Arrange 

                    TimeSheetRecordModel nullModel = null;

                    TimeSheetRecordModel invalidNullName = PrepareValidRecordModel(_randomUser.Id, _randomTSActivity.Id);
                    invalidNullName.Name = "";
                    invalidNullName.Id = _randomTSRecord.Id;

                    TimeSheetRecordModel invalidRecordId = PrepareValidRecordModel(_randomUser.Id, _randomTSActivity.Id);
                    invalidRecordId.Id = Guid.NewGuid();

                    TimeSheetRecordModel invalidNameMaxLength = PrepareValidRecordModel(_randomUser.Id, _randomTSActivity.Id);
                    invalidNameMaxLength.Name = "max50charmax50charmax50charmax50charmax50charmax50charmax50char";

                    TimeSheetRecordModel invalidTSActivityIdModel = PrepareValidRecordModel(_randomUser.Id, _randomTSActivity.Id);
                    invalidTSActivityIdModel.TSActivityId = Guid.NewGuid();
                    invalidTSActivityIdModel.Id = _randomTSRecord.Id;

                    TimeSheetRecordModel userNotInProjectModel = PrepareValidRecordModel(_randomUser2.Id, _randomTSActivity.Id);
                    userNotInProjectModel.Id = _randomTSRecord.Id;

                    TimeSheetRecordModel endTimeSmallerThanStartTimeModel = PrepareValidRecordModel(_randomUser.Id, _randomTSActivity.Id);
                    endTimeSmallerThanStartTimeModel.EndTime = DateTime.Now.AddMinutes(-10);
                    endTimeSmallerThanStartTimeModel.StartTime = DateTime.Now;
                    endTimeSmallerThanStartTimeModel.Id = _randomTSRecord.Id;

                    TimeSheetRecordModel invalidUserIdModel = PrepareValidRecordModel(999999, _randomTSActivity.Id);

                    Dictionary<string, TimeSheetRecordModel> invalidModels = new Dictionary<string, TimeSheetRecordModel>();
                    //invalidModels.Add(nameof(nullModel), nullModel);
                    invalidModels.Add(nameof(invalidNullName), invalidNullName);
                    invalidModels.Add(nameof(invalidNameMaxLength), invalidNameMaxLength);
                    invalidModels.Add(nameof(invalidTSActivityIdModel), invalidTSActivityIdModel);
                    invalidModels.Add(nameof(userNotInProjectModel), userNotInProjectModel);
                    invalidModels.Add(nameof(endTimeSmallerThanStartTimeModel), endTimeSmallerThanStartTimeModel);
                    invalidModels.Add(nameof(invalidUserIdModel), invalidUserIdModel);
                    invalidModels.Add(nameof(invalidRecordId), invalidRecordId);

                    //Act and Assert
                    foreach (var invalidModel in invalidModels)
                    {
                        var actualModel = _tsRecordService.Update(invalidModel.Value);

                        Assert.IsNotNull(actualModel);
                        Assert.IsNull(actualModel.Data);

                        switch (invalidModel.Key)
                        {
                            case nameof(nullModel):
                                Assert.AreEqual(ErrorType.BAD_REQUEST, actualModel.Error.Type);
                                Assert.AreEqual("Please fill in the required fields", actualModel.Error.Message);
                                break;
                            case nameof(invalidNullName):
                                Assert.AreEqual(ErrorType.BAD_REQUEST, actualModel.Error.Type);
                                Assert.AreEqual("INVALID_MODEL_NAME_NULL", actualModel.Error.Message);
                                break;
                            case nameof(invalidNameMaxLength):
                                Assert.AreEqual(ErrorType.BAD_REQUEST, actualModel.Error.Type);
                                Assert.AreEqual("INVALID_MODEL_TASK_NAME_MAX_LENGTH", actualModel.Error.Message);
                                break;
                            case nameof(invalidTSActivityIdModel):
                                Assert.AreEqual(ErrorType.NO_ROLE, actualModel.Error.Type);
                                Assert.AreEqual("User dont have permission with this activity", actualModel.Error.Message);
                                break;
                            case nameof(userNotInProjectModel):
                                Assert.AreEqual(ErrorType.NO_ROLE, actualModel.Error.Type);
                                Assert.AreEqual("No Role", actualModel.Error.Message);
                                break;
                            case nameof(endTimeSmallerThanStartTimeModel):
                                Assert.AreEqual(ErrorType.BAD_REQUEST, actualModel.Error.Type);
                                Assert.AreEqual("Task StartTime is greater than EndTime. Please check again", actualModel.Error.Message);
                                break;
                            case nameof(invalidUserIdModel):
                                Assert.AreEqual(ErrorType.NO_ROLE, actualModel.Error.Type);
                                Assert.AreEqual("No Role", actualModel.Error.Message);
                                break;
                            case nameof(invalidRecordId):
                                Assert.AreEqual(ErrorType.NOT_EXIST, actualModel.Error.Type);
                                Assert.AreEqual("INVALID_RECORD_NOT_FOUND.", actualModel.Error.Message);
                                break;
                        }
                    }
                })
                .ThenCleanDataTest(param =>
                {
                    var createModel = param.CleanData.Get<UserModel>("CreateModelRandomUser2");
                    if (createModel != null)
                    {
                        _userService.Delete(createModel.Id);
                    }
                });
        }

        [TestMethod]
        public void UserRoleEdit_OutDateTimeSheet_Fail()
        {
            _testService.StartLoginWithUser(_randomUser.Email, "123456")
                .ThenImplementTest(param =>
                {
                    //Arrange
                    DateTime lockDate = DateTime.UtcNow.AddDays(-_lockValueByDate - 1);
                    DateTime outDateStartDateTime = new DateTime(lockDate.Year, lockDate.Month, lockDate.Day, (int)_lockAfter.TotalHours, (int)_lockAfter.TotalMinutes, 0);

                    var outdatedTimeSheet  = new TSRecord()
                    {
                        Id = Guid.NewGuid(),
                        Name = "random TSRecord",
                        BacklogId = "haha",
                        TaskId = "haha",
                        StartTime = outDateStartDateTime.AddDays(-2),
                        EndTime = outDateStartDateTime.AddDays(-2).AddHours(2),
                        TSActivityId = _randomTSActivity.Id,
                        UserId = _randomUser.Id,
                        CreatedBy = 1,
                        CreatedDate = DateTime.Now
                    };
                    _tsRecordRespository.Add(outdatedTimeSheet);
                    _dbContext.Save();

                    var model = PrepareValidRecordModel(_randomUser.Id, _randomTSActivity.Id);
                    model.Id = outdatedTimeSheet.Id;
                    model.StartTime = DateTime.Now.AddHours(-2);
                    model.EndTime = DateTime.Now;

                    //Act

                    var actual = _tsRecordService.Update(model);
                    param.CleanData.Add("CreateModel", actual.Data);

                    //Assert
                    Assert.IsNotNull(actual.Error);
                    Assert.AreEqual(ErrorType.BAD_REQUEST, actual.Error.Type);
                    Assert.AreEqual("TIMESHEET_LOCKED", actual.Error.Message);
                })
                .ThenCleanDataTest(param =>
                {
                    var createModel = param.CleanData.Get<TimeSheetRecordModel>("CreateModel");
                    if (createModel != null)
                    {
                        _tsRecordService.Delete(createModel.Id);
                    }
                });
        }

        #endregion EDIT

        #region DELETE

        [TestMethod]
        public void AdminRoleDelete_ValidData_Success()
        {
            _testService.StartLoginWithAdmin()
              .ThenImplementTest(param =>
              {
                  //Arrange
                  var createModel = PrepareValidRecordModel(_randomUser.Id, _randomTSActivity.Id);
                  var actual = _tsRecordService.Create(createModel);
                  param.CleanData.Add("CreateModel", actual.Data);

                  //Act
                  var result = _tsRecordService.Delete(actual.Data.Id);

                  //Assert
                  Assert.IsNotNull(actual.Data);
                  Assert.IsNull(result.Error);
                  Assert.AreEqual(true, result.Data);

                  var expectedModel = _tsRecordRespository
                      .Query()
                      .Where(t => t.Id == actual.Data.Id && !t.DeletedBy.HasValue && !t.DeletedDate.HasValue)
                      .FirstOrDefault();
                  Assert.IsNull(expectedModel);
              })
              .ThenCleanDataTest(param =>
              {
                  var createModel = param.CleanData.Get<TimeSheetRecordModel>("CreateModel");
                  if (createModel != null)
                  {
                      _tsRecordService.Delete(createModel.Id);
                  }
              });
        }

        [TestMethod]
        public void UserRoleDelete_ValidData_Success()
        {
            _testService.StartLoginWithUser(_randomUser.Email, "123456")
              .ThenImplementTest(param =>
              {
                  //Arrange
                  var createModel = PrepareValidRecordModel(_randomUser.Id, _randomTSActivity.Id);
                  var actual = _tsRecordService.Create(createModel);
                  param.CleanData.Add("CreateModel", actual.Data);

                  //Act
                  var result = _tsRecordService.Delete(actual.Data.Id);

                  //Assert
                  Assert.IsNotNull(actual.Data);
                  Assert.IsNull(result.Error);
                  Assert.AreEqual(true, result.Data);

                  var expectedModel = _tsRecordRespository
                      .Query()
                      .Where(t => t.Id == actual.Data.Id && !t.DeletedBy.HasValue && !t.DeletedDate.HasValue)
                      .FirstOrDefault();
                  Assert.IsNull(expectedModel);
              })
              .ThenCleanDataTest(param =>
              {
                  var createModel = param.CleanData.Get<TimeSheetRecordModel>("CreateModel");
                  if (createModel != null)
                  {
                      _tsRecordService.Delete(createModel.Id);
                  }
              });
        }

        [TestMethod]
        public void AdminRoleDelete_InvalidRecordId_Fail()
        {
            _testService.StartLoginWithAdmin()
              .ThenImplementTest(param =>
              {
                  //Arrange
                  var randomGuid = Guid.NewGuid();

                  //Act
                  var result = _tsRecordService.Delete(randomGuid);

                  //Assert
                  Assert.IsFalse(result.Data);
                  Assert.IsNotNull(result.Error);
                  Assert.AreEqual("INVALID_RECORD_NOT_FOUND.", result.Error.Message);

                  var expectedModel = _tsRecordRespository.Query().Where(t => t.Id == randomGuid).FirstOrDefault();
                  Assert.IsNull(expectedModel);
              })
              .ThenCleanDataTest(param =>
              {
                  var createModel = param.CleanData.Get<TimeSheetRecordModel>("CreateModel");
                  if (createModel != null)
                  {
                      _tsRecordService.Delete(createModel.Id);
                  }
              });
        }

        [TestMethod]
        public void UserRoleDelete_InvalidRecordId_Fail()
        {
            _testService.StartLoginWithUser(_randomUser.Email, "123456")
              .ThenImplementTest(param =>
              {
                  //Arrange
                  var randomGuid = Guid.NewGuid();

                  //Act
                  var result = _tsRecordService.Delete(randomGuid);

                  //Assert
                  Assert.IsFalse(result.Data);
                  Assert.IsNotNull(result.Error);
                  Assert.AreEqual("INVALID_RECORD_NOT_FOUND.", result.Error.Message);

                  var expectedModel = _tsRecordRespository.Query().Where(t => t.Id == randomGuid).FirstOrDefault();
                  Assert.IsNull(expectedModel);
              })
              .ThenCleanDataTest(param =>
              {
                  var createModel = param.CleanData.Get<TimeSheetRecordModel>("CreateModel");
                  if (createModel != null)
                  {
                      _tsRecordService.Delete(createModel.Id);
                  }
              });
        }

        [TestMethod]
        public void UserRoleDelete_InvalidRecordWithAnotherUserId_Fail()
        {
            _testService.StartLoginWithUser(_randomUser.Email, "123456")
             .ThenImplementTest(param =>
             {
                 //Arrange
                 var anotherTimeSheetRecord = new TSRecord()
                 {
                     Id = Guid.NewGuid(),
                     UserId = _randomUser2.Id,
                     Name = "Tao khong muon unittest",
                     TSActivityId = _randomTSActivity.Id,
                     StartTime = DateTime.Now.AddMinutes(-10),
                     EndTime = DateTime.Now,
                     TaskId = "123",
                     BacklogId = "1234",
                     CreatedBy = 1,
                     CreatedDate = DateTime.Now
                 };
                 _tsRecordRespository.Add(anotherTimeSheetRecord);
                 _dbContext.Save();
                 param.CleanData.Add("CreateModel", anotherTimeSheetRecord);

                 //Act
                 var result = _tsRecordService.Delete(anotherTimeSheetRecord.Id);

                  //Assert
                  Assert.IsFalse(result.Data);
                 Assert.IsNotNull(result.Error);
                 Assert.AreEqual("No role.", result.Error.Message);

                 var expectedModel = _tsRecordRespository.Query().Where(t => t.Id == anotherTimeSheetRecord.Id).FirstOrDefault();
                 Assert.IsNotNull(expectedModel);
             })
             .ThenCleanDataTest(param =>
             {
                 var createModel = param.CleanData.Get<TSRecord>("CreateModel");
                 if (createModel != null)
                 {
                     _tsRecordService.Delete(createModel.Id);
                 }
             });
        }

        [TestMethod]
        public void UserRoleDelete_LockedTimeSheetRecord_Fail()
        {
            _testService.StartLoginWithUser(_randomUser.Email, "123456")
            .ThenImplementTest(param =>
            {
                 //Arrange
                 var anotherTimeSheetRecord = new TSRecord()
                {
                    Id = Guid.NewGuid(),
                    UserId = _randomUser.Id,
                    Name = "Tao khong muon unittest",
                    TSActivityId = _randomTSActivity.Id,
                    StartTime = DateTime.Now.AddMinutes(-10),
                    EndTime = DateTime.Now,
                    TaskId = "123",
                    BacklogId = "1234",
                    CreatedBy = 1,
                    CreatedDate = DateTime.Now
                };
                //Set 
                DateTime lockDate = DateTime.UtcNow.AddDays(-_lockValueByDate - 1);
                DateTime outDateStartDateTime = new DateTime(lockDate.Year, lockDate.Month, lockDate.Day, (int)_lockAfter.TotalHours, (int)_lockAfter.TotalMinutes, 0);
                anotherTimeSheetRecord.StartTime = outDateStartDateTime;
                anotherTimeSheetRecord.EndTime = outDateStartDateTime.AddHours(2);

                _tsRecordRespository.Add(anotherTimeSheetRecord);
                _dbContext.Save();


                //Act
                var result = _tsRecordService.Delete(anotherTimeSheetRecord.Id);

                 //Assert
                Assert.IsNotNull(result.Error);
                Assert.AreEqual("TIMESHEET_LOCKED", result.Error.Message);

                var expectedModel = _tsRecordRespository.Query().Where(t => t.Id == anotherTimeSheetRecord.Id).FirstOrDefault();
                Assert.IsNotNull(expectedModel);
            })
            .ThenCleanDataTest(param =>
            {
                var createModel = param.CleanData.Get<TimeSheetRecordModel>("CreateModel");
                if (createModel != null)
                {
                    _tsRecordService.Delete(createModel.Id);
                }
            });
        }

        #endregion DELETE

        #region PRIVATE METHOD

        private void InitData()
        {
            //Init user

            UserModel newData = new UserModel();
            newData.FirstName = "test FirstName";
            newData.LastName = "test LastName";
            newData.DoB = DateTime.Now;
            newData.Email = "test@kloon.vn";
            newData.PhoneNo = "1234567890";
            newData.Sex = Kloon.EmployeePerformance.Models.Common.SexEnum.FEMALE;
            newData.Status = true;
            newData.RoleId = (int)Kloon.EmployeePerformance.Models.Common.Roles.USER;
            newData.PositionId = _cache.Position.GetValues().First().Id;

            _randomUser = _userService.Create(newData).Data;


            UserModel newDataUser2 = new UserModel();
            newDataUser2.FirstName = "test FirstName 2";
            newDataUser2.LastName = "test LastName 2";
            newDataUser2.DoB = DateTime.Now;
            newDataUser2.Email = $"test" + RandomNumber(1000, 9999).ToString() + "@kloon.vn";
            newData.PhoneNo = "1234567890";
            newDataUser2.Sex = Kloon.EmployeePerformance.Models.Common.SexEnum.FEMALE;
            newDataUser2.Status = true;
            newDataUser2.RoleId = (int)Kloon.EmployeePerformance.Models.Common.Roles.USER;
            newDataUser2.PositionId = _cache.Position.GetValues().First().Id;
            _randomUser2 = _userService.Create(newDataUser2).Data;
            //Init project

            var newProject = new ProjectModel()
            {
                Name = "Name Project",
                Status = ProjectStatusEnum.OPEN,
                Description = "Description ",
                StartDate = DateTime.Now
            };
            _randomProject = _projectService.Create(newProject).Data;

            //Init projectUser

            _randomProjectUser = _projectUserService.Create(_randomProject.Id, _randomUser.Id).Data;

            //Init ActivityGroup and Activity
            _randomTSActivityGroup = new TSActivityGroup()
            {
                Id = Guid.NewGuid(),
                Name = "ActivityGroupForTest",
                Description = "Test Description",
                ProjectId = _randomProject.Id,
                CreatedBy = 1,
                CreatedDate = DateTime.Now
            };
            _tsActivityGroupRespository.Add(_randomTSActivityGroup);

            _randomTSActivity = new TSActivity()
            {
                Id = Guid.NewGuid(),
                Description = "Description Activity",
                Name = "Acitivity test Name",
                TSActivityGroupId = _randomTSActivityGroup.Id
            };
            _tsActivityRespository.Add(_randomTSActivity);

            //Init random TSRecord

            _randomTSRecord = new TSRecord()
            {
                Id = Guid.NewGuid(),
                Name = "random TSRecord",
                BacklogId = "haha",
                TaskId = "haha",
                StartTime = DateTime.Now.AddMinutes(-10),
                EndTime = DateTime.Now,
                TSActivityId = _randomTSActivity.Id,
                UserId = _randomUser.Id,
                CreatedBy = 1,
                CreatedDate = DateTime.Now
            };
            _tsRecordRespository.Add(_randomTSRecord);

            //var modelTSRecord = PrepareValidRecordModel(_randomUser.Id, _randomTSActivity.Id);
            //_randomTSRecord = _tsRecordService.Create(modelTSRecord).Data;

            //Init lockdateafter 00:00 and 1 day

            var isLockTimeSheet = _appSettingRespository.Query(t => t.Id == WorkSpaceSettingCommon.ISLOCKTIMESHEET).FirstOrDefault();
            var lockAfter = _appSettingRespository.Query(t => t.Id == WorkSpaceSettingCommon.LOCKAFTER).FirstOrDefault();
            var lockValueByDate = _appSettingRespository.Query(t => t.Id == WorkSpaceSettingCommon.LOCKVALUEBYDATE).FirstOrDefault();

            isLockTimeSheet.Value = "true";
            lockAfter.Value = "0";
            lockValueByDate.Value = "1";

            _dbContext.Save();
            _cache.WorkSpaceSetting.Clear();

            _isLockTimeSheet = Boolean.Parse(isLockTimeSheet.Value);
            _lockAfter = _cache.WorkSpaceSetting.GetLockAfter();
            _lockValueByDate = Int32.Parse(lockValueByDate.Value);

            _cache.TSActivities.Clear();
            _cache.TSActivityGroups.Clear();
        }


        public TimeSheetRecordModel PrepareValidRecordModel(int userId, Guid activityId)
        {
            TimeSheetRecordModel model = new TimeSheetRecordModel();
            model.UserId = userId;
            model.Name = "Task Name";
            model.TaskId = "Some Task for test";
            model.BacklogId = "SomeBacklog";
            model.TSActivityId = activityId;
            model.StartTime = DateTime.Now.AddMinutes(-10);
            model.EndTime = DateTime.Now;
            return model;
        }

        public int RandomNumber(int min, int max)
        {
            return _random.Next(min, max);
        }

        #endregion PRIVATE METHOD

    }
}




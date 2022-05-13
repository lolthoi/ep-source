using Kloon.EmployeePerformance.DataAccess;
using Kloon.EmployeePerformance.DataAccess.Domain;
using Kloon.EmployeePerformance.Logic.Services;
using Kloon.EmployeePerformance.Models.Common;
using Kloon.EmployeePerformance.Models.Project;
using Kloon.EmployeePerformance.Models.TimeSheet;
using Kloon.EmployeePerformance.Models.User;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace Kloon_EmployeePerformance_UnitTest.Services
{
    [TestClass]
    public class ActivityService_Test : TestBase
    {
        private IActivityGroupService _activityGroupService;
        private IActivityService _activityService;
        private IUserService _userService;
        private IProjectService _projectService;
        private IEntityRepository<TSActivity> _activityRepository;
        private IUnitOfWork<EmployeePerformanceContext> _dbContext;

        private UserModel _userNormal;
        private ProjectModel _existedProject;
        private ActivityGroupModel _existedActivityGroup;
        private ActivityModel _existedActivity;

        protected override void CleanEnvirontment()
        {
            _userService.Delete(_userNormal.Id);
            _projectService.Delete(_existedProject.Id);
            _activityGroupService.Delete(_existedActivityGroup.Id);
            var createdActivity = _activityRepository.Query(x => x.Id == _existedActivity.Id).FirstOrDefault();
            if (createdActivity != null)
            {
                _activityRepository.Delete(createdActivity);
                _dbContext.Save();
            }
        }

        protected override void InitEnvirontment()
        {
            _testService.StartLoginWithAdmin()
                .ThenImplementTest(param =>
                {
                    PrepareDataForUnitTest();
                });
        }

        protected override void InitServices()
        {
            _activityGroupService = _scope.ServiceProvider.GetService<IActivityGroupService>();
            _activityService = _scope.ServiceProvider.GetService<IActivityService>();
            _userService = _scope.ServiceProvider.GetService<IUserService>();
            _projectService = _scope.ServiceProvider.GetService<IProjectService>();
            _dbContext = _scope.ServiceProvider.GetService<IUnitOfWork<EmployeePerformanceContext>>();
            _activityRepository = _dbContext.GetRepository<TSActivity>();
        }

        #region CREATE
        [TestMethod]
        public void ADMIN_CREATE_ACTIVITY_WITH_INVALID_ACTIVITY_GROUP_THEN_ERROR()
        {
            _testService.StartLoginWithAdmin()
                .ThenImplementTest(param =>
                {
                    //Arrange
                    Guid Id = Guid.NewGuid();
                    var InvalidGroupIdModel = PrepareActivityModel(Id);
                    var actualModel = _activityService.Create(InvalidGroupIdModel);
                    //Assert
                    Assert.IsNull(actualModel.Data);
                    Assert.AreEqual("Activity Group not found", actualModel.Error.Message);
                })
                .ThenCleanDataTest(param =>
                {

                });
        }
        [TestMethod]
        public void ADMIN_CREATE_ACTIVITY_WITH_INVALID_NAME_THEN_ERROR()
        {
            _testService.StartLoginWithAdmin()
                .ThenImplementTest(param =>
                {
                    //Arrange
                    var NameNullModel = PrepareActivityModel(_existedActivityGroup.Id);
                    NameNullModel.Name = null;
                    var actualModel = _activityService.Create(NameNullModel);
                    //Assert
                    Assert.IsNull(actualModel.Data);
                    Assert.AreEqual("Name is required", actualModel.Error.Message);
                })
                .ThenCleanDataTest(param =>
                {

                });
        }
        [TestMethod]
        public void ADMIN_CREATE_ACTIVITY_WITH_INVALID_NAME_LENGTH_THEN_ERROR()
        {
            _testService.StartLoginWithAdmin()
              .ThenImplementTest(param =>
              {
                  //Arrange
                  var invalidNameMaxLength = PrepareActivityModel(_existedActivityGroup.Id);
                  invalidNameMaxLength.Name = "qwertyuiopasdfghjklmnbvcxzqwertyuiopasdfghjklmnbvcxzqwertyuiopasdfghjklmnbvcxzqwertyuiopasdfghjklmnbvcxzqwertyuiopasdfghjklmnbvcxzqwertyuiopasdfghjklmnbvcxz";
                  var actualModel = _activityService.Create(invalidNameMaxLength);

                  var invalidNameMinLength = PrepareActivityModel(_existedActivityGroup.Id);
                  invalidNameMinLength.Name = "s";
                  var actualModel2 = _activityService.Create(invalidNameMinLength);

                  //Assert
                  Assert.IsNull(actualModel.Data);
                  Assert.AreEqual("Name character is too short or too long", actualModel.Error.Message);
                  Assert.IsNull(actualModel2.Data);
                  Assert.AreEqual("Name character is too short or too long", actualModel.Error.Message);
              })
              .ThenCleanDataTest(param =>
              {
              });
        }
        [TestMethod]
        public void ADMIN_CREATE_ACTIVITY_WITH_DUPLICATED_NAME_THEN_ERROR()
        {
            _testService.StartLoginWithAdmin()
                .ThenImplementTest(param =>
                {
                    //Arrange
                    var DuplicatedNameModel = PrepareActivityModel(_existedActivityGroup.Id);
                    DuplicatedNameModel.Name = _existedActivity.Name;
                    var actualModel = _activityService.Create(DuplicatedNameModel);
                    //Assert
                    Assert.IsNull(actualModel.Data);
                    Assert.AreEqual("Activity Name existed", actualModel.Error.Message);
                })
                .ThenCleanDataTest(param =>
                {

                });
        }
        [TestMethod]
        public void USER_CREATE_ACTIVITY_WITH_NO_PERMISSION_THEN_ERROR()
        {
            _testService.StartLoginWithUser(_userNormal.Email, "123456")
               .ThenImplementTest(param =>
               {
                   //Arrange
                   var expectedModel = PrepareActivityModel(_existedActivityGroup.Id);
                   var actualModel = _activityService.Create(expectedModel);
                   //Assert
                   Assert.IsNull(actualModel.Data);
                   Assert.AreEqual("No Role", actualModel.Error.Message);
               })
               .ThenCleanDataTest(param =>
               {

               });
        }
        [TestMethod]
        public void ADMIN_CREATE_ACTIVITY_WITH_VALID_MODEL_THEN_SUCCESS()
        {
            _testService.StartLoginWithAdmin()
               .ThenImplementTest(param =>
               {
                   //Arrange
                   var expectedModel = PrepareActivityModel(_existedActivityGroup.Id);
                   var actualModel = _activityService.Create(expectedModel);
                   //Act
                   Assert.IsNotNull(actualModel.Data);
                   param.CleanData.Add("CreateModel", actualModel.Data);

                   var resultModel = _activityRepository.Query(x => x.Id == actualModel.Data.Id).FirstOrDefault();
                   //Assert
                   Assert.IsNotNull(resultModel);
                   Assert.AreEqual(resultModel.Id, actualModel.Data.Id);
                   Assert.AreEqual(resultModel.TSActivityGroupId, actualModel.Data.ActivityGroupId);
                   Assert.AreEqual(resultModel.Name, actualModel.Data.Name);
                   Assert.AreEqual(resultModel.Description, actualModel.Data.Description);
               })
               .ThenCleanDataTest(param =>
               {
                   var createModel = param.CleanData.Get<ActivityModel>("CreateModel");
                   var createdActivity = _activityRepository.Query(x => x.Id == createModel.Id).FirstOrDefault();
                   _activityRepository.Delete(createdActivity);
                   _dbContext.Save();
               });
        }
        #endregion

        #region UPDATE
        [TestMethod]
        public void ADMIN_UPDATE_ACTIVITY_WITH_INVALID_ACTIVITY_GROUP_THEN_ERROR()
        {
            _testService.StartLoginWithAdmin()
                .ThenImplementTest(param =>
                {
                    //Arrange
                    Guid Id = Guid.NewGuid();
                    var expectedModel = SystemExtension.Clone(_existedActivity);
                    expectedModel.ActivityGroupId = Id;
                    expectedModel.Name = "EDITED";

                    var actualModel = _activityService.Update(expectedModel);
                    //Assert
                    Assert.IsNull(actualModel.Data);
                    Assert.AreEqual("Activity Group not found", actualModel.Error.Message);
                })
                .ThenCleanDataTest(param =>
                {

                });
        }
        [TestMethod]
        public void ADMIN_UPDATE_ACTIVITY_WITH_INVALID_ID_THEN_ERROR()
        {
            _testService.StartLoginWithAdmin()
                .ThenImplementTest(param =>
                {
                    //Arrange
                    var expectedModel = SystemExtension.Clone(_existedActivity);
                    expectedModel.Name = "EDITED";

                    var createdActivity = _activityRepository.Query(x => x.Id == _existedActivity.Id).FirstOrDefault();
                    _activityRepository.Delete(createdActivity);
                    _dbContext.Save();

                    var actualModel = _activityService.Update(expectedModel);
                    //Assert
                    Assert.IsNull(actualModel.Data);
                    Assert.AreEqual("Activity not found", actualModel.Error.Message);
                })
                .ThenCleanDataTest(param =>
                {

                });
        }
        [TestMethod]
        public void ADMIN_UPDATE_ACTIVITY_WITH_INVALID_NAME_THEN_ERROR()
        {
            _testService.StartLoginWithAdmin()
                .ThenImplementTest(param =>
                {
                    //Arrange
                    var NameNullModel = SystemExtension.Clone(_existedActivity);
                    NameNullModel.Name = null;
                    var actualModel = _activityService.Update(NameNullModel);
                    //Assert
                    Assert.IsNull(actualModel.Data);
                    Assert.AreEqual("Name is required", actualModel.Error.Message);
                })
                .ThenCleanDataTest(param =>
                {

                });
        }
        [TestMethod]
        public void ADMIN_UPDATE_ACTIVITY_WITH_INVALID_NAME_LENGTH_THEN_ERROR()
        {
            _testService.StartLoginWithAdmin()
              .ThenImplementTest(param =>
              {
                  //Arrange
                  var invalidNameMaxLength = SystemExtension.Clone(_existedActivity);
                  invalidNameMaxLength.Name = "qwertyuiopasdfghjklmnbvcxzqwertyuiopasdfghjklmnbvcxzqwertyuiopasdfghjklmnbvcxzqwertyuiopasdfghjklmnbvcxzqwertyuiopasdfghjklmnbvcxzqwertyuiopasdfghjklmnbvcxz";
                  var actualModel = _activityService.Update(invalidNameMaxLength);

                  var invalidNameMinLength = SystemExtension.Clone(_existedActivity);
                  invalidNameMinLength.Name = "a";
                  var actualModel2 = _activityService.Update(invalidNameMinLength);

                  //Assert
                  Assert.IsNull(actualModel.Data);
                  Assert.AreEqual("Name character is too short or too long", actualModel.Error.Message);
                  Assert.IsNull(actualModel2.Data);
                  Assert.AreEqual("Name character is too short or too long", actualModel.Error.Message);
              })
              .ThenCleanDataTest(param =>
              {
              });
        }
        [TestMethod]
        public void ADMIN_UPDATE_ACTIVITY_WITH_DUPLICATED_NAME_THEN_ERROR()
        {
            _testService.StartLoginWithAdmin()
                .ThenImplementTest(param =>
                {
                    //Arrange
                    var secondActivity = PrepareActivityModel(_existedActivityGroup.Id);
                    secondActivity = _activityService.Create(secondActivity).Data;

                    Assert.IsNotNull(secondActivity);
                    param.CleanData.Add("CreateModel", secondActivity);

                    var DuplicatedNameModel = SystemExtension.Clone(_existedActivity);
                    DuplicatedNameModel.Name = secondActivity.Name;
                    var actualModel = _activityService.Update(DuplicatedNameModel);
                    //Assert
                    Assert.IsNull(actualModel.Data);
                    Assert.AreEqual("Activity Name existed", actualModel.Error.Message);
                })
                .ThenCleanDataTest(param =>
                {
                    var createModel = param.CleanData.Get<ActivityModel>("CreateModel");
                    var createdActivity = _activityRepository.Query(x => x.Id == createModel.Id).FirstOrDefault();
                    _activityRepository.Delete(createdActivity);
                    _dbContext.Save();
                });
        }
        [TestMethod]
        public void USER_UPDATE_ACTIVITY_WITH_NO_PERMISSION_THEN_ERROR()
        {
            _testService.StartLoginWithUser(_userNormal.Email, "123456")
                .ThenImplementTest(param =>
                {
                    //Arrange
                    var expectedModel = SystemExtension.Clone(_existedActivity);
                    expectedModel.Name = "EDITED";
                    var actualModel = _activityService.Update(expectedModel);
                    //Assert
                    Assert.IsNull(actualModel.Data);
                    Assert.AreEqual("No Role", actualModel.Error.Message);
                })
                .ThenCleanDataTest(param =>
                {
                });
        }
        [TestMethod]
        public void ADMIN_UPDATE_ACTIVITY_WITH_VALID_MODEL_THEN_SUCCESS()
        {
            _testService.StartLoginWithAdmin()
                .ThenImplementTest(param =>
                {
                    //Arrange
                    var activityModel = SystemExtension.Clone(_existedActivity);
                    activityModel.Name = "EDITED";
                    activityModel.Description = "Description after edited";
                    var actualModel = _activityService.Update(activityModel);
                    //Assert
                    var expectedModel = _activityRepository.Query().Where(x => x.Id == actualModel.Data.Id).FirstOrDefault();
                    Assert.IsNotNull(expectedModel);
                    Assert.AreEqual(expectedModel.Id, actualModel.Data.Id);
                    Assert.AreEqual(expectedModel.TSActivityGroupId, actualModel.Data.ActivityGroupId);
                    Assert.AreEqual(expectedModel.Name, actualModel.Data.Name);
                    Assert.AreEqual(expectedModel.Description, actualModel.Data.Description);
                })
                .ThenCleanDataTest(param =>
                {
                });
        }
        #endregion

        #region DELETE
        [TestMethod]
        public void ADMIN_DELETE_ACTIVITY_WITH_INVALID_ID_THEN_ERROR()
        {
            _testService.StartLoginWithAdmin()
                .ThenImplementTest(param =>
                {
                    //Arrange
                    Guid Id = Guid.NewGuid();
                    var actualModel = _activityService.Delete(Id);
                    //Assert
                    Assert.IsFalse(actualModel.Data);
                    Assert.AreEqual("Activity not found", actualModel.Error.Message);
                })
                .ThenCleanDataTest(param =>
                {
                });
        }
        [TestMethod]
        public void USER_DELETE_ACTIVITY_WITH_NO_PERMISSION_THEN_ERROR()
        {
            _testService.StartLoginWithUser(_userNormal.Email, "123456")
                .ThenImplementTest(param =>
                {
                    //Act
                    var expectedModel = _activityService.Delete(_existedActivity.Id);
                    //Assert
                    Assert.IsFalse(expectedModel.Data);
                    Assert.AreEqual("No Role", expectedModel.Error.Message);
                })
                .ThenCleanDataTest(param =>
                {
                });
        }
        [TestMethod]
        public void ADMIN_DELETE_ACTIVITY_WITH_VALID_ID_THEN_SUCCESS()
        {
            _testService.StartLoginWithAdmin()
                .ThenImplementTest(param =>
                {
                    //Act
                    var expectedModel = _activityService.Delete(_existedActivity.Id);
                    //Assert
                    Assert.IsTrue(expectedModel.Data);
                    Assert.IsNull(expectedModel.Error);
                })
                .ThenCleanDataTest(param =>
                {
                });
        }
        #endregion

        #region prepare
        private ActivityGroupModel PrepareActivityGroupModel()
        {
            ActivityGroupModel model = new()
            {
                ProjectId = null,
                Name = "Group Name " + Rand(),
                Description = "Description " + Rand(),
            };
            return model;
        }
        private ActivityModel PrepareActivityModel(Guid groupId)
        {
            ActivityModel model = new()
            {
                ActivityGroupId = groupId,
                Name = "Activity Name " + Rand(),
                Description = "Description " + Rand(),
            };
            return model;
        }
        private UserModel PrepareNormalUser()
        {
            UserModel newNormalUser = new()
            {
                FirstName = "FirstName" + Rand(),
                LastName = "Lastname " + Rand(),
                Email = "Email" + Rand() + "@kloon.vn",
                PositionId = new Random().Next(1, 3),
                Sex = SexEnum.FEMALE,
                Status = true,
                DoB = DateTime.Today.AddDays(-new Random().Next(20 * 635)),
                PhoneNo = "0" + Rand(),
                RoleId = (int)Roles.USER,
            };
            return newNormalUser;
        }
        private ProjectModel PrepareProject()
        {
            ProjectModel model = new()
            {
                Name = "Name " + Rand(),
                Status = (ProjectStatusEnum)randProjectRandom(),
                Description = "Create Description " + Rand(),
                StartDate = DateTime.Now
            };
            return model;
        }

        private void PrepareDataForUnitTest()
        {
            var userModel = PrepareNormalUser();
            _userNormal = _userService.Create(userModel).Data;

            var projectModel = PrepareProject();
            _existedProject = _projectService.Create(projectModel).Data;

            var activityGroupModel = PrepareActivityGroupModel();
            _existedActivityGroup = _activityGroupService.Create(activityGroupModel).Data;

            var activityModel = PrepareActivityModel(_existedActivityGroup.Id);
            _existedActivity = _activityService.Create(activityModel).Data;
        }

        private static int Rand()
        {
            return new Random().Next(1, 1000000);
        }
        private int randProjectRandom()
        {
            return new Random().Next(1, 3);
        }
        #endregion
    }
}

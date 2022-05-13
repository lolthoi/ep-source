using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kloon.EmployeePerformance.Logic.Services;
using Kloon.EmployeePerformance.DataAccess;
using Kloon.EmployeePerformance.DataAccess.Domain;
using Kloon.EmployeePerformance.Models.User;
using Kloon.EmployeePerformance.Models.Project;
using Kloon.EmployeePerformance.Models.TimeSheet;
using Kloon.EmployeePerformance.Models.Common;

namespace Kloon_EmployeePerformance_UnitTest.Services
{
    [TestClass]
    public class ActivityGroupUserService_Test : TestBase
    {
        private IActivityGroupService _activityGroupService;
        private IActivityGroupUserService _activityGroupUserService;
        private IUserService _userService;
        private IProjectService _projectService;
        private IEntityRepository<ActivityGroupUser> _activityGroupUserRepository;
        private IUnitOfWork<EmployeePerformanceContext> _dbContext;

        private UserModel _userNormal;
        private ProjectModel _existedProject;
        private ActivityGroupModel _existedActivityGroup;
        private ActivityGroupUserModel _existedActivityGroupUser;
        protected override void CleanEnvirontment()
        {
            _userService.Delete(_userNormal.Id);
            _projectService.Delete(_existedProject.Id);
            _activityGroupService.Delete(_existedActivityGroup.Id);
            var createdActivityGroupUser = _activityGroupUserRepository.Query(x => x.Id == _existedActivityGroupUser.Id).FirstOrDefault();
            if (createdActivityGroupUser != null)
            {
                _activityGroupUserRepository.Delete(createdActivityGroupUser);
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
            _activityGroupUserService = _scope.ServiceProvider.GetService<IActivityGroupUserService>();
            _userService = _scope.ServiceProvider.GetService<IUserService>();
            _projectService = _scope.ServiceProvider.GetService<IProjectService>();
            _dbContext = _scope.ServiceProvider.GetService<IUnitOfWork<EmployeePerformanceContext>>();
            _activityGroupUserRepository = _dbContext.GetRepository<ActivityGroupUser>();
        }

        #region CREATE
        [TestMethod]
        public void ADMIN_CREATE_ACTIVITY_GROUP_USER_WITH_INVALID_GROUP_THEN_ERROR()
        {
            _testService.StartLoginWithAdmin()
                .ThenImplementTest(param =>
                {
                    //Arrange
                    Guid Id = Guid.NewGuid();
                    var InvalidGroupIdModel = PrepareActivityGroupUserModel(Id, _userNormal.Id);
                    var actualModel = _activityGroupUserService.Create(InvalidGroupIdModel);
                    //Assert
                    Assert.IsNull(actualModel.Data);
                    Assert.AreEqual("Activity Group not found", actualModel.Error.Message);
                })
                .ThenCleanDataTest(param =>
                {

                });
        }
        [TestMethod]
        public void ADMIN_CREATE_ACTIVITY_GROUP_USER_WITH_INVALID_USER_THEN_ERROR()
        {
            _testService.StartLoginWithAdmin()
                .ThenImplementTest(param =>
                {
                    //Arrange
                    int Id = Rand();
                    var InvalidUserIdModel = PrepareActivityGroupUserModel(_existedActivityGroup.Id, Id);
                    var actualModel = _activityGroupUserService.Create(InvalidUserIdModel);
                    //Assert
                    Assert.IsNull(actualModel.Data);
                    Assert.AreEqual("UserId not found", actualModel.Error.Message);
                })
                .ThenCleanDataTest(param =>
                {

                });
        }
        [TestMethod]
        public void ADMIN_CREATE_ACTIVITY_GROUP_USER_WITH_INVALID_TS_ROLE_THEN_ERROR()
        {
            _testService.StartLoginWithAdmin()
                .ThenImplementTest(param =>
                {
                    //Arrange
                    var InvalidTSRoleModel = PrepareActivityGroupUserModel(_existedActivityGroup.Id, _userNormal.Id);
                    InvalidTSRoleModel.Role = (TSRole)100;
                    var actualModel = _activityGroupUserService.Create(InvalidTSRoleModel);
                    //Assert
                    Assert.IsNull(actualModel.Data);
                    Assert.AreEqual("Invalid TSRole", actualModel.Error.Message);
                })
                .ThenCleanDataTest(param =>
                {

                });
        }
        [TestMethod]
        public void USER_CREATE_ACTIVITY_GROUP_USER_WITH_NO_PERMISSION_THEN_ERROR()
        {
            _testService.StartLoginWithUser(_userNormal.Email, "123456")
               .ThenImplementTest(param =>
               {
                   //Arrange
                   var expectedModel = PrepareActivityGroupUserModel(_existedActivityGroup.Id, _userNormal.Id);
                   var actualModel = _activityGroupUserService.Create(expectedModel);
                   //Assert
                   Assert.IsNull(actualModel.Data);
                   Assert.AreEqual("No Role", actualModel.Error.Message);
               })
               .ThenCleanDataTest(param =>
               {

               });
        }
        [TestMethod]
        public void ADMIN_CREATE_ACTIVITY_GROUP_WITH_VALID_MODEL_THEN_SUCCESS()
        {
            _testService.StartLoginWithAdmin()
               .ThenImplementTest(param =>
               {
                   //Arrange
                   var expectedModel = PrepareActivityGroupUserModel(_existedActivityGroup.Id, _userNormal.Id);
                   var actualModel = _activityGroupUserService.Create(expectedModel);
                   //Act
                   Assert.IsNotNull(actualModel.Data);
                   param.CleanData.Add("CreateModel", actualModel.Data);

                   var resultModel = _activityGroupUserRepository.Query(x => x.Id == actualModel.Data.Id).FirstOrDefault();
                   //Assert
                   Assert.IsNotNull(resultModel);
                   Assert.AreEqual(resultModel.Id, actualModel.Data.Id);
                   Assert.AreEqual(resultModel.TSActivityGroupId, actualModel.Data.ActivityGroupId);
                   Assert.AreEqual(resultModel.UserId, actualModel.Data.UserId);
                   Assert.AreEqual((TSRole)resultModel.Role, actualModel.Data.Role);
               })
               .ThenCleanDataTest(param =>
               {
                   var createModel = param.CleanData.Get<ActivityGroupUserModel>("CreateModel");
                   var createdActivityGroupUser = _activityGroupUserRepository.Query(x => x.Id == createModel.Id).FirstOrDefault();
                   _activityGroupUserRepository.Delete(createdActivityGroupUser);
                   _dbContext.Save();
               });
        }
        #endregion

        #region UPDATE
        [TestMethod]
        public void ADMIN_UPDATE_ACTIVITY_GROUP_USER_WITH_INVALID_ACTIVITY_GROUP_THEN_ERROR()
        {
            _testService.StartLoginWithAdmin()
                .ThenImplementTest(param =>
                {
                    //Arrange
                    Guid Id = Guid.NewGuid();
                    var expectedModel = SystemExtension.Clone(_existedActivityGroupUser);
                    expectedModel.ActivityGroupId = Id;

                    var actualModel = _activityGroupUserService.Update(expectedModel);
                    //Assert
                    Assert.IsNull(actualModel.Data);
                    Assert.AreEqual("Activity Group not found", actualModel.Error.Message);
                })
                .ThenCleanDataTest(param =>
                {

                });
        }
        [TestMethod]
        public void ADMIN_UPDATE_ACTIVITY_GROUP_USER_WITH_INVALID_ID_THEN_ERROR()
        {
            _testService.StartLoginWithAdmin()
                .ThenImplementTest(param =>
                {
                    //Arrange
                    Guid Id = Guid.NewGuid();
                    var expectedModel = SystemExtension.Clone(_existedActivityGroupUser);
                    expectedModel.Id = Id;
                    var actualModel = _activityGroupUserService.Update(expectedModel);
                    //Assert
                    Assert.IsNull(actualModel.Data);
                    Assert.AreEqual("Activity Group User not found", actualModel.Error.Message);
                })
                .ThenCleanDataTest(param =>
                {

                });
        }
        [TestMethod]
        public void ADMIN_UPDATE_ACTIVITY_GROUP_USER_WITH_INVALID_USER_THEN_ERROR()
        {
            _testService.StartLoginWithAdmin()
                .ThenImplementTest(param =>
                {
                    //Arrange
                    int Id = Rand();
                    var expectedModel = SystemExtension.Clone(_existedActivityGroupUser);
                    expectedModel.UserId = Id;
                    var actualModel = _activityGroupUserService.Update(expectedModel);
                    //Assert
                    Assert.IsNull(actualModel.Data);
                    Assert.AreEqual("UserId not found", actualModel.Error.Message);
                })
                .ThenCleanDataTest(param =>
                {

                });
        }
        [TestMethod]
        public void ADMIN_UPDATE_ACTIVITY_GROUP_USER_WITH_INVALID_TS_ROLE_THEN_ERROR()
        {
            _testService.StartLoginWithAdmin()
                .ThenImplementTest(param =>
                {
                    //Arrange
                    var expectedModel = SystemExtension.Clone(_existedActivityGroupUser);
                    expectedModel.Role = (TSRole)100;
                    var actualModel = _activityGroupUserService.Update(expectedModel);
                    //Assert
                    Assert.IsNull(actualModel.Data);
                    Assert.AreEqual("Invalid TSRole", actualModel.Error.Message);
                })
                .ThenCleanDataTest(param =>
                {

                });
        }
        [TestMethod]
        public void USER_UPDATE_ACTIVITY_GROUP_USER_WITH_NO_PERMISSION_THEN_ERROR()
        {
            _testService.StartLoginWithUser(_userNormal.Email, "123456")
                .ThenImplementTest(param =>
                {
                    //Arrange
                    var expectedModel = SystemExtension.Clone(_existedActivityGroupUser);

                    var actualModel = _activityGroupUserService.Update(expectedModel);
                    //Assert
                    Assert.IsNull(actualModel.Data);
                    Assert.AreEqual("No Role", actualModel.Error.Message);
                })
                .ThenCleanDataTest(param =>
                {

                });
        }
        [TestMethod]
        public void ADMIN_UPDATE_ACTIVITY_GROUP_USER_WITH_VALID_MODEL_THEN_SUCCESS()
        {
            _testService.StartLoginWithAdmin()
                .ThenImplementTest(param =>
                {
                    //Arrange
                    var activityGroupUser = SystemExtension.Clone(_existedActivityGroupUser);
                    var actualModel = _activityGroupUserService.Update(activityGroupUser);
                    //Assert
                    var expectedModel = _activityGroupUserRepository.Query().Where(x => x.Id == actualModel.Data.Id).FirstOrDefault();
                    Assert.IsNotNull(expectedModel);
                    Assert.AreEqual(expectedModel.Id, actualModel.Data.Id);
                    Assert.AreEqual(expectedModel.TSActivityGroupId, actualModel.Data.ActivityGroupId);
                    Assert.AreEqual(expectedModel.UserId, actualModel.Data.UserId);
                    Assert.AreEqual((TSRole)expectedModel.Role, actualModel.Data.Role);
                })
                .ThenCleanDataTest(param =>
                {

                });
        }
        #endregion

        #region DELETE
        [TestMethod]
        public void ADMIN_DELETE_ACTIVITY_GROUP_USER_WITH_INVALID_ID_THEN_ERROR()
        {
            _testService.StartLoginWithAdmin()
                .ThenImplementTest(param =>
                {
                    //Arrange
                    Guid Id = Guid.NewGuid();
                    var actualModel = _activityGroupUserService.Delete(Id);
                    //Assert
                    Assert.IsFalse(actualModel.Data);
                    Assert.AreEqual("Activity Group User not found", actualModel.Error.Message);
                })
                .ThenCleanDataTest(param =>
                {
                });
        }
        [TestMethod]
        public void USER_DELETE_ACTIVITY_GROUP_USER_WITH_NO_PERMISSION_THEN_ERROR()
        {
            _testService.StartLoginWithUser(_userNormal.Email, "123456")
                .ThenImplementTest(param =>
                {
                    //Act
                    var expectedModel = _activityGroupUserService.Delete(_existedActivityGroupUser.Id);
                    //Assert
                    Assert.IsFalse(expectedModel.Data);
                    Assert.AreEqual("No Role", expectedModel.Error.Message);
                })
                .ThenCleanDataTest(param =>
                {
                });
        }
        [TestMethod]
        public void ADMIN_DELETE_ACTIVITY_GROUP_USER_WITH_VALID_ID_THEN_SUCCESS()
        {
            _testService.StartLoginWithAdmin()
                .ThenImplementTest(param =>
                {
                    //Act
                    var expectedModel = _activityGroupUserService.Delete(_existedActivityGroupUser.Id);
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
        private ActivityGroupUserModel PrepareActivityGroupUserModel(Guid groupId, int userId)
        {
            ActivityGroupUserModel model = new()
            {
                ActivityGroupId = groupId,
                UserId = userId,
                Role = (TSRole)1,
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

            var activityGroupUserModel = PrepareActivityGroupUserModel(_existedActivityGroup.Id, _userNormal.Id);
            _existedActivityGroupUser = _activityGroupUserService.Create(activityGroupUserModel).Data;
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

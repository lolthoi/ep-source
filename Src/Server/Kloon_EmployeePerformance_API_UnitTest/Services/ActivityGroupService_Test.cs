using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.DependencyInjection;
using System;
using Kloon.EmployeePerformance.Logic.Services;
using Kloon.EmployeePerformance.Models.TimeSheet;
using Kloon.EmployeePerformance.Models.User;
using Kloon.EmployeePerformance.Models.Common;
using Kloon.EmployeePerformance.Models.Project;
using Kloon.EmployeePerformance.DataAccess;
using Kloon.EmployeePerformance.DataAccess.Domain;
using System.Linq;

namespace Kloon_EmployeePerformance_UnitTest.Services
{
    [TestClass]
    public class ActivityGroupService_Test : TestBase
    {
        private IActivityGroupService _activityGroupService;
        private IUserService _userService;
        private IProjectService _projectService;
        private IEntityRepository<TSActivityGroup> _activityGroupRepository;
        private IUnitOfWork<EmployeePerformanceContext> _dbContext;

        private UserModel _userNormal;
        private ProjectModel _existedProject;
        private ActivityGroupModel _existedActivityGroup;

        protected override void CleanEnvirontment()
        {
            _userService.Delete(_userNormal.Id);
            _projectService.Delete(_existedProject.Id);
            var createdActivityGroup = _activityGroupRepository.Query(x => x.Id == _existedActivityGroup.Id).FirstOrDefault();
            if (createdActivityGroup != null)
            {
                _activityGroupRepository.Delete(createdActivityGroup);
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
            _userService = _scope.ServiceProvider.GetService<IUserService>();
            _projectService = _scope.ServiceProvider.GetService<IProjectService>();
            _dbContext = _scope.ServiceProvider.GetService<IUnitOfWork<EmployeePerformanceContext>>();
            _activityGroupRepository = _dbContext.GetRepository<TSActivityGroup>();
        }

        #region CREATE
        [TestMethod]
        public void ADMIN_CREATE_ACTIVITY_GROUP_WITH_INVALID_NAME_THEN_ERROR()
        {
            _testService.StartLoginWithAdmin()
                .ThenImplementTest(param =>
                {
                    //Arrange
                    var NameNullModel = PrepareActivityGroupModel();
                    NameNullModel.Name = null;
                    var actualModel = _activityGroupService.Create(NameNullModel);
                    //Assert
                    Assert.IsNull(actualModel.Data);
                    Assert.AreEqual("Name is required", actualModel.Error.Message);
                })
                .ThenCleanDataTest(param =>
                {

                });
        }
        [TestMethod]
        public void ADMIN_CREATE_ACTIVITY_GROUP_WITH_DUPLICATED_PROJECT_NAME_THEN_ERROR()
        {
            _testService.StartLoginWithAdmin()
                .ThenImplementTest(param =>
                {
                    //Arrange
                    var existedProject = _projectService.GetById(_existedProject.Id).Data;
                    var DuplicatedNameModel = PrepareActivityGroupModel();
                    DuplicatedNameModel.Name = existedProject.Name;
                    var actualModel = _activityGroupService.Create(DuplicatedNameModel);
                    //Assert
                    Assert.IsNull(actualModel.Data);
                    Assert.AreEqual("Project Name existed", actualModel.Error.Message);
                })
                .ThenCleanDataTest(param =>
                {

                });
        }
        [TestMethod]
        public void ADMIN_CREATE_ACTIVITY_GROUP_INVALID_NAME_LENGTH_THEN_ERROR()
        {
            _testService.StartLoginWithAdmin()
                .ThenImplementTest(param =>
                {
                    //Arrange
                    var invalidNameMaxLength = PrepareActivityGroupModel();
                    invalidNameMaxLength.Name = "qwertyuiopasdfghjklmnbvcxzqwertyuiopasdfghjklmnbvcxzqwertyuiopasdfghjklmnbvcxzqwertyuiopasdfghjklmnbvcxzqwertyuiopasdfghjklmnbvcxzqwertyuiopasdfghjklmnbvcxz";
                    var actualModel = _activityGroupService.Create(invalidNameMaxLength);

                    var invalidNameMinLength = PrepareActivityGroupModel();
                    invalidNameMinLength.Name = "s";
                    var actualModel2 = _activityGroupService.Create(invalidNameMinLength);

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
        public void ADMIN_CREATE_ACTIVITY_GROUP_WITH_DUPLICATED_GROUP_NAME_THEN_ERROR()
        {
            _testService.StartLoginWithAdmin()
                .ThenImplementTest(param =>
                {
                    //Arrange
                    var DuplicatedNameModel = PrepareActivityGroupModel();
                    DuplicatedNameModel.Name = _existedActivityGroup.Name;
                    var actualModel = _activityGroupService.Create(DuplicatedNameModel);
                    //Assert
                    Assert.IsNull(actualModel.Data);
                    Assert.AreEqual("Activity Group Name existed", actualModel.Error.Message);
                })
                .ThenCleanDataTest(param =>
                {

                });
        }
        [TestMethod]
        public void USER_CREATE_ACTIVITY_GROUP_NO_PERMISSION_THEN_ERROR()
        {
            _testService.StartLoginWithUser(_userNormal.Email, "123456")
                .ThenImplementTest(param =>
                {
                    //Arrange
                    var expectedModel = PrepareActivityGroupModel();
                    var actualModel = _activityGroupService.Create(expectedModel);
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
                    var activityGroupModel = PrepareActivityGroupModel();
                    var actualModel = _activityGroupService.Create(activityGroupModel);
                    //Act
                    Assert.IsNotNull(actualModel.Data);
                    param.CleanData.Add("CreateModel", actualModel.Data);
                    //Assert
                    var expectedModel = _activityGroupRepository.Query().Where(x => x.Id == actualModel.Data.Id).FirstOrDefault();
                    Assert.IsNotNull(expectedModel);
                    Assert.AreEqual(expectedModel.Id, actualModel.Data.Id);
                    Assert.AreEqual(expectedModel.Name, actualModel.Data.Name);
                    Assert.AreEqual(expectedModel.Description, actualModel.Data.Description);
                })
                .ThenCleanDataTest(param =>
                {
                    var createModel = param.CleanData.Get<ActivityGroupModel>("CreateModel");
                    var createdActivityGroup = _activityGroupRepository.Query(x => x.Id == createModel.Id).FirstOrDefault();
                    _activityGroupRepository.Delete(createdActivityGroup);
                    _dbContext.Save();
                });
        }
        #endregion

        #region GET
        [TestMethod]
        public void ADMIN_GET_ALL_ACTIVITY_GROUP_THEN_SUCCESS()
        {
            _testService.StartLoginWithAdmin()
             .ThenImplementTest(param =>
             {
                 //Arange
                 var activityGroupModel = PrepareActivityGroupModel();
                 var createdModel = _activityGroupService.Create(activityGroupModel);
                 //Act
                 Assert.IsNotNull(createdModel.Data);
                 param.CleanData.Add("CreateModel", createdModel.Data);
                 var listActivityGroup = _activityGroupService.GetAll();
                 var actualModel = listActivityGroup.Data.Where(x => x.Id == createdModel.Data.Id).FirstOrDefault();
                 //Assert
                 Assert.IsNull(listActivityGroup.Error);
                 Assert.AreEqual(createdModel.Data.Id, actualModel.Id);
                 Assert.AreEqual(createdModel.Data.Name, actualModel.Name);
                 Assert.AreEqual(createdModel.Data.Description, actualModel.Description);
             })
             .ThenCleanDataTest(param =>
             {
                 var createModel = param.CleanData.Get<ActivityGroupModel>("CreateModel");
                 var createdActivityGroup = _activityGroupRepository.Query(x => x.Id == createModel.Id).FirstOrDefault();
                 _activityGroupRepository.Delete(createdActivityGroup);
                 _dbContext.Save();
             });
        }
        [TestMethod]
        public void USER_GET_ALL_ACTIVITY_GROUP_NO_PERMISSION_THEN_ERROR()
        {
            _testService.StartLoginWithUser(_userNormal.Email, "123456")
             .ThenImplementTest(param =>
             {
                 //Arange
                 var listActivityGroup = _activityGroupService.GetAll();
                 //Assert
                 Assert.IsNull(listActivityGroup.Data);
                 Assert.AreEqual("No Role", listActivityGroup.Error.Message);

             })
             .ThenCleanDataTest(param =>
             {
                 var createModel = param.CleanData.Get<ActivityGroupModel>("CreateModel");
                 var createdActivityGroup = _activityGroupRepository.Query(x => x.Id == createModel.Id).FirstOrDefault();
                 _activityGroupRepository.Delete(createdActivityGroup);
                 _dbContext.Save();
             });
        }
        [TestMethod]
        public void ADMIN_GET_BY_ID_ACTIVITY_GROUP_THEN_SUCCESS()
        {
            _testService.StartLoginWithAdmin()
             .ThenImplementTest(param =>
             {
                 //Arange
                 var activityGroupModel = PrepareActivityGroupModel();
                 var createdModel = _activityGroupService.Create(activityGroupModel);
                 //Act
                 Assert.IsNotNull(createdModel.Data);
                 param.CleanData.Add("CreateModel", createdModel.Data);
                 var actualModel = _activityGroupService.GetById(createdModel.Data.Id);
                 //Assert
                 Assert.IsNotNull(actualModel.Data);
                 Assert.AreEqual(createdModel.Data.Id, actualModel.Data.Id);
                 Assert.AreEqual(createdModel.Data.ProjectId, actualModel.Data.ProjectId);
                 Assert.AreEqual(createdModel.Data.Name, actualModel.Data.Name);
                 Assert.AreEqual(createdModel.Data.Description, actualModel.Data.Description);
             })
             .ThenCleanDataTest(param =>
             {
                 var createModel = param.CleanData.Get<ActivityGroupModel>("CreateModel");
                 var createdActivityGroup = _activityGroupRepository.Query(x => x.Id == createModel.Id).FirstOrDefault();
                 _activityGroupRepository.Delete(createdActivityGroup);
             });
        }
        [TestMethod]
        public void ADMIN_GET_BY_ID_ACTIVITY_GROUP_NOT_FOUND_THEN_ERROR()
        {
            _testService.StartLoginWithAdmin()
             .ThenImplementTest(param =>
             {
                 //Arange
                 var activityGroupModel = PrepareActivityGroupModel();
                 var createdModel = _activityGroupService.Create(activityGroupModel);

                 var createdActivityGroup = _activityGroupRepository.Query(x => x.Id == createdModel.Data.Id).FirstOrDefault();
                 _activityGroupRepository.Delete(createdActivityGroup);
                 _dbContext.Save();
                 //Act
                 var actualModel = _activityGroupService.GetById(createdModel.Data.Id);
                 //Assert
                 Assert.IsNull(actualModel.Data);
                 Assert.AreEqual("Activity Group not found", actualModel.Error.Message);
             })
             .ThenCleanDataTest(param =>
             {
             });
        }
        [TestMethod]
        public void USER_GET_BY_ID_ACTIVITY_GROUP_NO_PERMISSION_THEN_ERROR()
        {
            _testService.StartLoginWithUser(_userNormal.Email, "123456")
             .ThenImplementTest(param =>
             {
                 //Arange
                 var listActivityGroup = _activityGroupService.GetById(_existedActivityGroup.Id);
                 //Assert
                 Assert.IsNull(listActivityGroup.Data);
                 Assert.AreEqual("No Role", listActivityGroup.Error.Message);

             })
             .ThenCleanDataTest(param =>
             {
             });
        }
        #endregion

        #region UPDATE
        [TestMethod]
        public void ADMIN_UPDATE_ACTIVITY_GROUP_WITH_INVALID_NAME_THEN_ERROR()
        {
            _testService.StartLoginWithAdmin()
                .ThenImplementTest(param =>
                {
                    //Arrange
                    var NameNullModel = SystemExtension.Clone(_existedActivityGroup);
                    NameNullModel.Name = null;
                    var actualModel = _activityGroupService.Update(NameNullModel);
                    //Assert
                    Assert.IsNull(actualModel.Data);
                    Assert.AreEqual("Name is required", actualModel.Error.Message);
                })
                .ThenCleanDataTest(param =>
                {

                });
        }
        [TestMethod]
        public void ADMIN_UPDATE_ACTIVITY_GROUP_WITH_INVALID_NAME_LENGTH_THEN_ERROR()
        {
            _testService.StartLoginWithAdmin()
                .ThenImplementTest(param =>
                {
                    //Arrange
                    var invalidNameMaxLength = SystemExtension.Clone(_existedActivityGroup);
                    invalidNameMaxLength.Name = "qwertyuiopasdfghjklmnbvcxzqwertyuiopasdfghjklmnbvcxzqwertyuiopasdfghjklmnbvcxzqwertyuiopasdfghjklmnbvcxzqwertyuiopasdfghjklmnbvcxzqwertyuiopasdfghjklmnbvcxz";
                    var actualModel = _activityGroupService.Update(invalidNameMaxLength);

                    var invalidNameMinLength = SystemExtension.Clone(_existedActivityGroup);
                    invalidNameMinLength.Name = "a";
                    var actualModel2 = _activityGroupService.Update(invalidNameMinLength);

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
        public void ADMIN_UPDATE_ACTIVITY_GROUP_WITH_DUPLICATED_PROJECT_NAME_THEN_ERROR()
        {
            _testService.StartLoginWithAdmin()
                .ThenImplementTest(param =>
                {
                    //Arrange
                    var existedProject = _projectService.GetById(_existedProject.Id).Data;
                    var DuplicatedNameModel = SystemExtension.Clone(_existedActivityGroup);
                    DuplicatedNameModel.Name = existedProject.Name;
                    var actualModel = _activityGroupService.Update(DuplicatedNameModel);
                    //Assert
                    Assert.IsNull(actualModel.Data);
                    Assert.AreEqual("Project Name existed", actualModel.Error.Message);
                })
                .ThenCleanDataTest(param =>
                {

                });
        }
        [TestMethod]
        public void ADMIN_UPDATE_ACTIVITY_GROUP_WITH_DUPLICATED_GROUP_NAME_THEN_ERROR()
        {
            _testService.StartLoginWithAdmin()
                .ThenImplementTest(param =>
                {
                    //Arrange
                    var secondActivityGroup = PrepareActivityGroupModel();
                    secondActivityGroup = _activityGroupService.Create(secondActivityGroup).Data;
                    Assert.IsNotNull(secondActivityGroup);
                    param.CleanData.Add("CreateModel", secondActivityGroup);
                    var DuplicatedNameModel = SystemExtension.Clone(_existedActivityGroup);
                    DuplicatedNameModel.Name = secondActivityGroup.Name;
                    var actualModel = _activityGroupService.Update(DuplicatedNameModel);
                    //Assert
                    Assert.IsNull(actualModel.Data);
                    Assert.AreEqual("Activity Group Name existed", actualModel.Error.Message);
                })
                .ThenCleanDataTest(param =>
                {
                    var createModel = param.CleanData.Get<ActivityGroupModel>("CreateModel");
                    var createdActivityGroup = _activityGroupRepository.Query(x => x.Id == createModel.Id).FirstOrDefault();
                    _activityGroupRepository.Delete(createdActivityGroup);
                    _dbContext.Save();
                });
        }
        [TestMethod]
        public void ADMIN_UPDATE_ACTIVITY_GROUP_BUT_NOT_FOUND_THEN_ERROR()
        {
            _testService.StartLoginWithAdmin()
                .ThenImplementTest(param =>
                {
                    //Arrange
                    var expectedModel = SystemExtension.Clone(_existedActivityGroup);
                    expectedModel.Name = "EDITED";

                    var createdActivityGroup = _activityGroupRepository.Query(x => x.Id == _existedActivityGroup.Id).FirstOrDefault();
                    _activityGroupRepository.Delete(createdActivityGroup);
                    _dbContext.Save();

                    var actualModel = _activityGroupService.Update(expectedModel);
                    //Assert
                    Assert.IsNull(actualModel.Data);
                    Assert.AreEqual("Activity Group not found", actualModel.Error.Message);
                })
                .ThenCleanDataTest(param =>
                {

                });
        }
        [TestMethod]
        public void ADMIN_UPDATE_INEDITEABLE_ACTIVITY_GROUP_THEN_ERROR()
        {
            _testService.StartLoginWithAdmin()
                .ThenImplementTest(param =>
                {
                    //Arrangew
                    var inediteableGroup = PrepareActivityGroupModel();
                    inediteableGroup.ProjectId = _existedProject.Id;
                    var actualModel = _activityGroupService.Create(inediteableGroup);
                    //Act
                    Assert.IsNotNull(actualModel.Data);
                    param.CleanData.Add("CreateModel", actualModel.Data);

                    var expectedModel = SystemExtension.Clone(actualModel.Data);
                    expectedModel.Name = "EDITED";

                    var resultModel = _activityGroupService.Update(expectedModel);
                    //Assert
                    Assert.IsNull(resultModel.Data);
                    Assert.AreEqual("Inediteable Group", resultModel.Error.Message);
                })
                .ThenCleanDataTest(param =>
                {
                    var createModel = param.CleanData.Get<ActivityGroupModel>("CreateModel");
                    var createdActivityGroup = _activityGroupRepository.Query(x => x.Id == createModel.Id).FirstOrDefault();
                    _activityGroupRepository.Delete(createdActivityGroup);
                    _dbContext.Save();
                });
        }
        [TestMethod]
        public void USER_UPDATE_ACTIVITY_GROUP_NO_PERMISSION_THEN_ERROR()
        {
            _testService.StartLoginWithUser(_userNormal.Email, "123456")
                .ThenImplementTest(param =>
                {
                    //Arrange
                    var expectedModel = SystemExtension.Clone(_existedActivityGroup);
                    expectedModel.Name = "EDITED";
                    var actualModel = _activityGroupService.Update(expectedModel);
                    //Assert
                    Assert.IsNull(actualModel.Data);
                    Assert.AreEqual("No Role", actualModel.Error.Message);
                })
                .ThenCleanDataTest(param =>
                {

                });
        }
        [TestMethod]
        public void ADMIN_UPDATE_ACTIVITY_GROUP_WITH_VALID_MODEL_THEN_SUCCESS()
        {
            _testService.StartLoginWithAdmin()
                .ThenImplementTest(param =>
                {
                    //Arrange
                    var activityGroupModel = SystemExtension.Clone(_existedActivityGroup);
                    activityGroupModel.Name = "EDITED";
                    activityGroupModel.Description = "Description after edited";
                    var actualModel = _activityGroupService.Update(activityGroupModel);
                    //Assert
                    var expectedModel = _activityGroupRepository.Query().Where(x => x.Id == actualModel.Data.Id).FirstOrDefault();
                    Assert.IsNotNull(expectedModel);
                    Assert.AreEqual(expectedModel.Id, actualModel.Data.Id);
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
        public void ADMIN_DELETE_ACTIVITY_GROUP_BUT_NOT_FOUND_THEN_ERROR()
        {
            _testService.StartLoginWithAdmin()
                .ThenImplementTest(param =>
                {
                    //Arrange
                    Guid Id = Guid.NewGuid();
                    var actualModel = _activityGroupService.Delete(Id);
                    //Assert
                    Assert.IsFalse(actualModel.Data);
                    Assert.AreEqual("Activity Group not found", actualModel.Error.Message);
                })
                .ThenCleanDataTest(param =>
                {

                });
        }
        [TestMethod]
        public void ADMIN_DELETE_UNDELETEABLE_ACTIVITY_GROUP_THEN_ERROR()
        {
            _testService.StartLoginWithAdmin()
                .ThenImplementTest(param =>
                {
                    //Arrange
                    var undeleteableGroup = PrepareActivityGroupModel();
                    undeleteableGroup.ProjectId = _existedProject.Id;
                    var actualModel = _activityGroupService.Create(undeleteableGroup);
                    //Act
                    Assert.IsNotNull(actualModel.Data);
                    param.CleanData.Add("CreateModel", actualModel.Data);

                    var resultModel = _activityGroupService.Delete(actualModel.Data.Id);
                    //Assert
                    Assert.IsFalse(resultModel.Data);
                    Assert.AreEqual("Undeleteable Group", resultModel.Error.Message);
                })
                .ThenCleanDataTest(param =>
                {
                    var createModel = param.CleanData.Get<ActivityGroupModel>("CreateModel");
                    var createdActivityGroup = _activityGroupRepository.Query(x => x.Id == createModel.Id).FirstOrDefault();
                    _activityGroupRepository.Delete(createdActivityGroup);
                    _dbContext.Save();
                });
        }
        [TestMethod]
        public void ADMIN_DELETE_ACTIVITY_GROUP_THEN_SUCCESS()
        {
            _testService.StartLoginWithAdmin()
                .ThenImplementTest(param =>
                {
                    //Act
                    var expectedModel = _activityGroupService.Delete(_existedActivityGroup.Id);
                    //Assert
                    Assert.IsTrue(expectedModel.Data);
                    Assert.IsNull(expectedModel.Error);
                })
                .ThenCleanDataTest(param =>
                {
                });
        }
        [TestMethod]
        public void USER_DELETE_ACTIVITY_GROUP_NO_PERMISSION_THEN_ERROR()
        {
            _testService.StartLoginWithUser(_userNormal.Email, "123456")
                .ThenImplementTest(param =>
                {
                    //Act
                    var expectedModel = _activityGroupService.Delete(_existedActivityGroup.Id);
                    //Assert
                    Assert.IsFalse(expectedModel.Data);
                    Assert.AreEqual("No Role", expectedModel.Error.Message);
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

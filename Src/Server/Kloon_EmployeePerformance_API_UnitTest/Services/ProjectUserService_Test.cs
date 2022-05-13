using Kloon.EmployeePerformance.DataAccess;
using Kloon.EmployeePerformance.DataAccess.Domain;
using Kloon.EmployeePerformance.Logic.Services;
using Kloon.EmployeePerformance.Models.Common;
using Kloon.EmployeePerformance.Models.Project;
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
    public class ProjectUserService_Test : TestBase
    {
        private readonly Random _rand = new Random();
        private IProjectService _projectService;
        private IUserService _userService;
        private IProjectUserService _projectUserService;

        private IUnitOfWork<EmployeePerformanceContext> _dbContext;
        private IEntityRepository<Project> _projects;
        private IEntityRepository<ProjectUser> _projectUsers;
        private IEntityRepository<User> _users;

        private ProjectModel _projectModel1;

        private ProjectUserModel _projectUserModel;

        private UserModel _userModel;

        protected override void InitServices()
        {
            _projectService = _scope.ServiceProvider.GetService<IProjectService>();
            _userService = _scope.ServiceProvider.GetService<IUserService>();
            _projectUserService = _scope.ServiceProvider.GetService<IProjectUserService>();

            _dbContext = _scope.ServiceProvider.GetService<IUnitOfWork<EmployeePerformanceContext>>();
            _projects = _dbContext.GetRepository<Project>();
            _projectUsers = _dbContext.GetRepository<ProjectUser>();
            _users = _dbContext.GetRepository<User>();
        }


        protected override void InitEnvirontment()
        {
            _testService.StartLoginWithAdmin()
                .ThenImplementTest(param =>
                {
                    _projectModel1 = InitProjectModel();
                    _projectModel1 = _projectService.Create(_projectModel1).Data;

                    _userModel = InitUserModel();
                    _userModel = _userService.Create(_userModel).Data;

                    _projectUserModel = _projectUserService.Create(_projectModel1.Id, _userModel.Id).Data;

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

                    var project = _projects.Query(x => x.Id == _projectModel1.Id).FirstOrDefault();
                    if (project != null)
                    {
                        _projects.Delete(project);
                    }

                    var user = _users.Query(x => x.Id == _userModel.Id).FirstOrDefault();
                    if (user != null)
                    {
                        _users.Delete(user);
                    }
                });
        }

        #region GET ALL

        [TestMethod]
        public void AdminRoleGetAll_ValidProjectId_Success()
        {
            _testService.StartLoginWithAdmin()
                .ThenImplementTest(param =>
                {
                    #region Init Data

                    var userModel = InitUserModel();
                    userModel = _userService.Create(userModel).Data;

                    var createModel = _projectUserService.Create(_projectModel1.Id, userModel.Id);
                    param.CleanData.Add("CreateModel", createModel.Data);

                    #endregion

                    var result = _projectUserService.GetAll(_projectModel1.Id, "");
                    var actualModel = result.Data.Where(x => x.Id == createModel.Data.Id).FirstOrDefault();

                    Assert.IsNotNull(result.Data);
                    Assert.IsNull(result.Error);
                    Assert.IsNotNull(actualModel);

                    Assert.AreEqual(createModel.Data.Id, actualModel.Id);
                    Assert.AreEqual(createModel.Data.ProjectId, actualModel.ProjectId);
                    Assert.AreEqual(createModel.Data.UserId, actualModel.UserId);
                    Assert.AreEqual(createModel.Data.ProjectRoleId, actualModel.ProjectRoleId);

                })
                .ThenCleanDataTest(param =>
                {
                    var createModel = param.CleanData.Get<ProjectUserModel>("CreateModel");
                    if (createModel != null)
                    {
                        var projectUser = _projectUsers.Query(x => x.Id == createModel.Id).FirstOrDefault();                      
                        var user = _users.Query(x => x.Id == projectUser.UserId).FirstOrDefault();
                        if (projectUser != null)
                        {
                            _projectUsers.Delete(projectUser);
                        }
                        if (user != null)
                        {
                            _users.Delete(user);
                        }
                    }
                });
        }

        [TestMethod]
        public void AdminRoleGetAll_InvalidProjectId_Fail()
        {
            _testService.StartLoginWithAdmin()
                .ThenImplementTest(param =>
                {
                    var actual = _projectUserService.GetAll(0);

                    Assert.IsNotNull(actual.Error);
                    Assert.IsNull(actual.Data);
                    Assert.AreEqual(ErrorType.BAD_REQUEST, actual.Error.Type);
                    Assert.AreEqual("Project not found", actual.Error.Message);
                })
                .ThenCleanDataTest(param =>
                {

                });
        }

        [TestMethod]
        public void UserRoleGetAll_ValidProjectId_Success()
        {
            _testService.StartLoginWithUser(_userModel.Email, "123456")
                .ThenImplementTest(param =>
                {
                    #region Init Data

                    var result = _projectUserService.GetAll(_projectModel1.Id, "");
                    var actualModel = result.Data.Where(x => x.Id == _projectUserModel.Id).FirstOrDefault();

                    #endregion

                    Assert.IsNotNull(result.Data);
                    Assert.IsNull(result.Error);
                    Assert.IsNotNull(actualModel);

                    Assert.AreEqual(_projectUserModel.Id, actualModel.Id);
                    Assert.AreEqual(_projectUserModel.ProjectId, actualModel.ProjectId);
                    Assert.AreEqual(_projectUserModel.UserId, actualModel.UserId);
                    Assert.AreEqual(_projectUserModel.ProjectRoleId, actualModel.ProjectRoleId);

                })
                .ThenCleanDataTest(param =>
                {
                    
                });
        }

        [TestMethod]
        public void UserRoleGetAll_InvalidProjectId_Fail()
        {
            _testService.StartLoginWithUser(_userModel.Email, "123456")
                .ThenImplementTest(param =>
                {
                    var actual = _projectUserService.GetAll(0);

                    Assert.IsNotNull(actual.Error);
                    Assert.IsNull(actual.Data);
                    Assert.AreEqual(ErrorType.BAD_REQUEST, actual.Error.Type);
                    Assert.AreEqual("Project not found", actual.Error.Message);
                })
                .ThenCleanDataTest(param =>
                {

                });
        }

        #endregion

        #region GetById

        [TestMethod]
        public void AdminRoleGetById_ValidData_Success()
        {
            _testService.StartLoginWithAdmin()
                .ThenImplementTest(param =>
                {
                    #region Init Data

                    var userModel = InitUserModel();
                    userModel = _userService.Create(userModel).Data;

                    var createModel = _projectUserService.Create(_projectModel1.Id, userModel.Id);
                    param.CleanData.Add("CreateModel", createModel.Data);

                    #endregion

                    var actualModel = _projectUserService.GetById(_projectModel1.Id, createModel.Data.Id);

                    Assert.IsNotNull(actualModel.Data);
                    Assert.IsNull(actualModel.Error);
                    Assert.IsNotNull(actualModel);

                    Assert.AreEqual(createModel.Data.Id, actualModel.Data.Id);
                    Assert.AreEqual(createModel.Data.ProjectId, actualModel.Data.ProjectId);
                    Assert.AreEqual(createModel.Data.UserId, actualModel.Data.UserId);

                })
                .ThenCleanDataTest(param =>
                {
                    var createModel = param.CleanData.Get<ProjectUserModel>("CreateModel");
                    if (createModel != null)
                    {
                        var projectUser = _projectUsers.Query(x => x.Id == createModel.Id).FirstOrDefault();
                        var user = _users.Query(x => x.Id == projectUser.UserId).FirstOrDefault();
                        if (projectUser != null)
                        {
                            _projectUsers.Delete(projectUser);
                        }
                        if (user != null)
                        {
                            _users.Delete(user);
                        }
                    }
                });
        }

        [TestMethod]
        public void AdminRoleGetById_InvalidModel_Fail()
        {
            _testService.StartLoginWithAdmin()
                .ThenImplementTest(param =>
                {
                    var nullProjectIdAndNullProjectUserId = _projectUserService.GetById(0, new Guid());

                    Assert.IsNotNull(nullProjectIdAndNullProjectUserId.Error);
                    Assert.IsNull(nullProjectIdAndNullProjectUserId.Data);
                    Assert.AreEqual(ErrorType.BAD_REQUEST, nullProjectIdAndNullProjectUserId.Error.Type);
                    Assert.AreEqual("Project not found", nullProjectIdAndNullProjectUserId.Error.Message);

                    var nullNullProjectUserId = _projectUserService.GetById(_projectModel1.Id, new Guid());

                    Assert.IsNotNull(nullNullProjectUserId.Error);
                    Assert.IsNull(nullNullProjectUserId.Data);
                    Assert.AreEqual(ErrorType.NOT_EXIST, nullNullProjectUserId.Error.Type);
                    Assert.AreEqual("Project User not found", nullNullProjectUserId.Error.Message);
                })
                .ThenCleanDataTest(param =>
                {

                });
        }

        [TestMethod]
        public void UserRoleGetById_ValidData_Success()
        {
            _testService.StartLoginWithUser(_userModel.Email, "123456")
                .ThenImplementTest(param =>
                {
                    #region Init Data

                    var actualModel = _projectUserService.GetById(_projectModel1.Id, _projectUserModel.Id);

                    #endregion

                    Assert.IsNotNull(actualModel.Data);
                    Assert.IsNull(actualModel.Error);
                    Assert.IsNotNull(actualModel);

                    Assert.AreEqual(_projectUserModel.Id, actualModel.Data.Id);
                    Assert.AreEqual(_projectUserModel.ProjectId, actualModel.Data.ProjectId);
                    Assert.AreEqual(_projectUserModel.UserId, actualModel.Data.UserId);
                    Assert.AreEqual(_projectUserModel.ProjectRoleId, actualModel.Data.ProjectRoleId);

                })
                .ThenCleanDataTest(param =>
                {

                });
        }

        [TestMethod]
        public void UserRoleGetById_InvalidData_Fail()
        {
            _testService.StartLoginWithUser(_userModel.Email, "123456")
                .ThenImplementTest(param =>
                {
                    var actual = _projectUserService.GetById(_projectModel1.Id, new Guid());

                    Assert.IsNotNull(actual.Error);
                    Assert.IsNull(actual.Data);
                    Assert.AreEqual(ErrorType.NOT_EXIST, actual.Error.Type);
                    Assert.AreEqual("Project User not found", actual.Error.Message);
                })
                .ThenCleanDataTest(param =>
                {

                });
        }

        #endregion

        #region Create

        [TestMethod]
        public void AdminRoleCreate_ValidData_Success()
        {
            _testService.StartLoginWithAdmin()
                .ThenImplementTest(param =>
                {
                    #region Init Data

                    var userModel = InitUserModel();
                    userModel = _userService.Create(userModel).Data;

                    var createModel = _projectUserService.Create(_projectModel1.Id, userModel.Id);
                    param.CleanData.Add("CreateModel", createModel.Data);

                    #endregion

                    Assert.IsNotNull(createModel.Data);
                    Assert.IsNull(createModel.Error);
                    var actualModel = _projectUserService.GetById(_projectModel1.Id ,createModel.Data.Id).Data;
                    Assert.IsNotNull(actualModel);

                    Assert.AreEqual(createModel.Data.Id, actualModel.Id);
                    Assert.AreEqual(createModel.Data.ProjectId, actualModel.ProjectId);
                    Assert.AreEqual(createModel.Data.UserId, actualModel.UserId);
                    Assert.AreEqual(createModel.Data.ProjectRoleId, actualModel.ProjectRoleId);

                })
                .ThenCleanDataTest(param =>
                {
                    var createModel = param.CleanData.Get<ProjectUserModel>("CreateModel");
                    if (createModel != null)
                    {
                        var projectUser = _projectUsers.Query(x => x.Id == createModel.Id).FirstOrDefault();
                        var user = _users.Query(x => x.Id == projectUser.UserId).FirstOrDefault();
                        if (projectUser != null)
                        {
                            _projectUsers.Delete(projectUser);
                        }
                        if (user != null)
                        {
                            _users.Delete(user);
                        }
                    }
                });
        }

        [TestMethod]
        public void AdminRoleCreate_InvalidData_Fail()
        {
            _testService.StartLoginWithAdmin()
                .ThenImplementTest(param =>
                {
                    ProjectUserCreateModel nullModel = new ProjectUserCreateModel(0, 0);
                    ProjectUserCreateModel userIdNotFound = new ProjectUserCreateModel(_projectModel1.Id, 5);
                    ProjectUserCreateModel projectIdNotFound = new ProjectUserCreateModel(1000, _userModel.Id);
                    ProjectUserCreateModel userReadyExitsInProject = new ProjectUserCreateModel(_projectModel1.Id, _userModel.Id);

                    Dictionary<string, ProjectUserCreateModel> invalidModels = new Dictionary<string, ProjectUserCreateModel>();
                    invalidModels.Add(nameof(nullModel), nullModel);
                    invalidModels.Add(nameof(userIdNotFound), userIdNotFound);
                    invalidModels.Add(nameof(projectIdNotFound), projectIdNotFound);
                    invalidModels.Add(nameof(userReadyExitsInProject), userReadyExitsInProject);

                    foreach (var invalidModel in invalidModels)
                    {
                        var actualModel = _projectUserService.Create(invalidModel.Value.ProjectId, invalidModel.Value.UserId);

                        Assert.IsNotNull(actualModel);
                        Assert.IsNull(actualModel.Data);

                        switch (invalidModel.Key)
                        {
                            case nameof(nullModel):
                                Assert.AreEqual(ErrorType.NOT_EXIST, actualModel.Error.Type);
                                Assert.AreEqual("Please choose the user to add to the Project.", actualModel.Error.Message);
                                break;
                            case nameof(userIdNotFound):
                                Assert.AreEqual(ErrorType.NOT_EXIST, actualModel.Error.Type);
                                Assert.AreEqual("Please choose the user to add to the Project.", actualModel.Error.Message);
                                break;
                            case nameof(projectIdNotFound):
                                Assert.AreEqual(ErrorType.NOT_EXIST, actualModel.Error.Type);
                                Assert.AreEqual("Project not found", actualModel.Error.Message);
                                break;
                            case nameof(userReadyExitsInProject):
                                Assert.AreEqual(ErrorType.DUPLICATED, actualModel.Error.Type);
                                Assert.AreEqual("User already exists in project", actualModel.Error.Message);
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
        public void UserRoleCreate_NoPermission_Fail()
        {
            _testService.StartLoginWithUser(_userModel.Email, "123456")
                .ThenImplementTest(param =>
                {
                    //Arrange

                    ProjectUserCreateModel model = new ProjectUserCreateModel(_projectModel1.Id, _userModel.Id);

                    //Act
                    var actual = _projectUserService.Create(model.ProjectId, model.UserId);

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

        #region Update

        [TestMethod]
        public void AdminRoleUpdate_ValidData_Success()
        {
            _testService.StartLoginWithAdmin()
                .ThenImplementTest(param =>
                {
                    #region Init Data

                    var userModel = InitUserModel();
                    userModel = _userService.Create(userModel).Data;

                    var createModel = _projectUserService.Create(_projectModel1.Id, userModel.Id);
                    param.CleanData.Add("CreateModel", createModel.Data);

                    var modelUpdate = new
                    {
                        projectId = _projectModel1.Id,
                        projectUserId = createModel.Data.Id,
                        projectRoleId = randProjectRandom(),
                    };

                    var updateProjectUserModel = _projectUserService.Update(modelUpdate.projectId, modelUpdate.projectUserId, modelUpdate.projectRoleId);

                    #endregion

                    Assert.IsNotNull(updateProjectUserModel.Data);
                    Assert.IsNull(updateProjectUserModel.Error);
                    var actualModel = _projectUserService.GetById(modelUpdate.projectId, updateProjectUserModel.Data.Id).Data;
                    Assert.IsNotNull(actualModel);

                    Assert.AreEqual(createModel.Data.Id, actualModel.Id);
                    Assert.AreEqual(createModel.Data.ProjectId, actualModel.ProjectId);
                    Assert.AreEqual(createModel.Data.UserId, actualModel.UserId);

                })
                .ThenCleanDataTest(param =>
                {
                    var createModel = param.CleanData.Get<ProjectUserModel>("CreateModel");
                    if (createModel != null)
                    {
                        var projectUser = _projectUsers.Query(x => x.Id == createModel.Id).FirstOrDefault();
                        var user = _users.Query(x => x.Id == projectUser.UserId).FirstOrDefault();
                        if (projectUser != null)
                        {
                            _projectUsers.Delete(projectUser);
                        }
                        if (user != null)
                        {
                            _users.Delete(user);
                        }
                    }
                });
        }

        [TestMethod]
        public void AdminRoleUpdate_InvalidData_Fail()
        {
            _testService.StartLoginWithAdmin()
                .ThenImplementTest(param =>
                {
                    ProjectUserCreateModel nullModel = new ProjectUserCreateModel(0, Guid.NewGuid(), 0);

                    var userModel = InitUserModel();
                    var createUser = _userService.Create(userModel);
                    var currentUser = _users.Query(x => x.Id == createUser.Data.Id).FirstOrDefault();
                    var createUserWithCurrentUser = _projectUserService.Create(_projectModel1.Id, currentUser.Id);
                    param.CleanData.Add("createUserWithCurrentUser", createUserWithCurrentUser.Data);

                    currentUser.DeletedBy = 1;
                    currentUser.DeletedDate = DateTime.Now;
                    _users.Edit(currentUser);
                    ProjectUserCreateModel userIdNotFound = new ProjectUserCreateModel(_projectModel1.Id, createUserWithCurrentUser.Data.Id, randProjectRandom());

                    var projectModel = InitProjectModel();
                    var createProject = _projectService.Create(projectModel);
                    var currentProject = _projects.Query(x => x.Id == createProject.Data.Id).FirstOrDefault();
                    var createProjectWithCurrentProject = _projectUserService.Create(currentProject.Id, _userModel.Id);
                    param.CleanData.Add("createProjectWithCurrentProject", createProjectWithCurrentProject.Data);

                    currentProject.DeletedBy = 1;
                    currentProject.DeletedDate = DateTime.Now;
                    _projects.Edit(currentProject);
                    ProjectUserCreateModel projectIdNotFound = new ProjectUserCreateModel(currentProject.Id, createProjectWithCurrentProject.Data.Id, randProjectRandom());

                    Dictionary<string, ProjectUserCreateModel> invalidModels = new Dictionary<string, ProjectUserCreateModel>();
                    invalidModels.Add(nameof(nullModel), nullModel);
                    invalidModels.Add(nameof(userIdNotFound), userIdNotFound);
                    //invalidModels.Add(nameof(projectIdNotFound), projectIdNotFound);

                    foreach (var invalidModel in invalidModels)
                    {
                        var actualModel = _projectUserService.Update(invalidModel.Value.ProjectId, invalidModel.Value.ProjectUserId, invalidModel.Value.ProjectRoleId);

                        Assert.IsNotNull(actualModel);
                        Assert.IsNull(actualModel.Data);

                        switch (invalidModel.Key)
                        {
                            case nameof(nullModel):
                                Assert.AreEqual(ErrorType.NOT_EXIST, actualModel.Error.Type);
                                Assert.AreEqual("Project Member not found", actualModel.Error.Message);
                                break;
                            case nameof(userIdNotFound):
                                Assert.AreEqual(ErrorType.NOT_EXIST, actualModel.Error.Type);
                                Assert.AreEqual("Please choose the user to add to the Project.", actualModel.Error.Message);
                                break;
                            case nameof(projectIdNotFound):
                                Assert.AreEqual(ErrorType.NOT_EXIST, actualModel.Error.Type);
                                Assert.AreEqual("Project not found", actualModel.Error.Message);
                                break;
                            default:
                                Assert.Fail();
                                break;
                        }
                    }
                })
                .ThenCleanDataTest(param =>
                {
                    var createUserWithCurrentUser = param.CleanData.Get<ProjectUserModel>("createUserWithCurrentUser");
                    if (createUserWithCurrentUser != null)
                    {
                        var projectUser = _projectUsers.Query(x => x.Id == createUserWithCurrentUser.Id).FirstOrDefault();
                        var user = _users.Query(x => x.Id == projectUser.UserId).FirstOrDefault();
                        if (projectUser != null)
                        {
                            _projectUsers.Delete(projectUser);
                        }
                        if (user != null)
                        {
                            _users.Delete(user);
                        }
                    }
                    var createProjectWithCurrentProject = param.CleanData.Get<ProjectUserModel>("createProjectWithCurrentProject");
                    if (createProjectWithCurrentProject != null)
                    {
                        var projectUser = _projectUsers.Query(x => x.Id == createProjectWithCurrentProject.Id).FirstOrDefault();
                        var project = _projects.Query(x => x.Id == projectUser.ProjectId).FirstOrDefault();
                        if (projectUser != null)
                        {
                            _projectUsers.Delete(projectUser);
                        }
                        if (project != null)
                        {
                            _projects.Delete(project);
                        }
                    }
                });
        }

        [TestMethod]
        public void UserRoleUpdate_NoPermission_Fail()
        {
            _testService.StartLoginWithUser(_userModel.Email, "123456")
                .ThenImplementTest(param =>
                {
                    //Arrange

                    var modelUpdate = new
                    {
                        projectId = _projectModel1.Id,
                        projectUserId = _projectUserModel.Id,
                        projectRoleId = randProjectRandom(),
                    };

                    //Act

                    var actualModel = _projectUserService.Update(modelUpdate.projectId, modelUpdate.projectUserId, modelUpdate.projectRoleId); 

                    //Assert

                    Assert.IsNotNull(actualModel.Error);
                    Assert.IsNull(actualModel.Data);
                    Assert.AreEqual(ErrorType.NO_ROLE, actualModel.Error.Type);
                    Assert.AreEqual("No Role", actualModel.Error.Message);
                })
                .ThenCleanDataTest(param =>
                {
                });
        }

        #endregion

        #region Delete

        [TestMethod]
        public void AdminRoleDeleteProjectUser_ValidProjectId_Success()
        {
            _testService.StartLoginWithAdmin()
                .ThenImplementTest(param =>
                {
                    #region Init Data

                    var userModel = InitUserModel();
                    userModel = _userService.Create(userModel).Data;

                    var createModel = _projectUserService.Create(_projectModel1.Id, userModel.Id);
                    param.CleanData.Add("CreateModel", createModel.Data);

                    #endregion

                    var result = _projectUserService.Delete(_projectModel1.Id, createModel.Data.Id);

                    Assert.IsNotNull(result.Data);
                    Assert.IsNull(result.Error);
                    Assert.AreEqual(true, result.Data);

                    var actualModel = _projectUserService.GetById(_projectModel1.Id, createModel.Data.Id);
                    Assert.IsNull(actualModel.Data);
                    Assert.IsNotNull(actualModel.Error);
                    Assert.AreEqual(ErrorType.NOT_EXIST, actualModel.Error.Type);
                    Assert.AreEqual("Project User not found", actualModel.Error.Message);

                })
                .ThenCleanDataTest(param =>
                {
                    var createModel = param.CleanData.Get<ProjectUserModel>("CreateModel");
                    if (createModel != null)
                    {
                        var projectUser = _projectUsers.Query(x => x.Id == createModel.Id).FirstOrDefault();
                        var user = _users.Query(x => x.Id == projectUser.UserId).FirstOrDefault();
                        if (projectUser != null)
                        {
                            _projectUsers.Delete(projectUser);
                        }
                        if (user != null)
                        {
                            _users.Delete(user);
                        }
                    }
                });
        }

        [TestMethod]
        public void AdminRoleDeteleProjectUser_InValidData_Fail()
        {
            _testService.StartLoginWithAdmin()
              .ThenImplementTest(param =>
              {
                  #region Init data

                  var result = _projectUserService.Delete(_projectModel1.Id, Guid.NewGuid());

                  #endregion

                  #region Get and check data

                  Assert.IsNotNull(result.Error);
                  Assert.IsNotNull(result.Data);
                  Assert.AreEqual(ErrorType.NOT_EXIST, result.Error.Type);
                  Assert.AreEqual("Project Member not found", result.Error.Message);

                  #endregion
              })
              .ThenCleanDataTest(param => { });
        }

        [TestMethod]
        public void UserRoleDeleteProjectUser_NoPermission_Fail()
        {
            _testService.StartLoginWithUser(_userModel.Email, "123456")
              .ThenImplementTest(param =>
              {

                  #region Init data

                  var createdModel = _projectUserService.Delete(_projectModel1.Id, _projectUserModel.Id);

                  #endregion

                  #region Get and check data

                  Assert.IsNotNull(createdModel.Error);
                  Assert.IsNotNull(createdModel.Data);
                  Assert.AreEqual(createdModel.Data, false);
                  Assert.AreEqual(ErrorType.NO_ROLE, createdModel.Error.Type);
                  Assert.AreEqual("No Role", createdModel.Error.Message);

                  #endregion
              })
              .ThenCleanDataTest(param => { });
        }

        #endregion


        #region Init

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

        #region private

        private string AppendSpaceToString(string str, int numberOfLength)
        {
            StringBuilder sb = new StringBuilder();
            if ((numberOfLength - str.Length) > 0)
            {
                sb.Append(' ', (numberOfLength - str.Length));
            }
            sb.Append(str);
            return sb.ToString();
        }
        #endregion
    }

    public class ProjectUserCreateModel
    {
        public ProjectUserCreateModel(int projectId, int userId)
        {
            ProjectId = projectId;
            UserId = userId;
        }
        public ProjectUserCreateModel(int projectId, Guid projectUserId, int projectRoleId)
        {
            ProjectId = projectId;
            ProjectUserId = projectUserId;
            ProjectRoleId = projectRoleId;
        }
        public int ProjectId { get; set; }
        public int UserId { get; set; }
        public Guid ProjectUserId { get; set; }
        public int ProjectRoleId { get; set; }
    }
}

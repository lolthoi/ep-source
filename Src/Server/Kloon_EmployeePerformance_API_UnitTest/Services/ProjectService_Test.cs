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
    public class ProjectService_Test : TestBase
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


        #region Get All

        [TestMethod]
        public void AdminRoleGetAll_Success()
        {
            _testService.StartLoginWithAdmin()
                .ThenImplementTest(param =>
                {
                    #region Init Data

                    ProjectModel projectModel = InitProjectModel();

                    var createdModel = _projectService.Create(projectModel);
                    param.CleanData.Add("CreateModel", createdModel.Data);

                    #endregion

                    #region Get and check data

                    var result = _projectService.GetAll("");
                    var actualModel = result.Data.Where(x => x.Id == createdModel.Data.Id).FirstOrDefault();

                    Assert.IsNotNull(result.Data);
                    Assert.IsNull(result.Error);
                    Assert.IsNotNull(actualModel);
                    Assert.AreEqual(projectModel.Id, actualModel.Id);

                    Assert.AreEqual(projectModel.Name, actualModel.Name);
                    Assert.AreEqual(projectModel.Status, actualModel.Status);
                    Assert.AreEqual(projectModel.Description, actualModel.Description);
                    Assert.AreEqual(projectModel.StartDate, actualModel.StartDate);

                    #endregion
                })
                .ThenCleanDataTest(param =>
                {
                    var createModel = param.CleanData.Get<ProjectModel>("CreateModel");
                    if (createModel != null)
                    {
                        var project = _projects.Query(x => x.Id == createModel.Id).FirstOrDefault();
                        if (project != null)
                        {
                            _projects.Delete(project);
                        }
                    }
                });
        }

        [TestMethod]
        public void AdminRoleGetAllWithSearch_Success()
        {
            _testService.StartLoginWithAdmin()
                .ThenImplementTest(param =>
                {
                    #region Init Data

                    ProjectModel projectModel = InitProjectModel();

                    var createdModel = _projectService.Create(projectModel);
                    param.CleanData.Add("CreateModel", createdModel.Data);

                    #endregion

                    #region Get and check data

                    var result = _projectService.GetAll(createdModel.Data.Name);
                    var actualModel = result.Data.Where(x => x.Id == createdModel.Data.Id).FirstOrDefault();

                    Assert.IsNotNull(result.Data);
                    Assert.IsNull(result.Error);
                    Assert.IsNotNull(actualModel);
                    Assert.AreEqual(1, result.Data.Count);
                    Assert.AreEqual(projectModel.Id, actualModel.Id);

                    Assert.AreEqual(projectModel.Name, actualModel.Name);
                    Assert.AreEqual(projectModel.Status, actualModel.Status);
                    Assert.AreEqual(projectModel.Description, actualModel.Description);
                    Assert.AreEqual(projectModel.StartDate, actualModel.StartDate);

                    #endregion
                })
                .ThenCleanDataTest(param =>
                {
                    var createModel = param.CleanData.Get<ProjectModel>("CreateModel");
                    if (createModel != null)
                    {
                        var project = _projects.Query(x => x.Id == createModel.Id).FirstOrDefault();
                        if (project != null)
                        {
                            _projects.Delete(project);
                        }
                    }
                });
        }

        [TestMethod]
        public void UserRoleGetAll_Success()
        {
            _testService.StartLoginWithUser(_userModel.Email, "123456")
                .ThenImplementTest(param =>
                {
                    var data = _projectService.GetAll("");
                    var actualModel = data.Data.Where(x => x.Id == _projectModel1.Id).FirstOrDefault();

                    Assert.IsNotNull(data.Data);
                    Assert.IsNull(data.Error);
                    Assert.AreEqual(1, data.Data.Count);

                    Assert.AreEqual(_projectModel1.Id, actualModel.Id);
                    Assert.AreEqual(_projectModel1.Name, actualModel.Name);
                    Assert.AreEqual(_projectModel1.Status, actualModel.Status);
                    Assert.AreEqual(_projectModel1.Description, actualModel.Description);
                    Assert.AreEqual(_projectModel1.StartDate, actualModel.StartDate);
                })
                .ThenCleanDataTest(param =>
                {
                });
        }

        #endregion

        #region Get By Id

        [TestMethod]
        public void AdminRoleGetById_ValidProjectId_Success()
        {
            _testService.StartLoginWithAdmin()
                .ThenImplementTest(param =>
                {

                    #region Init Data

                    ProjectModel projectModel = InitProjectModel();

                    var createdModel = _projectService.Create(projectModel);
                    param.CleanData.Add("CreateModel", createdModel.Data);

                    #endregion

                    #region Get and check data

                    var actualModel = _projectService.GetById(createdModel.Data.Id);

                    Assert.IsNotNull(actualModel.Data);
                    Assert.IsNull(actualModel.Error);
                    Assert.IsNotNull(actualModel);
                    Assert.AreEqual(projectModel.Id, actualModel.Data.Id);

                    Assert.AreEqual(projectModel.Name, actualModel.Data.Name);
                    Assert.AreEqual(projectModel.Status, actualModel.Data.Status);
                    Assert.AreEqual(projectModel.Description, actualModel.Data.Description);
                    Assert.AreEqual(projectModel.StartDate, actualModel.Data.StartDate);

                    #endregion
                })
                .ThenCleanDataTest(param =>
                {
                    var createModel = param.CleanData.Get<ProjectModel>("CreateModel");
                    if (createModel != null)
                    {
                        var project = _projects.Query(x => x.Id == createModel.Id).FirstOrDefault();
                        if (project != null)
                        {
                            _projects.Delete(project);
                        }
                    }
                });
        }

        [TestMethod]
        public void AdminRoleGetById_InvalidProjectId_Fail()
        {
            _testService.StartLoginWithAdmin()
                .ThenImplementTest(param =>
                {
                    #region Init Data

                    var result = _projectService.GetById(0);

                    #endregion

                    #region Get and check data

                    Assert.IsNull(result.Data);
                    Assert.IsNotNull(result.Error);
                    Assert.AreEqual(ErrorType.NOT_EXIST, result.Error.Type);
                    Assert.AreEqual("Project not found", result.Error.Message);

                    #endregion
                }).
                ThenCleanDataTest(param =>
                {

                });
        }

        [TestMethod]
        public void UserRoleGetById_ValidProjectId_Success()
        {
            _testService.StartLoginWithUser(_userModel.Email, "123456")
                .ThenImplementTest(param =>
                {
                    #region Init Data

                    var actualModel = _projectService.GetById(_projectModel1.Id);

                    #endregion

                    #region Get and check data

                    Assert.IsNotNull(actualModel.Data);
                    Assert.IsNull(actualModel.Error);
                    Assert.IsNotNull(actualModel);
                    Assert.AreEqual(_projectModel1.Id, actualModel.Data.Id);

                    Assert.AreEqual(_projectModel1.Name, actualModel.Data.Name);
                    Assert.AreEqual(_projectModel1.Status, actualModel.Data.Status);
                    Assert.AreEqual(_projectModel1.Description, actualModel.Data.Description);
                    Assert.AreEqual(_projectModel1.StartDate, actualModel.Data.StartDate);

                    #endregion
                })
                .ThenCleanDataTest(param =>
                {
                });
        }

        [TestMethod]
        public void UserRoleGetById_InvalidProjectId_Success()
        {
            _testService.StartLoginWithUser(_userModel.Email, "123456")
                .ThenImplementTest(param =>
                {
                    #region Init Data

                    var result = _projectService.GetById(0);

                    #endregion

                    #region Get and check data

                    Assert.IsNull(result.Data);
                    Assert.IsNotNull(result.Error);
                    Assert.AreEqual(ErrorType.NOT_AUTHORIZED, result.Error.Type);
                    Assert.AreEqual("You do not have access to this project", result.Error.Message);

                    #endregion

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

                    ProjectModel projectModel = InitProjectModel();
                    var createModel = _projectService.Create(projectModel);
                    param.CleanData.Add("CreateModel", createModel.Data);

                    #endregion

                    #region Get and Check Data

                    Assert.IsNotNull(createModel.Data);
                    var actualModel = _projectService.GetById(createModel.Data.Id).Data;
                    Assert.IsNotNull(actualModel);
                    Assert.AreEqual(projectModel.Id, actualModel.Id);
                    Assert.AreEqual(projectModel.Name, actualModel.Name);
                    Assert.AreEqual(projectModel.Status, actualModel.Status);
                    Assert.AreEqual(projectModel.Description, actualModel.Description);
                    Assert.AreEqual(projectModel.StartDate, actualModel.StartDate);

                    #endregion

                })
                .ThenCleanDataTest(param =>
                {
                    var createModel = param.CleanData.Get<ProjectModel>("CreateModel");
                    if (createModel != null)
                    {
                        var project = _projects.Query(x => x.Id == createModel.Id).FirstOrDefault();
                        if (project != null)
                        {
                            _projects.Delete(project);
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
                    ProjectModel nullModel = null;

                    ProjectModel invalidNullName = InitProjectModel();
                    invalidNullName.Name = "";

                    ProjectModel invalidMaxlengthName = InitProjectModel();
                    invalidMaxlengthName.Name = AppendSpaceToString(invalidMaxlengthName.Name, 60);

                    ProjectModel invalidMaxlengthDescription = InitProjectModel();
                    invalidMaxlengthDescription.Description = AppendSpaceToString(invalidMaxlengthDescription.Description, 510);

                    ProjectModel invalidEndDateLessThanStartDate = InitProjectModel();
                    invalidEndDateLessThanStartDate.EndDate = invalidEndDateLessThanStartDate.StartDate.AddDays(-1);

                    ProjectModel invalidNameIsAlreadyExists = InitProjectModel();
                    invalidNameIsAlreadyExists.Name = _projectModel1.Name;

                    Dictionary<string, ProjectModel> invalidModels = new Dictionary<string, ProjectModel>();
                    invalidModels.Add(nameof(nullModel), nullModel);
                    invalidModels.Add(nameof(invalidNullName), invalidNullName);
                    invalidModels.Add(nameof(invalidMaxlengthName), invalidMaxlengthName);
                    invalidModels.Add(nameof(invalidMaxlengthDescription), invalidMaxlengthDescription);
                    invalidModels.Add(nameof(invalidEndDateLessThanStartDate), invalidEndDateLessThanStartDate);
                    invalidModels.Add(nameof(invalidNameIsAlreadyExists), invalidNameIsAlreadyExists);

                    foreach (var invalidModel in invalidModels)
                    {
                        var actualModel = _projectService.Create(invalidModel.Value);

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
                                Assert.AreEqual("Name is required", actualModel.Error.Message);
                                break;
                            case nameof(invalidMaxlengthName):
                                Assert.AreEqual(ErrorType.BAD_REQUEST, actualModel.Error.Type);
                                Assert.AreEqual("Max length of Project name is 50", actualModel.Error.Message);
                                break;
                            case nameof(invalidMaxlengthDescription):
                                Assert.AreEqual(ErrorType.BAD_REQUEST, actualModel.Error.Type);
                                Assert.AreEqual("Max length of Description is 500", actualModel.Error.Message);
                                break;
                            case nameof(invalidEndDateLessThanStartDate):
                                Assert.AreEqual(ErrorType.BAD_REQUEST, actualModel.Error.Type);
                                Assert.AreEqual("The date in the field Start Date must be less than the date in field End Date", actualModel.Error.Message);
                                break;
                            case nameof(invalidNameIsAlreadyExists):
                                Assert.AreEqual(ErrorType.DUPLICATED, actualModel.Error.Type);
                                Assert.AreEqual("The Project Name already exists", actualModel.Error.Message);
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

                    ProjectModel model = InitProjectModel();

                    //Act
                    var actual = _projectService.Create(model);

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
        public void AdminRoleUpdateProject_Valid_Success()
        {
            _testService
                .StartLoginWithAdmin()
                .ThenImplementTest(param =>
                {
                    #region Init data

                    ProjectModel projectModel = InitProjectModel();
                    var createdModel = _projectService.Create(projectModel);
                    param.CleanData.Add("CreateModel", createdModel.Data);

                    var modelUpdate = InitProjectModel();
                    modelUpdate.Id = createdModel.Data.Id;
                    var updateProjectModel = _projectService.Update(modelUpdate);

                    #endregion

                    #region Get and check data

                    Assert.IsNotNull(updateProjectModel.Data);
                    var actualModel = _projectService.GetById(updateProjectModel.Data.Id).Data;
                    Assert.IsNotNull(actualModel);
                    Assert.AreEqual(modelUpdate.Id, actualModel.Id);
                    Assert.AreEqual(modelUpdate.Name, actualModel.Name);
                    Assert.AreEqual(modelUpdate.Status, actualModel.Status);
                    Assert.AreEqual(modelUpdate.Description, actualModel.Description);
                    Assert.AreEqual(modelUpdate.StartDate, actualModel.StartDate);

                    #endregion
                })
                .ThenCleanDataTest(param =>
                {
                    var createModel = param.CleanData.Get<ProjectModel>("CreateModel");
                    if (createModel != null)
                    {
                        var project = _projects.Query(x => x.Id == createModel.Id).FirstOrDefault();
                        if (project != null)
                        {
                            _projects.Delete(project);
                        }
                    }
                });
        }

        [TestMethod]
        public void AdminRoleUpdateProject_InvalidData_Fail()
        {
            _testService.StartLoginWithAdmin()
                .ThenImplementTest(param =>
                {
                    ProjectModel nullModel = null;

                    ProjectModel invalidNullName = InitProjectModel();
                    invalidNullName.Id = _projectModel1.Id;
                    invalidNullName.Name = "";

                    ProjectModel invalidMaxlengthName = InitProjectModel();
                    invalidMaxlengthName.Id = _projectModel1.Id;
                    invalidMaxlengthName.Name = AppendSpaceToString(invalidMaxlengthName.Name, 60);

                    ProjectModel invalidMaxlengthDescription = InitProjectModel();
                    invalidMaxlengthDescription.Id = _projectModel1.Id;
                    invalidMaxlengthDescription.Description = AppendSpaceToString(invalidMaxlengthDescription.Description, 510);

                    ProjectModel invalidEndDateLessThanStartDate = InitProjectModel();
                    invalidEndDateLessThanStartDate.Id = _projectModel1.Id;
                    invalidEndDateLessThanStartDate.EndDate = invalidEndDateLessThanStartDate.StartDate.AddDays(-1);

                    ProjectModel projectModel = InitProjectModel();
                    var createdModel = _projectService.Create(projectModel);
                    param.CleanData.Add("CreateModel", createdModel.Data);
                    ProjectModel invalidNameIsAlreadyExists = InitProjectModel();
                    invalidNameIsAlreadyExists.Id = createdModel.Data.Id;
                    invalidNameIsAlreadyExists.Name = _projectModel1.Name;

                    Dictionary<string, ProjectModel> invalidModels = new Dictionary<string, ProjectModel>();
                    invalidModels.Add(nameof(nullModel), nullModel);
                    invalidModels.Add(nameof(invalidNullName), invalidNullName);
                    invalidModels.Add(nameof(invalidMaxlengthName), invalidMaxlengthName);
                    invalidModels.Add(nameof(invalidMaxlengthDescription), invalidMaxlengthDescription);
                    invalidModels.Add(nameof(invalidEndDateLessThanStartDate), invalidEndDateLessThanStartDate);
                    invalidModels.Add(nameof(invalidNameIsAlreadyExists), invalidNameIsAlreadyExists);

                    foreach (var invalidModel in invalidModels)
                    {
                        var actualModel = _projectService.Update(invalidModel.Value);

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
                                Assert.AreEqual("Name is required", actualModel.Error.Message);
                                break;
                            case nameof(invalidMaxlengthName):
                                Assert.AreEqual(ErrorType.BAD_REQUEST, actualModel.Error.Type);
                                Assert.AreEqual("Max length of Project name is 50", actualModel.Error.Message);
                                break;
                            case nameof(invalidMaxlengthDescription):
                                Assert.AreEqual(ErrorType.BAD_REQUEST, actualModel.Error.Type);
                                Assert.AreEqual("Max length of Description is 500", actualModel.Error.Message);
                                break;
                            case nameof(invalidEndDateLessThanStartDate):
                                Assert.AreEqual(ErrorType.BAD_REQUEST, actualModel.Error.Type);
                                Assert.AreEqual("The date in the field Start Date must be less than the date in field End Date", actualModel.Error.Message);
                                break;
                            case nameof(invalidNameIsAlreadyExists):
                                Assert.AreEqual(ErrorType.DUPLICATED, actualModel.Error.Type);
                                Assert.AreEqual("The Project Name already exists", actualModel.Error.Message);
                                break;
                            default:
                                Assert.Fail();
                                break;
                        }
                    }
                })
                .ThenCleanDataTest(param =>
                {
                    var createModel = param.CleanData.Get<ProjectModel>("CreateModel");
                    if (createModel != null)
                    {
                        var project = _projects.Query(x => x.Id == createModel.Id).FirstOrDefault();
                        if (project != null)
                        {
                            _projects.Delete(project);
                        }
                    }
                });
        }


        [TestMethod]
        public void UserRoleUpdateProject_NoPermission_Fail()
        {
            _testService.StartLoginWithUser(_userModel.Email, "123456")
                .ThenImplementTest(param =>
                {
                    //Arrange

                    ProjectModel model = InitProjectModel();
                    model.Id = _projectModel1.Id;

                    //Act
                    var actual = _projectService.Update(model);

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

        #region Delete

        [TestMethod]
        public void AdminRoleDeteleProject_ValidProject_Success()
        {
            _testService.StartLoginWithAdmin()
              .ThenImplementTest(param =>
              {
                  #region Init data

                  ProjectModel projectModel = InitProjectModel();
                  var createdModel = _projectService.Create(projectModel);
                  param.CleanData.Add("CreateModel", createdModel.Data);
                  var result = _projectService.Delete(createdModel.Data.Id);

                  #endregion

                  #region Get and check data

                  Assert.IsNotNull(result.Data);
                  Assert.IsNull(result.Error);
                  Assert.AreEqual(true, result.Data);

                  var actualModel = _projectService.GetById(createdModel.Data.Id);
                  Assert.IsNull(actualModel.Data);
                  Assert.IsNotNull(actualModel.Error);
                  Assert.AreEqual(ErrorType.NOT_EXIST, actualModel.Error.Type);
                  Assert.AreEqual("Project not found", actualModel.Error.Message);

                  #endregion

              })
              .ThenCleanDataTest(param =>
              {
                  var createModel = param.CleanData.Get<ProjectModel>("CreateModel");
                  if (createModel != null)
                  {
                      var project = _projects.Query(x => x.Id == createModel.Id).FirstOrDefault();
                      if (project != null)
                      {
                          _projects.Delete(project);
                      }
                  }
              });
        }

        [TestMethod]
        public void AdminRoleDeteleProject_InValidProject_Fail()
        {
            _testService.StartLoginWithAdmin()
              .ThenImplementTest(param =>
              {
                  #region Init data

                  var result = _projectService.Delete(0);

                  #endregion

                  #region Get and check data

                  Assert.IsNotNull(result.Error);
                  Assert.IsNotNull(result.Data);
                  Assert.AreEqual(ErrorType.NOT_EXIST, result.Error.Type);
                  Assert.AreEqual("Project not found", result.Error.Message);

                  #endregion
              })
              .ThenCleanDataTest(param => { });
        }

        [TestMethod]
        public void AdminRoleDeteleProject_DeleteDuplicated_Fail()
        {
            _testService.StartLoginWithAdmin()
              .ThenImplementTest(param =>
              {
                  #region Init data

                  ProjectModel projectModel = InitProjectModel();
                  var createdModel = _projectService.Create(projectModel);
                  param.CleanData.Add("CreateModel", createdModel.Data);

                  _projectService.Delete(createdModel.Data.Id);
                  var result = _projectService.Delete(createdModel.Data.Id);

                  #endregion

                  #region Get and check data

                  Assert.IsNotNull(result.Error);
                  Assert.IsNotNull(result.Data);
                  Assert.AreEqual(ErrorType.NOT_EXIST, result.Error.Type);
                  Assert.AreEqual("Project not found", result.Error.Message);

                  #endregion
              })
              .ThenCleanDataTest(param =>
              {
                  var createModel = param.CleanData.Get<ProjectModel>("CreateModel");
                  if (createModel != null)
                  {
                      var project = _projects.Query(x => x.Id == createModel.Id).FirstOrDefault();
                      if (project != null)
                      {
                          _projects.Delete(project);
                      }
                  }
              });
        }

        [TestMethod]
        public void UserRoleDeleteProject_NoPermission_Fail()
        {
            _testService.StartLoginWithUser(_userModel.Email, "123456")
              .ThenImplementTest(param =>
              {

                  #region Init data

                  ProjectModel model = InitProjectModel();
                  var createdModel = _projectService.Delete(model.Id);

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
}

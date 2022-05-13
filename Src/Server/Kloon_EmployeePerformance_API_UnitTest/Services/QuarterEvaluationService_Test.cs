using Kloon.EmployeePerformance.DataAccess;
using Kloon.EmployeePerformance.DataAccess.Domain;
using Kloon.EmployeePerformance.Logic.Caches;
using Kloon.EmployeePerformance.Logic.Services;
using Kloon.EmployeePerformance.Models.Common;
using Kloon.EmployeePerformance.Models.Criteria;
using Kloon.EmployeePerformance.Models.Project;
using Kloon.EmployeePerformance.Models.Template;
using Kloon.EmployeePerformance.Models.User;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kloon_EmployeePerformance_UnitTest.Services
{
    [TestClass]
    public class QuarterEvaluationService_Test : TestBase
    {
        private IQuarterEvaluationService _quarterEvaluationService;
        private IProjectService _projectService;
        private IUserService _userService;
        private IProjectUserService _projectUserService;
        private IEvaluationTemplateService _evaluationTemplateService;
        private ICriteriaStoreService _criteriaStoreService;
        private IUnitOfWork<EmployeePerformanceContext> _dbContext;
        private IEntityRepository<Criteria> _criterias;
        private IEntityRepository<CriteriaType> _criteriaTypes;
        private IEntityRepository<QuarterEvaluation> _quarterEvaluations;
        private IEntityRepository<User> _users;
        private IEntityRepository<Project> _projects;
        private IEntityRepository<ProjectUser> _projectUsers;
        private IEntityRepository<EvaluationTemplate> _evaluationTemplates;
        private IEntityRepository<QuarterCriteriaTemplate> _quarterCriteriaTemplates;
        private CacheProvider _caches;
        private UserModel _userNormal;
        private UserModel _userLeader;
        private ProjectModel _project;
        private ProjectUserModel _projectUserNormal;
        private ProjectUserModel _projectUserLeader;
        private CriteriaStoreModel _criteriaStoreModel;
        private CriteriaStoreModel _criteriaTypeStoreModel;
        private int year;
        private int quarter;

        protected override void CleanEnvirontment()
        {
            _testService.StartLoginWithAdmin()
                .ThenImplementTest(param =>
                {
                    var projectUserNormail = _projectUsers.Query(x => x.ProjectId == _project.Id && x.UserId == _userNormal.Id).FirstOrDefault();
                    if (projectUserNormail != null)
                    {
                        _projectUsers.Delete(projectUserNormail);
                    }
                    var projectUserLeader = _projectUsers.Query(x => x.ProjectId == _project.Id && x.UserId == _userLeader.Id).FirstOrDefault();
                    if (projectUserLeader != null)
                    {
                        _projectUsers.Delete(projectUserLeader);
                    }
                    var user = _users.Query(x => x.Id == _userNormal.Id).FirstOrDefault();
                    if (user != null)
                    {
                        _users.Delete(user);
                    }
                    var userLeader = _users.Query(x => x.Id == _userLeader.Id).FirstOrDefault();
                    if (userLeader != null)
                    {
                        _users.Delete(userLeader);
                    }
                    var project = _projects.Query(x => x.Id == _project.Id).FirstOrDefault();
                    if (project != null)
                    {
                        _projects.Delete(project);
                    }
                    _criteriaStoreService.Delete(_criteriaStoreModel.Id);
                    _criteriaStoreService.Delete(_criteriaTypeStoreModel.Id);
                    _dbContext.Save();
                    _caches.Projects.Clear();
                    _caches.Users.Clear();
                });
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
            _projectService = _scope.ServiceProvider.GetService<IProjectService>();
            _userService = _scope.ServiceProvider.GetService<IUserService>();
            _projectUserService = _scope.ServiceProvider.GetService<IProjectUserService>();
            _quarterEvaluationService = _scope.ServiceProvider.GetService<IQuarterEvaluationService>();
            _evaluationTemplateService = _scope.ServiceProvider.GetService<IEvaluationTemplateService>();
            _criteriaStoreService = _scope.ServiceProvider.GetService<ICriteriaStoreService>();
            _dbContext = _scope.ServiceProvider.GetService<IUnitOfWork<EmployeePerformanceContext>>();
            _criterias = _dbContext.GetRepository<Criteria>();
            _criteriaTypes = _dbContext.GetRepository<CriteriaType>();
            _quarterEvaluations = _dbContext.GetRepository<QuarterEvaluation>();
            _users = _dbContext.GetRepository<User>();
            _projects = _dbContext.GetRepository<Project>();
            _projectUsers = _dbContext.GetRepository<ProjectUser>();
            _caches = _scope.ServiceProvider.GetService<CacheProvider>();
            _evaluationTemplates = _dbContext.GetRepository<EvaluationTemplate>();
            _quarterCriteriaTemplates = _dbContext.GetRepository<QuarterCriteriaTemplate>();
        }

        [TestMethod]
        public void AdminGenerateEvaluation_InvalidParams_Fail()
        {
            _testService.StartLoginWithAdmin()
                .ThenImplementTest(param =>
                {
                    //Arange
                    year = 2010;
                    quarter = 0;
                    var result = _quarterEvaluationService.GenerateEvaluation(year, quarter);
                    //Assert
                    Assert.IsFalse(result.Data);
                    Assert.AreEqual("Invalid input year or quarter", result.Error.Message);
                })
                .ThenCleanDataTest(param =>
                {

                });
        }
        [TestMethod]
        public void AdminGenerateEvaluation_InvalidPosition_Fail()
        {
            _testService.StartLoginWithAdmin()
                .ThenImplementTest(param =>
                {
                    //Arrange
                    year = DateTime.Now.Year;
                    quarter = (int)Math.Ceiling(DateTime.Today.Month / 3m);
                    ProjectModel suitableProject = PrepareProject();
                    suitableProject.Status = ProjectStatusEnum.OPEN;
                    ProjectModel project1 = _projectService.Create(suitableProject).Data;
                    Assert.IsNotNull(project1);
                    param.CleanData.Add("CreatedProject", project1);

                    var suitableProjectMember = _projectUserService.Create(suitableProject.Id, _userNormal.Id).Data;
                    Assert.IsNotNull(suitableProjectMember);
                    param.CleanData.Add("CreatedProjectMember", suitableProjectMember);
                    var suitableProjectLeader = _projectUserService.Create(suitableProject.Id, _userLeader.Id).Data;
                    suitableProjectLeader = _projectUserService.Update(suitableProject.Id, suitableProjectLeader.Id, (int)ProjectRoles.PM).Data;
                    Assert.IsNotNull(suitableProjectLeader);
                    param.CleanData.Add("CreatedProjectLeader", suitableProjectLeader);

                    //Assert
                    var currentEvaluation = _quarterEvaluationService.GenerateEvaluation(year, quarter);
                    Assert.IsNotNull(currentEvaluation.Data);
                    Assert.IsFalse(currentEvaluation.Data);
                    Assert.AreEqual("Sending evaluation mail will not proceed because you have not created enough evaluation criteria for all positions", currentEvaluation.Error.Message);
                })
                .ThenCleanDataTest(param =>
                {
                    var CreatedProjectMember = param.CleanData.Get<ProjectUserModel>("CreatedProjectMember");
                    if (CreatedProjectMember != null)
                    {
                        var projectUserNormail = _projectUsers.Query(x => x.ProjectId == CreatedProjectMember.ProjectId && x.UserId == CreatedProjectMember.UserId).FirstOrDefault();
                        if (projectUserNormail != null)
                        {
                            _projectUsers.Delete(projectUserNormail);
                        }
                    }
                    var CreatedProjectLeader = param.CleanData.Get<ProjectUserModel>("CreatedProjectLeader");
                    if (CreatedProjectLeader != null)
                    {
                        var projectUserLeader = _projectUsers.Query(x => x.ProjectId == CreatedProjectLeader.ProjectId && x.UserId == CreatedProjectLeader.UserId).FirstOrDefault();
                        if (projectUserLeader != null)
                        {
                            _projectUsers.Delete(projectUserLeader);
                        }
                    }
                    var CreatedProject = param.CleanData.Get<ProjectModel>("CreatedProject");
                    if (CreatedProject != null)
                    {
                        var project = _projects.Query(x => x.Id == CreatedProject.Id).FirstOrDefault();
                        if (project != null)
                        {
                            _projects.Delete(project);
                        }
                    }
                    _dbContext.Save();
                    _caches.Projects.Clear();
                    _caches.Users.Clear();
                });
        }
        [TestMethod]
        public void AdminGenerateEvaluation_InvalidParamsButNoSuitableProject_Fail()
        {
            _testService.StartLoginWithAdmin()
                .ThenImplementTest(param =>
                {
                    //Arrange
                    year = 2021;
                    quarter = 2;
                    var result = _quarterEvaluationService.GenerateEvaluation(year, quarter);
                    //Assert
                    Assert.IsNotNull(result.Data);
                    Assert.IsFalse(result.Data);
                    Assert.AreEqual("No suitable project for evaluation", result.Error.Message);
                })
                .ThenCleanDataTest(param =>
                {

                });
        }
        [TestMethod]
        public void AdminGenerateEvaluation_InvalidParamsButNoSuitableUser_Fail()
        {
            _testService.StartLoginWithAdmin()
                .ThenImplementTest(param =>
                {

                    //Arrange
                    year = DateTime.Now.Year;
                    quarter = (int)Math.Ceiling(DateTime.Today.Month / 3m);
                    ProjectModel suitableProject = PrepareProject();
                    suitableProject.Status = ProjectStatusEnum.OPEN;
                    suitableProject.StartDate = DateTime.Now.AddDays(-1);
                    ProjectModel project1 = _projectService.Create(suitableProject).Data;
                    Assert.IsNotNull(project1);
                    param.CleanData.Add("CreatedProject", project1);

                    //Assert
                    var currentEvaluation = _quarterEvaluationService.GenerateEvaluation(year, quarter);
                    Assert.IsFalse(currentEvaluation.Data);
                    Assert.AreEqual("No suitable employee for evaluation", currentEvaluation.Error.Message);
                })
                .ThenCleanDataTest(param =>
                {
                    var CreatedProject = param.CleanData.Get<ProjectModel>("CreatedProject");
                    if (CreatedProject != null)
                    {
                        var project = _projects.Query(x => x.Id == CreatedProject.Id).FirstOrDefault();
                        if (project != null)
                        {
                            _projects.Delete(project);
                        }
                    }
                    _dbContext.Save();
                    _caches.Projects.Clear();
                });
        }

        [TestMethod]
        public void AdminGenerateEvaluation_ValidDataButDuplicate_Fail()
        {
            _testService.StartLoginWithAdmin()
                .ThenImplementTest(param =>
                {
                    //Arrange
                    year = DateTime.Now.Year;
                    quarter = (int)Math.Ceiling(DateTime.Today.Month / 3m);
                    ProjectModel suitableProject = PrepareProject();
                    suitableProject.Status = ProjectStatusEnum.OPEN;
                    ProjectModel project1 = _projectService.Create(suitableProject).Data;
                    Assert.IsNotNull(project1);
                    param.CleanData.Add("CreatedProject", project1);

                    var suitableProjectMember = _projectUserService.Create(suitableProject.Id, _userNormal.Id).Data;
                    Assert.IsNotNull(suitableProjectMember);
                    param.CleanData.Add("CreatedProjectMember", suitableProjectMember);
                    var suitableProjectLeader = _projectUserService.Create(suitableProject.Id, _userLeader.Id).Data;
                    suitableProjectLeader = _projectUserService.Update(suitableProject.Id, suitableProjectLeader.Id, (int)ProjectRoles.PM).Data;
                    Assert.IsNotNull(suitableProjectLeader);
                    param.CleanData.Add("CreatedProjectLeader", suitableProjectLeader);

                    var evaluationTemplateModel = InitEvaluationTemplateViewModel();
                    var createEvaluationTemplateModel = _evaluationTemplateService.Create(evaluationTemplateModel);
                    Assert.IsNotNull(createEvaluationTemplateModel.Data);
                    param.CleanData.Add("createEvaluationTemplateModel", createEvaluationTemplateModel.Data);

                    //Assert
                    var result = _quarterEvaluationService.GenerateEvaluation(year, quarter);
                    Assert.IsTrue(result.Data);
                    var duplicatedEvaluation = _quarterEvaluationService.GenerateEvaluation(year, quarter);
                    Assert.IsFalse(duplicatedEvaluation.Data);
                    Assert.AreEqual("Evaluation of this quarter has been already done", duplicatedEvaluation.Error.Message);
                })
                .ThenCleanDataTest(param =>
                {
                    var CreatedProjectMember = param.CleanData.Get<ProjectUserModel>("CreatedProjectMember");
                    if (CreatedProjectMember != null)
                    {
                        var projectUserNormail = _projectUsers.Query(x => x.ProjectId == CreatedProjectMember.ProjectId && x.UserId == CreatedProjectMember.UserId).FirstOrDefault();
                        if (projectUserNormail != null)
                        {
                            _projectUsers.Delete(projectUserNormail);
                        }
                    }
                    var CreatedProjectLeader = param.CleanData.Get<ProjectUserModel>("CreatedProjectLeader");
                    if (CreatedProjectLeader != null)
                    {
                        var projectUserLeader = _projectUsers.Query(x => x.ProjectId == CreatedProjectLeader.ProjectId && x.UserId == CreatedProjectLeader.UserId).FirstOrDefault();
                        if (projectUserLeader != null)
                        {
                            _projectUsers.Delete(projectUserLeader);
                        }
                    }
                    var CreatedProject = param.CleanData.Get<ProjectModel>("CreatedProject");
                    if (CreatedProject != null)
                    {
                        var project = _projects.Query(x => x.Id == CreatedProject.Id).FirstOrDefault();
                        if (project != null)
                        {
                            _projects.Delete(project);
                        }
                    }

                    var createEvaluationTemplateModel = param.CleanData.Get<EvaluationTemplateViewModel>("createEvaluationTemplateModel");
                    if (createEvaluationTemplateModel != null)
                    {
                        var evaluationTemplateModel = _evaluationTemplates.Query(x => x.Id == createEvaluationTemplateModel.Id).FirstOrDefault();
                        if (evaluationTemplateModel != null)
                        {
                            _evaluationTemplates.Delete(evaluationTemplateModel);
                        }
                    }
                    var quarterEvaluations = _quarterEvaluations.Query().ToList();
                    if (quarterEvaluations.Count > 0)
                    {
                        foreach (var item in quarterEvaluations)
                        {

                            _quarterEvaluations.Delete(item);
                        }

                    }
                    _dbContext.Save();
                    _caches.Projects.Clear();
                    _caches.Users.Clear();
                });
        }


        [TestMethod]
        public void UserGenerateEvaluation_WithPermission_Fail()
        {
            _testService.StartLoginWithUser(_userNormal.Email, "123456")
                .ThenImplementTest(param =>
                {
                    //Arrange
                    year = 2021;
                    quarter = 2;
                    //Assert
                    var result = _quarterEvaluationService.GenerateEvaluation(year, quarter);
                    Assert.IsFalse(result.Data);
                    Assert.AreEqual("No Role", result.Error.Message);
                }).ThenCleanDataTest(param =>
                {

                });
        }
        [TestMethod]
        public void AdminGenerateEvaluation_ValidData_Success()
        {
            _testService.StartLoginWithAdmin()
                .ThenImplementTest(param =>
                {

                    //Arrange
                    year = DateTime.Now.Year;
                    quarter = (int)Math.Ceiling(DateTime.Today.Month / 3m);
                    ProjectModel suitableProject = PrepareProject();
                    suitableProject.Status = ProjectStatusEnum.OPEN;
                    ProjectModel project1 = _projectService.Create(suitableProject).Data;
                    Assert.IsNotNull(project1);
                    param.CleanData.Add("CreatedProject2", project1);

                    var suitableProjectMember = _projectUserService.Create(suitableProject.Id, _userNormal.Id).Data;
                    Assert.IsNotNull(suitableProjectMember);
                    param.CleanData.Add("CreatedProjectMember2", suitableProjectMember);
                    var suitableProjectLeader = _projectUserService.Create(suitableProject.Id, _userLeader.Id).Data;
                    suitableProjectLeader = _projectUserService.Update(suitableProject.Id, suitableProjectLeader.Id, (int)ProjectRoles.PM).Data;
                    Assert.IsNotNull(suitableProjectLeader);
                    param.CleanData.Add("CreatedProjectLeader2", suitableProjectLeader);

                    var evaluationTemplateModel = InitEvaluationTemplateViewModel();
                    var createEvaluationTemplateModel = _evaluationTemplateService.Create(evaluationTemplateModel);
                    Assert.IsNotNull(createEvaluationTemplateModel.Data);
                    param.CleanData.Add("createEvaluationTemplateModel2", createEvaluationTemplateModel.Data);

                    //Assert
                    var result = _quarterEvaluationService.GenerateEvaluation(year, quarter);

                    var quarterCriteriaTemplate = _quarterCriteriaTemplates.Query(x => x.Name == evaluationTemplateModel.Name).FirstOrDefault();
                    Assert.IsNotNull(quarterCriteriaTemplate);
                    Assert.AreEqual(year, quarterCriteriaTemplate.Year);
                    Assert.AreEqual(quarter, quarterCriteriaTemplate.Quarter);
                    Assert.AreEqual(evaluationTemplateModel.Name, quarterCriteriaTemplate.Name);
                    Assert.AreEqual(evaluationTemplateModel.PositionId, quarterCriteriaTemplate.PositionId);

                    var criteriaType = _criteriaTypes.Query(x => x.Name == _criteriaTypeStoreModel.Name).FirstOrDefault();
                    Assert.IsNotNull(criteriaType);
                    Assert.AreEqual(quarterCriteriaTemplate.Id, criteriaType.QuarterCriteriaTemplateId);
                    Assert.AreEqual(criteriaType.Name, _criteriaTypeStoreModel.Name);
                    Assert.AreEqual(criteriaType.Description, _criteriaTypeStoreModel.Description);

                    var criteria = _criterias.Query(x => x.Name == _criteriaStoreModel.Name).FirstOrDefault();
                    Assert.IsNotNull(criteria);
                    Assert.AreEqual(criteriaType.Id, criteria.CriteriaTypeId);
                    Assert.AreEqual(criteria.Name, _criteriaStoreModel.Name);
                    Assert.AreEqual(criteria.Description, _criteriaStoreModel.Description);

                    Assert.IsTrue(result.Data);
                    Assert.IsNull(result.Error);

                }).ThenCleanDataTest(param =>
                {
                    var CreatedProjectMember = param.CleanData.Get<ProjectUserModel>("CreatedProjectMember2");
                    if (CreatedProjectMember != null)
                    {
                        var projectUserNormail = _projectUsers.Query(x => x.ProjectId == CreatedProjectMember.ProjectId && x.UserId == CreatedProjectMember.UserId).FirstOrDefault();
                        if (projectUserNormail != null)
                        {
                            _projectUsers.Delete(projectUserNormail);
                        }
                    }
                    var CreatedProjectLeader = param.CleanData.Get<ProjectUserModel>("CreatedProjectLeader2");
                    if (CreatedProjectLeader != null)
                    {
                        var projectUserLeader = _projectUsers.Query(x => x.ProjectId == CreatedProjectLeader.ProjectId && x.UserId == CreatedProjectLeader.UserId).FirstOrDefault();
                        if (projectUserLeader != null)
                        {
                            _projectUsers.Delete(projectUserLeader);
                        }
                    }
                    var CreatedProject = param.CleanData.Get<ProjectModel>("CreatedProject2");
                    if (CreatedProject != null)
                    {
                        var project = _projects.Query(x => x.Id == CreatedProject.Id).FirstOrDefault();
                        if (project != null)
                        {
                            _projects.Delete(project);
                        }
                    }

                    var createEvaluationTemplateModel = param.CleanData.Get<EvaluationTemplateViewModel>("createEvaluationTemplateModel2");
                    if (createEvaluationTemplateModel != null)
                    {
                        var evaluationTemplateModel = _evaluationTemplates.Query(x => x.Id == createEvaluationTemplateModel.Id).FirstOrDefault();
                        if (evaluationTemplateModel != null)
                        {
                            _evaluationTemplates.Delete(evaluationTemplateModel);
                        }
                    }
                    var quarterEvaluations = _quarterEvaluations.Query().ToList();
                    if (quarterEvaluations.Count > 0)
                    {
                        foreach (var item in quarterEvaluations)
                        {

                            _quarterEvaluations.Delete(item);
                        }

                    }
                    _dbContext.Save();
                    _caches.Projects.Clear();
                    _caches.Users.Clear();
                });
        }

        #region
        private UserModel PrepareNormalUser()
        {
            UserModel newNormalUser = new()
            {
                FirstName = "FirstName" + Rand(),
                LastName = "Lastname " + Rand(),
                Email = "Email" + Rand() + "@kloon.vn",
                PositionId = new Random().Next(2, 6),
                Sex = SexEnum.FEMALE,
                Status = true,
                DoB = DateTime.Today.AddDays(-new Random().Next(20 * 635)),
                PhoneNo = "0" + Rand(),
                RoleId = (int)Roles.USER,
            };
            return newNormalUser;
        }
        private UserModel PrepareLeaderUser()
        {
            UserModel newLeaderUser = new()
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
            return newLeaderUser;
        }
        private ProjectModel PrepareProject()
        {
            ProjectModel newProject = new()
            {
                Name = "Project " + Rand(),
                Status = (ProjectStatusEnum.CLOSED),
                Description = "Project description" + Rand(),
                StartDate = DateTime.UtcNow,
            };
            return newProject;
        }

        private EvaluationTemplateViewModel InitEvaluationTemplateViewModel()
        {
            var id = Guid.NewGuid();
            var newId = Guid.NewGuid();
            var model = new EvaluationTemplateViewModel()
            {
                Id = id,
                Name = "Evaluation Template " + Rand(),
                PositionId = _userNormal.PositionId,

            };
            model.CriteriaTemplateViewModels = new List<CriteriaTemplateViewModel>()
            {
                new CriteriaTemplateViewModel()
                {
                    Id = newId,
                    TemplateId = id,
                    TypeId = null,
                    CriteriaStoreId = _criteriaTypeStoreModel.Id,
                    CriteriaTypeStoreId = null,
                    Description = "Criteria Type Description: " + Rand(),
                    OrderNo = _criteriaTypeStoreModel.OrderNo,
                    Name = "Criteria Type Name: " + Rand(),
                },
                new CriteriaTemplateViewModel()
                {
                    TemplateId = id,
                    TypeId = newId,
                    CriteriaStoreId = _criteriaStoreModel.Id,
                    CriteriaTypeStoreId = _criteriaTypeStoreModel.Id,
                    Description = "Criteria Description: " + Rand(),
                    OrderNo = _criteriaStoreModel.OrderNo,
                    Name = "Criteria Name: " + Rand(),
                }
            };
            return model;
        }

        private CriteriaStoreModel InitCriteriaModel(Guid typeId)
        {
            var model = new CriteriaStoreModel
            {
                Name = "Criteria Type " + Rand(),
                TypeId = typeId,
                Description = "Criteria Type DesCription " + Rand()
            };

            return model;
        }

        private CriteriaStoreModel InitCriteriaTypeModel()
        {
            var model = new CriteriaStoreModel
            {
                Id = new Guid(),
                Name = "Criteria Type " + Rand(),
                TypeId = null,
                Description = "Criteria Type DesCription " + Rand()
            };

            return model;
        }
        private void PrepareDataForUnitTest()
        {
            var userModel = PrepareNormalUser();
            _userNormal = _userService.Create(userModel).Data;
            var leaderModel = PrepareLeaderUser();
            _userLeader = _userService.Create(leaderModel).Data;
            var projectModel = PrepareProject();
            _project = _projectService.Create(projectModel).Data;

            _projectUserNormal = _projectUserService.Create(_project.Id, _userNormal.Id).Data;
            _projectUserLeader = _projectUserService.Create(_project.Id, _userLeader.Id).Data;
            _projectUserLeader = _projectUserService.Update(_project.Id, _projectUserLeader.Id, (int)ProjectRoles.PM).Data;

            var criteriaTypeStore = InitCriteriaTypeModel();
            var criteriaStore = InitCriteriaModel(criteriaTypeStore.Id);

            _criteriaTypeStoreModel = _criteriaStoreService.Add(criteriaTypeStore).Data;
            _criteriaStoreModel = _criteriaStoreService.Add(criteriaStore).Data;
        }
        private static int Rand()
        {
            var _rand = new Random();
            return _rand.Next(1, 999999999);
        }
        #endregion
    }
}

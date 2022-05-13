using Kloon.EmployeePerformance.Logic.Caches;
using Kloon.EmployeePerformance.Logic.Services;
using Kloon.EmployeePerformance.Models.User;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kloon.EmployeePerformance.Models.Project;
using Kloon.EmployeePerformance.Models.QuarterEvaluation;
using Kloon.EmployeePerformance.Models.Common;
using Kloon.EmployeePerformance.DataAccess.Domain;
using Kloon.EmployeePerformance.DataAccess;

namespace Kloon_EmployeePerformance_UnitTest.Services
{
    [TestClass]
    public class UserQuarterEvaluationService_Test : TestBase
    {

        private IEntityRepository<UserQuarterEvaluation> _userQuarterEvaluationRepository;
        private IEntityRepository<QuarterEvaluation> _quarterEvaluationRepository;

        private IUnitOfWork<EmployeePerformanceContext> _dbContext;
        private IUserQuarterEvaluationService _userQuarterEvaluationService;
        private IUserService _userService;
        private IProjectService _projectService;
        private IProjectUserService _projectUserService;
        private CacheProvider _cache;
        UserModel user_Leader = new UserModel();
        UserModel user_Member = new UserModel();
        ProjectModel project = new ProjectModel();
        ProjectUserModel projectUser_Leader = new ProjectUserModel();
        ProjectUserModel projectUser_Member = new ProjectUserModel();
        QuarterEvaluation quarterEvaluation = new QuarterEvaluation();
        UserQuarterEvaluation userQuarterEvaluation = new UserQuarterEvaluation();
        private const string testDataUserDisplayName = "TEST DATA DISPLAYNAME";
        private const string testDataProjectName = "TEST DATA PROJECTNAME";
        private const string testDataEmail = "testemail";


        protected override void CleanEnvirontment()
        {
            _userService.Delete(user_Leader.Id);
            _userService.Delete(user_Member.Id);
            _projectService.Delete(project.Id);
            _projectUserService.Delete(project.Id, projectUser_Leader.Id);
            _projectUserService.Delete(project.Id, projectUser_Member.Id);
            _quarterEvaluationRepository.Delete(quarterEvaluation);
            _userQuarterEvaluationRepository.Delete(userQuarterEvaluation);
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
            _userService = _scope.ServiceProvider.GetService<IUserService>();
            _projectService = _scope.ServiceProvider.GetService<IProjectService>();
            _projectUserService = _scope.ServiceProvider.GetService<IProjectUserService>();
            _userQuarterEvaluationService = _scope.ServiceProvider.GetService<IUserQuarterEvaluationService>();
            _dbContext = _scope.ServiceProvider.GetService<IUnitOfWork<EmployeePerformanceContext>>();
            _quarterEvaluationRepository = _dbContext.GetRepository<QuarterEvaluation>();
            _userQuarterEvaluationRepository = _dbContext.GetRepository<UserQuarterEvaluation>();
            _cache = _scope.ServiceProvider.GetService<CacheProvider>();
        }

        #region Function Create
        [TestMethod]
        public void Create_User_Quarter_Evaluation__ValidData_Then_Succcess()
        {
            _testService
                 .StartLoginWithAdmin()
                 .ThenImplementTest(param =>
                 {
                     //Arrange

                     //Actual 

                     //Assert
                     var expectedModel = _userQuarterEvaluationRepository.Query().FirstOrDefault();
                     Assert.IsNotNull(expectedModel);

                     Assert.AreEqual(expectedModel.Id, userQuarterEvaluation.Id);
                     Assert.AreEqual(expectedModel.QuarterEvaluationId, userQuarterEvaluation.QuarterEvaluationId);
                     Assert.AreEqual(expectedModel.NoteGoodThing, userQuarterEvaluation.NoteGoodThing);
                     Assert.AreEqual(expectedModel.NoteBadThing, userQuarterEvaluation.NoteBadThing);
                     Assert.AreEqual(expectedModel.NoteOther, userQuarterEvaluation.NoteOther);

                 });
        }

        [TestMethod]
        public void Create_User_Quarter_Evaluation_InValidData_Then_Error()
        {
            _testService
            .StartLoginWithAdmin()
            .ThenImplementTest(param =>
            {
                //Arrange 
                var quarterEvaluationFake1 = PrepareQuarterEvalaution();
                var GoodThingNullModel = PrepareValidUserQuaterEvaluation(quarterEvaluationFake1.Id);
                GoodThingNullModel.NoteGoodThing = null;

                var quarterEvaluationFake2 = PrepareQuarterEvalaution();
                var BadThingNullModel = PrepareValidUserQuaterEvaluation(quarterEvaluationFake2.Id);
                BadThingNullModel.NoteBadThing = null;

                var quarterEvaluationFake3 = PrepareQuarterEvalaution();
                var GoodThingMaxLengthModel = PrepareValidUserQuaterEvaluation(quarterEvaluationFake3.Id);
                GoodThingMaxLengthModel.NoteGoodThing = AppendLetterToString(GoodThingMaxLengthModel.NoteGoodThing, 1001);

                var quarterEvaluationFake4 = PrepareQuarterEvalaution();
                var BadThingMaxLengthModel = PrepareValidUserQuaterEvaluation(quarterEvaluationFake4.Id);
                BadThingMaxLengthModel.NoteBadThing = AppendLetterToString(BadThingMaxLengthModel.NoteBadThing, 1001);

                Dictionary<string, UserQuarterEvaluationModel> invalidsModel = new Dictionary<string, UserQuarterEvaluationModel>();
                invalidsModel.Add(nameof(GoodThingNullModel), GoodThingNullModel);
                invalidsModel.Add(nameof(BadThingNullModel), BadThingNullModel);
                invalidsModel.Add(nameof(GoodThingMaxLengthModel), GoodThingMaxLengthModel);
                invalidsModel.Add(nameof(BadThingMaxLengthModel), BadThingMaxLengthModel);

                //Act
                //Assert

                foreach (var invalidModel in invalidsModel)
                {
                    var actualModel = _userQuarterEvaluationService.Create(invalidModel.Value);
                    param.CleanData.Add("CreateModel", actualModel.Data);
                    Assert.IsNotNull(actualModel);
                    Assert.IsNull(actualModel.Data);
                    switch (invalidModel.Key)
                    {
                        case nameof(GoodThingNullModel):
                            Assert.AreEqual("INVALID_MODEL_NOTE_GOOD_THING_NULL", actualModel.Error.Message);
                            break;
                        case nameof(BadThingNullModel):
                            Assert.AreEqual("INVALID_MODEL_NOTE_BAD_THING_NULL", actualModel.Error.Message);
                            break;
                        case nameof(GoodThingMaxLengthModel):
                            Assert.AreEqual("INVALID_MODEL_NOTE_GOOD_THING_MAX_LENGTH", actualModel.Error.Message);
                            break;
                        case nameof(BadThingMaxLengthModel):
                            Assert.AreEqual("INVALID_MODEL_NOTE_BAD_THING_MAX_LENGTH", actualModel.Error.Message);
                            break;
                        default:
                            Assert.Fail();
                            break;
                    }
                }
            })
            .ThenCleanDataTest(param =>
            {
                var createModel = param.CleanData.Get<UserModel>("CreateModel");
                if (createModel != null)
                {
                    _userService.Delete(createModel.Id);
                }
            })
            ;
        }

        [TestMethod]
        public void Create_User_Quarter_Evaluation_DuplicateId_Then_Error()
        {
            _testService
                .StartLoginWithAdmin()
                .ThenImplementTest(param =>
                {
                    //Arrange
                    var model = PrepareValidUserQuaterEvaluation(quarterEvaluation.Id);
                    //Actual 
                    var result = _userQuarterEvaluationService.Create(model);
                    param.CleanData.Add("DeleteData", result.Data);
                    //Assert
                    Assert.IsNotNull(result.Error);
                    Assert.IsNull(result.Data);
                    Assert.AreEqual(ErrorType.DUPLICATED, result.Error.Type);
                    Assert.AreEqual("USER_QUARTER_EVALUATION_DUPLICATE", result.Error.Message);


                })
                .ThenCleanDataTest(param =>
                {
                    var createModel = param.CleanData.Get<UserQuarterEvaluation>("DeleteData");
                    if (createModel != null)
                    {
                        _userQuarterEvaluationRepository.Delete(createModel);
                    }

                });
        }

        [TestMethod]
        public void Create_User_Quarter_Evaluation_Is_Evaluated_Then_Error()
        {
            var quarterEvaluationFake = PrepareQuarterEvalaution();
            _testService
              .StartLoginWithAdmin()
              .ThenImplementTest(param =>
              {
                  //Arrange
                 
                  var dateTimeQuarter = DateTime.Now.AddDays(-10);
                  quarterEvaluationFake.Quarter = GetYearQuarter(dateTimeQuarter);
                  quarterEvaluationFake.Year = dateTimeQuarter.Year;
                  quarterEvaluationFake.CreatedDate = dateTimeQuarter;

                  _quarterEvaluationRepository.Add(quarterEvaluationFake);
                  _dbContext.Save();

                  var readyData = PrepareValidUserQuaterEvaluation(quarterEvaluationFake.Id);
                  
                  //Actual
                  var result = _userQuarterEvaluationService.Create(readyData);
                  param.CleanData.Add("DeleteData", result.Data);
                  //Assert
                  Assert.IsNotNull(result.Error);
                  Assert.IsNull(result.Data);
                  Assert.AreEqual(ErrorType.NOT_EXIST, result.Error.Type);
                  Assert.AreEqual("EVALIATION DATE IS EXPIRED", result.Error.Message);
              })
              .ThenCleanDataTest(param =>
              {
                  _quarterEvaluationRepository.Delete(quarterEvaluationFake);
                  var createModel = param.CleanData.Get<UserQuarterEvaluation>("DeleteData");
                  if (createModel != null)
                  {
                      _userQuarterEvaluationRepository.Delete(createModel);
                  }

              });
        }
        #endregion
        #region EDIT
        [TestMethod]
        public void Edit_User_Quarter_Evaluation_Valid_Model_Then_Success()
        {
            var modelArrangeQuarterEvaluation = PrepareQuarterEvalaution();
            _testService
            .StartLoginWithAdmin()
            .ThenImplementTest(param => {
                
                _quarterEvaluationRepository.Add(modelArrangeQuarterEvaluation);
                _dbContext.Save();
                var modelArrange = PrepareValidUserQuaterEvaluation(modelArrangeQuarterEvaluation.Id);
                var actualModel = _userQuarterEvaluationService.Create(modelArrange);
                param.CleanData.Add("CleanData", actualModel.Data);
                actualModel.Data.NoteGoodThing = "asdasdas";
                actualModel.Data.NoteBadThing = "asdasd";
                actualModel.Data.NoteOther = "asdasdasd";
                var result = _userQuarterEvaluationService.Update(actualModel.Data);
            
                Assert.IsNotNull(result.Data);
                Assert.AreEqual(result.Data.NoteGoodThing, actualModel.Data.NoteGoodThing);
                Assert.AreEqual(result.Data.NoteBadThing, actualModel.Data.NoteBadThing);
                Assert.AreEqual(result.Data.NoteOther, actualModel.Data.NoteOther);
            }).ThenCleanDataTest(param=> 
            {

              

                var createModel = param.CleanData.Get<UserQuarterEvaluationModel>("CleanData");
                if (createModel != null)
                {
                    /////
                    var readyData = _userQuarterEvaluationRepository.Query().Where(t => t.Id == createModel.Id).FirstOrDefault();
                    if(readyData != null)
                        _userQuarterEvaluationRepository.Delete(readyData);
                }
                _quarterEvaluationRepository.Delete(modelArrangeQuarterEvaluation);
                _dbContext.Save();
            });
        }
        [TestMethod]
        public void Edit_User_Quarter_Evaluation_Invalid_Model_Then_Error()
        {
            _testService
           .StartLoginWithAdmin()
           .ThenImplementTest(param =>
           {
           //Arrange 
           var quarterEvaluationFake1 = PrepareQuarterEvalaution();
           var GoodThingNullModel = PrepareValidUserQuaterEvaluation(quarterEvaluationFake1.Id);
           GoodThingNullModel.NoteGoodThing = null;

           var quarterEvaluationFake2 = PrepareQuarterEvalaution();
           var BadThingNullModel = PrepareValidUserQuaterEvaluation(quarterEvaluationFake2.Id);
           BadThingNullModel.NoteBadThing = null;

           var quarterEvaluationFake3 = PrepareQuarterEvalaution();
           var GoodThingMaxLengthModel = PrepareValidUserQuaterEvaluation(quarterEvaluationFake3.Id);
           GoodThingMaxLengthModel.NoteGoodThing = AppendLetterToString(GoodThingMaxLengthModel.NoteGoodThing, 1001);

           var quarterEvaluationFake4 = PrepareQuarterEvalaution();
           var BadThingMaxLengthModel = PrepareValidUserQuaterEvaluation(quarterEvaluationFake4.Id);
           BadThingMaxLengthModel.NoteBadThing = AppendLetterToString(BadThingMaxLengthModel.NoteBadThing, 1001);

           Dictionary<string, UserQuarterEvaluationModel> invalidsModel = new Dictionary<string, UserQuarterEvaluationModel>();
           invalidsModel.Add(nameof(GoodThingNullModel), GoodThingNullModel);
           invalidsModel.Add(nameof(BadThingNullModel), BadThingNullModel);
           invalidsModel.Add(nameof(GoodThingMaxLengthModel), GoodThingMaxLengthModel);
           invalidsModel.Add(nameof(BadThingMaxLengthModel), BadThingMaxLengthModel);

               //Act
               //Assert

               foreach (var invalidModel in invalidsModel)
               {
                   var actualModel = _userQuarterEvaluationService.Update(invalidModel.Value);
                   param.CleanData.Add("CreateModel", actualModel.Data);
                   Assert.IsNotNull(actualModel);
                   Assert.IsNull(actualModel.Data);
                   switch (invalidModel.Key)
                   {
                       case nameof(GoodThingNullModel):
                           Assert.AreEqual("INVALID_MODEL_NOTE_GOOD_THING_NULL", actualModel.Error.Message);
                           break;
                       case nameof(BadThingNullModel):
                           Assert.AreEqual("INVALID_MODEL_NOTE_BAD_THING_NULL", actualModel.Error.Message);
                           break;
                       case nameof(GoodThingMaxLengthModel):
                           Assert.AreEqual("INVALID_MODEL_NOTE_GOOD_THING_MAX_LENGTH", actualModel.Error.Message);
                           break;
                       case nameof(BadThingMaxLengthModel):
                           Assert.AreEqual("INVALID_MODEL_NOTE_BAD_THING_MAX_LENGTH", actualModel.Error.Message);
                           break;
                       default:
                           Assert.Fail();
                           break;
                   }

               }
           })
           .ThenCleanDataTest(param =>
           {
                var createModel = param.CleanData.Get<UserModel>("CreateModel");
                if (createModel != null)
                {
                    _userService.Delete(createModel.Id);
                }
            })
            ;
        }

        [TestMethod]
        public void Edit_User_Quarter_Evaluation_Assessment_Duration_Then_Error()
        {
            var quarterEvaluationFake = PrepareQuarterEvalaution();
            _testService
              .StartLoginWithAdmin()
              .ThenImplementTest(param =>
              {
                  //Arrange
                  _quarterEvaluationRepository.Add(quarterEvaluationFake);
                  _dbContext.Save();

                  var userQuarterEFake = BuildUserQuarterEvaluation(quarterEvaluationFake.Id);
                  var dateTimeQuarter = DateTime.Now.AddDays(-10);
                  userQuarterEFake.CreatedDate = dateTimeQuarter;
                  _userQuarterEvaluationRepository.Add(userQuarterEFake);
                  _dbContext.Save();
                  var readyData = _userQuarterEvaluationService.GetByQuarterEvaluationId(quarterEvaluationFake.Id);
                  //Actual
                
                  param.CleanData.Add("DeleteData", readyData.Data);
                  var actualModel = _userQuarterEvaluationService.Update(readyData.Data);
                  //Assert
                  Assert.IsNotNull(actualModel.Error);
                  Assert.IsNull(actualModel.Data);
                  Assert.AreEqual(ErrorType.NOT_EXIST, actualModel.Error.Type);
                  Assert.AreEqual(" ASSESSMENT_DURATION", actualModel.Error.Message);
              })
              .ThenCleanDataTest(param =>
              {
              
                  var createModel = param.CleanData.Get < UserQuarterEvaluationModel>("DeleteData");
                  var deleteModel = _userQuarterEvaluationRepository.Query().Where(x => x.Id == createModel.Id).FirstOrDefault();
                  if (deleteModel != null)
                  {
                      _userQuarterEvaluationRepository.Delete(deleteModel);
                  }

                  _quarterEvaluationRepository.Delete(quarterEvaluationFake);
                  _dbContext.Save();
              });
        }
        #endregion

        #region
        [TestMethod]
        public void GetbyId_User_Quarter_Evaluation_Then_Success()
        {
            var quarterEvaluationFake = PrepareQuarterEvalaution();
            _testService
              .StartLoginWithAdmin()
              .ThenImplementTest(param =>
              {
                  //Arrange
                  _quarterEvaluationRepository.Add(quarterEvaluationFake);
                  _dbContext.Save();
                  var modelArrange = PrepareValidUserQuaterEvaluation(quarterEvaluationFake.Id);
                  var readyData = _userQuarterEvaluationService.Create(modelArrange);
                  param.CleanData.Add("DeleteData", readyData.Data);
                  var actualData = _userQuarterEvaluationService.GetByQuarterEvaluationId(quarterEvaluationFake.Id);

                  Assert.IsNotNull(actualData.Data);
                  Assert.IsNull(actualData.Error);
                  Assert.AreEqual(actualData.Data.NoteGoodThing, readyData.Data.NoteGoodThing);
                  Assert.AreEqual(actualData.Data.NoteBadThing, readyData.Data.NoteBadThing);
                  Assert.AreEqual(actualData.Data.NoteOther, readyData.Data.NoteOther);
              })
              .ThenCleanDataTest(param =>
              {

                  var createModel = param.CleanData.Get<UserQuarterEvaluationModel>("DeleteData");
                  if (createModel != null)
                  {
                      /////
                      var readyData = _userQuarterEvaluationRepository.Query().Where(t => t.Id == createModel.Id).FirstOrDefault();
                      if (readyData != null)
                          _userQuarterEvaluationRepository.Delete(readyData);
                  }
                  _quarterEvaluationRepository.Delete(quarterEvaluationFake);
                  _dbContext.Save();

              });
        }
        [TestMethod]
        public void GetbyId_User_Quarter_Evaluation_Not_Found_Then_Error()
        {
            var quarterEvaluationFake = PrepareQuarterEvalaution();
            _testService
              .StartLoginWithAdmin()
              .ThenImplementTest(param =>
              {
                  //Arrange
                  _quarterEvaluationRepository.Add(quarterEvaluationFake);
                  _dbContext.Save();
                  var modelArrange = PrepareValidUserQuaterEvaluation(quarterEvaluationFake.Id);
                  var readyData = _userQuarterEvaluationService.Create(modelArrange);
               
                  var result = _userQuarterEvaluationRepository.Query().Where(x => x.Id == modelArrange.Id).FirstOrDefault();
                  _userQuarterEvaluationRepository.Delete(result);
                  _dbContext.Save();
                  var actualData = _userQuarterEvaluationService.GetByQuarterEvaluationId(quarterEvaluationFake.Id);

                  Assert.IsNull(actualData.Data);
                  Assert.IsNotNull(actualData.Error);
                  Assert.AreEqual("USER_QUARTER_NOT_FOUND", actualData.Error.Message);
                  Assert.AreEqual(ErrorType.NOT_EXIST, actualData.Error.Type);
              })
              .ThenCleanDataTest(param =>
              {
                  _quarterEvaluationRepository.Delete(quarterEvaluationFake);
                  _dbContext.Save();

              });
        }
        #endregion
        public void PrepareDataForUnitTest()
        {
            user_Leader = PrepareValidUserModel(1);
            var userActual1 = _userService.Create(user_Leader).Data;

            user_Member = PrepareValidUserModel(2);
            var userActual2 = _userService.Create(user_Member).Data;

            project = PrepareValidProjectModel(1);
            var projectActual = _projectService.Create(project).Data;

            projectUser_Leader = PrepareValidProjectUserModel_Leader(user_Leader.Id);
            var projectUserModel1 = _projectUserService.Create(project.Id, user_Leader.Id);

            projectUser_Member = PrepareValidProjectUserModel_Member(user_Member.Id);
            var projectUserModel2 = _projectUserService.Create(project.Id, user_Member.Id);

            quarterEvaluation = PrepareQuarterEvalaution();
            _quarterEvaluationRepository.Add(quarterEvaluation);
            _dbContext.Save();


            userQuarterEvaluation = BuildUserQuarterEvaluation(quarterEvaluation.Id);
            _userQuarterEvaluationRepository.Add(userQuarterEvaluation);
            _dbContext.Save();

        }
        public QuarterEvaluation PrepareQuarterEvalaution()
        {
            var newData = new QuarterEvaluation()
            {
                Id = Guid.NewGuid(),
                Year = DateTime.UtcNow.Year,
                Quarter = GetYearQuarter(DateTime.UtcNow),
                UserId = user_Member.Id,
                PositionId = user_Member.PositionId,
                ProjectId = project.Id,
                ProjectLeaderId = user_Leader.Id,
                PointAverage = 0,
                CreatedDate = DateTime.UtcNow.Date,
                CreatedBy = user_Leader.Id,
                RowVersion = BitConverter.GetBytes(DateTime.UtcNow.Ticks)
            };
            return newData;
        }
        public UserModel PrepareValidUserModel(int counter)
        {
            UserModel newData = new UserModel();
            newData.FirstName = testDataUserDisplayName + (counter + GetRandomInt()).ToString();
            newData.LastName = testDataUserDisplayName + (counter + GetRandomInt()).ToString();
            newData.DoB = DateTime.Now;
            newData.Email = testDataEmail + (counter + GetRandomInt()).ToString() + "@kloon.vn";
            newData.PhoneNo = "1234567890";
            newData.Sex = Kloon.EmployeePerformance.Models.Common.SexEnum.FEMALE;
            newData.Status = true;
            newData.RoleId = (int)Kloon.EmployeePerformance.Models.Common.Roles.USER;
            newData.PositionId = _cache.Position.GetValues().First().Id;

            return newData;
        }
        public UserQuarterEvaluationModel PrepareValidUserQuaterEvaluation(Guid id)
        {
            var model = new UserQuarterEvaluationModel()
            {
                Id = new Guid(),
                QuarterEvaluationId = id,
                NoteGoodThing = "hello Long",
                NoteBadThing = "hello World",
                NoteOther = "hello Sky"
            };
            return model;
        }
        public ProjectModel PrepareValidProjectModel(int counter)
        {
            ProjectModel newData = new ProjectModel();
            newData.Name = testDataProjectName + (counter + GetRandomInt()).ToString();
            newData.Description = "Abc" + (counter + GetRandomInt()).ToString();
            newData.StartDate = DateTime.UtcNow;
            newData.EndDate = null;
            return newData;
        }
        public ProjectUserModel PrepareValidProjectUserModel_Leader(int userId)
        {
            ProjectUserModel newData = new ProjectUserModel();
            newData.Id = new Guid();
            newData.UserId = userId;
            newData.ProjectRoleId = ProjectRoles.PM;
            newData.ProjectId = project.Id;
            return newData;
        }
        public ProjectUserModel PrepareValidProjectUserModel_Member(int userId)
        {
            ProjectUserModel newData = new ProjectUserModel();
            newData.Id = new Guid();
            newData.UserId = userId;
            newData.ProjectRoleId = ProjectRoles.MEMBER;
            newData.ProjectId = project.Id;
            return newData;
        }
        public UserQuarterEvaluation BuildUserQuarterEvaluation(Guid quarterEId)
        {
            UserQuarterEvaluation newData = new UserQuarterEvaluation();
            newData.Id = new Guid();
            newData.QuarterEvaluationId = quarterEId;
            newData.NoteGoodThing = "hello Long";
            newData.NoteBadThing = "hello World";
            newData.NoteOther = "hello Sky";
            newData.CreatedDate = DateTime.UtcNow.Date;
            newData.CreatedBy = user_Leader.Id;
            return newData;
        }
        private int GetRandomInt()
        {
            Random rd = new Random();
            return rd.Next(1, 99999);
        }
        private int GetYearQuarter(DateTime date)
        {
            if (date.Month >= 4 && date.Month <= 6)
                return 2;
            else if (date.Month >= 7 && date.Month <= 9)
                return 3;
            else if (date.Month >= 10 && date.Month <= 12)
                return 4;
            else
                return 1;
        }
        public string AppendLetterToString(string str, int numberOfLength)
        {
            StringBuilder sb = new StringBuilder();
            if ((numberOfLength - str.Length) > 0)
            {
                sb.Append('a', (numberOfLength - str.Length));
            }
            sb.Append(str);
            return sb.ToString();
        }
    }
}



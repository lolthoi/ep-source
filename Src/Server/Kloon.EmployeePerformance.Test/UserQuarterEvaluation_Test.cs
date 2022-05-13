using Kloon.EmployeePerformance.Models.Common;
using Kloon.EmployeePerformance.Models.Project;
using Kloon.EmployeePerformance.Models.QuarterEvaluation;
using Kloon.EmployeePerformance.Models.User;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kloon.EmployeePerformance.Test
{
    [TestClass]
    public class UserEvaluation_Test : TestBase
    {
        private readonly Random _rand = new Random();
        const string _url1 = "/UserQuarterEvaluation";
        const string _url2 = "/UserQuarterEvaluation/QuarterEvaluation";
        public List<UserQuarterEvaluationModel> userQuarterEvaluation = new List<UserQuarterEvaluationModel>();
        public QuarterEvaluationModel quarterEvaluation = new QuarterEvaluationModel();
        public ProjectModel projectModel = new ProjectModel();
        public UserModel userModel = new UserModel();
        [TestInitialize]
        public void InitData()
        {
            InitUserQuarterEvaluationData();
        }

        [TestCleanup]
        public void CleanData()
        {
            ClearData();
        }


        #region CREATE
        [TestMethod]
        public void CREATE_USER_QUARTER_EVALUATION_WHEN_VALID_MODEL_THEN_SUCCESS()
        {
            var abc = BuildQuaterEvaluationModel();
            var bcd = Helper.AdminPost<QuarterEvaluationModel>(_url2, abc);
            var expectedModel = BuildUserEvaluationModel(bcd.Data.Id);
            var actualModel = Helper.AdminPost<UserQuarterEvaluationModel>(_url1, expectedModel);
            if (actualModel.Error == null)
            {
                userQuarterEvaluation.Add(actualModel.Data);
            }
            Assert.AreEqual(expectedModel.NoteGoodThing, actualModel.Data.NoteGoodThing);
            Assert.AreEqual(expectedModel.NoteBadThing, actualModel.Data.NoteBadThing);
            Assert.AreEqual(expectedModel.QuarterEvaluationId, actualModel.Data.QuarterEvaluationId);
            Assert.AreEqual(expectedModel.NoteOther, actualModel.Data.NoteOther);
        }

        #endregion

        private UserModel BuildUserModel()
        {
            var user = new UserModel
            {
                Id = new Random().Next(1, 3),
                Email = "Email" + rand() + "@kloon.vn",
                FirstName = "Firstname " + rand(),
                LastName = "Lastname " + rand(),
                PositionId = new Random().Next(1, 3),
                Sex = (SexEnum)new Random().Next(1, 2),
                DoB = DateTime.Today.AddDays(-new Random().Next(20 * 635)),
                PhoneNo = "0" + rand(),
                RoleId = new Random().Next(1, 2),
            };
            return user;
        }
        private ProjectModel BuildProjectModel()
        {
            var project = new ProjectModel
            {
                Id = new Random().Next(1, 3),
                Name = "ABC" + rand(),
                Description = "Abc" + rand(),
                StartDate = DateTime.Today.AddDays(-new Random().Next(20 * 635)),
                EndDate = DateTime.Today.AddDays(+new Random().Next(20 * 635)),
            };
            return project;
        }

        private QuarterEvaluationModel BuildQuaterEvaluationModel()
        {
            var user = BuildUserModel();
            var project = BuildProjectModel();
            var year = DateTime.UtcNow.Year;
            var evaluation = new QuarterEvaluationModel()
            {
                Id = new Guid(),
                Year = year,
                Quarter = 1,
                UserId = user.Id,
                PositionId = new Random().Next(1, 3),
                ProjectId = project.Id,
                ProjectLeaderId = new Random().Next(1, 2),
                PointAverage = new Random().Next(1, 10),
                NoteByLeader = null,
            };
            return evaluation;
        }
        private UserQuarterEvaluationModel BuildUserEvaluationModel(Guid id)
        {

            var model = new UserQuarterEvaluationModel
            {
                Id = new Guid(),
                QuarterEvaluationId = id,
                NoteGoodThing = "Good thing" + rand(),
                NoteBadThing = "Bad Thing" + rand(),
                NoteOther = "Other" + rand(),

            };
            return model;
        }

        #region InitData
        private void InitUserQuarterEvaluationData()
        {
            var quarterEvaluation = BuildQuaterEvaluationModel();

            var createEvaluation = Helper.AdminPost<QuarterEvaluationModel>(_url2, quarterEvaluation);
            if (createEvaluation.Error != null) { return; }
            var userEvaluation = BuildUserEvaluationModel(createEvaluation.Data.Id);
            var createResult = Helper.AdminPost<UserQuarterEvaluationModel>(_url1, userEvaluation);
            if (createResult.Error == null)
            {
                userQuarterEvaluation.Add(createResult.Data);
            }
        }
        #endregion
        #region CleanData
        private void ClearData()
        {

            userQuarterEvaluation.ForEach(x =>
            {
                if (x != null)
                {
                    var uri = _url1 + "/" + x.Id;
                    Helper.AdminDelete<bool>(uri);
                }

            });
        }
        #endregion
        #region Random
        public int rand()
        {
            return _rand.Next(1, 100000);
        }
        #endregion
    }
}

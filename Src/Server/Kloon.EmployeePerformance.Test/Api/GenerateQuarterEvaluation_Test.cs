using Kloon.EmployeePerformance.DataAccess;
using Kloon.EmployeePerformance.DataAccess.Domain;
using Kloon.EmployeePerformance.Logic.Services;
using Kloon.EmployeePerformance.Logic.Services.Base;
using Kloon.EmployeePerformance.Models;
using Kloon.EmployeePerformance.Models.Common;
using Kloon.EmployeePerformance.Models.Project;
using Kloon.EmployeePerformance.Models.User;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kloon.EmployeePerformance.Test.Api
{
    [TestClass]
    public class GenerateQuarterEvaluation_Test : TestBase
    {
        private readonly Random _rand = new();
        const string _url_user = "/User";
        const string _url_project = "/project";
        const string _url_qe = "/mail";
        public List<UserModel> dataUser = new();
        public List<ProjectModel> dataProject = new();
        public List<QuarterEvaluation> dataEvaluation = new();

        [TestMethod]
        public void ADMIN_GENERATE_EVALUATION_WHEN_NO_EXISTED_EVALUATION_BEFORE_THEN_SUCCESS()
        {

        }
        [TestMethod]
        public void ADMIN_GENERATE_EVALUATION_WHEN_DUPLICATED_EVALUATION_THEN_ERROR()
        {

        }
        [TestMethod]
        public void USER_GENERATE_EVALUATION_WHEN_HAVE_NO_PERMISSION_THEN_ERROR()
        {

        }

        #region User
        private void InitUserData()
        {
            var userModel = InitUserModel();
            var createUser = Helper.AdminPost<UserModel>(_url_user, userModel);
            if (createUser.Error == null)
            {
                dataUser.Add(createUser.Data);
            }
        }
        private void ClearUserData()
        {
            dataUser.ForEach(x =>
            {
                if (x != null)
                {
                    var uri = _url_user + "/" + x.Id;
                    Helper.AdminDelete<bool>(uri);
                }

            });
        }
        private UserModel InitUserModel()
        {
            var model = new UserModel
            {
                Email = "Email" + Rand() + "@kloon.vn",
                FirstName = "Firstname " + Rand(),
                LastName = "Lastname " + Rand(),
                PositionId = new Random().Next(1, 3),
                Sex = (SexEnum)new Random().Next(1, 2),
                DoB = DateTime.Today.AddDays(-new Random().Next(20 * 635)),
                PhoneNo = "0" + Rand(),
                RoleId = new Random().Next(1, 2),
            };
            return model;
        }
        #endregion

        #region Project
        private void InitProjectData()
        {
            var projectModel = InitProjectModel();
            var createProject = Helper.AdminPost<ProjectModel>(_url_project, projectModel);
            if (createProject.Error == null)
            {
                dataProject.Add(createProject.Data);
            }
        }
        private void ClearProjectData()
        {
            dataProject.ForEach(x =>
            {
                if (x != null)
                {
                    var uri = _url_project + "/" + x.Id;
                    Helper.AdminDelete<bool>(uri);
                }

            });
        }
        private ProjectModel InitProjectModel()
        {
            var model = new ProjectModel
            {
                Name = "Name " + Rand(),
                Status = (ProjectStatusEnum)new Random().Next(1, 3),
                Description = "Create Description " + Rand()
            };
            return model;
        }
        #endregion

        #region Quarter Evaluations
        private void InitQEData()
        {
            var now = DateTime.UtcNow;

            var UserModel = InitUserModel();
            var createUser = Helper.AdminPost<UserModel>(_url_user, UserModel);
            if (createUser.Error == null)
            {
                dataUser.Add(createUser.Data);
            }

            var ProjectModel = InitProjectModel();
            var createProject = Helper.AdminPost<ProjectModel>(_url_project, ProjectModel);
            if (createProject.Error == null)
            {
                dataProject.Add(createProject.Data);
            }

            var item = new QuarterEvaluation
            {
                Id = Guid.NewGuid(),
                Year = new Random().Next(2021, 2022),
                Quarter = new Random().Next(1, 4),
                UserId = UserModel.Id,
                PositionId = UserModel.PositionId,
                ProjectId = ProjectModel.Id,
                ProjectLeaderId = new Random().Next(1, 3),
                PointAverage = 0,
                CreatedDate = now,
                CreatedBy = 1,
            };



            //var createEvaluation = Helper.AdminPost<>(_url_project);
            //if (createEvaluation.Error == null)
            //{
            //    dataEvaluation.Add(createEvaluation.Data);
            //}
        }
        private void ClearQEData()
        {
            dataProject.ForEach(x =>
            {
                if (x != null)
                {
                    var uri = _url_project + "/" + x.Id;
                    Helper.AdminDelete<bool>(uri);
                }

            });
        }
        private ProjectModel InitQEModel()
        {
            var model = new ProjectModel
            {
                Name = "Name " + Rand(),
                Status = (ProjectStatusEnum)new Random().Next(1, 3),
                Description = "Create Description " + Rand()
            };
            return model;
        }
        #endregion

        #region Random func
        private int Rand()
        {
            return _rand.Next(1, 1000000);
        }
        #endregion

        #region Count Quarter
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
        private DateTime MinusQuarters(DateTime originalDate, int quarters)
        {
            return originalDate.AddMonths(quarters * -3);
        }
        private DateTime AddQuarters(DateTime originalDate, int quarters)
        {
            return originalDate.AddMonths(quarters * 3);
        }
        private DateTime GetFirstDayOfQuarter(DateTime originalDate)
        {
            return AddQuarters(new DateTime(originalDate.Year, 1, 1), GetYearQuarter(originalDate) - 1);
        }
        #endregion
    }
}

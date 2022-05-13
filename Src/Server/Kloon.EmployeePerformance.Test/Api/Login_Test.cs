using Kloon.EmployeePerformance.Models.Authentication;
using Kloon.EmployeePerformance.Models.Common;
using Kloon.EmployeePerformance.Models.User;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Kloon.EmployeePerformance.Test.Api
{
    [TestClass]
    public class Login_Test: TestBase
    {
        private readonly Random _rand = new Random();
        const string _url = "/Account";
        public List<UserModel> dataInit = new List<UserModel>();
        private UserModel tempUserCreated = new UserModel();

        [TestInitialize]
        public void InitData()
        {
            InitUserData();
        }

        [TestCleanup]
        public void CleanData()
        {
            ClearUserData();
        }

        [TestMethod]
        public void USERLOGIN_INVALIDMODEL_THENSUCCESS()
        {
            var loginModel = new LoginModel
            {
                Email = tempUserCreated.Email,
                Password = "123456",
                RememberMe = true
            };

            var actual = Helper.AdminPost<LoginResult>(_url, loginModel);

            Assert.AreEqual(tempUserCreated.Email, actual.Data.Email);
            Assert.AreEqual(tempUserCreated.FirstName, actual.Data.FirstName);
            Assert.AreEqual(tempUserCreated.LastName, actual.Data.LastName);
            Assert.AreEqual(tempUserCreated.RoleId, (int)actual.Data.AppRole);
            Assert.AreEqual(true, actual.Data.IsSuccessful);
        }

        [TestMethod]
        public void USERLOGIN_INVALIDPASSWORD_THENERROR()
        {
            var loginModel = new LoginModel
            {
                Email = tempUserCreated.Email,
                Password = "123456GFHFGH",
                RememberMe = true
            };

            var actual = Helper.AdminPost<LoginResult>(_url, loginModel);

            Assert.IsNull(actual.Data);
            Assert.AreEqual("\"WRONG PASSWORD\"", actual.Error.Message.ToString());
        }

        [TestMethod]
        public void USERLOGIN_INVALIDUSERNAME_THENERROR()
        {
            var loginModel = new LoginModel
            {
                Email = tempUserCreated.Email + "aaaaaaaaa",
                Password = "123456",
                RememberMe = true
            };

            var actual = Helper.AdminPost<LoginResult>(_url, loginModel);
            Assert.IsNull(actual.Data);
            //Assert.AreEqual(ErrorType.BAD_REQUEST, actual.Error.StatusCode);
            Assert.AreEqual("\"CANT NOT IDENTITY USER\"", actual.Error.Message);
        }

        #region Init User
        private void InitUserData()
        {
            tempUserCreated = InitUserModel();
            var createResult = Helper.AdminPost<UserModel>("/user", tempUserCreated);
            if (createResult.Error == null)
            {
                dataInit.Add(createResult.Data);
            }
        }
        private void ClearUserData()
        {
            dataInit.ForEach(x =>
            {
                var uri = _url + "/" + x.Id;
                Helper.AdminDelete<bool>(uri);
            });
        }
        private UserModel InitUserModel()
        {
            var model = new UserModel
            {
                Email = "Email" + rand() + "@kloon.vn",
                FirstName = "Firstname " + rand(),
                LastName = "Lastname " + rand(),
                PositionId = new Random().Next(1, 3),
                Sex = (SexEnum)new Random().Next(1, 2),
                DoB = DateTime.Today.AddDays(-new Random().Next(20 * 635)),
                PhoneNo = "0" + rand(),
                RoleId = new Random().Next(1, 2),
            };
            return model;
        }
        #endregion

        #region Random func
        private int rand()
        {
            return _rand.Next(1, 1000000);
        }
        #endregion
    }
}

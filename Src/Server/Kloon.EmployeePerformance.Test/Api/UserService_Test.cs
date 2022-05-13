using Kloon.EmployeePerformance.Models.Common;
using Kloon.EmployeePerformance.Models.User;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kloon.EmployeePerformance.Test.Api
{
    [TestClass]
    public class UserService_Test : TestBase
    {
        private readonly Random _rand = new Random();
        const string _url = "/User";
        public List<UserModel> dataInit = new List<UserModel>();

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

        #region ADD
        [TestMethod]
        public void ADMIN_ADD_USER_WHEN_VALID_DATA_THEN_SUCCESS()
        {
            var expectedModel = InitUserModel();
            var actualModel = Helper.AdminPost<UserModel>(_url, expectedModel);
            if (actualModel.Error == null)
            {
                dataInit.Add(actualModel.Data);
            }
            Assert.AreEqual(expectedModel.Email, actualModel.Data.Email);
            Assert.AreEqual(expectedModel.FirstName, actualModel.Data.FirstName);
            Assert.AreEqual(expectedModel.LastName, actualModel.Data.LastName);
            Assert.AreEqual(expectedModel.PositionId, actualModel.Data.PositionId);
            Assert.AreEqual(expectedModel.Sex, actualModel.Data.Sex);
            Assert.AreEqual(expectedModel.DoB, actualModel.Data.DoB);
            Assert.AreEqual(expectedModel.PhoneNo, actualModel.Data.PhoneNo);
            Assert.AreEqual(expectedModel.RoleId, actualModel.Data.RoleId);
        }
        [TestMethod]
        public void ADMIN_ADD_USER_WHEN_INVALID_EMAIL_THEN_ERROR()
        {
            var expectedModel = InitUserModel();
            expectedModel.Email = null;
            var actualModel = Helper.AdminPost<UserModel>(_url, expectedModel);
            dataInit.Add(actualModel.Data);
            var errorMess = JsonConvert.DeserializeObject<string>(actualModel.Error.Message);
            Assert.AreEqual("INVALID_MODEL_EMAIL_NULL", errorMess);
            Assert.IsFalse(actualModel.IsSuccess);
            Assert.IsNotNull(actualModel.Error);
        }
        [TestMethod]
        public void ADMIN_ADD_USER_WHEN_INVALID_EMAIL_LENGTH_THEN_ERROR()
        {
            var expectedModel = InitUserModel();
            expectedModel.Email = "asdfghjklqwertyuiopasdfghjklzxcvbndfwruuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuhzdhmlsdje3@kloon.vn";
            var actualModel = Helper.AdminPost<UserModel>(_url, expectedModel);
            dataInit.Add(actualModel.Data);
            var errorMess = JsonConvert.DeserializeObject<string>(actualModel.Error.Message);
            Assert.AreEqual("INVALID_MODEL_EMAIL_MAX_LENGTH", errorMess);
            Assert.IsFalse(actualModel.IsSuccess);
            Assert.IsNotNull(actualModel.Error);
        }
        [TestMethod]
        public void ADMIN_ADD_USER_WHEN_INVALID_EMAIL_WRONG_FORMAT_THEN_ERROR()
        {
            var expectedModel = InitUserModel();
            expectedModel.Email = "xcvbnmlsdje3@gmaildASfcoASfm";
            var actualModel = Helper.AdminPost<UserModel>(_url, expectedModel);
            dataInit.Add(actualModel.Data);
            var errorMess = JsonConvert.DeserializeObject<string>(actualModel.Error.Message);
            Assert.AreEqual("INVALID_MODEL_EMAIL_FORMAT_WRONG", errorMess);
            Assert.IsFalse(actualModel.IsSuccess);
            Assert.IsNotNull(actualModel.Error);
        }
        [TestMethod]
        public void ADMIN_ADD_USER_WHEN_INVALID_EMAIL_DUPLICATE_THEN_ERROR()
        {
            var expectedModel = InitUserModel();
            expectedModel.Email = dataInit.FirstOrDefault().Email;
            var actualModel = Helper.AdminPost<UserModel>(_url, expectedModel);
            dataInit.Add(actualModel.Data);
            var errorMess = JsonConvert.DeserializeObject<string>(actualModel.Error.Message);
            Assert.AreEqual("INVALID_MODEL_DUPLICATED_EMAIL", errorMess);
            Assert.IsFalse(actualModel.IsSuccess);
            Assert.IsNotNull(actualModel.Error);
        }
        [TestMethod]
        public void ADMIN_ADD_USER_WHEN_INVALID_FIRST_NAME_THEN_ERROR()
        {
            var expectedModel = InitUserModel();
            expectedModel.FirstName = null;
            var actualModel = Helper.AdminPost<UserModel>(_url, expectedModel);
            dataInit.Add(actualModel.Data);
            var errorMess = JsonConvert.DeserializeObject<string>(actualModel.Error.Message);
            Assert.AreEqual("INVALID_MODEL_FIRST_NAME_NULL", errorMess);
            Assert.IsFalse(actualModel.IsSuccess);
            Assert.IsNotNull(actualModel.Error);
        }
        [TestMethod]
        public void ADMIN_ADD_USER_WHEN_INVALID_FIRST_NAME_MAX_LENGTH_THEN_ERROR()
        {
            var expectedModel = InitUserModel();
            expectedModel.FirstName = "asdfghjklpoaerjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjajkhlrgaoiregiaortubamroptuiaoprbjyariouyoiiuytrewqasf";
            var actualModel = Helper.AdminPost<UserModel>(_url, expectedModel);
            dataInit.Add(actualModel.Data);
            var errorMess = JsonConvert.DeserializeObject<string>(actualModel.Error.Message);
            Assert.AreEqual("INVALID_MODEL_FIRST_NAME_MAX_LENGTH", errorMess);
            Assert.IsFalse(actualModel.IsSuccess);
            Assert.IsNotNull(actualModel.Error);
        }
        [TestMethod]
        public void ADMIN_ADD_USER_WHEN_INVALID_LAST_NAME_THEN_ERROR()
        {
            var expectedModel = InitUserModel();
            expectedModel.LastName = null;
            var actualModel = Helper.AdminPost<UserModel>(_url, expectedModel);
            dataInit.Add(actualModel.Data);
            var errorMess = JsonConvert.DeserializeObject<string>(actualModel.Error.Message);
            Assert.AreEqual("INVALID_MODEL_LAST_NAME_NULL", errorMess);
            Assert.IsFalse(actualModel.IsSuccess);
            Assert.IsNotNull(actualModel.Error);
        }
        [TestMethod]
        public void ADMIN_ADD_USER_WHEN_INVALID_LAST_NAME_MAX_LENGTH_THEN_ERROR()
        {
            var expectedModel = InitUserModel();
            expectedModel.LastName = "asdfghjklpoiuytreoi;hrgpoijiotguuWEJHYTIOP/t4ojhieoth[E49TYIOPGYHT09gj[4o[gjwqasf";
            var actualModel = Helper.AdminPost<UserModel>(_url, expectedModel);
            dataInit.Add(actualModel.Data);
            var errorMess = JsonConvert.DeserializeObject<string>(actualModel.Error.Message);
            Assert.AreEqual("INVALID_MODEL_LAST_NAME_MAX_LENGTH", errorMess);
            Assert.IsFalse(actualModel.IsSuccess);
            Assert.IsNotNull(actualModel.Error);
        }
        [TestMethod]
        public void ADMIN_ADD_USER_WHEN_INVALID_PHONE_LENGTH_THEN_ERROR()
        {
            var expectedModel = InitUserModel();
            expectedModel.PhoneNo = "098765432123645";
            var actualModel = Helper.AdminPost<UserModel>(_url, expectedModel);
            dataInit.Add(actualModel.Data);
            var errorMess = JsonConvert.DeserializeObject<string>(actualModel.Error.Message);
            Assert.AreEqual("INVALID_MODEL_PHONE_MAX_LENGTH", errorMess);
            Assert.IsFalse(actualModel.IsSuccess);
            Assert.IsNotNull(actualModel.Error);
        }
        [TestMethod]
        public void ADMIN_ADD_USER_WHEN_INVALID_POSITION_NULL_THEN_ERROR()
        {
            var expectedModel = InitUserModel();
            expectedModel.PositionId = 34;
            var actualModel = Helper.AdminPost<UserModel>(_url, expectedModel);
            dataInit.Add(actualModel.Data);
            var errorMess = JsonConvert.DeserializeObject<string>(actualModel.Error.Message);
            Assert.AreEqual("INVALID_MODEL_POSITION_NULL", errorMess);
            Assert.IsFalse(actualModel.IsSuccess);
            Assert.IsNotNull(actualModel.Error);
        }
        [TestMethod]
        public void ADMIN_ADD_USER_WHEN_INVALID_SEX_NULL_THEN_ERROR()
        {
            var expectedModel = InitUserModel();
            expectedModel.Sex = (SexEnum)3;
            var actualModel = Helper.AdminPost<UserModel>(_url, expectedModel);
            dataInit.Add(actualModel.Data);
            var errorMess = JsonConvert.DeserializeObject<string>(actualModel.Error.Message);
            Assert.AreEqual("INVALID_MODEL_SEX_NULL", errorMess);
            Assert.IsFalse(actualModel.IsSuccess);
            Assert.IsNotNull(actualModel.Error);
        }
        [TestMethod]
        public void ADMIN_ADD_USER_WHEN_INVALID_ROLE_NULL_THEN_ERROR()
        {
            var expectedModel = InitUserModel();
            expectedModel.RoleId = 4;
            var actualModel = Helper.AdminPost<UserModel>(_url, expectedModel);
            dataInit.Add(actualModel.Data);
            var errorMess = JsonConvert.DeserializeObject<string>(actualModel.Error.Message);
            Assert.AreEqual("INVALID_MODEL_ROLE_NULL", errorMess);
            Assert.IsFalse(actualModel.IsSuccess);
            Assert.IsNotNull(actualModel.Error);
        }
        [TestMethod]
        public void USER_ADD_USER_WHEN_HAVE_NO_PERMISSION_THEN_ERROR()
        {
            var expectedModel = InitUserModel();
            var actualModel = Helper.UserPost<UserModel>(_url, expectedModel);
            var errorMess = JsonConvert.DeserializeObject<string>(actualModel.Error.Message);
            Assert.IsNull(actualModel.Data);
            Assert.IsFalse(actualModel.IsSuccess);
            Assert.AreEqual("No Role", errorMess);
        }
        #endregion

        #region EDIT
        [TestMethod]
        public void ADMIN_EDIT_USER_WHEN_VALID_DATA_THEN_SUCCESS()
        {
            var item = dataInit.FirstOrDefault();
            var getResult = Helper.AdminGet<UserModel>($"{_url}/{item.Id}");

            var editModel = getResult.Data;

            var model = new UserModel()
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

            editModel.Email = model.Email;
            editModel.FirstName = model.FirstName;
            editModel.LastName = model.LastName;
            editModel.PositionId = model.PositionId;
            editModel.Sex = model.Sex;
            editModel.DoB = model.DoB;
            editModel.PhoneNo = model.PhoneNo;
            editModel.RoleId = model.RoleId;

            var editResult = Helper.AdminPut<UserModel>(_url, editModel);

            Assert.IsTrue(editResult.IsSuccess);
            Assert.AreEqual(model.Email, editResult.Data.Email);
            Assert.AreEqual(model.FirstName, editResult.Data.FirstName);
            Assert.AreEqual(model.LastName, editResult.Data.LastName);
            Assert.AreEqual(model.PositionId, editResult.Data.PositionId);
            Assert.AreEqual(model.Sex, editResult.Data.Sex);
            Assert.AreEqual(model.DoB, editResult.Data.DoB);
            Assert.AreEqual(model.PhoneNo, editResult.Data.PhoneNo);
            Assert.AreEqual(model.RoleId, editResult.Data.RoleId);
        }
        [TestMethod]
        public void ADMIN_EDIT_USER_WHEN_INVALID_EMAIL_THEN_ERROR()
        {
            var item = dataInit.FirstOrDefault();
            var getResult = Helper.AdminGet<UserModel>($"{_url}/{item.Id}");
            var editModel = getResult.Data;
            editModel.Email = null;
            var editResult = Helper.AdminPut<UserModel>(_url, editModel);
            var errorMess = JsonConvert.DeserializeObject<string>(editResult.Error.Message);
            Assert.AreEqual("INVALID_MODEL_EMAIL_NULL", errorMess);
        }
        [TestMethod]
        public void ADMIN_EDIT_USER_WHEN_INVALID_EMAIL_LENGTH_THEN_ERROR()
        {
            var item = dataInit.FirstOrDefault();
            var getResult = Helper.AdminGet<UserModel>($"{_url}/{item.Id}");
            var editModel = getResult.Data;
            editModel.Email = "asdfghjklqwertyuiopasdfghjklzxcvbndfwruuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuhzdhmlsdje3@kloon.vn"; ;
            var editResult = Helper.AdminPut<UserModel>(_url, editModel);
            var errorMess = JsonConvert.DeserializeObject<string>(editResult.Error.Message);
            Assert.AreEqual("INVALID_MODEL_EMAIL_MAX_LENGTH", errorMess);
        }
        [TestMethod]
        public void ADMIN_EDIT_USER_WHEN_INVALID_EMAIL_WRONG_FORMAT_THEN_ERROR()
        {
            var item = dataInit.FirstOrDefault();
            var getResult = Helper.AdminGet<UserModel>($"{_url}/{item.Id}");
            var editModel = getResult.Data;
            editModel.Email = "ahgiospg@gmail.comDGh"; ;
            var editResult = Helper.AdminPut<UserModel>(_url, editModel);
            var errorMess = JsonConvert.DeserializeObject<string>(editResult.Error.Message);
            Assert.AreEqual("INVALID_MODEL_EMAIL_FORMAT_WRONG", errorMess);
        }
        [TestMethod]
        public void ADMIN_EDIT_USER_WHEN_INVALID_EMAIL_DUPLICATE_THEN_ERROR()
        {
            var expectedModel = InitUserModel();
            var actualModel = Helper.AdminPost<UserModel>(_url, expectedModel);
            if (actualModel.Error == null)
            {
                dataInit.Add(actualModel.Data);
            }
            var actualItem = dataInit.Last();

            var item = dataInit.First();
            var getResult = Helper.AdminGet<UserModel>($"{_url}/{item.Id}");
            var editModel = getResult.Data;

            editModel.Email = actualItem.Email;
            var editResult = Helper.AdminPut<UserModel>(_url, editModel);
            var errorMess = JsonConvert.DeserializeObject<string>(editResult.Error.Message);
            Assert.AreEqual("INVALID_MODEL_DUPLICATED_EMAIL", errorMess);
        }
        [TestMethod]
        public void ADMIN_EDIT_USER_WHEN_INVALID_FIRST_NAME_THEN_ERROR()
        {
            var item = dataInit.FirstOrDefault();
            var getResult = Helper.AdminGet<UserModel>($"{_url}/{item.Id}");
            var editModel = getResult.Data;
            editModel.FirstName = null;
            var editResult = Helper.AdminPut<UserModel>(_url, editModel);
            var errorMess = JsonConvert.DeserializeObject<string>(editResult.Error.Message);
            Assert.AreEqual("INVALID_MODEL_FIRST_NAME_NULL", errorMess);
        }
        [TestMethod]
        public void ADMIN_EDIT_USER_WHEN_INVALID_FIRST_NAME_MAX_LENGTH_THEN_ERROR()
        {
            var item = dataInit.FirstOrDefault();
            var getResult = Helper.AdminGet<UserModel>($"{_url}/{item.Id}");
            var editModel = getResult.Data;
            editModel.FirstName = "asdfghjklpoaerjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjajkhlrgaoiregiaortubamroptuiaoprbjyariouyoiiuytrewqasf";
            var editResult = Helper.AdminPut<UserModel>(_url, editModel);
            var errorMess = JsonConvert.DeserializeObject<string>(editResult.Error.Message);
            Assert.AreEqual("INVALID_MODEL_FIRST_NAME_MAX_LENGTH", errorMess);
        }
        [TestMethod]
        public void ADMIN_EDIT_USER_WHEN_INVALID_LAST_NAME_THEN_ERROR()
        {
            var item = dataInit.FirstOrDefault();
            var getResult = Helper.AdminGet<UserModel>($"{_url}/{item.Id}");
            var editModel = getResult.Data;
            editModel.LastName = null;
            var editResult = Helper.AdminPut<UserModel>(_url, editModel);
            var errorMess = JsonConvert.DeserializeObject<string>(editResult.Error.Message);
            Assert.AreEqual("INVALID_MODEL_LAST_NAME_NULL", errorMess);
        }
        [TestMethod]
        public void ADMIN_EDIT_USER_WHEN_INVALID_LAST_NAME_MAX_LENGTH_THEN_ERROR()
        {
            var item = dataInit.FirstOrDefault();
            var getResult = Helper.AdminGet<UserModel>($"{_url}/{item.Id}");
            var editModel = getResult.Data;
            editModel.LastName = "asdfghjklpoaerjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjajkhlrgaoiregiaortubamroptuiaoprbjyariouyoiiuytrewqasf";
            var editResult = Helper.AdminPut<UserModel>(_url, editModel);
            var errorMess = JsonConvert.DeserializeObject<string>(editResult.Error.Message);
            Assert.AreEqual("INVALID_MODEL_LAST_NAME_MAX_LENGTH", errorMess);
        }
        [TestMethod]
        public void ADMIN_EDIT_USER_WHEN_INVALID_PHONE_LENGTH_THEN_ERROR()
        {
            var item = dataInit.FirstOrDefault();
            var getResult = Helper.AdminGet<UserModel>($"{_url}/{item.Id}");
            var editModel = getResult.Data;
            editModel.PhoneNo = "65461265464646";
            var editResult = Helper.AdminPut<UserModel>(_url, editModel);
            var errorMess = JsonConvert.DeserializeObject<string>(editResult.Error.Message);
            Assert.AreEqual("INVALID_MODEL_PHONE_MAX_LENGTH", errorMess);
        }
        [TestMethod]
        public void ADMIN_EDIT_USER_WHEN_INVALID_POSITION_NULL_THEN_ERROR()
        {
            var item = dataInit.FirstOrDefault();
            var getResult = Helper.AdminGet<UserModel>($"{_url}/{item.Id}");
            var editModel = getResult.Data;
            editModel.PositionId = 34;
            var editResult = Helper.AdminPut<UserModel>(_url, editModel);
            var errorMess = JsonConvert.DeserializeObject<string>(editResult.Error.Message);
            Assert.AreEqual("INVALID_MODEL_POSITION_NULL", errorMess);
        }
        [TestMethod]
        public void ADMIN_EDIT_USER_WHEN_INVALID_SEX_NULL_THEN_ERROR()
        {
            var item = dataInit.FirstOrDefault();
            var getResult = Helper.AdminGet<UserModel>($"{_url}/{item.Id}");
            var editModel = getResult.Data;
            editModel.Sex = (SexEnum)5;
            var editResult = Helper.AdminPut<UserModel>(_url, editModel);
            var errorMess = JsonConvert.DeserializeObject<string>(editResult.Error.Message);
            Assert.AreEqual("INVALID_MODEL_SEX_NULL", errorMess);
        }
        [TestMethod]
        public void ADMIN_EDIT_USER_WHEN_INVALID_ROLE_NULL_THEN_ERROR()
        {
            var item = dataInit.FirstOrDefault();
            var getResult = Helper.AdminGet<UserModel>($"{_url}/{item.Id}");
            var editModel = getResult.Data;
            editModel.RoleId = 4;
            var editResult = Helper.AdminPut<UserModel>(_url, editModel);
            var errorMess = JsonConvert.DeserializeObject<string>(editResult.Error.Message);
            Assert.AreEqual("INVALID_MODEL_ROLE_NULL", errorMess);
        }
        [TestMethod]
        public void USER_EDIT_USER_WHEN_HAVE_NO_PERMISSION_THEN_ERROR()
        {
            var item = dataInit.FirstOrDefault();
            var getResult = Helper.AdminGet<UserModel>($"{_url}/{item.Id}");
            var editModel = getResult.Data;
            editModel.FirstName = "ueser have no permission";
            var editResult = Helper.UserPost<UserModel>(_url, editModel);
            var errorMess = JsonConvert.DeserializeObject<string>(editResult.Error.Message);
            Assert.IsNull(editResult.Data);
            Assert.IsFalse(editResult.IsSuccess);
            Assert.AreEqual("No Role", errorMess);
        }
        #endregion

        #region GetAll
        [TestMethod]
        public void Admin_GetAll_When_nullsearchText_Then_Success()
        {

            var result = Helper.AdminGet<List<UserModel>>(_url);
            Assert.IsNotNull(result.Data);
            Assert.IsTrue(result.Data.Count > 0);

        }


        [TestMethod]
        public void Admin_GetAll_Search_When_ValidData_Then_Success()
        {

            var searchText = "";
            var item = dataInit.FirstOrDefault();
            if (item != null)
                searchText = item.FirstName + item.LastName + item.Email;
            var url = $"{_url}?key ={ searchText}";
            var result = Helper.AdminGet<List<UserModel>>(url);

            Assert.IsNotNull(result.Data);
            Assert.IsTrue(result.Data.Count > 0);

        }


        [TestMethod]
        public void User_GetAll_Then_Success()
        {
            var result = Helper.UserGet<List<UserModel>>(_url);

            Assert.IsNotNull(result.Data);
            Assert.IsFalse(result.Data.Count > 2);

        }

        #endregion


        #region GetbyId
        [TestMethod]
        public void Admin_GetById_ValidUserId_Success()
        {
            var item = dataInit.FirstOrDefault();
            var getResult = Helper.AdminGet<UserModel>($"{_url}/{item.Id}");
            var resultItem = getResult.Data;


            Assert.IsNotNull(resultItem);

            Assert.AreEqual(item.Id, resultItem.Id);


        }
        [TestMethod]
        public void Admin_GetById_InvalidUserModel_Error()
        {
            var item = dataInit.FirstOrDefault();

            var getResult = Helper.AdminGet<UserModel>($"{_url}/{item.Id}");
            var itemDelete = Helper.AdminDelete<bool>($"{_url}/{item.Id}");
            var getResult2 = Helper.AdminGet<UserModel>($"{_url}/{item.Id}");


            var errorMess = JsonConvert.DeserializeObject<string>(getResult2.Error.Message);

            Assert.AreEqual("User not found", errorMess);

        }

        [TestMethod]
        public void User_GetById_ValidUserId_Success()
        {

            var item = dataInit.FirstOrDefault();
            var getResult = Helper.UserGet<UserModel>($"{_url}/{item.Id}");
            var resultItem = getResult.Data;
            Assert.IsNotNull(resultItem);
            Assert.AreEqual(item.Id, resultItem.Id);
        }

        [TestMethod]
        public void User_GetById_InvalidUserId_Error()
        {
            var item = dataInit.FirstOrDefault();
            item.Id = 0;
            var getResult = Helper.UserGet<UserModel>($"{_url}/{item.Id}");
            var errorMess = JsonConvert.DeserializeObject<string>(getResult.Error.Message);
            Assert.IsNotNull(errorMess);
            Assert.AreEqual("User not found", errorMess);
        }
        #endregion


        #region Delete
        [TestMethod]
        public void Admin_Delete_User_When_ValidModel_Then_Success()
        {
            var item = dataInit.FirstOrDefault();

            var getResult = Helper.AdminGet<UserModel>($"{_url}/{item.Id}");
            Assert.IsNotNull(getResult.Data);


            var deleteResult = Helper.AdminDelete<bool>($"{_url}/{item.Id}");
            Assert.IsTrue(deleteResult.Data);

            var itemDelete = Helper.AdminGet<UserModel>($"{_url}/{item.Id}");
            var errorMess = JsonConvert.DeserializeObject<string>(itemDelete.Error.Message);
            Assert.IsFalse(itemDelete.IsSuccess);
            Assert.AreEqual("User not found", errorMess);
        }

        [TestMethod]
        public void User_Delete_User_NoPermission_Fail()
        {
            var item = dataInit.FirstOrDefault();

            var getResult = Helper.UserGet<UserModel>($"{_url}/{item.Id}");
            Assert.IsNotNull(getResult.Data);


            var deleteResult = Helper.UserDelete<bool>($"{_url}/{item.Id}");
            var errorMess = JsonConvert.DeserializeObject<string>(deleteResult.Error.Message);
            Assert.IsFalse(deleteResult.Data);

            Assert.AreEqual("No Role", errorMess);
        }
        #endregion

        #region Init User
        private void InitUserData()
        {
            var userModel = InitUserModel();
            var createResult = Helper.AdminPost<UserModel>(_url, userModel);
            if (createResult.Error == null)
            {
                dataInit.Add(createResult.Data);
            }
        }
        private void ClearUserData()
        {
            dataInit.ForEach(x =>
            {
                if (x != null)
                {
                    var uri = _url + "/" + x.Id;
                    Helper.AdminDelete<bool>(uri);
                }

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

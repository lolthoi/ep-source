using Kloon.EmployeePerformance.Logic.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kloon.EmployeePerformance.Models.User;
using Kloon.EmployeePerformance.Logic.Caches;
using Kloon.EmployeePerformance.Models.Common;
using Newtonsoft.Json;

namespace Kloon_EmployeePerformance_UnitTest.Services
{
    [TestClass]
    public class UserService_Test : TestBase
    {
        #region VARIABLE AND INIT

        private IUserService _userService;
        private CacheProvider _cache;

        private const string testDataUserDisplayName = "TEST DATA DISPLAYNAME";
        private const string testDataEmail = "testemail";

        private UserModel _randomUser;

        protected override void CleanEnvirontment()
        {

        }

        protected override void InitEnvirontment()
        {
            _testService
                .StartLoginWithAdmin()
                .ThenImplementTest(param =>
                {
                    PrepareDataForUnitTest();
                });
        }

        protected override void InitServices()
        {
            _userService = _scope.ServiceProvider.GetService<IUserService>();
            _cache = _scope.ServiceProvider.GetService<CacheProvider>();
        }
        #endregion

        #region FUNCTION CREATE

        [TestMethod]
        public void AdminRoleCreate_ValidData_Success()
        {
            _testService
                .StartLoginWithAdmin()
                .ThenImplementTest(param =>
                {
                    //Arrange
                    var model = PrepareValidUserModel(1);

                    //Act
                    var actual = _userService.Create(model);

                    Assert.IsNotNull(actual.Data);
                    param.CleanData.Add("CreateModel", actual.Data);

                    //Assert
                    var expectedModel = _userService.GetById(actual.Data.Id).Data;
                    Assert.IsNotNull(expectedModel);

                    Assert.AreEqual(expectedModel.Id, actual.Data.Id);
                    Assert.AreEqual(expectedModel.FirstName, actual.Data.FirstName);
                    Assert.AreEqual(expectedModel.LastName, actual.Data.LastName);
                    Assert.AreEqual(expectedModel.PhoneNo, actual.Data.PhoneNo);
                    Assert.AreEqual(expectedModel.PositionId, actual.Data.PositionId);
                    Assert.AreEqual(expectedModel.RoleId, actual.Data.RoleId);
                    Assert.AreEqual(expectedModel.Status, actual.Data.Status);
                    Assert.AreEqual(expectedModel.Sex, actual.Data.Sex);

                })
                .ThenCleanDataTest(param =>
                {
                    var createModel = param.CleanData.Get<UserModel>("CreateModel");
                    if (createModel != null)
                    {
                        _userService.Delete(createModel.Id);
                    }
                });
        }

        [TestMethod]
        public void AdminRoleCreate_InValidData_Success()
        {
            _testService
                .StartLoginWithAdmin()
                .ThenImplementTest(param =>
                {
                    //Arrange
                    var firstNameNullModel = PrepareValidUserModel(1);
                    firstNameNullModel.FirstName = null;

                    var firstNameMaxLength = PrepareValidUserModel(2);
                    firstNameMaxLength.FirstName = AppendSpaceToString(firstNameMaxLength.FirstName, 60);

                    var lastNameNullModel = PrepareValidUserModel(3);
                    lastNameNullModel.LastName = null;

                    var lastNameMaxLengthModel = PrepareValidUserModel(4);
                    lastNameMaxLengthModel.LastName = AppendSpaceToString(lastNameMaxLengthModel.LastName, 60);

                    var emailNullModel = PrepareValidUserModel(5);
                    emailNullModel.Email = null;

                    var emailMaxLengthModel = PrepareValidUserModel(6);
                    emailMaxLengthModel.Email = AppendLetterToString(emailMaxLengthModel.Email, 100);

                    var emailEmailWrongFormat = PrepareValidUserModel(7);
                    emailEmailWrongFormat.Email = "wrongFormat@gmail.com";

                    var invalidPositionModel = PrepareValidUserModel(8);
                    invalidPositionModel.PositionId = _cache.Position.GetValues().LastOrDefault().Id + GetRandomInt();

                    var invalidRoleModel = PrepareValidUserModel(9);
                    invalidRoleModel.RoleId = 3;

                    Dictionary<string, UserModel> invalidModels = new Dictionary<string, UserModel>();
                    invalidModels.Add(nameof(firstNameNullModel), firstNameNullModel);
                    invalidModels.Add(nameof(firstNameMaxLength), firstNameMaxLength);
                    invalidModels.Add(nameof(lastNameNullModel), lastNameNullModel);
                    invalidModels.Add(nameof(lastNameMaxLengthModel), lastNameMaxLengthModel);
                    invalidModels.Add(nameof(emailNullModel), emailNullModel);
                    invalidModels.Add(nameof(emailMaxLengthModel), emailMaxLengthModel);
                    invalidModels.Add(nameof(emailEmailWrongFormat), emailEmailWrongFormat);
                    invalidModels.Add(nameof(invalidPositionModel), invalidPositionModel);
                    invalidModels.Add(nameof(invalidRoleModel), invalidRoleModel);

                    //Act

                    //Assert

                    foreach (var invalidModel in invalidModels)
                    {
                        var actualModel = _userService.Create(invalidModel.Value); ;

                        Assert.IsNotNull(actualModel);
                        Assert.IsNull(actualModel.Data);

                        switch (invalidModel.Key)
                        {
                            case nameof(firstNameNullModel):
                                Assert.AreEqual("INVALID_MODEL_FIRST_NAME_NULL", actualModel.Error.Message);
                                break;
                            case nameof(firstNameMaxLength):
                                Assert.AreEqual("INVALID_MODEL_FIRST_NAME_MAX_LENGTH", actualModel.Error.Message);
                                break;
                            case nameof(lastNameNullModel):
                                Assert.AreEqual("INVALID_MODEL_LAST_NAME_NULL", actualModel.Error.Message);
                                break;
                            case nameof(lastNameMaxLengthModel):
                                Assert.AreEqual("INVALID_MODEL_LAST_NAME_MAX_LENGTH", actualModel.Error.Message);
                                break;
                            case nameof(emailNullModel):
                                Assert.AreEqual("INVALID_MODEL_EMAIL_NULL", actualModel.Error.Message);
                                break;
                            case nameof(emailMaxLengthModel):
                                Assert.AreEqual("INVALID_MODEL_EMAIL_MAX_LENGTH", actualModel.Error.Message);
                                break;
                            case nameof(emailEmailWrongFormat):
                                Assert.AreEqual("INVALID_MODEL_EMAIL_FORMAT_WRONG", actualModel.Error.Message);
                                break;
                            case nameof(invalidPositionModel):
                                Assert.AreEqual("INVALID_MODEL_POSITION_NULL", actualModel.Error.Message);
                                break;
                            case nameof(invalidRoleModel):
                                Assert.AreEqual("INVALID_MODEL_ROLE_NULL", actualModel.Error.Message);
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
            _testService
               .StartLoginWithUser(_accountSetting.User.UserName, _accountSetting.User.Password)
               .ThenImplementTest(param =>
               {
                   //Arrange

                   UserModel model = PrepareValidUserModel(1);

                   //Act
                   var actual = _userService.Create(model);

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

        #region FUNCTION UPDATE

        [TestMethod]
        public void AdminRoleUpdate_ValidData_Success()
        {
            _testService
                .StartLoginWithAdmin()
                .ThenImplementTest(param =>
                {
                    //Arange

                    var model = PrepareValidUserModel(1);
                    var createdData = _userService.Create(model);

                    Assert.IsNotNull(createdData.Data);
                    param.CleanData.Add("CreateModel", createdData.Data);
                    createdData.Data.FirstName = "Some random name for testing";

                    //Act
                    var actual = _userService.Update(createdData.Data);

                    //Assert
                    var expectedModel = _userService.GetById(actual.Data.Id).Data;
                    Assert.IsNotNull(expectedModel);

                    Assert.IsNotNull(actual.Data);
                    Assert.AreEqual(expectedModel.FirstName, actual.Data.FirstName);
                    Assert.AreEqual(expectedModel.LastName, actual.Data.LastName);
                    Assert.AreEqual(expectedModel.PhoneNo, actual.Data.PhoneNo);
                    Assert.AreEqual(expectedModel.PositionId, actual.Data.PositionId);
                    Assert.AreEqual(expectedModel.RoleId, actual.Data.RoleId);
                    Assert.AreEqual(expectedModel.Status, actual.Data.Status);
                    Assert.AreEqual(expectedModel.Sex, actual.Data.Sex);

                })
                .ThenCleanDataTest(param =>
                {
                    var createModel = param.CleanData.Get<UserModel>("CreateModel");
                    if (createModel != null)
                    {
                        _userService.Delete(createModel.Id);
                    }
                });
        }

        [TestMethod]
        public void AdminRoleUpdate_InvalidData_Fail()
        {
            _testService
                .StartLoginWithAdmin()
                .ThenImplementTest(param =>
                {
                    //Arange

                    var model = PrepareValidUserModel(1);
                    var createdData = _userService.Create(model);

                    Assert.IsNotNull(createdData.Data);
                    param.CleanData.Add("CreateModel", createdData.Data);

                    var firstNameNullModel = SystemExtension.Clone(createdData.Data);
                    firstNameNullModel.FirstName = null;

                    var firstNameMaxLength = SystemExtension.Clone(createdData.Data);
                    firstNameMaxLength.FirstName = AppendSpaceToString(firstNameMaxLength.FirstName, 100);

                    var lastNameNullModel = SystemExtension.Clone(createdData.Data);
                    lastNameNullModel.LastName = null;

                    var lastNameMaxLengthModel = SystemExtension.Clone(createdData.Data);
                    lastNameMaxLengthModel.LastName = AppendSpaceToString(lastNameMaxLengthModel.LastName, 100);

                    var emailNullModel = SystemExtension.Clone(createdData.Data);
                    emailNullModel.Email = null;

                    var emailMaxLengthModel = SystemExtension.Clone(createdData.Data);
                    emailMaxLengthModel.Email = AppendLetterToString(emailMaxLengthModel.Email, 100);

                    var emailEmailWrongFormat = SystemExtension.Clone(createdData.Data); ;
                    emailEmailWrongFormat.Email = "wrongFormat@gmail.com";

                    var invalidPositionModel = SystemExtension.Clone(createdData.Data);
                    invalidPositionModel.PositionId = _cache.Position.GetValues().LastOrDefault().Id + GetRandomInt();

                    var invalidRoleModel = SystemExtension.Clone(createdData.Data);
                    invalidRoleModel.RoleId = 3;

                    Dictionary<string, UserModel> invalidModels = new Dictionary<string, UserModel>();
                    invalidModels.Add(nameof(firstNameNullModel), firstNameNullModel);
                    invalidModels.Add(nameof(firstNameMaxLength), firstNameMaxLength);
                    invalidModels.Add(nameof(lastNameNullModel), lastNameNullModel);
                    invalidModels.Add(nameof(lastNameMaxLengthModel), lastNameMaxLengthModel);
                    invalidModels.Add(nameof(emailNullModel), emailNullModel);
                    invalidModels.Add(nameof(emailMaxLengthModel), emailMaxLengthModel);
                    invalidModels.Add(nameof(emailEmailWrongFormat), emailEmailWrongFormat);
                    invalidModels.Add(nameof(invalidPositionModel), invalidPositionModel);
                    invalidModels.Add(nameof(invalidRoleModel), invalidRoleModel);
                    //Act

                    //Assert

                    foreach (var invalidModel in invalidModels)
                    {
                        var actualModel = _userService.Update(invalidModel.Value); ;

                        Assert.IsNotNull(actualModel);
                        Assert.IsNull(actualModel.Data);

                        switch (invalidModel.Key)
                        {
                            case nameof(firstNameNullModel):
                                Assert.AreEqual("INVALID_MODEL_FIRST_NAME_NULL", actualModel.Error.Message);
                                break;
                            case nameof(firstNameMaxLength):
                                Assert.AreEqual("INVALID_MODEL_FIRST_NAME_MAX_LENGTH", actualModel.Error.Message);
                                break;
                            case nameof(lastNameNullModel):
                                Assert.AreEqual("INVALID_MODEL_LAST_NAME_NULL", actualModel.Error.Message);
                                break;
                            case nameof(lastNameMaxLengthModel):
                                Assert.AreEqual("INVALID_MODEL_LAST_NAME_MAX_LENGTH", actualModel.Error.Message);
                                break;
                            case nameof(emailNullModel):
                                Assert.AreEqual("INVALID_MODEL_EMAIL_NULL", actualModel.Error.Message);
                                break;
                            case nameof(emailMaxLengthModel):
                                Assert.AreEqual("INVALID_MODEL_EMAIL_MAX_LENGTH", actualModel.Error.Message);
                                break;
                            case nameof(emailEmailWrongFormat):
                                Assert.AreEqual("INVALID_MODEL_EMAIL_FORMAT_WRONG", actualModel.Error.Message);
                                break;
                            case nameof(invalidPositionModel):
                                Assert.AreEqual("INVALID_MODEL_POSITION_NULL", actualModel.Error.Message);
                                break;
                            case nameof(invalidRoleModel):
                                Assert.AreEqual("INVALID_MODEL_ROLE_NULL", actualModel.Error.Message);
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
                });
        }

        [TestMethod]
        public void UserRoleUpdate_NoPermission_Fail()
        {
            _testService
              .StartLoginWithUser(_accountSetting.User.UserName, _accountSetting.User.Password)
              .ThenImplementTest(param =>
              {
                  //Arrange

                  _randomUser.FirstName = "Somename for test";

                  //Act

                  var actual = _userService.Update(_randomUser);

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

        #region FUNCTION DELETE

        [TestMethod]
        public void AdminRoleDelete_ValidData_Success()
        {
            _testService
               .StartLoginWithAdmin()
               .ThenImplementTest(param =>
               {
                   //Arange

                   var model = PrepareValidUserModel(1);
                   var createdData = _userService.Create(model);
                   Assert.IsNotNull(createdData.Data);
                   param.CleanData.Add("CreateModel", createdData.Data);

                   //Act
                   var actual = _userService.Delete(createdData.Data.Id);

                   //Assert

                   Assert.AreEqual(true, actual.Data);

                   //check exist
                   var expectedModel = _userService.GetById(createdData.Data.Id).Data;
                   Assert.IsNull(expectedModel);


               })
               .ThenCleanDataTest(param =>
               {
                   var createModel = param.CleanData.Get<UserModel>("CreateModel");
                   if (createModel != null)
                   {
                       _userService.Delete(createModel.Id);
                   }
               });
        }

        [TestMethod]
        public void AdminRoleDelete_InvalidUserId_Fail()
        {
            _testService
              .StartLoginWithAdmin()
              .ThenImplementTest(param =>
              {
                  //Arange

                  var model = PrepareValidUserModel(1);
                  var createdData = _userService.Create(model);
                  Assert.IsNotNull(createdData.Data);
                  param.CleanData.Add("CreateModel", createdData.Data);

                  createdData.Data.Id = createdData.Data.Id + GetRandomInt();

                  //Act
                  var actual = _userService.Delete(createdData.Data.Id);

                  //Assert

                  Assert.AreEqual(false, actual.Data);
                  Assert.AreEqual("User not found", actual.Error.Message);
                  Assert.AreEqual(ErrorType.NOT_EXIST, actual.Error.Type);


              })
              .ThenCleanDataTest(param =>
              {
                  var createModel = param.CleanData.Get<UserModel>("CreateModel");
                  if (createModel != null)
                  {
                      _userService.Delete(createModel.Id);
                  }
              });
        }

        [TestMethod]
        public void AdminRoleDelete_SelfDelete_Fail()
        {
            _testService
              .StartLoginWithAdmin()
              .ThenImplementTest(param =>
              {
                  //Arange
                  var currenUserId = param.CurrentUser.User.Id;

                  //Act
                  var actual = _userService.Delete(currenUserId);

                  //Assert

                  Assert.AreEqual(false, actual.Data);
                  Assert.AreEqual("Cannot delete yourself", actual.Error.Message);
                  Assert.AreEqual(ErrorType.CONFLICTED, actual.Error.Type);


              })
              .ThenCleanDataTest(param =>
              {

              });
        }

        [TestMethod]
        public void UserRoleDelete_NoPermission_Fail()
        {
            _testService
              .StartLoginWithUser(_accountSetting.User.UserName, _accountSetting.User.Password)
              .ThenImplementTest(param =>
              {
                  //Arrange

                  _randomUser.FirstName = "Somename for test";

                  //Act

                  var actual = _userService.Delete(_randomUser.Id);

                  //Assert
                  Assert.IsNotNull(actual.Error);
                  Assert.IsFalse(actual.Data);
                  Assert.AreEqual(ErrorType.NO_ROLE, actual.Error.Type);
                  Assert.AreEqual("No Role", actual.Error.Message);

              })
              .ThenCleanDataTest(param =>
              {

              });
        }

        #endregion

        #region FUNCTION GETALL

        [TestMethod]
        public void AdminRoleGetAll_ValidDataWithEmptySearchText_Success()
        {
            _testService
                .StartLoginWithAdmin()
                .ThenImplementTest(param =>
                {
                    //Arrange
                    string searchText = "";

                    var model = PrepareValidUserModel(1);
                    var createdModel = _userService.Create(model);
                    Assert.IsNotNull(createdModel.Data);
                    param.CleanData.Add("CreateModel", createdModel.Data);

                    //Act
                    var searchResult = _userService.GetAll(searchText);
                    var actual = searchResult.Data.Where(t => t.Id == createdModel.Data.Id).FirstOrDefault();

                    //Assert
                    Assert.IsNull(searchResult.Error);
                    Assert.AreEqual(createdModel.Data.Id, actual.Id);
                    Assert.AreEqual(createdModel.Data.Email, actual.Email);
                    Assert.AreEqual(createdModel.Data.FirstName, actual.FirstName);
                    Assert.AreEqual(createdModel.Data.LastName, actual.LastName);
                    Assert.AreEqual(createdModel.Data.RoleId, actual.RoleId);
                    Assert.AreEqual(createdModel.Data.Sex, actual.Sex);
                    Assert.AreEqual(createdModel.Data.Status, actual.Status);

                })
                .ThenImplementTest(param =>
                {
                    var createModel = param.CleanData.Get<UserModel>("CreateModel");
                    if (createModel != null)
                    {
                        _userService.Delete(createModel.Id);
                    }
                });
        }

        [TestMethod]
        public void UserRoleGetAll_ValidDataWithEmptySearchText_Success()
        {
            _testService
               .StartLoginWithUser(_randomUser.Email, "123456")
               .ThenImplementTest(param =>
               {
                   //Arrange
                   string searchText = "";

                   //Act
                   var searchResult = _userService.GetAll(searchText);
                   var actual = searchResult.Data.Where(t => t.Id == param.CurrentUser.User.Id).FirstOrDefault();

                   //Assert
                   Assert.IsNotNull(actual);
                   Assert.IsNull(searchResult.Error);
                   Assert.AreEqual(1, searchResult.Data.Count);
                   Assert.AreEqual(param.CurrentUser.User.Id, actual.Id);
                   Assert.AreEqual(param.CurrentUser.User.Email, actual.Email);
                   Assert.AreEqual(param.CurrentUser.User.FirstName, actual.FirstName);
                   Assert.AreEqual(param.CurrentUser.User.LastName, actual.LastName);
                   Assert.AreEqual(param.CurrentUser.User.Role, actual.RoleId);

               })
               .ThenImplementTest(param =>
               {

               });
        }

        [TestMethod]
        public void AdminRoleGetAll_ValidDataWithValidEmailSearchText_Success()
        {
            _testService
              .StartLoginWithAdmin()
              .ThenImplementTest(param =>
              {
                  //Arrange
                  string searchText = _randomUser.Email;

                  //Act
                  var searchResult = _userService.GetAll(searchText);
                  var actual = searchResult.Data.Where(t => t.Email == searchText).FirstOrDefault();

                  //Assert
                  Assert.IsNotNull(actual);
                  Assert.IsNull(searchResult.Error);
                  Assert.AreEqual(1, searchResult.Data.Count);
                  Assert.AreEqual(_randomUser.Id, actual.Id);
                  Assert.AreEqual(_randomUser.Email, actual.Email);
                  Assert.AreEqual(_randomUser.FirstName, actual.FirstName);
                  Assert.AreEqual(_randomUser.LastName, actual.LastName);

              })
              .ThenImplementTest(param =>
              {

              });
        }

        #endregion

        #region FUNCTION GETBYID

        [TestMethod]
        public void AdminRoleGetById_ValidUserId_Success()
        {
            _testService
                .StartLoginWithAdmin()
                .ThenImplementTest(param =>
                {
                    //Arrange

                    var model = PrepareValidUserModel(1);
                    var createdModel = _userService.Create(model);
                    Assert.IsNotNull(createdModel.Data);
                    param.CleanData.Add("CreateModel", createdModel.Data);

                    //Act

                    var actual = _userService.GetById(createdModel.Data.Id);

                    //Assert

                    Assert.IsNotNull(actual.Data);
                    Assert.IsNull(actual.Error);
                    Assert.AreEqual(createdModel.Data.Id, actual.Data.Id);
                    Assert.AreEqual(createdModel.Data.FirstName, actual.Data.FirstName);
                    Assert.AreEqual(createdModel.Data.LastName, actual.Data.LastName);
                    Assert.AreEqual(createdModel.Data.Email, actual.Data.Email);
                    Assert.AreEqual(createdModel.Data.RoleId, actual.Data.RoleId);
                    Assert.AreEqual(createdModel.Data.PhoneNo, actual.Data.PhoneNo);
                    Assert.AreEqual(createdModel.Data.Status, actual.Data.Status);

                })
                .ThenCleanDataTest(param =>
                {
                    var createModel = param.CleanData.Get<UserModel>("CreateModel");
                    if (createModel != null)
                    {
                        _userService.Delete(createModel.Id);
                    }
                });
        }

        [TestMethod]
        public void UserRoleGetById_ValidUserId_Success()
        {
            _testService
                .StartLoginWithUser(_randomUser.Email, "123456")
                .ThenImplementTest(param =>
                {
                    //Arrange

                    //Act
                    var actual = _userService.GetById(param.CurrentUser.User.Id);

                    //Assert

                    Assert.IsNotNull(actual.Data);
                    Assert.IsNull(actual.Error);
                    Assert.AreEqual(param.CurrentUser.User.Id, actual.Data.Id);
                    Assert.AreEqual(param.CurrentUser.User.FirstName, actual.Data.FirstName);
                    Assert.AreEqual(param.CurrentUser.User.LastName, actual.Data.LastName);
                    Assert.AreEqual(param.CurrentUser.User.Email, actual.Data.Email);

                })
                .ThenCleanDataTest(param =>
                {
                    var createModel = param.CleanData.Get<UserModel>("CreateModel");
                    if (createModel != null)
                    {
                        _userService.Delete(createModel.Id);
                    }
                });
        }

        [TestMethod]
        public void AdminRoleGetById_InvalidUserId_Fail()
        {
            _testService
                .StartLoginWithAdmin()
                .ThenImplementTest(param =>
                {
                    //Arrange

                    var model = PrepareValidUserModel(1);
                    var createdModel = _userService.Create(model);
                    Assert.IsNotNull(createdModel.Data);
                    param.CleanData.Add("CreateModel", createdModel.Data);

                    //Act

                    var actual = _userService.GetById(createdModel.Data.Id + GetRandomInt());

                    //Assert

                    Assert.IsNull(actual.Data);
                    Assert.IsNotNull(actual.Error);
                    Assert.AreEqual("User not found", actual.Error.Message);
                    Assert.AreEqual(ErrorType.NOT_EXIST, actual.Error.Type);


                })
                .ThenCleanDataTest(param =>
                {
                    var createModel = param.CleanData.Get<UserModel>("CreateModel");
                    if (createModel != null)
                    {
                        _userService.Delete(createModel.Id);
                    }
                });
        }

        [TestMethod]
        public void UserRoleGetById_InvalidUserId_Fail()
        {
            _testService
               .StartLoginWithUser(_randomUser.Email, "123456")
               .ThenImplementTest(param =>
               {
                   //Arrange

                   //Act

                   var actual = _userService.GetById(param.CurrentUser.User.Id + GetRandomInt());

                   //Assert

                   Assert.IsNull(actual.Data);
                   Assert.IsNotNull(actual.Error);
                   Assert.AreEqual("User not found", actual.Error.Message);
                   Assert.AreEqual(ErrorType.NOT_EXIST, actual.Error.Type);


               })
               .ThenCleanDataTest(param =>
               {

               });
        }

        #endregion

        #region Private method

        public void PrepareDataForUnitTest()
        {
            var model = PrepareValidUserModel(1);
            _randomUser = _userService.Create(model).Data;
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

        private int GetRandomInt()
        {
            Random rd = new Random();
            return rd.Next(1, 99999);
        }

        public string AppendSpaceToString(string str, int numberOfLength)
        {
            StringBuilder sb = new StringBuilder();
            if ((numberOfLength - str.Length) > 0)
            {
                sb.Append(' ', (numberOfLength - str.Length));
            }
            sb.Append(str);
            return sb.ToString();
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

        #endregion
    }
}

public static class SystemExtension
{
    public static T Clone<T>(this T source)
    {
        var serialized = JsonConvert.SerializeObject(source);
        return JsonConvert.DeserializeObject<T>(serialized);
    }
}

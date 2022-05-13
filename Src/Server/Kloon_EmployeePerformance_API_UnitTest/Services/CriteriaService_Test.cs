using Kloon.EmployeePerformance.DataAccess;
using Kloon.EmployeePerformance.DataAccess.Domain;
using Kloon.EmployeePerformance.Logic.Services;
using Kloon.EmployeePerformance.Models.Common;
using Kloon.EmployeePerformance.Models.Criteria;
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
    [TestClass()]
    public class CriteriaService_Test : TestBase
    {
        private ICriteriaStoreService _criteriaService;
        private IUserService _userService;
        private IUnitOfWork<EmployeePerformanceContext> _dbContext;
        private IEntityRepository<CriteriaStore> _criteriaStores;
        private IEntityRepository<CriteriaTypeStore> _criteriaTypeStores;
        private IEntityRepository<User> _users;

        private UserModel _userModel;
        private CriteriaStoreModel _criteriaModel;
        private CriteriaStoreModel _criteriaTypeModel;

        protected override void InitServices()
        {
            _criteriaService = _scope.ServiceProvider.GetService<ICriteriaStoreService>();
            _userService = _scope.ServiceProvider.GetService<IUserService>();

            _dbContext = _scope.ServiceProvider.GetService<IUnitOfWork<EmployeePerformanceContext>>();
            _criteriaStores = _dbContext.GetRepository<CriteriaStore>();
            _criteriaTypeStores = _dbContext.GetRepository<CriteriaTypeStore>();
            _users = _dbContext.GetRepository<User>();
        }

        protected override void InitEnvirontment()
        {
            _testService.StartLoginWithAdmin()
                .ThenImplementTest(param =>
                {
                    _userModel = new UserModel()
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
                    _userModel = _userService.Create(_userModel).Data;



                    _criteriaTypeModel = InitCriteriaTypeModel();
                    _criteriaTypeModel = _criteriaService.Add(_criteriaTypeModel).Data;

                    _criteriaModel = InitCriteriaModel(_criteriaTypeModel.Id);
                    _criteriaModel = _criteriaService.Add(_criteriaModel).Data;

                });
        }

        protected override void CleanEnvirontment()
        {
            _testService.StartLoginWithAdmin()
               .ThenImplementTest(param =>
               {
                   var criterias = _criteriaStores.Query().ToList();
                   if (criterias != null && criterias.Count > 0)
                   {
                       foreach (var item in criterias)
                       {
                           _criteriaStores.Delete(item);
                       }
                   }

                   var criteriaTypes = _criteriaTypeStores.Query().ToList();
                   if (criteriaTypes != null && criterias.Count > 0)
                   {
                       foreach (var item in criteriaTypes)
                       {
                           _criteriaTypeStores.Delete(item);
                       }
                   }

                   var user = _users.Query(x => x.Id == _userModel.Id).FirstOrDefault();
                   if (user != null)
                   {
                       _users.Delete(user);
                   }
                   _dbContext.Save();
               });
        }

        #region Get All

        [TestMethod]
        public void AdminRoleGetAllCriterias_Success()
        {
            _testService.StartLoginWithAdmin()
                .ThenImplementTest(param =>
                {
                    #region Init Data

                    CriteriaStoreModel criteriaModel = InitCriteriaTypeModel();

                    var createdModel = _criteriaService.Add(criteriaModel);
                    param.CleanData.Add("CreateModel", createdModel.Data);

                    #endregion

                    #region Get and check data

                    var result = _criteriaService.GetAll("");
                    var actualModel = result.Data.Where(x => x.Id == createdModel.Data.Id).FirstOrDefault();

                    Assert.IsNotNull(result.Data);
                    Assert.IsNull(result.Error);
                    Assert.IsNotNull(actualModel);
                    Assert.AreEqual(criteriaModel.Id, actualModel.Id);

                    Assert.AreEqual(criteriaModel.Name, actualModel.Name);
                    Assert.AreEqual(criteriaModel.Description, actualModel.Description);
                    Assert.AreEqual(criteriaModel.TypeId, actualModel.TypeId);

                    #endregion
                })
                .ThenCleanDataTest(param =>
                {
                    var createModel = param.CleanData.Get<CriteriaStoreModel>("CreateModel");
                    if (createModel != null)
                    {
                        var criteriaType = _criteriaTypeStores.Query(x => x.Id == createModel.Id).FirstOrDefault();
                        if (criteriaType != null)
                        {
                            _criteriaTypeStores.Delete(criteriaType);
                        }
                    }
                });
        }

        [TestMethod]
        public void AdminRoleGetAllCriteriasWithSearch_Success()
        {
            _testService.StartLoginWithAdmin()
                .ThenImplementTest(param =>
                {
                    #region Init Data

                    CriteriaStoreModel criteriaModel = InitCriteriaTypeModel();

                    var createdModel = _criteriaService.Add(criteriaModel);
                    param.CleanData.Add("CreateModel", createdModel.Data);

                    #endregion

                    #region Get and check data

                    var result = _criteriaService.GetAll(criteriaModel.Name);
                    var actualModel = result.Data.Where(x => x.Id == createdModel.Data.Id).FirstOrDefault();

                    Assert.IsNotNull(result.Data);
                    Assert.IsNull(result.Error);
                    Assert.IsNotNull(actualModel);
                    Assert.AreEqual(criteriaModel.Id, actualModel.Id);

                    Assert.AreEqual(criteriaModel.Name, actualModel.Name);
                    Assert.AreEqual(criteriaModel.Description, actualModel.Description);
                    Assert.AreEqual(criteriaModel.TypeId, actualModel.TypeId);

                    #endregion
                })
                .ThenCleanDataTest(param =>
                {
                    var createModel = param.CleanData.Get<CriteriaStoreModel>("CreateModel");
                    if (createModel != null)
                    {
                        var criteriaType = _criteriaTypeStores.Query(x => x.Id == createModel.Id).FirstOrDefault();
                        if (criteriaType != null)
                        {
                            _criteriaTypeStores.Delete(criteriaType);
                        }
                    }
                });
        }


        [TestMethod]
        public void UserRoleGetAllCriterias_Success()
        {
            _testService.StartLoginWithUser(_userModel.Email, "123456")
              .ThenImplementTest(param =>
              {

                  #region Init data

                  var createdModel = _criteriaService.GetAll("");

                  #endregion

                  #region Get and check data

                  Assert.IsNull(createdModel.Error);
                  Assert.IsNotNull(createdModel.Data);

                  #endregion
              })
              .ThenCleanDataTest(param => { });
        }

        #endregion

        #region Get By Id

        [TestMethod]
        public void AdminRoleGetById_Success()
        {
            _testService.StartLoginWithAdmin()
                .ThenImplementTest(param =>
                {
                    #region Init Data

                    CriteriaStoreModel criteriaModel = InitCriteriaTypeModel();

                    var createdModel = _criteriaService.Add(criteriaModel);
                    param.CleanData.Add("CreateModel", createdModel.Data);

                    #endregion

                    #region Get and check data

                    var result = _criteriaService.Get(createdModel.Data.Id);
                    var actualModel = result.Data;

                    Assert.IsNotNull(result.Data);
                    Assert.IsNull(result.Error);
                    Assert.IsNotNull(actualModel);
                    Assert.AreEqual(criteriaModel.Id, actualModel.Id);

                    Assert.AreEqual(criteriaModel.Name, actualModel.Name);
                    Assert.AreEqual(criteriaModel.Description, actualModel.Description);
                    Assert.AreEqual(criteriaModel.TypeId, actualModel.TypeId);

                    #endregion
                })
                .ThenCleanDataTest(param =>
                {
                    var createModel = param.CleanData.Get<CriteriaStoreModel>("CreateModel");
                    if (createModel != null)
                    {
                        var criteriaType = _criteriaTypeStores.Query(x => x.Id == createModel.Id).FirstOrDefault();
                        if (criteriaType != null)
                        {
                            _criteriaTypeStores.Delete(criteriaType);
                        }
                    }
                });
        }


        [TestMethod]
        public void AdminRoleGetById_InvalidCriteriaId_Success()
        {
            _testService.StartLoginWithAdmin()
                .ThenImplementTest(param =>
                {
                    #region Init Data

                    var result = _criteriaService.Get(new Guid());

                    #endregion

                    #region Get and check data

                    Assert.IsNull(result.Data);
                    Assert.IsNotNull(result.Error);
                    Assert.AreEqual(ErrorType.BAD_REQUEST, result.Error.Type);
                    Assert.AreEqual("INVALID_ID", result.Error.Message);

                    #endregion

                })
                .ThenCleanDataTest(param =>
                {
                });
        }

        [TestMethod]
        public void UserRoleGetById_NoPermission_Success()
        {
            _testService.StartLoginWithUser(_userModel.Email, "123456")
                .ThenImplementTest(param =>
                {
                    #region Init Data

                    var result = _criteriaService.Get(new Guid());

                    #endregion

                    #region Get and check data

                    Assert.IsNull(result.Data);
                    Assert.IsNotNull(result.Error);
                    Assert.AreEqual(ErrorType.NO_ROLE, result.Error.Type);
                    Assert.AreEqual("No Role", result.Error.Message);

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

                    CriteriaStoreModel typeCriteriaModel = InitCriteriaTypeModel();
                    var createdTypeModel = _criteriaService.Add(typeCriteriaModel);
                    param.CleanData.Add("createdTypeModel", createdTypeModel.Data);

                    CriteriaStoreModel criteriaModel = InitCriteriaModel(createdTypeModel.Data.Id);
                    var createdModel = _criteriaService.Add(criteriaModel);
                    param.CleanData.Add("createdModel", createdModel.Data);

                    #endregion

                    #region Get and Check Data

                    Assert.IsNotNull(createdTypeModel.Data);
                    Assert.IsNotNull(createdModel.Data);

                    var actualTypeModel = _criteriaService.Get(createdTypeModel.Data.Id).Data;
                    var actualModel = _criteriaService.Get(createdModel.Data.Id).Data;

                    Assert.IsNotNull(actualTypeModel);
                    Assert.AreEqual(typeCriteriaModel.Name, actualTypeModel.Name);
                    Assert.AreEqual(typeCriteriaModel.TypeId, actualTypeModel.TypeId);
                    Assert.AreEqual(typeCriteriaModel.Description, actualTypeModel.Description);

                    Assert.IsNotNull(actualModel);
                    Assert.AreEqual(criteriaModel.Name, actualModel.Name);
                    Assert.AreEqual(criteriaModel.TypeId, actualModel.TypeId);
                    Assert.AreEqual(criteriaModel.Description, actualModel.Description);

                    #endregion
                })
                .ThenCleanDataTest(param =>
                {
                    var createdModel = param.CleanData.Get<CriteriaStoreModel>("createdModel");
                    if (createdModel != null)
                    {
                        var criteria = _criteriaStores.Query(x => x.Id == createdModel.Id).FirstOrDefault();
                        if (criteria != null)
                        {
                            _criteriaStores.Delete(criteria);
                        }
                    }

                    var createdTypeModel = param.CleanData.Get<CriteriaStoreModel>("createdTypeModel");
                    if (createdTypeModel != null)
                    {
                        var criteriaType = _criteriaTypeStores.Query(x => x.Id == createdTypeModel.Id).FirstOrDefault();
                        if (criteriaType != null)
                        {
                            _criteriaTypeStores.Delete(criteriaType);
                        }
                    }
                });
        }

        [TestMethod]
        public void AdminRoleCreate_InvalidData_Fail()
        {
            _testService
                .StartLoginWithAdmin()
                .ThenImplementTest(param =>
                {
                    #region Init Data

                    CriteriaStoreModel nullModel = null;

                    CriteriaStoreModel invalidNullName = InitCriteriaTypeModel();
                    invalidNullName.Name = "";

                    CriteriaStoreModel invalidMaxlengthName = InitCriteriaTypeModel();
                    invalidMaxlengthName.Name = AppendSpaceToString(invalidMaxlengthName.Name, 260);

                    CriteriaStoreModel invalidNullDescription = InitCriteriaTypeModel();
                    invalidNullDescription.Name = "";

                    CriteriaStoreModel invalidMaxlengthDescription = InitCriteriaTypeModel();
                    invalidMaxlengthDescription.Name = AppendSpaceToString(invalidMaxlengthName.Name, 510);

                    CriteriaStoreModel invalidModelTypeNameDuplicate = InitCriteriaTypeModel();
                    invalidModelTypeNameDuplicate.Name = _criteriaTypeModel.Name;

                    CriteriaStoreModel invalidModelNameDuplicate = InitCriteriaModel(_criteriaTypeModel.Id);
                    invalidModelNameDuplicate.Name = _criteriaModel.Name;

                    List<InvalidModel<CriteriaStoreModel>> invalidModels = new List<InvalidModel<CriteriaStoreModel>>();

                    invalidModels.Add(new InvalidModel<CriteriaStoreModel>("INVALID_MODEL", nullModel));
                    invalidModels.Add(new InvalidModel<CriteriaStoreModel>("INVALID_MODEL", invalidNullName));
                    invalidModels.Add(new InvalidModel<CriteriaStoreModel>("INVALID_MODEL", invalidMaxlengthName));
                    invalidModels.Add(new InvalidModel<CriteriaStoreModel>("INVALID_MODEL", invalidNullDescription));
                    invalidModels.Add(new InvalidModel<CriteriaStoreModel>("INVALID_MODEL", invalidMaxlengthDescription));
                    invalidModels.Add(new InvalidModel<CriteriaStoreModel>("CRITERIA_TYPE_DUPLICATE", invalidModelTypeNameDuplicate));
                    invalidModels.Add(new InvalidModel<CriteriaStoreModel>("CRITERIA_DUPLICATE", invalidModelNameDuplicate));

                    #endregion

                    #region Get and check data

                    foreach (var invalidModel in invalidModels)
                    {
                        var actualModel = _criteriaService.Add(invalidModel.Value);

                        Assert.IsNotNull(actualModel);
                        Assert.IsNull(actualModel.Data);

                        switch (invalidModel.Key)
                        {
                            case "INVALID_MODEL":
                                Assert.AreEqual(ErrorType.NOT_EXIST, actualModel.Error.Type);
                                Assert.AreEqual("INVALID_MODEL", actualModel.Error.Message);
                                break;
                            case "CRITERIA_TYPE_DUPLICATE":
                                Assert.AreEqual(ErrorType.DUPLICATED, actualModel.Error.Type);
                                Assert.AreEqual("CRITERIA_TYPE_DUPLICATE", actualModel.Error.Message);
                                break;
                            case "CRITERIA_DUPLICATE":
                                Assert.AreEqual(ErrorType.DUPLICATED, actualModel.Error.Type);
                                Assert.AreEqual("CRITERIA_DUPLICATE", actualModel.Error.Message);
                                break;
                            default:
                                Assert.Fail();
                                break;
                        }
                    }

                    #endregion
                })
                .ThenCleanDataTest(param =>
                {

                });
        }

        [TestMethod]
        public void UserRoleAddCriteria_NoPermission_Fail()
        {
            _testService.StartLoginWithUser(_userModel.Email, "123456")
              .ThenImplementTest(param =>
              {

                  #region Init data

                  CriteriaStoreModel typeModel = InitCriteriaTypeModel();
                  var createdModelType = _criteriaService.Add(typeModel);

                  CriteriaStoreModel model = InitCriteriaModel(typeModel.Id);
                  var createdModel = _criteriaService.Add(model);

                  #endregion

                  #region Get and check data

                  Assert.IsNotNull(createdModelType.Error);
                  Assert.IsNull(createdModelType.Data);
                  Assert.AreEqual(ErrorType.NO_ROLE, createdModelType.Error.Type);
                  Assert.AreEqual("No Role", createdModelType.Error.Message);

                  Assert.IsNotNull(createdModel.Error);
                  Assert.IsNull(createdModel.Data);
                  Assert.AreEqual(ErrorType.NO_ROLE, createdModel.Error.Type);
                  Assert.AreEqual("No Role", createdModel.Error.Message);

                  #endregion
              })
              .ThenCleanDataTest(param => { });
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

                    CriteriaStoreModel typeCriteriaModel = InitCriteriaTypeModel();
                    var createdTypeModel = _criteriaService.Add(typeCriteriaModel);
                    param.CleanData.Add("createdTypeModel", createdTypeModel.Data);

                    CriteriaStoreModel criteriaModel = InitCriteriaModel(createdTypeModel.Data.Id);
                    var createdModel = _criteriaService.Add(criteriaModel);
                    param.CleanData.Add("createdModel", createdModel.Data);

                    var modelTypeUpdate = InitCriteriaTypeModel();
                    modelTypeUpdate.Id = createdTypeModel.Data.Id;
                    var updateTypeModel = _criteriaService.Edit(modelTypeUpdate);

                    var modelUpdate = InitCriteriaTypeModel();
                    modelUpdate.Id = createdModel.Data.Id;
                    modelUpdate.TypeId = modelTypeUpdate.Id;
                    var updateModel = _criteriaService.Edit(modelUpdate);

                    #endregion

                    #region Get and Check Data

                    Assert.IsNotNull(updateTypeModel.Data);
                    Assert.IsNotNull(updateModel.Data);

                    var actualTypeModel = _criteriaService.Get(updateTypeModel.Data.Id).Data;
                    var actualModel = _criteriaService.Get(updateModel.Data.Id).Data;

                    Assert.IsNotNull(actualTypeModel);
                    Assert.AreEqual(modelTypeUpdate.Name, actualTypeModel.Name);
                    Assert.AreEqual(modelTypeUpdate.TypeId, actualTypeModel.TypeId);
                    Assert.AreEqual(modelTypeUpdate.Description, actualTypeModel.Description);

                    Assert.IsNotNull(actualModel);
                    Assert.AreEqual(modelUpdate.Name, actualModel.Name);
                    Assert.AreEqual(modelUpdate.TypeId, actualModel.TypeId);
                    Assert.AreEqual(modelUpdate.Description, actualModel.Description);

                    #endregion
                })
                .ThenCleanDataTest(param =>
                {
                    var createdModel = param.CleanData.Get<CriteriaStoreModel>("createdModel");
                    if (createdModel != null)
                    {
                        var criteria = _criteriaStores.Query(x => x.Id == createdModel.Id).FirstOrDefault();
                        if (criteria != null)
                        {
                            _criteriaStores.Delete(criteria);
                        }
                    }

                    var createdTypeModel = param.CleanData.Get<CriteriaStoreModel>("createdTypeModel");
                    if (createdTypeModel != null)
                    {
                        var criteriaType = _criteriaTypeStores.Query(x => x.Id == createdTypeModel.Id).FirstOrDefault();
                        if (criteriaType != null)
                        {
                            _criteriaTypeStores.Delete(criteriaType);
                        }
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
                    #region Init Data

                    CriteriaStoreModel nullModel = null;

                    CriteriaStoreModel invalidNullName = InitCriteriaTypeModel();
                    invalidNullName.Id = _criteriaTypeModel.Id;
                    invalidNullName.Name = "";

                    CriteriaStoreModel invalidMaxlengthName = InitCriteriaTypeModel();
                    invalidMaxlengthName.Id = _criteriaTypeModel.Id;
                    invalidMaxlengthName.Name = AppendSpaceToString(invalidMaxlengthName.Name, 260);

                    CriteriaStoreModel invalidNullDescription = InitCriteriaTypeModel();
                    invalidNullDescription.Id = _criteriaTypeModel.Id;
                    invalidNullDescription.Name = "";

                    CriteriaStoreModel invalidMaxlengthDescription = InitCriteriaTypeModel();
                    invalidMaxlengthDescription.Id = _criteriaTypeModel.Id;
                    invalidMaxlengthDescription.Name = AppendSpaceToString(invalidMaxlengthName.Name, 510);

                    CriteriaStoreModel criteriaTypeModel = InitCriteriaTypeModel();
                    var createdTypeModel = _criteriaService.Add(criteriaTypeModel);
                    param.CleanData.Add("createdTypeModel", createdTypeModel.Data);
                    CriteriaStoreModel invalidModelTypeNameDuplicate = InitCriteriaTypeModel();
                    invalidModelTypeNameDuplicate.Id = createdTypeModel.Data.Id;
                    invalidModelTypeNameDuplicate.Name = _criteriaTypeModel.Name;

                    CriteriaStoreModel criteriaModel = InitCriteriaModel(_criteriaTypeModel.Id);
                    var createdModel = _criteriaService.Add(criteriaModel);
                    param.CleanData.Add("createdModel", createdModel.Data);
                    CriteriaStoreModel invalidModelNameDuplicate = InitCriteriaModel(_criteriaTypeModel.Id);
                    invalidModelNameDuplicate.Id = createdModel.Data.Id;
                    invalidModelNameDuplicate.TypeId = _criteriaModel.TypeId;
                    invalidModelNameDuplicate.Name = _criteriaModel.Name;

                    CriteriaStoreModel invalidTypeModelId = InitCriteriaTypeModel();
                    invalidTypeModelId.Id = new Guid();

                    CriteriaStoreModel invalidModelId = InitCriteriaModel(_criteriaTypeModel.Id);
                    invalidModelId.Id = new Guid();

                    List<InvalidModel<CriteriaStoreModel>> invalidModels = new List<InvalidModel<CriteriaStoreModel>>();

                    invalidModels.Add(new InvalidModel<CriteriaStoreModel>("INVALID_MODEL", nullModel));
                    invalidModels.Add(new InvalidModel<CriteriaStoreModel>("INVALID_MODEL", invalidNullName));
                    invalidModels.Add(new InvalidModel<CriteriaStoreModel>("INVALID_MODEL", invalidMaxlengthName));
                    invalidModels.Add(new InvalidModel<CriteriaStoreModel>("INVALID_MODEL", invalidNullDescription));
                    invalidModels.Add(new InvalidModel<CriteriaStoreModel>("INVALID_MODEL", invalidMaxlengthDescription));
                    invalidModels.Add(new InvalidModel<CriteriaStoreModel>("CRITERIA_TYPE_DUPLICATE", invalidModelTypeNameDuplicate));
                    invalidModels.Add(new InvalidModel<CriteriaStoreModel>("CRITERIA_DUPLICATE", invalidModelNameDuplicate));
                    invalidModels.Add(new InvalidModel<CriteriaStoreModel>("NOTFOUND_CRITERIATYPE", invalidTypeModelId));
                    invalidModels.Add(new InvalidModel<CriteriaStoreModel>("NOTFOUND_CRITERIA", invalidModelId));

                    #endregion

                    #region Get and check data

                    foreach (var invalidModel in invalidModels)
                    {
                        var actualModel = _criteriaService.Edit(invalidModel.Value);

                        Assert.IsNotNull(actualModel);
                        Assert.IsNull(actualModel.Data);

                        switch (invalidModel.Key)
                        {
                            case "INVALID_MODEL":
                                Assert.AreEqual(ErrorType.NOT_EXIST, actualModel.Error.Type);
                                Assert.AreEqual("INVALID_MODEL", actualModel.Error.Message);
                                break;
                            case "CRITERIA_TYPE_DUPLICATE":
                                Assert.AreEqual(ErrorType.DUPLICATED, actualModel.Error.Type);
                                Assert.AreEqual("CRITERIA_TYPE_DUPLICATE", actualModel.Error.Message);
                                break;
                            case "CRITERIA_DUPLICATE":
                                Assert.AreEqual(ErrorType.DUPLICATED, actualModel.Error.Type);
                                Assert.AreEqual("CRITERIA_DUPLICATE", actualModel.Error.Message);
                                break;
                            case "NOTFOUND_CRITERIATYPE":
                                Assert.AreEqual(ErrorType.NOT_EXIST, actualModel.Error.Type);
                                Assert.AreEqual("NOTFOUND_CRITERIATYPE", actualModel.Error.Message);
                                break;
                            case "NOTFOUND_CRITERIA":
                                Assert.AreEqual(ErrorType.NOT_EXIST, actualModel.Error.Type);
                                Assert.AreEqual("NOTFOUND_CRITERIA", actualModel.Error.Message);
                                break;
                            default:
                                Assert.Fail();
                                break;
                        }
                    }

                    #endregion
                })
                .ThenCleanDataTest(param =>
                {
                    var createdModel = param.CleanData.Get<CriteriaStoreModel>("createdModel");
                    if (createdModel != null)
                    {
                        var criteria = _criteriaStores.Query(x => x.Id == createdModel.Id).FirstOrDefault();
                        if (criteria != null)
                        {
                            _criteriaStores.Delete(criteria);
                        }
                    }

                    var createdTypeModel = param.CleanData.Get<CriteriaStoreModel>("createdTypeModel");
                    if (createdTypeModel != null)
                    {
                        var criteriaType = _criteriaTypeStores.Query(x => x.Id == createdTypeModel.Id).FirstOrDefault();
                        if (criteriaType != null)
                        {
                            _criteriaTypeStores.Delete(criteriaType);
                        }
                    }
                });
        }

        [TestMethod]
        public void UserRoleUpdateCriteria_NoPermission_Fail()
        {
            _testService.StartLoginWithUser(_userModel.Email, "123456")
              .ThenImplementTest(param =>
              {

                  #region Init data

                  CriteriaStoreModel typeModel = InitCriteriaTypeModel();
                  var createdModelType = _criteriaService.Edit(typeModel);

                  CriteriaStoreModel model = InitCriteriaModel(typeModel.Id);
                  var createdModel = _criteriaService.Edit(model);

                  #endregion

                  #region Get and check data

                  Assert.IsNotNull(createdModelType.Error);
                  Assert.IsNull(createdModelType.Data);
                  Assert.AreEqual(ErrorType.NO_ROLE, createdModelType.Error.Type);
                  Assert.AreEqual("No Role", createdModelType.Error.Message);

                  Assert.IsNotNull(createdModel.Error);
                  Assert.IsNull(createdModel.Data);
                  Assert.AreEqual(ErrorType.NO_ROLE, createdModel.Error.Type);
                  Assert.AreEqual("No Role", createdModel.Error.Message);

                  #endregion
              })
              .ThenCleanDataTest(param => { });
        }


        #endregion

        #region Delete

        [TestMethod]
        public void AdminRoleDeleteCriteria_ValidCriteriaId_Success()
        {
            _testService
                .StartLoginWithAdmin()
                .ThenImplementTest(param =>
                {
                    #region Init Data

                    CriteriaStoreModel criteriaModel = InitCriteriaTypeModel();
                    var createdModel = _criteriaService.Add(criteriaModel);
                    param.CleanData.Add("CreateModel", createdModel.Data);
                    var result = _criteriaService.Delete(createdModel.Data.Id);

                    #endregion

                    #region Get and check data

                    Assert.IsNotNull(result.Data);
                    Assert.IsNull(result.Error);
                    Assert.AreEqual(true, result.Data);

                    var actualModel = _criteriaService.Get(createdModel.Data.Id);
                    Assert.IsNull(actualModel.Data);
                    Assert.IsNull(actualModel.Error);

                    #endregion
                })
                .ThenCleanDataTest(param =>
                {
                    var createModel = param.CleanData.Get<CriteriaStoreModel>("CreateModel");
                    if (createModel != null)
                    {
                        var project = _criteriaTypeStores.Query(x => x.Id == createModel.Id).FirstOrDefault();
                        if (project != null)
                        {
                            _criteriaTypeStores.Delete(project);
                        }
                    }
                });
        }

        [TestMethod]
        public void AdminRoleDeleteCriteria_InValidCriteriaId_Fail()
        {
            _testService.StartLoginWithAdmin()
              .ThenImplementTest(param =>
              {
                  #region Init data

                  var result = _criteriaService.Delete(new Guid());
                  var result2 = _criteriaService.Delete(Guid.Empty);

                  #endregion

                  #region Get and check data

                  Assert.IsNotNull(result2.Error);
                  Assert.IsNotNull(result2.Data);
                  Assert.AreEqual(ErrorType.BAD_REQUEST, result2.Error.Type);
                  Assert.AreEqual("INVALID_ID", result2.Error.Message);

                  Assert.IsNotNull(result.Error);
                  Assert.IsNotNull(result.Data);
                  Assert.AreEqual(ErrorType.BAD_REQUEST, result.Error.Type);
                  Assert.AreEqual("INVALID_ID", result.Error.Message);

                  #endregion
              })
              .ThenCleanDataTest(param => { });
        }

        [TestMethod]
        public void UserRoleDeleteCriteria_NoPermission_Fail()
        {
            _testService.StartLoginWithUser(_userModel.Email, "123456")
              .ThenImplementTest(param =>
              {

                  #region Init data

                  CriteriaStoreModel typeModel = InitCriteriaTypeModel();
                  var createdModelType = _criteriaService.Delete(typeModel.Id);

                  CriteriaStoreModel model = InitCriteriaModel(typeModel.Id);
                  var createdModel = _criteriaService.Delete(model.Id);

                  #endregion

                  #region Get and check data

                  Assert.IsNotNull(createdModelType.Error);
                  Assert.IsNotNull(createdModelType.Data);
                  Assert.AreEqual(createdModelType.Data, false);
                  Assert.AreEqual(ErrorType.NO_ROLE, createdModelType.Error.Type);
                  Assert.AreEqual("No Role", createdModelType.Error.Message);

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



        #region Private

        private CriteriaStoreModel InitCriteriaModel(Guid typeId)
        {
            var model = new CriteriaStoreModel
            {
                Name = "Criteria Type " + rand(),
                TypeId = typeId,
                Description = "Criteria Type DesCription " + rand()
            };

            return model;
        }

        private CriteriaStoreModel InitCriteriaTypeModel()
        {
            var model = new CriteriaStoreModel
            {
                Name = "Criteria Type " + rand(),
                TypeId = null,
                Description = "Criteria Type DesCription " + rand()
            };

            return model;
        }

        private int rand()
        {
            return new Random().Next(1, 1000000);
        }

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

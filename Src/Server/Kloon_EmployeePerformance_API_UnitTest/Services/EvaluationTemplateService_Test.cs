using Kloon.EmployeePerformance.DataAccess;
using Kloon.EmployeePerformance.DataAccess.Domain;
using Kloon.EmployeePerformance.Logic.Services;
using Kloon.EmployeePerformance.Models.Common;
using Kloon.EmployeePerformance.Models.Criteria;
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
    public class EvaluationTemplateService_Test : TestBase
    {
        private IEvaluationTemplateService _evaluationTemplateService;
        private ICriteriaStoreService _criteriaStoreService;
        private IUserService _userService;

        private IEntityRepository<EvaluationTemplate> _evaluationTemplateRepository;
        private IEntityRepository<CriteriaTypeTemplate> _criteriaTypeTemplateRepository;
        private IEntityRepository<CriteriaTemplate> _criteriaTemplateRepository;
        private IUnitOfWork<EmployeePerformanceContext> _dbContext;

        private CriteriaStoreModel _criteriaModel;
        private CriteriaStoreModel _criteriaTypeModel;
        private UserModel _userNormal;

        private EvaluationTemplateViewModel evalutionTemplateViewModel = new();
        private CriteriaTemplateViewModel criteriaTemplateViewModel = new();
        private CriteriaTemplateViewModel criteriaTypeTemplateViewModel = new();
        private List<CriteriaTemplateViewModel> listCriteriaTemplateViewModel = new();

        protected override void CleanEnvirontment()
        {
            if (evalutionTemplateViewModel != null)
                _evaluationTemplateService.Delete(evalutionTemplateViewModel.Id);
            _criteriaStoreService.Delete(_criteriaModel.Id);
            _criteriaStoreService.Delete(_criteriaTypeModel.Id);
            _userService.Delete(_userNormal.Id);
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
            _evaluationTemplateService = _scope.ServiceProvider.GetService<IEvaluationTemplateService>();
            _criteriaStoreService = _scope.ServiceProvider.GetService<ICriteriaStoreService>();
            _userService = _scope.ServiceProvider.GetService<IUserService>();
            _dbContext = _scope.ServiceProvider.GetService<IUnitOfWork<EmployeePerformanceContext>>();
            _evaluationTemplateRepository = _dbContext.GetRepository<EvaluationTemplate>();
            _criteriaTypeTemplateRepository = _dbContext.GetRepository<CriteriaTypeTemplate>();
            _criteriaTemplateRepository = _dbContext.GetRepository<CriteriaTemplate>();
        }

        #region GetAll
        [TestMethod]
        public void RoleAdmin_GetAll_Then_Success()
        {
            _testService.StartLoginWithAdmin()
                         .ThenImplementTest(param =>
                         {
                             //Arrange
                             List<EvaluationTemplateViewModel> evaluationTemplates = new();
                             evaluationTemplates.Add(evalutionTemplateViewModel);
                             var modelArrange = evaluationTemplates.FirstOrDefault();
                             //Act
                             var result = _evaluationTemplateService.GetAll();
                             var modelActual = result.Data.FirstOrDefault();
                             //Assert

                             Assert.IsNotNull(result.Data);
                             Assert.IsNull(result.Error);
                             Assert.AreEqual(modelArrange.Id, modelActual.Id);
                             Assert.AreEqual(modelArrange.Name, modelActual.Name);
                             Assert.AreEqual(modelArrange.PositionId, modelActual.PositionId);
                             Assert.AreEqual(modelArrange.CriteriaTemplateViewModels.Count, modelActual.CriteriaTemplateViewModels.Count);

                         })
                         .ThenCleanDataTest(param =>
                         {

                         });
        }
        [TestMethod]
        public void RoleUser_GetAll_Then_Error()
        {
            _testService.StartLoginWithUser(_userNormal.Email, "123456")
                        .ThenImplementTest(param =>
                        {
                            //Arrange
                            //Actual
                            var result = _evaluationTemplateService.GetAll();
                            //Assert
                            Assert.IsNotNull(result.Error);
                            Assert.IsNull(result.Data);
                            Assert.AreEqual(ErrorType.NO_ROLE, result.Error.Type);
                            Assert.AreEqual("No Role", result.Error.Message);

                        })
                        .ThenCleanDataTest(param =>
                        {

                        });
        }
        #endregion

        #region GetById
        [TestMethod]
        public void Role_Admin_GetById_NotFound_Then_Error()
        {
            _testService.StartLoginWithAdmin()
                        .ThenImplementTest(param =>
                        {
                            //Arrange

                            //Actual
                            var result = _evaluationTemplateService.GetById(evalutionTemplateViewModel.Id);

                            var deleteActual = _evaluationTemplateService.Delete(result.Data.Id);

                            var actual = _evaluationTemplateService.GetById(evalutionTemplateViewModel.Id);
                            //Assert
                            Assert.IsTrue(deleteActual.Data);
                            Assert.IsNull(actual.Data);
                            Assert.IsNotNull(actual.Error);
                            Assert.AreEqual(actual.Error.Message, "Not_Found");
                        })
                        .ThenCleanDataTest(param =>
                        {

                        });
        }
        [TestMethod]
        public void Role_Admin_GetById_InvalidID_Then_Error()
        {
            _testService.StartLoginWithAdmin()
                           .ThenImplementTest(param =>
                           {
                               //Arrange
                               var id = Guid.Empty;

                               //Actual
                               var result = _evaluationTemplateService.GetById(id);
                               //Assert
                               Assert.IsNull(result.Data);
                               Assert.IsNotNull(result.Error);
                               Assert.AreEqual(result.Error.Message, "INVALID_ID");
                           })
                           .ThenCleanDataTest(param =>
                           {

                           });
        }
        [TestMethod]
        public void Role_Admin_GetById_ValidID_Then_Success()
        {
            _testService.StartLoginWithAdmin()
                              .ThenImplementTest(param =>
                              {
                                  //Arrange
                                  //Actual
                                  var result = _evaluationTemplateService.GetById(evalutionTemplateViewModel.Id);
                                  //Assert
                                  Assert.IsNotNull(result.Data);
                                  Assert.IsNull(result.Error);
                                  Assert.AreEqual(result.Data.Id, evalutionTemplateViewModel.Id);
                                  Assert.AreEqual(result.Data.Name, evalutionTemplateViewModel.Name);
                                  Assert.AreEqual(result.Data.PositionId, evalutionTemplateViewModel.PositionId);
                                  Assert.AreEqual(result.Data.CriteriaTemplateViewModels.Count, evalutionTemplateViewModel.CriteriaTemplateViewModels.Count);
                              })
                              .ThenCleanDataTest(param =>
                              {
                              });
        }
        [TestMethod]
        public void RoleUser_GetById_Then_Error()
        {
            _testService.StartLoginWithUser(_userNormal.Email, "123456")
                        .ThenImplementTest(param =>
                        {
                            //Arrange
                            //Actual
                            var result = _evaluationTemplateService.GetById(evalutionTemplateViewModel.Id);
                            //Assert
                            Assert.IsNotNull(result.Error);
                            Assert.IsNull(result.Data);
                            Assert.AreEqual(ErrorType.NO_ROLE, result.Error.Type);
                            Assert.AreEqual("No Role", result.Error.Message);

                        })
                        .ThenCleanDataTest(param =>
                        {

                        });
        }
        #endregion

        #region CREATE
        [TestMethod]
        public void ADMIN_CREATE_TEMPLATE_BUT_NO_CRITERIA_EXISTED_THEN_ERROR()
        {
            _testService.StartLoginWithAdmin()
                .ThenImplementTest(param =>
                {
                    //Arrange
                    _criteriaStoreService.Delete(_criteriaModel.Id);
                    _criteriaStoreService.Delete(_criteriaTypeModel.Id);
                    var expectedModel = PrepareEvaluationTemplateViewModel();
                    var actualModel = _evaluationTemplateService.Create(expectedModel);
                    //Assert
                    Assert.IsNull(actualModel.Data);
                    Assert.AreEqual("INVALID_TEMPLATE_REQUIRED_AT_LEASE_ONE_CRITERIA", actualModel.Error.Message);
                })
                .ThenCleanDataTest(param =>
                {

                });
        }
        [TestMethod]
        public void ADMIN_CREATE_TEMPLATE_WITH_INVALID_PARAMS_NAME_NULL_THEN_ERROR()
        {
            _testService.StartLoginWithAdmin()
                .ThenImplementTest(param =>
                {
                    //Arrange
                    var expectedModel = PrepareEvaluationTemplateViewModel();
                    expectedModel.Name = null;
                    var actualModel = _evaluationTemplateService.Create(expectedModel);
                    //Assert
                    Assert.IsNull(actualModel.Data);
                    Assert.AreEqual("INVALID_TEMPLATE_NAME_NULL", actualModel.Error.Message);
                })
                .ThenCleanDataTest(param =>
                {

                });
        }
        [TestMethod]
        public void ADMIN_CREATE_TEMPLATE_WITH_INVALID_PARAMS_NAME_LENGTH_THEN_ERROR()
        {
            _testService.StartLoginWithAdmin()
                .ThenImplementTest(param =>
                {
                    //Arrange
                    var expectedModel = PrepareEvaluationTemplateViewModel();
                    expectedModel.Name = "asdfghjklpoiuytrewqzxcvbnmsasdfghjklpoiuytrewqzxcvbnmsasdfghjklpoiuytrewqzxcvbnmsasdfghjklpoiuytrewqzxcvbnmsasdfghjklpoiuytrewqzxcvbnmsasdfghjklpoiuytrewqzxcvbnmsasdfghjklpoiuytrewqzxcvbnmsasdfghjklpoiuytrewqzxcvbnmsasdfghjklpoiuytrewqzxcvbnmsasdfghjklpoiuytrewqzxcvbnmsasdfghjklpoiuytrewqzxcvbnms";
                    var actualModel = _evaluationTemplateService.Create(expectedModel);
                    //Assert
                    Assert.IsNull(actualModel.Data);
                    Assert.AreEqual("INVALID_TEMPLATE_NAME_MAX_LENGTH", actualModel.Error.Message);
                })
                .ThenCleanDataTest(param =>
                {

                });
        }
        [TestMethod]
        public void ADMIN_CREATE_TEMPLATE_WITH_INVALID_PARAMS_POSITION_NULL_THEN_ERROR()
        {
            _testService.StartLoginWithAdmin()
                .ThenImplementTest(param =>
                {
                    //Arrange
                    var expectedModel = PrepareEvaluationTemplateViewModel();
                    expectedModel.PositionId = 0;
                    var actualModel = _evaluationTemplateService.Create(expectedModel);
                    //Assert
                    Assert.IsNull(actualModel.Data);
                    Assert.AreEqual("INVALID_TEMPLATE_POSITION_NULL", actualModel.Error.Message);
                })
                .ThenCleanDataTest(param =>
                {

                });
        }
        [TestMethod]
        public void ADMIN_CREATE_TEMPLATE_WITH_INVALID_PARMAS_DUPLICATED_POSITION_THEN_ERROR()
        {
            _testService.StartLoginWithAdmin()
                .ThenImplementTest(param =>
                {
                    //Arrange
                    var existedModel = _evaluationTemplateRepository.Query(x => !x.DeletedDate.HasValue).FirstOrDefault();
                    var expectedModel = PrepareEvaluationTemplateViewModel();
                    expectedModel.PositionId = existedModel.PositionId;
                    var actualModel = _evaluationTemplateService.Create(expectedModel);
                    //Assert
                    Assert.IsNull(actualModel.Data);
                    Assert.AreEqual("INVALID_TEMPLATE_DUPLICATED_POSITION", actualModel.Error.Message);
                })
                .ThenCleanDataTest(param =>
                {

                });
        }
        [TestMethod]
        public void ADMIN_CREATE_TEMPLATE_WITH_VALID_PARAMS_BUT_NO_CRITERIA_SELECTED_THEN_ERROR()
        {
            _testService.StartLoginWithAdmin()
                .ThenImplementTest(param =>
                {
                    //Arrange
                    var emptyEvaluationTemplateModel = PrepareEvaluationTemplateViewModel();
                    emptyEvaluationTemplateModel.PositionId = 13;
                    emptyEvaluationTemplateModel.CriteriaTemplateViewModels = null;
                    var actualModel = _evaluationTemplateService.Create(emptyEvaluationTemplateModel);
                    //Assert
                    Assert.IsNull(actualModel.Data);
                    Assert.AreEqual("INVALID_TEMPLATE_NO_SELECTED_CRITERIA", actualModel.Error.Message);
                })
                .ThenCleanDataTest(param =>
                {

                });
        }
        [TestMethod]
        public void ADMIN_CREATE_TEMPLATE_WITH_NOT_STORED_CRITERIA_THEN_ERROR()
        {
            _testService.StartLoginWithAdmin()
                .ThenImplementTest(param =>
                {
                    //Arrange
                    var fakeStoreId = Guid.NewGuid();
                    var fakeTypeStoreId = Guid.NewGuid();
                    var fakeTemplate = PrepareEvaluationTemplateViewModel();
                    var fakeCriteriaType = PrepareCriteriaTypeTemplateViewModel(fakeTemplate.Id, _criteriaTypeModel.Id);
                    var fakeCriteria = PrepareCriteriaTemplateViewModel(fakeTemplate.Id, fakeCriteriaType.Id, fakeStoreId, fakeTypeStoreId);
                    List<CriteriaTemplateViewModel> fakeList = new();
                    fakeList.Add(fakeCriteriaType);
                    fakeList.Add(fakeCriteria);
                    var fakeStoredCriteriaModel = PrepareEvaluationTemplateViewModel();
                    fakeStoredCriteriaModel.PositionId = 13;
                    fakeStoredCriteriaModel.CriteriaTemplateViewModels = fakeList;
                    var actualModel = _evaluationTemplateService.Create(fakeStoredCriteriaModel);
                    //Assert
                    Assert.IsNull(actualModel.Data);
                    Assert.AreEqual("INVALID_TEMPLATE_NOT_STORED_CRITERIA", actualModel.Error.Message);
                })
                .ThenCleanDataTest(param =>
                {

                });
        }
        [TestMethod]
        public void ADMIN_CREATE_TEMPLATE_WITH_CHILDLESS_CRITERIA_TYPE_THEN_ERROR()
        {
            _testService.StartLoginWithAdmin()
                .ThenImplementTest(param =>
                {
                    //Arrange
                    List<CriteriaTemplateViewModel> listEmptyCriteriaType = new();
                    var expectedModel = new EvaluationTemplateViewModel
                    {
                        Name = "Evaluation Template From Client " + Rand(),
                        PositionId = 13,
                        CriteriaTemplateViewModels = listEmptyCriteriaType,
                    };

                    var emptyCriteriaType = PrepareCriteriaTypeTemplateViewModel(expectedModel.Id, _criteriaTypeModel.Id);
                    listEmptyCriteriaType.Add(emptyCriteriaType);


                    var actualModel = _evaluationTemplateService.Create(expectedModel);
                    //Assert
                    Assert.IsNull(actualModel.Data);
                    Assert.AreEqual("INVALID_TEMPLATE_LACK_OF_CRITERIA", actualModel.Error.Message);
                })
                .ThenCleanDataTest(param =>
                {

                });
        }
        [TestMethod]
        public void USER_CREATE_TEMPLATE_WITH_VALID_PARAMS_BUT_NO_PERMISSION_THEN_ERROR()
        {
            _testService.StartLoginWithUser(_userNormal.Email, "123456")
                .ThenImplementTest(param =>
                {
                    //Arrange
                    var expectedModel = PrepareEvaluationTemplateViewModel();
                    var actualModel = _evaluationTemplateService.Create(expectedModel);
                    //Assert
                    Assert.IsNull(actualModel.Data);
                    Assert.AreEqual("No Role", actualModel.Error.Message);
                }).ThenCleanDataTest(param =>
                {

                });
        }
        [TestMethod]
        public void ADMIN_CREATE_TEMPLATE_WITH_VALID_PARAMS_THEN_SUCCESS()
        {
            _testService.StartLoginWithAdmin()
                .ThenImplementTest(param =>
                {
                    //Arrange
                    var templateModel = PrepareEvaluationTemplateViewModel();
                    templateModel.PositionId = 15;
                    var actualModel = _evaluationTemplateService.Create(templateModel);
                    //Act
                    Assert.IsNotNull(actualModel.Data);
                    param.CleanData.Add("CreateModel", actualModel.Data);
                    //Assert
                    var expectedModel = _evaluationTemplateRepository.Query().Where(x => x.Id == actualModel.Data.Id).FirstOrDefault();
                    Assert.IsNotNull(expectedModel);

                    Assert.AreEqual(expectedModel.Id, actualModel.Data.Id);
                    Assert.AreEqual(expectedModel.PositionId, actualModel.Data.PositionId);
                    Assert.AreEqual(expectedModel.Name, actualModel.Data.Name);
                    Assert.AreEqual(expectedModel.CriteriaTypeTemplates.Count, actualModel.Data.CriteriaTemplateViewModels.Where(x => x.CriteriaTypeStoreId == null).ToList().Count);
                }).ThenCleanDataTest(param =>
                {
                    var createModel = param.CleanData.Get<EvaluationTemplateViewModel>("CreateModel");
                    _evaluationTemplateService.Delete(createModel.Id);
                });
        }
        #endregion

        #region EDIT
        [TestMethod]
        public void ADMIN_EDIT_TEMPLATE_BUT_TEMPLATE_NOT_FOUND_THEN_ERROR()
        {
            _testService.StartLoginWithAdmin()
                .ThenImplementTest(param =>
                {
                    //Arrange
                    var expectedModel = SystemExtension.Clone(evalutionTemplateViewModel);
                    expectedModel.PositionId = 20;
                    expectedModel.Name = "Deleted template";
                    _evaluationTemplateService.Delete(evalutionTemplateViewModel.Id);

                    var actualModel = _evaluationTemplateService.Edit(expectedModel);
                    //Assert
                    Assert.IsNull(actualModel.Data);
                    Assert.AreEqual("TEMPLATE_NOT_FOUND", actualModel.Error.Message);
                })
                .ThenCleanDataTest(param =>
                {

                });
        }
        [TestMethod]
        public void ADMIN_EDIT_TEMPALTE_WITH_INVALID_PARAMS_NAME_NULL_THEN_ERROR()
        {
            _testService.StartLoginWithAdmin()
                .ThenImplementTest(param =>
                {
                    //Arrange
                    var nameNullModel = SystemExtension.Clone(evalutionTemplateViewModel);
                    nameNullModel.Name = null;
                    //Assert
                    var actualModel = _evaluationTemplateService.Edit(nameNullModel);
                    Assert.IsNotNull(actualModel);
                    Assert.IsNull(actualModel.Data);
                    Assert.AreEqual("INVALID_TEMPLATE_NAME_NULL", actualModel.Error.Message);
                })
                .ThenCleanDataTest(param =>
                {

                });
        }
        [TestMethod]
        public void ADMIN_EDIT_TEMPALTE_WITH_INVALID_PARAMS_NAME_LENGTH_THEN_ERROR()
        {
            _testService.StartLoginWithAdmin()
                .ThenImplementTest(param =>
                {
                    //Arrange
                    var nameMaxLengthModel = SystemExtension.Clone(evalutionTemplateViewModel);
                    nameMaxLengthModel.Name = "qwertyuioplkjhgfdsazxcvbnmqwertyuioplkjhgfdsazxcvbnmqwertyuioplkjhgfdsazxcvbnmqwertyuioplkjhgfdsazxcvbnmqwertyuioplkjhgfdsazxcvbnmqwertyuioplkjhgfdsazxcvbnmqwertyuioplkjhgfdsazxcvbnmqwertyuioplkjhgfdsazxcvbnmqwertyuioplkjhgfdsazxcvbnmqwertyuioplkjhgfdsazxcvbnm";
                    //Assert
                    var actualModel = _evaluationTemplateService.Edit(nameMaxLengthModel);
                    Assert.IsNotNull(actualModel);
                    Assert.IsNull(actualModel.Data);
                    Assert.AreEqual("INVALID_TEMPLATE_NAME_MAX_LENGTH", actualModel.Error.Message);
                })
                .ThenCleanDataTest(param =>
                {
                });
        }
        [TestMethod]
        public void ADMIN_EDIT_TEMPALTE_WITH_INVALID_PARAMS_POSITION_NULL_THEN_ERROR()
        {
            _testService.StartLoginWithAdmin()
                .ThenImplementTest(param =>
                {
                    //Arrange
                    var positionNullModel = SystemExtension.Clone(evalutionTemplateViewModel);
                    positionNullModel.PositionId = 0;
                    //Assert
                    var actualModel = _evaluationTemplateService.Edit(positionNullModel);
                    Assert.IsNotNull(actualModel);
                    Assert.IsNull(actualModel.Data);
                    Assert.AreEqual("INVALID_TEMPLATE_POSITION_NULL", actualModel.Error.Message);
                })
                .ThenCleanDataTest(param =>
                {
                });
        }
        [TestMethod]
        public void ADMIN_EDIT_TEMPALTE__WITH_INVALID_PARMAS_DUPLICATED_POSITION_THEN_ERROR()
        {
            _testService.StartLoginWithAdmin()
                .ThenImplementTest(param =>
                {
                    //Arrange
                    var templateModel = PrepareEvaluationTemplateViewModel();
                    templateModel.PositionId = 15;
                    var actualModel = _evaluationTemplateService.Create(templateModel);

                    var expectedModel = SystemExtension.Clone(evalutionTemplateViewModel);
                    expectedModel.PositionId = 15;
                    var editedModel = _evaluationTemplateService.Edit(expectedModel);

                    //Act
                    Assert.IsNotNull(actualModel.Data);
                    param.CleanData.Add("CreateModel", actualModel.Data);
                    //Assert
                    Assert.IsNull(editedModel.Data);
                    Assert.AreEqual("INVALID_TEMPLATE_DUPLICATED_POSITION", editedModel.Error.Message);
                })
                .ThenCleanDataTest(param =>
                {
                    var createModel = param.CleanData.Get<EvaluationTemplateViewModel>("CreateModel");
                    _evaluationTemplateService.Delete(createModel.Id);
                });
        }
        [TestMethod]
        public void ADMIN_EDIT_TEMPLATE_WITH_VALID_PARAMS_BUT_NO_SELECTED_CRTITERIA_THEN_ERROR()
        {
            _testService.StartLoginWithAdmin()
                .ThenImplementTest(param =>
                {
                    //Arrange
                    var expectedModel = SystemExtension.Clone(evalutionTemplateViewModel);
                    expectedModel.PositionId = 20;
                    expectedModel.Name = "No selected criteria template";
                    expectedModel.CriteriaTemplateViewModels = null;
                    var actualModel = _evaluationTemplateService.Edit(expectedModel);
                    //Assert
                    Assert.IsNull(actualModel.Data);
                    Assert.AreEqual("INVALID_TEMPLATE_NO_SELECTED_CRITERIA", actualModel.Error.Message);
                })
                .ThenCleanDataTest(param =>
                {

                });
        }
        [TestMethod]
        public void ADMIN_EDIT_TEMPLATE_WITH_NOT_STORED_CRITERIA_THEN_ERROR()
        {
            _testService.StartLoginWithAdmin()
                .ThenImplementTest(param =>
                {
                    //Arrange
                    var fakeStoreId = Guid.NewGuid();
                    var fakeTypeStoreId = Guid.NewGuid();
                    var fakeTemplate = PrepareEvaluationTemplateViewModel();
                    var fakeCriteriaType = PrepareCriteriaTypeTemplateViewModel(fakeTemplate.Id, _criteriaTypeModel.Id);
                    var fakeCriteria = PrepareCriteriaTemplateViewModel(fakeTemplate.Id, fakeCriteriaType.Id, fakeStoreId, fakeTypeStoreId);
                    List<CriteriaTemplateViewModel> fakeList = new();
                    fakeList.Add(fakeCriteriaType);
                    fakeList.Add(fakeCriteria);

                    var expectedModel = SystemExtension.Clone(evalutionTemplateViewModel);
                    expectedModel.PositionId = 20;
                    expectedModel.Name = "fake criteria";
                    expectedModel.CriteriaTemplateViewModels = fakeList;
                    var actualModel = _evaluationTemplateService.Edit(expectedModel);
                    //Assert
                    Assert.IsNull(actualModel.Data);
                    Assert.AreEqual("INVALID_TEMPLATE_NOT_STORED_CRITERIA", actualModel.Error.Message);
                })
                .ThenCleanDataTest(param =>
                {

                });
        }
        [TestMethod]
        public void ADMIN_EDIT_TEMPLATE_WITH_CHILDLESS_CRITERIA_TYPE_THEN_ERROR()
        {
            _testService.StartLoginWithAdmin()
                .ThenImplementTest(param =>
                {
                    //Arrange
                    List<CriteriaTemplateViewModel> listEmptyCriteriaType = new();

                    var expectedModel = SystemExtension.Clone(evalutionTemplateViewModel);
                    expectedModel.CriteriaTemplateViewModels = listEmptyCriteriaType;

                    var emptyCriteriaType = PrepareCriteriaTypeTemplateViewModel(expectedModel.Id, _criteriaTypeModel.Id);
                    listEmptyCriteriaType.Add(emptyCriteriaType);

                    var actualModel = _evaluationTemplateService.Edit(expectedModel);
                    //Assert
                    Assert.IsNull(actualModel.Data);
                    Assert.AreEqual("INVALID_TEMPLATE_LACK_OF_CRITERIA", actualModel.Error.Message);
                })
                .ThenCleanDataTest(param =>
                {

                });
        }
        [TestMethod]
        public void USER_EDIT_TEMPLATE_WITH_VALID_PARAMS_BUT_NO_PERMISSION_THEN_ERROR()
        {
            _testService.StartLoginWithUser(_userNormal.Email, "123456")
                .ThenImplementTest(param =>
                {
                    //Arrange
                    var expectedModel = SystemExtension.Clone(evalutionTemplateViewModel);
                    var actualModel = _evaluationTemplateService.Edit(expectedModel);
                    //Assert
                    Assert.IsNull(actualModel.Data);
                    Assert.AreEqual("No Role", actualModel.Error.Message);
                }).ThenCleanDataTest(param =>
                {

                });
        }
        [TestMethod]
        public void ADMIN_EDIT_TEMPLATE_WITH_VALID_PARAMS_THEN_SUCCESS()
        {
            _testService.StartLoginWithAdmin()
                .ThenImplementTest(param =>
                {
                    //Arrange
                    var expectedModel = SystemExtension.Clone(evalutionTemplateViewModel);

                    var fakeCriteriaTypeModel = PrepareDataCriteriaTypeStore();
                    var fakeCriteriaType = _criteriaStoreService.Add(fakeCriteriaTypeModel).Data;
                    Assert.IsNotNull(fakeCriteriaType);
                    param.CleanData.Add("CreateTypeModel", fakeCriteriaType);

                    var fakeCriteriaModel = PrepareDataCriteriaStore(fakeCriteriaTypeModel.Id);
                    var fakeCriteria = _criteriaStoreService.Add(fakeCriteriaModel).Data;
                    Assert.IsNotNull(fakeCriteria);
                    param.CleanData.Add("CreateModel1", fakeCriteria);

                    var fakeCriteriaModel2 = PrepareDataCriteriaStore(fakeCriteriaTypeModel.Id);
                    var fakeCriteria2 = _criteriaStoreService.Add(fakeCriteriaModel2).Data;
                    Assert.IsNotNull(fakeCriteria2);
                    param.CleanData.Add("CreateModel2", fakeCriteria2);

                    var fakeCriteriaTypeTemplateViewModel = PrepareCriteriaTypeTemplateViewModel(expectedModel.Id, fakeCriteriaType.Id);
                    var fakeCriteriaTypeViewModel1 = PrepareCriteriaTemplateViewModel(expectedModel.Id, fakeCriteriaTypeTemplateViewModel.Id, fakeCriteria.Id, fakeCriteriaType.Id);
                    var fakeCriteriaTypeViewModel2 = PrepareCriteriaTemplateViewModel(expectedModel.Id, fakeCriteriaTypeTemplateViewModel.Id, fakeCriteria2.Id, fakeCriteriaType.Id);

                    listCriteriaTemplateViewModel.Add(fakeCriteriaTypeTemplateViewModel);
                    listCriteriaTemplateViewModel.Add(fakeCriteriaTypeViewModel1);
                    listCriteriaTemplateViewModel.Add(fakeCriteriaTypeViewModel2);
                    //Act
                    expectedModel.PositionId = 20;
                    expectedModel.Name = "edit model template";
                    expectedModel.CriteriaTemplateViewModels = listCriteriaTemplateViewModel;
                    var editedModel = _evaluationTemplateService.Edit(expectedModel);

                    var actualModel = _evaluationTemplateRepository.Query().Where(x => x.Id == editedModel.Data.Id).FirstOrDefault();
                    //Assert
                    Assert.IsNotNull(actualModel);
                    Assert.AreEqual(actualModel.Name, editedModel.Data.Name);
                    Assert.AreEqual(actualModel.PositionId, editedModel.Data.PositionId);
                    Assert.AreEqual(actualModel.CriteriaTypeTemplates.Count, editedModel.Data.CriteriaTemplateViewModels.Where(x => x.CriteriaTypeStoreId == null).ToList().Count);
                })
                .ThenCleanDataTest(param =>
                {
                    var createModel1 = param.CleanData.Get<CriteriaStoreModel>("CreateModel1");
                    _criteriaStoreService.Delete(createModel1.Id);
                    var createModel2 = param.CleanData.Get<CriteriaStoreModel>("CreateModel2");
                    _criteriaStoreService.Delete(createModel2.Id);
                    var createTypeModel = param.CleanData.Get<CriteriaStoreModel>("CreateTypeModel");
                    _criteriaStoreService.Delete(createTypeModel.Id);
                });
        }
        #endregion

        #region DELETE
        [TestMethod]
        public void Role_Admin_Delete_InvalidId_Then_Error()
        {
            _testService.StartLoginWithAdmin()
                   .ThenImplementTest(param =>
                   {
                       //Arrange
                       evalutionTemplateViewModel.Id = Guid.Empty;

                       //Actual
                       var deleteActual = _evaluationTemplateService.Delete(evalutionTemplateViewModel.Id);
                       //Assert
                       Assert.IsFalse(deleteActual.Data);
                       Assert.IsNotNull(deleteActual.Error);
                       Assert.AreEqual(deleteActual.Error.Message, "INVALID_ID");

                   })
                   .ThenCleanDataTest(param =>
                   {

                   });
        }
        [TestMethod]
        public void Role_Admin_Delete_ValidId_Then_Success()
        {

            _testService.StartLoginWithAdmin()
                   .ThenImplementTest(param =>
                   {
                       //Arrange

                       //Actual
                       var deleteActual = _evaluationTemplateService.Delete(evalutionTemplateViewModel.Id);
                       //Assert
                       Assert.IsTrue(deleteActual.Data);
                       Assert.IsNull(deleteActual.Error);

                   })
                   .ThenCleanDataTest(param =>
                   {

                   });
        }
        [TestMethod]
        public void Role_User_Delete_NoPermission_Then_Error()
        {
            _testService.StartLoginWithUser(_userNormal.Email, "123456")
                          .ThenImplementTest(param =>
                          {
                              //Arrange
                              //Actual
                              var result = _evaluationTemplateService.Delete(evalutionTemplateViewModel.Id);
                              //Assert
                              Assert.IsNotNull(result.Error);
                              Assert.IsFalse(result.Data);
                              Assert.AreEqual(ErrorType.NO_ROLE, result.Error.Type);
                              Assert.AreEqual("No Role", result.Error.Message);

                          })
                          .ThenCleanDataTest(param =>
                          {

                          });
        }
        [TestMethod]
        public void Role_Admin_Delete_NotFound_Then_Error()
        {
            _testService.StartLoginWithAdmin()
                           .ThenImplementTest(param =>
                           {
                               //Arrange

                               //Actual


                               var deleteActual = _evaluationTemplateService.Delete(evalutionTemplateViewModel.Id);
                               var delete = _evaluationTemplateService.Delete(evalutionTemplateViewModel.Id);
                               //Assert
                               Assert.IsTrue(deleteActual.Data);
                               Assert.IsNull(deleteActual.Error);
                               Assert.IsFalse(delete.Data);
                               Assert.IsNotNull(delete.Error);

                               Assert.AreEqual(delete.Error.Message, "Not_Found");
                           })
                           .ThenCleanDataTest(param =>
                           {

                           });
        }
        #endregion

        #region Prepare
        private UserModel PrepareNormalUser()
        {
            UserModel newNormalUser = new()
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
            return newNormalUser;
        }
        private CriteriaStoreModel PrepareDataCriteriaStore(Guid typeId)
        {
            var model = new CriteriaStoreModel
            {
                Name = "CriteriaStore Name:  " + Rand(),
                TypeId = typeId,
                Description = "CriteriaStoreDescription " + Rand()
            };
            return model;
        }
        private CriteriaStoreModel PrepareDataCriteriaTypeStore()
        {
            var model = new CriteriaStoreModel
            {
                Name = "CriteriaTypeStore Name: " + Rand(),
                TypeId = null,
                Description = "CriteriaTypeStore Description " + Rand()
            };
            return model;
        }

        private EvaluationTemplateViewModel PrepareEvaluationTemplateViewModel()
        {
            var model = new EvaluationTemplateViewModel
            {
                Name = "Evaluation Template From Client " + Rand(),
                PositionId = 12,
                CriteriaTemplateViewModels = listCriteriaTemplateViewModel,
            };
            return model;
        }
        private CriteriaTemplateViewModel PrepareCriteriaTypeTemplateViewModel(Guid templateId, Guid criteriaStoreId)
        {
            var model = new CriteriaTemplateViewModel
            {
                TemplateId = templateId,
                TypeId = null,
                CriteriaStoreId = criteriaStoreId,
                CriteriaTypeStoreId = null,
                Description = "Criteria Type Description: " + Rand(),
                OrderNo = 1,
                Name = "Criteria Type Name: " + Rand(),
            };
            return model;
        }
        private CriteriaTemplateViewModel PrepareCriteriaTemplateViewModel(Guid templateId, Guid typeId, Guid criteriaStoreId, Guid criteriaTypeStoreId)
        {
            var model = new CriteriaTemplateViewModel
            {
                TemplateId = templateId,
                TypeId = typeId,
                CriteriaStoreId = criteriaStoreId,
                CriteriaTypeStoreId = criteriaTypeStoreId,
                Description = "Criteria Description: " + Rand(),
                OrderNo = 1,
                Name = "Criteria Name: " + Rand(),
            };
            return model;
        }

        private void PrepareDataForUnitTest()
        {
            var criteriaTypeStoreModel = PrepareDataCriteriaTypeStore();
            _criteriaTypeModel = _criteriaStoreService.Add(criteriaTypeStoreModel).Data;
            var criteriaStoreModel = PrepareDataCriteriaStore(criteriaTypeStoreModel.Id);
            _criteriaModel = _criteriaStoreService.Add(criteriaStoreModel).Data;
            var userModel = PrepareNormalUser();
            _userNormal = _userService.Create(userModel).Data;

            evalutionTemplateViewModel = PrepareEvaluationTemplateViewModel();
            criteriaTypeTemplateViewModel = PrepareCriteriaTypeTemplateViewModel(evalutionTemplateViewModel.Id, _criteriaTypeModel.Id);
            criteriaTemplateViewModel = PrepareCriteriaTemplateViewModel(evalutionTemplateViewModel.Id, criteriaTypeTemplateViewModel.Id, criteriaStoreModel.Id, criteriaTypeStoreModel.Id);
            listCriteriaTemplateViewModel.Add(criteriaTypeTemplateViewModel);
            listCriteriaTemplateViewModel.Add(criteriaTemplateViewModel);
            _evaluationTemplateService.Create(evalutionTemplateViewModel);
        }
        private static int Rand()
        {
            return new Random().Next(1, 1000000);
        }
        #endregion
    }
}


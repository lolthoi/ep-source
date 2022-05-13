using Kloon.EmployeePerformance.DataAccess;
using Kloon.EmployeePerformance.DataAccess.Domain;
using Kloon.EmployeePerformance.Logic.Services.Base;
using Kloon.EmployeePerformance.Models.Common;
using Kloon.EmployeePerformance.Models.Template;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kloon.EmployeePerformance.Logic.Services
{
    public interface IEvaluationTemplateService
    {
        ResultModel<EvaluationTemplateViewModel> GetById(Guid Id);
        ResultModel<List<EvaluationTemplateViewModel>> GetAll();
        ResultModel<EvaluationTemplateViewModel> Create(EvaluationTemplateViewModel model);
        ResultModel<EvaluationTemplateViewModel> Edit(EvaluationTemplateViewModel model);
        ResultModel<bool> Delete(Guid Id);
        ResultModel<bool> ReOrder(Guid id, List<CriteriaTemplateViewModel> models);
    }
    public class EvaluationTemplateService : IEvaluationTemplateService
    {
        private readonly IAuthenLogicService<EvaluationTemplateService> _logicService;
        private readonly IUnitOfWork<EmployeePerformanceContext> _dbContext;
        private readonly IEntityRepository<EvaluationTemplate> _evaluationTemplateRepository;
        private readonly IEntityRepository<CriteriaTypeTemplate> _criteriaTypeTemplateRepository;
        private readonly IEntityRepository<CriteriaTemplate> _criteriaTemplateRepository;
        private readonly IEntityRepository<CriteriaTypeStore> _criteriaTypeStoreRepository;
        private readonly IEntityRepository<CriteriaStore> _criteriaStoreRepository;

        public EvaluationTemplateService(IAuthenLogicService<EvaluationTemplateService> logicService)
        {
            _logicService = logicService;
            _dbContext = logicService.DbContext;
            _evaluationTemplateRepository = _dbContext.GetRepository<EvaluationTemplate>();
            _criteriaTypeTemplateRepository = _dbContext.GetRepository<CriteriaTypeTemplate>();
            _criteriaTemplateRepository = _dbContext.GetRepository<CriteriaTemplate>();
            _criteriaTypeStoreRepository = _dbContext.GetRepository<CriteriaTypeStore>();
            _criteriaStoreRepository = _dbContext.GetRepository<CriteriaStore>();
        }

        public ResultModel<EvaluationTemplateViewModel> Create(EvaluationTemplateViewModel model)
        {
            var result = _logicService
                .Start()
                .ThenAuthorize(Roles.ADMINISTRATOR)
                .ThenValidate(current =>
                {
                    var listCriteria = _logicService.Cache.CriteriaStores.GetValues().Where(x => !x.DeletedDate.HasValue).ToList();
                    var listCriteriaType = _logicService.Cache.CriteriaTypeStores.GetValues().Where(x => !x.DeletedDate.HasValue).ToList();
                    if (listCriteria.Count == 0)
                        return new ErrorModel(ErrorType.BAD_REQUEST, "INVALID_TEMPLATE_REQUIRED_AT_LEASE_ONE_CRITERIA");
                    if (string.IsNullOrEmpty(model.Name))
                        return new ErrorModel(ErrorType.BAD_REQUEST, "INVALID_TEMPLATE_NAME_NULL");
                    if (model.Name.Length > 255)
                        return new ErrorModel(ErrorType.BAD_REQUEST, "INVALID_TEMPLATE_NAME_MAX_LENGTH");
                    var position = _logicService.Cache.Position.GetValues();
                    if (!position.Any(x => x.Id == model.PositionId))
                        return new ErrorModel(ErrorType.BAD_REQUEST, "INVALID_TEMPLATE_POSITION_NULL");
                    var existedTemplate = _evaluationTemplateRepository.Query().Any(x => x.PositionId == model.PositionId);
                    if (existedTemplate)
                        return new ErrorModel(ErrorType.BAD_REQUEST, "INVALID_TEMPLATE_DUPLICATED_POSITION");
                    if (model.CriteriaTemplateViewModels == null || model.CriteriaTemplateViewModels.Count == 0)
                        return new ErrorModel(ErrorType.BAD_REQUEST, "INVALID_TEMPLATE_NO_SELECTED_CRITERIA");

                    var listCriteriaTypeModel = model.CriteriaTemplateViewModels.Where(x => x.CriteriaTypeStoreId == null).ToList();
                    var listCriteriaModel = model.CriteriaTemplateViewModels.Except(listCriteriaTypeModel).ToList();

                    var checkCriteriaType = listCriteriaTypeModel.Where(t => !listCriteriaType.Any(x => x.Id == t.CriteriaStoreId)).ToList();
                    var checkCriteria = listCriteriaModel.Where(t => !listCriteria.Any(x => x.Id == t.CriteriaStoreId)).ToList();

                    if (checkCriteria.Count > 0 || checkCriteriaType.Count > 0)
                        return new ErrorModel(ErrorType.NOT_EXIST, "INVALID_TEMPLATE_NOT_STORED_CRITERIA");

                    var listEmptyCriteriaType = listCriteriaTypeModel.Where(x => !listCriteriaModel.Select(t => t.CriteriaTypeStoreId).Contains(x.CriteriaStoreId)).ToList();
                    if (listEmptyCriteriaType.Count > 0)
                        return new ErrorModel(ErrorType.NOT_EXIST, "INVALID_TEMPLATE_LACK_OF_CRITERIA");

                    return null;
                }).ThenImplement(current =>
                {
                    try
                    {
                        _dbContext.BeginTransaction();
                        EvaluationTemplate template = new()
                        {
                            Name = model.Name,
                            PositionId = model.PositionId,
                            CreatedBy = current.Id,
                            CreatedDate = DateTime.UtcNow,
                        };
                        _evaluationTemplateRepository.Add(template);
                        model.Id = template.Id;
                        var listCriteriaType = model.CriteriaTemplateViewModels.Where(t => t.CriteriaTypeStoreId == null).ToList();
                        var groupByCriteria = model.CriteriaTemplateViewModels.GroupBy(t => t.CriteriaTypeStoreId).Where(t => t.Key != null).ToList();
                        foreach (var criteriaType in listCriteriaType)
                        {
                            var listCriteria = groupByCriteria.Where(t => t.Key.Value == criteriaType.CriteriaStoreId).FirstOrDefault().ToArray();
                            CriteriaTypeTemplate modelType = new()
                            {
                                CriteriaTypeStoreId = criteriaType.CriteriaStoreId,
                                EvaluationTemplateId = template.Id,
                                OrderNo = criteriaType.OrderNo,
                                CreatedBy = current.Id,
                                CreatedDate = DateTime.UtcNow,
                            };
                            _criteriaTypeTemplateRepository.Add(modelType);
                            criteriaType.Id = modelType.Id;
                            criteriaType.TemplateId = modelType.EvaluationTemplateId;
                            foreach (var criteria in listCriteria)
                            {
                                CriteriaTemplate model = new()
                                {
                                    CriteriaTypeTemplateId = criteriaType.Id,
                                    EvaluationTemplateId = template.Id,
                                    CriteriaStoreId = criteria.CriteriaStoreId,
                                    OrderNo = criteria.OrderNo,
                                    CreatedBy = current.Id,
                                    CreatedDate = DateTime.UtcNow,
                                };
                                _criteriaTemplateRepository.Add(model);
                                criteria.Id = model.Id;
                                criteria.TemplateId = model.EvaluationTemplateId;
                                criteria.TypeId = criteriaType.Id;
                            }
                        }
                        _dbContext.Save();
                        _dbContext.CommitTransaction();
                        return model;
                    }
                    catch (Exception e)
                    {
                        _dbContext.RollbackTransaction();
                        throw new Exception(e.Message);
                    }
                });
            return result;
        }

        public ResultModel<bool> Delete(Guid templateId)
        {
            var result = _logicService.Start()
                .ThenAuthorize(Roles.ADMINISTRATOR)
                .ThenValidate(x =>
                {
                    if (templateId == Guid.Empty)
                        return new ErrorModel(ErrorType.BAD_REQUEST, "INVALID_ID");

                    var evaluationTemplate = _evaluationTemplateRepository.Query().Where(y => y.Id == templateId).FirstOrDefault();
                    if (evaluationTemplate == null)
                    {
                        return new ErrorModel(ErrorType.BAD_REQUEST, "Not_Found");
                    }
                    return null;
                }).ThenImplement(x =>
                {
                    var query = _evaluationTemplateRepository.Query().Where(t => t.Id == templateId && !t.DeletedDate.HasValue).FirstOrDefault();
                    var criteriaTypeTemplate = _criteriaTypeTemplateRepository.Query().Where(x => x.EvaluationTemplateId == query.Id).ToList();
                    if (criteriaTypeTemplate != null)
                    {
                        foreach (var item1 in criteriaTypeTemplate)
                        {
                            var criteriaTemplate = _criteriaTemplateRepository.Query().Where(x => x.CriteriaTypeTemplateId == item1.Id).ToList();

                            if (criteriaTemplate != null)
                            {
                                foreach (var item in criteriaTemplate)
                                {
                                    _criteriaTemplateRepository.Delete(item);
                                }
                            }
                            _criteriaTypeTemplateRepository.Delete(item1);
                        }
                    }
                    _evaluationTemplateRepository.Delete(query);
                    _dbContext.Save();
                    return true;
                });
            return result;
        }

        public ResultModel<EvaluationTemplateViewModel> Edit(EvaluationTemplateViewModel model)
        {
            EvaluationTemplate template = new();
            var result = _logicService
                 .Start()
                 .ThenAuthorize(Roles.ADMINISTRATOR)
                 .ThenValidate(current =>
                 {
                     var isExisted = _evaluationTemplateRepository.Query().Any(x => x.Id == model.Id);
                     if (!isExisted)
                         return new ErrorModel(ErrorType.BAD_REQUEST, "TEMPLATE_NOT_FOUND");
                     if (string.IsNullOrEmpty(model.Name))
                         return new ErrorModel(ErrorType.BAD_REQUEST, "INVALID_TEMPLATE_NAME_NULL");
                     if (model.Name.Length > 255)
                         return new ErrorModel(ErrorType.BAD_REQUEST, "INVALID_TEMPLATE_NAME_MAX_LENGTH");
                     var position = _logicService.Cache.Position.GetValues();
                     if (!position.Any(x => x.Id == model.PositionId))
                         return new ErrorModel(ErrorType.BAD_REQUEST, "INVALID_TEMPLATE_POSITION_NULL");
                     var existedTemplate = _evaluationTemplateRepository.Query().Any(x => x.Id != model.Id && x.PositionId == model.PositionId);
                     if (existedTemplate)
                         return new ErrorModel(ErrorType.BAD_REQUEST, "INVALID_TEMPLATE_DUPLICATED_POSITION");
                     if (model.CriteriaTemplateViewModels == null || model.CriteriaTemplateViewModels.Count == 0)
                         return new ErrorModel(ErrorType.BAD_REQUEST, "INVALID_TEMPLATE_NO_SELECTED_CRITERIA");

                     var listCriteria = _logicService.Cache.CriteriaStores.GetValues().Where(x => !x.DeletedDate.HasValue).ToList();
                     var listCriteriaType = _logicService.Cache.CriteriaTypeStores.GetValues().Where(x => !x.DeletedDate.HasValue).ToList();

                     var listCriteriaTypeModel = model.CriteriaTemplateViewModels.Where(x => x.CriteriaTypeStoreId == null).ToList();
                     var listCriteriaModel = model.CriteriaTemplateViewModels.Except(listCriteriaTypeModel).ToList();

                     var checkCriteriaType = listCriteriaTypeModel.Where(t => !listCriteriaType.Any(x => x.Id == t.CriteriaStoreId)).ToList();
                     var checkCriteria = listCriteriaModel.Where(t => !listCriteria.Any(x => x.Id == t.CriteriaStoreId)).ToList();
                     if (checkCriteria.Count > 0 || checkCriteriaType.Count > 0)
                         return new ErrorModel(ErrorType.NOT_EXIST, "INVALID_TEMPLATE_NOT_STORED_CRITERIA");

                     var listEmptyCriteriaType = listCriteriaTypeModel.Where(x => !listCriteriaModel.Select(t => t.CriteriaTypeStoreId).Contains(x.CriteriaStoreId)).ToList();
                     if (listEmptyCriteriaType.Count > 0)
                         return new ErrorModel(ErrorType.NOT_EXIST, "INVALID_TEMPLATE_LACK_OF_CRITERIA");

                     return null;
                 })
                 .ThenImplement(current =>
                 {
                     try
                     {
                         template = _evaluationTemplateRepository.Query().Where(x => x.Id.Equals(model.Id)).FirstOrDefault();
                         _dbContext.BeginTransaction();
                         template.Name = model.Name;
                         template.PositionId = model.PositionId;
                         template.ModifiedDate = DateTime.UtcNow;
                         template.ModifiedBy = current.Id;
                         _evaluationTemplateRepository.Edit(template);
                         List<CriteriaTemplateViewModel> listDb = new();
                         var domainCriteriaTypes = _criteriaTypeTemplateRepository.Query(x => x.EvaluationTemplateId == template.Id).ToList();
                         foreach (var item in domainCriteriaTypes)
                         {
                             var domainCriteriaType = new CriteriaTemplateViewModel()
                             {
                                 Id = item.Id,
                                 TemplateId = item.EvaluationTemplateId,
                                 TypeId = null,
                                 CriteriaStoreId = item.CriteriaTypeStoreId,
                                 CriteriaTypeStoreId = null,
                                 Description = null,
                                 OrderNo = item.OrderNo,
                                 Name = null,
                             };
                             listDb.Add(domainCriteriaType);
                         }
                         var domainCriterias = _criteriaTemplateRepository.Query(x => x.EvaluationTemplateId == template.Id).ToList();
                         foreach (var item in domainCriterias)
                         {
                             var storedCriteriaType = domainCriteriaTypes.Where(x => x.Id == item.CriteriaTypeTemplateId).FirstOrDefault();
                             var domainCriteria = new CriteriaTemplateViewModel()
                             {
                                 Id = item.Id,
                                 TemplateId = item.EvaluationTemplateId,
                                 TypeId = item.CriteriaTypeTemplateId,
                                 CriteriaStoreId = item.CriteriaStoreId,
                                 CriteriaTypeStoreId = storedCriteriaType.CriteriaTypeStoreId,
                                 Description = null,
                                 OrderNo = item.OrderNo,
                                 Name = null,
                             };
                             listDb.Add(domainCriteria);
                         }
                         var criteriaTypeStore = _criteriaTypeStoreRepository.Query().ToList();
                         var criteriaStore = _criteriaStoreRepository.Query().ToList();
                         var newCriterias = model.CriteriaTemplateViewModels.Where(t => !listDb.Any(x => x.CriteriaStoreId == t.CriteriaStoreId)).ToList();
                         if (newCriterias.Count > 0)
                         {
                             var listCriteriaType = newCriterias.Where(x => x.CriteriaTypeStoreId == null).ToList();
                             var groupByCriteriaType = newCriterias.GroupBy(t => t.CriteriaTypeStoreId).Where(t => t.Key != null).ToList();
                             if (listCriteriaType.Count > 0)
                             {
                                 foreach (var criteriaType in listCriteriaType)
                                 {
                                     var listCriteria = groupByCriteriaType.Where(t => t.Key.Value == criteriaType.CriteriaStoreId).FirstOrDefault().ToArray();
                                     CriteriaTypeTemplate domainCriteriaType = new()
                                     {
                                         CriteriaTypeStoreId = criteriaType.CriteriaStoreId,
                                         EvaluationTemplateId = template.Id,
                                         OrderNo = criteriaType.OrderNo,
                                         CreatedBy = current.Id,
                                         CreatedDate = DateTime.UtcNow,
                                     };
                                     _criteriaTypeTemplateRepository.Add(domainCriteriaType);
                                     criteriaType.Id = domainCriteriaType.Id;
                                     criteriaType.TemplateId = domainCriteriaType.EvaluationTemplateId;
                                     if (listCriteria.Length > 0)
                                     {
                                         foreach (var criteria in listCriteria)
                                         {
                                             CriteriaTemplate domainCriteria = new()
                                             {
                                                 CriteriaTypeTemplateId = criteriaType.Id,
                                                 EvaluationTemplateId = template.Id,
                                                 CriteriaStoreId = criteria.CriteriaStoreId,
                                                 OrderNo = criteria.OrderNo,
                                                 CreatedBy = current.Id,
                                                 CreatedDate = DateTime.UtcNow,
                                             };
                                             _criteriaTemplateRepository.Add(domainCriteria);
                                             criteria.Id = domainCriteria.Id;
                                             criteria.TemplateId = domainCriteria.EvaluationTemplateId;
                                             criteria.TypeId = criteriaType.Id;
                                         }
                                     }
                                 }
                             }
                             else if (groupByCriteriaType.Count > 0)
                             {
                                 foreach (var criterias in groupByCriteriaType)
                                 {
                                     foreach (var item in criterias)
                                     {
                                         var typeId = domainCriteriaTypes.Where(x => x.CriteriaTypeStoreId == item.CriteriaTypeStoreId).FirstOrDefault();
                                         CriteriaTemplate domainCriteria = new()
                                         {
                                             CriteriaTypeTemplateId = typeId.Id,
                                             EvaluationTemplateId = template.Id,
                                             CriteriaStoreId = item.CriteriaStoreId,
                                             OrderNo = item.OrderNo,
                                             CreatedBy = current.Id,
                                             CreatedDate = DateTime.UtcNow,
                                         };
                                         _criteriaTemplateRepository.Add(domainCriteria);
                                         item.Id = domainCriteria.Id;
                                         item.TemplateId = domainCriteria.EvaluationTemplateId;
                                         item.TypeId = domainCriteria.CriteriaTypeTemplateId;
                                     }
                                 }
                             }
                         }
                         var removeCriterias = listDb.Where(t => !model.CriteriaTemplateViewModels.Any(x => x.CriteriaStoreId == t.CriteriaStoreId)).ToList();
                         if (removeCriterias.Count > 0)
                         {
                             var listCriteriaType = removeCriterias.Where(x => x.CriteriaTypeStoreId == null).ToList();
                             var groupByCriteriaType = removeCriterias.GroupBy(t => t.CriteriaTypeStoreId).Where(t => t.Key != null).ToList();
                             if (listCriteriaType.Count > 0)
                             {
                                 foreach (var criteriaType in listCriteriaType)
                                 {
                                     var listCriteria = groupByCriteriaType.Where(t => t.Key.Value == criteriaType.CriteriaStoreId).FirstOrDefault().ToArray();
                                     if (listCriteria.Length > 0)
                                     {
                                         foreach (var criteria in listCriteria)
                                         {
                                             var deletedCriteria = domainCriterias.Where(x => x.CriteriaStoreId == criteria.CriteriaStoreId).FirstOrDefault();
                                             _criteriaTemplateRepository.Delete(deletedCriteria);
                                         }
                                     }
                                     var deletedCriteriaType = domainCriteriaTypes.Where(x => x.CriteriaTypeStoreId == criteriaType.CriteriaStoreId).FirstOrDefault();
                                     _criteriaTypeTemplateRepository.Delete(deletedCriteriaType);
                                 }
                             }
                             else if (groupByCriteriaType.Count > 0)
                             {
                                 foreach (var criterias in groupByCriteriaType)
                                 {
                                     foreach (var item in criterias)
                                     {
                                         var deletedCriteria = domainCriterias.Where(x => x.CriteriaStoreId == item.CriteriaStoreId).FirstOrDefault();
                                         _criteriaTemplateRepository.Delete(deletedCriteria);
                                     }
                                 }
                             }
                         }
                         _dbContext.Save();
                         _dbContext.CommitTransaction();
                         return model;
                     }
                     catch (Exception e)
                     {
                         _dbContext.RollbackTransaction();
                         throw new Exception(e.Message);
                     }
                 });
            return result;
        }

        public ResultModel<List<EvaluationTemplateViewModel>> GetAll()
        {
            var result = _logicService
                .Start()
                .ThenAuthorize(Roles.ADMINISTRATOR)
                .ThenValidate(current => null)
                .ThenImplement(current =>
                {

                    List<EvaluationTemplateViewModel> listEvaluationTemplateModel = new();

                    var query = _evaluationTemplateRepository.Query().Where(x => !x.DeletedDate.HasValue).ToList();
                    var criteriaTypeTemplate = _criteriaTypeTemplateRepository.Query().ToList();
                    var criteriaTemplate = _criteriaTemplateRepository.Query().ToList();
                    var criteriaStore = _logicService.Cache.CriteriaStores.GetValues().ToList();
                    var criteriaTypeStore = _logicService.Cache.CriteriaTypeStores.GetValues().ToList();
                    foreach (var evaTemplate in query)
                    {
                        List<CriteriaTemplateViewModel> listCriteriaTypeTemplateModel = new();

                        var criTypeTemplate = criteriaTypeTemplate.Where(x => x.EvaluationTemplateId == evaTemplate.Id).ToList();
                        EvaluationTemplateViewModel evaluationTemplateModel = new()
                        {
                            Id = evaTemplate.Id,
                            Name = evaTemplate.Name,
                            PositionId = evaTemplate.PositionId,
                            CriteriaTemplateViewModels = new List<CriteriaTemplateViewModel>(),
                        };


                        foreach (var dadType in criTypeTemplate)
                        {

                            var crTypeTemplate = criteriaTypeTemplate.Where(t => t.Id == dadType.Id).FirstOrDefault();
                            var crTypeStore = criteriaTypeStore.Where(t => t.Id == crTypeTemplate.CriteriaTypeStoreId).FirstOrDefault();
                            var criteriaTypeTemplateModel = new CriteriaTemplateViewModel()
                            {
                                Id = crTypeTemplate.Id,
                                Name = crTypeStore.Name,
                                Description = crTypeStore.Description,
                                OrderNo = crTypeTemplate.OrderNo,
                                TypeId = null,
                                CriteriaStoreId = dadType.CriteriaTypeStoreId,
                                CriteriaTypeStoreId = null,
                            };
                            listCriteriaTypeTemplateModel.Add(criteriaTypeTemplateModel);

                            var listCriteriaTemplate = criteriaTemplate.Where(t => t.CriteriaTypeTemplateId == crTypeTemplate.Id).ToList();
                            if (crTypeTemplate.CriteriaTemplates != null)
                            {
                                foreach (var child in crTypeTemplate.CriteriaTemplates)
                                {
                                    var crStore = criteriaStore.Where(t => t.Id == child.CriteriaStoreId).FirstOrDefault();
                                    var criteriaTemplateModel = new CriteriaTemplateViewModel()
                                    {
                                        Id = child.Id,
                                        Name = crStore.Name,
                                        Description = crStore.Description,
                                        OrderNo = child.OrderNo,
                                        TypeId = criteriaTypeTemplateModel.Id,
                                        CriteriaTypeStoreId = crTypeTemplate.CriteriaTypeStoreId,
                                        CriteriaStoreId = child.CriteriaStoreId,
                                    };
                                    listCriteriaTypeTemplateModel.Add(criteriaTemplateModel);
                                }
                            }
                        }

                        //Add template vao list
                        evaluationTemplateModel.CriteriaTemplateViewModels = listCriteriaTypeTemplateModel.OrderBy(t => t.OrderNo).ToList();
                        listEvaluationTemplateModel.Add(evaluationTemplateModel);
                    }

                    return listEvaluationTemplateModel;
                });
            return result;

        }

        public ResultModel<EvaluationTemplateViewModel> GetById(Guid templateId)
        {


            var result = _logicService
                .Start()
                .ThenAuthorize(Roles.ADMINISTRATOR)
                .ThenValidate(current =>
                {
                    var evaluationTemplate = _evaluationTemplateRepository.Query().Where(x => x.Id == templateId).FirstOrDefault();
                    if (templateId == Guid.Empty)
                    {
                        return new ErrorModel(ErrorType.BAD_REQUEST, "INVALID_ID");
                    }

                    if (evaluationTemplate == null)
                    {
                        return new ErrorModel(ErrorType.BAD_REQUEST, "Not_Found");
                    }
                    return null;
                })
                .ThenImplement(current =>
                {

                    var query = _evaluationTemplateRepository.Query().Where(x => x.Id == templateId && !x.DeletedDate.HasValue).FirstOrDefault();
                    var criteriaTypeTemplate = _criteriaTypeTemplateRepository.Query().ToList();
                    var criteriaTemplate = _criteriaTemplateRepository.Query().ToList();
                    var criteriaStore = _logicService.Cache.CriteriaStores.GetValues().ToList();
                    var criteriaTypeStore = _logicService.Cache.CriteriaTypeStores.GetValues().ToList();


                    EvaluationTemplateViewModel evaluationTemplateModel = new()
                    {
                        Id = query.Id,
                        Name = query.Name,
                        PositionId = query.PositionId,
                        CriteriaTemplateViewModels = new List<CriteriaTemplateViewModel>(),
                    };

                    List<CriteriaTemplateViewModel> listCriteriaTypeTemplateModel = new();

                    var criTypeTemplate = _criteriaTypeTemplateRepository.Query().Where(x => x.EvaluationTemplateId == query.Id).ToList();
                    foreach (var dadType in criTypeTemplate)
                    {
                        var crTypeTemplate = criteriaTypeTemplate.Where(t => t.Id == dadType.Id).FirstOrDefault();
                        var crTypeStore = criteriaTypeStore.Where(t => t.Id == crTypeTemplate.CriteriaTypeStoreId).FirstOrDefault();
                        var criteriaTypeTemplateModel = new CriteriaTemplateViewModel()
                        {
                            Id = crTypeTemplate.Id,
                            Name = crTypeStore.Name,
                            Description = crTypeStore.Description,
                            OrderNo = crTypeTemplate.OrderNo,
                            TypeId = null,
                            CriteriaStoreId = dadType.CriteriaTypeStoreId,
                            CriteriaTypeStoreId = null,
                        };
                        listCriteriaTypeTemplateModel.Add(criteriaTypeTemplateModel);

                        var listCriteriaTemplate = criteriaTemplate.Where(t => t.CriteriaTypeTemplateId == crTypeTemplate.Id).ToList();
                        if (crTypeTemplate.CriteriaTemplates != null)
                        {
                            foreach (var child in crTypeTemplate.CriteriaTemplates)
                            {
                                var crStore = criteriaStore.Where(t => t.Id == child.CriteriaStoreId).FirstOrDefault();
                                var criteriaTemplateModel = new CriteriaTemplateViewModel()
                                {
                                    Id = child.Id,
                                    Name = crStore.Name,
                                    Description = crStore.Description,
                                    OrderNo = child.OrderNo,
                                    TypeId = criteriaTypeTemplateModel.Id,
                                    CriteriaTypeStoreId = crTypeTemplate.CriteriaTypeStoreId,
                                    CriteriaStoreId = child.CriteriaStoreId,
                                };
                                listCriteriaTypeTemplateModel.Add(criteriaTemplateModel);
                            }
                        }
                    }

                    //Add template vao list
                    evaluationTemplateModel.CriteriaTemplateViewModels = listCriteriaTypeTemplateModel.OrderBy(t => t.OrderNo).ToList();



                    return evaluationTemplateModel;
                });
            return result;
        }

        public ResultModel<bool> ReOrder(Guid id, List<CriteriaTemplateViewModel> models)
        {
            var result = _logicService.Start()
                 .ThenAuthorize(Roles.ADMINISTRATOR)
                 .ThenValidate(t =>
                 {
                     if (id == Guid.Empty)
                     {
                         return new ErrorModel(ErrorType.BAD_REQUEST, "INVALID_ID");
                     }
                     if (models == null)
                     {
                         return new ErrorModel(ErrorType.BAD_REQUEST, "Not_Found");
                     }
                     return null;
                 })
                 .ThenImplement(t =>
                 {
                     int index = 0;
                     models.ForEach(x =>
                     {
                         x.OrderNo = index++;
                     });
                     var etemplate = _evaluationTemplateRepository.Query().Where(x => x.Id == id && !x.DeletedDate.HasValue).FirstOrDefault();
                     var allCriteriaTypeTemplate = _criteriaTypeTemplateRepository.Query().Where(x => !x.DeletedDate.HasValue).ToList();
                     var allCriteriaTemplate = _criteriaTemplateRepository.Query().Where(x => !x.DeletedDate.HasValue).ToList();
                     foreach (var criTypeTemplate in allCriteriaTypeTemplate)
                     {
                         var criteriaTypeTemplate = models.Where(t => t.Id == criTypeTemplate.Id).FirstOrDefault();
                         if (criteriaTypeTemplate != null)
                         {
                             criTypeTemplate.OrderNo = criteriaTypeTemplate.OrderNo;
                         }


                         foreach (var criTemplate in allCriteriaTemplate)
                         {

                             var criteriaTemplate = models.Where(t => t.Id == criTemplate.Id && t.CriteriaTypeStoreId != null).FirstOrDefault();
                             if (criteriaTemplate != null)
                             {
                                 criTemplate.OrderNo = criteriaTemplate.OrderNo;
                             }
                         }
                     };
                     _dbContext.Save();

                     return true;
                 });
            return result;
        }
    }
}
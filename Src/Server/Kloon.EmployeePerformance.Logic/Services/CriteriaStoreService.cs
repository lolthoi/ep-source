using Kloon.EmployeePerformance.DataAccess;
using Kloon.EmployeePerformance.DataAccess.Domain;
using Kloon.EmployeePerformance.Logic.Services.Base;
using Kloon.EmployeePerformance.Models.Common;
using Kloon.EmployeePerformance.Models.Criteria;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kloon.EmployeePerformance.Logic.Services
{
    public interface ICriteriaStoreService
    {
        ResultModel<List<CriteriaStoreModel>> GetAll(string key);
        ResultModel<CriteriaStoreModel> Get(Guid id);
        ResultModel<CriteriaStoreModel> Add(CriteriaStoreModel model);
        ResultModel<CriteriaStoreModel> Edit(CriteriaStoreModel model);
        ResultModel<bool> Delete(Guid Id);
        ResultModel<bool> ReOrder(List<CriteriaStoreModel> models);
    }
    public class CriteriaStoreService : ICriteriaStoreService
    {
        private readonly IEntityRepository<CriteriaStore> _criteriaStoreRepository;
        private readonly IAuthenLogicService<CriteriaStoreService> _logicService;
        private readonly IEntityRepository<CriteriaTypeStore> _criteriaTypeStoreRepository;
        private readonly IUnitOfWork<EmployeePerformanceContext> _dbContext;
        public CriteriaStoreService(IUnitOfWork<EmployeePerformanceContext> dbContext, IAuthenLogicService<CriteriaStoreService> logicService)
        {
            _dbContext = dbContext;
            _criteriaTypeStoreRepository = _dbContext.GetRepository<CriteriaTypeStore>();
            _criteriaStoreRepository = _dbContext.GetRepository<CriteriaStore>();
            _logicService = logicService;
        }

        #region GET
        public ResultModel<List<CriteriaStoreModel>> GetAll(string key)
        {
            var result = _logicService.Start()
                .ThenAuthorize(Roles.ADMINISTRATOR, Roles.USER)
                .ThenValidate(x => null)
                .ThenImplement(x =>
                {
                    var data = _logicService.Cache.CriteriaTypeStores.GetValues().Where(x => !x.DeletedDate.HasValue)
                                .Where(x => !x.DeletedDate.HasValue)
                                .Select(x => new CriteriaStoreModel
                                {
                                    Id = x.Id,
                                    TypeId = null,
                                    Description = x.Description,
                                    OrderNo = x.OrderNo,
                                    Name = x.Name
                                })
                                .Union(_logicService.Cache.CriteriaStores.GetValues().Where(x => !x.DeletedDate.HasValue)
                                .Select(x => new CriteriaStoreModel
                                {
                                    Id = x.Id,
                                    TypeId = x.CriteriaTypeId,
                                    Description = x.Description,
                                    OrderNo = x.OrderNo,
                                    Name = x.Name
                                }))
                                .OrderBy(x => x.OrderNo)
                                .ToList();
                    if (!string.IsNullOrEmpty(key))
                    {
                        data = data.Where(x => x.Name.Contains(key, StringComparison.OrdinalIgnoreCase)).ToList();
                    }
                    return data;
                });
            return result;
        }

        public ResultModel<CriteriaStoreModel> Get(Guid id)
        {
            var result = _logicService.Start()
                .ThenAuthorize(Roles.ADMINISTRATOR)
                .ThenValidate(x =>
                {
                    if (id == Guid.Empty)
                    {
                        return new ErrorModel(ErrorType.BAD_REQUEST, "INVALID_ID");
                    }
                    return null;
                })
                .ThenImplement(x => {
                    var data = new CriteriaStoreModel();
                    var criteriaType = _logicService.Cache.CriteriaTypeStores.Get(id);
                    if (criteriaType == null || criteriaType.DeletedDate.HasValue)
                    {
                        var criteria = _logicService.Cache.CriteriaStores.Get(id);
                        if (criteria == null || criteria.DeletedDate.HasValue)
                            return null;
                        data.Id = criteria.Id;
                        data.Description = criteria.Description;
                        data.TypeId = criteria.CriteriaTypeId;
                        data.Name = criteria.Name;
                        data.OrderNo = criteria.OrderNo;
                    }
                    else
                    {

                        data.Id = criteriaType.Id;
                        data.Description = criteriaType.Description;
                        data.TypeId = null;
                        data.Name = criteriaType.Name;
                        data.OrderNo = criteriaType.OrderNo;
                    }

                    _logicService.Cache.CriteriaStores.Clear();
                    _logicService.Cache.CriteriaTypeStores.Clear();
                    return data;
                });
            return result;
        }
        #endregion

        public ResultModel<CriteriaStoreModel> Add(CriteriaStoreModel model)
        {
            var result = _logicService
                .Start()
                .ThenAuthorize(Roles.ADMINISTRATOR)
                .ThenValidate(current =>
                {
                    if (model == null)
                    {
                        return new ErrorModel(ErrorType.NOT_EXIST, "INVALID_MODEL");
                    }
                    if (string.IsNullOrEmpty(model.Name) || model.Name.Length > 255 || (model.Description != null && model.Description.Length > 500))
                        return new ErrorModel(ErrorType.NOT_EXIST, "INVALID_MODEL");

                    if (model.TypeId == null)
                    {
                        var isExis = _criteriaTypeStoreRepository.Query().Any(x => x.Name.ToLower().Equals(model.Name.ToLower().Trim()) && !x.DeletedDate.HasValue);
                        if (isExis)
                        {
                            return new ErrorModel(ErrorType.DUPLICATED, "CRITERIA_TYPE_DUPLICATE");
                        }
                    }
                    else
                    {
                        var isExis = _criteriaStoreRepository.Query().Any(x => x.CriteriaTypeId == model.TypeId && x.Name.ToLower().Equals(model.Name.ToLower().Trim()) && !x.DeletedDate.HasValue);
                        if (isExis)
                        {
                            return new ErrorModel(ErrorType.DUPLICATED, "CRITERIA_DUPLICATE");
                        }
                    }
                    return null;
                })
                .ThenImplement(x =>
                {
                    Guid id = Guid.NewGuid();
                    if (model.TypeId == null)
                    {
                        var maxOrderType = _criteriaTypeStoreRepository.Query().Count() == 0 ? 0 : _criteriaTypeStoreRepository.Query().Max(t => t.OrderNo);
                        var entity = new CriteriaTypeStore
                        {
                            Id = id,
                            Name = model.Name.Trim(),
                            CreatedBy = 1,
                            OrderNo = maxOrderType + 1,
                            CreatedDate = DateTime.UtcNow,
                            Description = string.IsNullOrEmpty(model.Description) ? model.Description : model.Description.Trim()
                        };
                        model.OrderNo = maxOrderType + 1;
                        _criteriaTypeStoreRepository.Add(entity);
                    }
                    else
                    {
                        var maxOrderType = _criteriaStoreRepository.Query().Count() == 0 ? 0 : _criteriaStoreRepository.Query().Max(t => t.OrderNo);
                        var entity = new CriteriaStore
                        {
                            Id = id,
                            Name = model.Name.Trim(),
                            CreatedBy = 1,
                            OrderNo = maxOrderType + 1,
                            CreatedDate = DateTime.UtcNow,
                            CriteriaTypeId = model.TypeId.Value,
                            Description = string.IsNullOrEmpty(model.Description) ? model.Description : model.Description.Trim()
                        };
                        model.OrderNo = maxOrderType + 1;
                        _criteriaStoreRepository.Add(entity);
                    }
                    _dbContext.Save();
                    model.Id = id;
                    _logicService.Cache.CriteriaStores.Clear();
                    _logicService.Cache.CriteriaTypeStores.Clear();
                    return model;
                });

            return result;
        }

        public ResultModel<bool> Delete(Guid Id)
        {
            var result = _logicService.Start()
                .ThenAuthorize(Roles.ADMINISTRATOR)
                .ThenValidate(x => {
                    if (Id == Guid.Empty)
                        return new ErrorModel(ErrorType.BAD_REQUEST, "INVALID_ID");

                    var isType = _criteriaTypeStoreRepository.Query().Any(x => x.Id == Id && !x.DeletedDate.HasValue);
                    if (!isType)
                    {
                        var isCriteria = _criteriaStoreRepository.Query().Any(x => x.Id == Id && !x.DeletedDate.HasValue);
                        if (!isCriteria)
                            return new ErrorModel(ErrorType.BAD_REQUEST, "NOTFOUND");
                    }
                    return null;
                })
                .ThenImplement(x =>
                {
                    var entity = _criteriaTypeStoreRepository.Query().Where(x => x.Id == Id && !x.DeletedDate.HasValue).FirstOrDefault();
                    if (entity != null)
                    {
                        entity.DeletedDate = DateTime.UtcNow;
                        entity.DeletedBy = x.Id;
                        _criteriaTypeStoreRepository.Edit(entity);
                    }
                    else
                    {
                        var citeria = _criteriaStoreRepository.Query().Where(x => x.Id == Id && !x.DeletedDate.HasValue).FirstOrDefault();
                        if (citeria == null)
                            return false;

                        citeria.DeletedDate = DateTime.UtcNow;
                        citeria.DeletedBy = x.Id;

                        _criteriaStoreRepository.Edit(citeria);
                    }
                    _dbContext.Save();
                    _logicService.Cache.CriteriaStores.Clear();
                    _logicService.Cache.CriteriaTypeStores.Clear();
                    return true;
                });
            return result;
        }

        public ResultModel<CriteriaStoreModel> Edit(CriteriaStoreModel model)
        {

            var result = _logicService.Start()
                .ThenAuthorize(Roles.ADMINISTRATOR)
                .ThenValidate(x =>
                {
                    if (model == null)
                    {
                        return new ErrorModel(ErrorType.NOT_EXIST, "INVALID_MODEL");
                    }
                    if (string.IsNullOrEmpty(model.Name) || model.Name.Length > 255 || (model.Description != null && model.Description.Length > 500))
                        return new ErrorModel(ErrorType.NOT_EXIST, "INVALID_MODEL");

                    if (model.TypeId == null)
                    {
                        var isExis = _criteriaTypeStoreRepository.Query().Any(x => x.Id == model.Id && !x.DeletedDate.HasValue);
                        if (!isExis)
                        {
                            return new ErrorModel(ErrorType.NOT_EXIST, "NOTFOUND_CRITERIATYPE");
                        }

                        var isDuplicate = _criteriaTypeStoreRepository.Query().Any(x => x.Name.ToLower().Equals(model.Name.ToLower().Trim()) && x.Id != model.Id);
                        if(isDuplicate)
                            return new ErrorModel(ErrorType.DUPLICATED, "CRITERIA_TYPE_DUPLICATE");
                    }
                    else
                    {
                        var isExis = _criteriaStoreRepository.Query().Any(x => x.Id == model.Id && !x.DeletedDate.HasValue);
                        if (!isExis)
                            return new ErrorModel(ErrorType.NOT_EXIST, "NOTFOUND_CRITERIA");

                        var isDuplicate = _criteriaStoreRepository.Query().Any(x => x.Name.ToLower().Equals(model.Name.Trim().ToLower()) && x.Id != model.Id && x.CriteriaTypeId == model.TypeId && x.DeletedDate== null);
                        if (isDuplicate)
                            return new ErrorModel(ErrorType.DUPLICATED, "CRITERIA_DUPLICATE");
                    }
                    return null;
                })
                .ThenImplement(x => {
                    if (model.TypeId == null)
                    {
                        var entity = _criteriaTypeStoreRepository.Query().Where(x => x.Id.Equals(model.Id) && !x.DeletedDate.HasValue).FirstOrDefault();
                        entity.Name = model.Name.Trim();
                        entity.Description = string.IsNullOrEmpty(model.Description) ? model.Description : model.Description.Trim();
                        entity.ModifiedDate = DateTime.UtcNow;
                        _criteriaTypeStoreRepository.Edit(entity);
                    }
                    else
                    {
                        var entity = _criteriaStoreRepository.Query().Where(x => x.Id.Equals(model.Id) && !x.DeletedDate.HasValue).FirstOrDefault();
                        var maxOrderType = _criteriaStoreRepository.Query().Where(x => x.CriteriaTypeId == model.TypeId).Count() > 0 ?
                            _criteriaStoreRepository.Query().Where(x => x.CriteriaTypeId == model.TypeId).Max(t => t.OrderNo) 
                            : 0;

                        //Update Order change criteria type
                        if (entity.CriteriaTypeId != model.TypeId)
                            entity.OrderNo = maxOrderType + 1;

                        entity.Name = model.Name.Trim();
                        entity.Description = string.IsNullOrEmpty(model.Description) ? model.Description : model.Description.Trim();
                        entity.ModifiedDate = DateTime.UtcNow;
                        entity.CriteriaTypeId = model.TypeId.Value;
                        _criteriaStoreRepository.Edit(entity);
                    }
                    _dbContext.Save();
                    _logicService.Cache.CriteriaStores.Clear();
                    _logicService.Cache.CriteriaTypeStores.Clear();
                    return model;
                });
            return result;
        }

        public ResultModel<bool> ReOrder(List<CriteriaStoreModel> models)
        {
            int index = 0;
            models.ForEach(x =>
            {
                x.OrderNo = index++;
            });
            var result = _logicService.Start()
                .ThenAuthorize(Roles.ADMINISTRATOR)
                .ThenValidate(t => null)
                .ThenImplement(t => {
                    var criteriaType = _criteriaTypeStoreRepository.Query().Where(x => !x.DeletedDate.HasValue).ToList();
                    var criteria = _criteriaStoreRepository.Query().Where(x => !x.DeletedDate.HasValue).ToList();

                    criteriaType.ForEach(x => {
                        var input = models.Where(t => t.Id == x.Id).FirstOrDefault();
                        if (input != null)
                        {
                            x.OrderNo = input.OrderNo;
                        }
                    });

                    criteria.ForEach(x => {
                        var input = models.Where(t => t.Id == x.Id && t.TypeId != null).FirstOrDefault();
                        if (input != null)
                        {
                            x.OrderNo = input.OrderNo;
                            x.CriteriaTypeId = input.TypeId.Value;
                        }
                    });
                    _dbContext.Save();
                    _logicService.Cache.CriteriaStores.Clear();
                    _logicService.Cache.CriteriaTypeStores.Clear();
                    return true;
                });
            return result;
        }
    }
}

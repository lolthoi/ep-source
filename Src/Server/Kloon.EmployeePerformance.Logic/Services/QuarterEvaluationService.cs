using Kloon.EmployeePerformance.DataAccess;
using Kloon.EmployeePerformance.DataAccess.Domain;
using Kloon.EmployeePerformance.Logic.Caches.Data;
using Kloon.EmployeePerformance.Logic.Common;
using Kloon.EmployeePerformance.Logic.Services.Base;
using Kloon.EmployeePerformance.Models.Common;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kloon.EmployeePerformance.Logic.Services
{
    public interface IQuarterEvaluationService
    {
        ResultModel<bool> GenerateEvaluation(int inputYear, int inputQuarter);
        ResultModel<bool> DeleteAllEvaluation();
    }
    public class QuarterEvaluationService : IQuarterEvaluationService
    {
        private readonly IAuthenLogicService<QuarterEvaluationService> _logicService;
        private readonly IUnitOfWork<EmployeePerformanceContext> _dbContext;
        private readonly IEntityRepository<QuarterEvaluation> _quarterEvaluationRepository;
        private readonly IEntityRepository<CriteriaTypeStore> _criteriaTypeStoreRepository;
        private readonly IEntityRepository<CriteriaStore> _criteriaStoreRepository;
        private readonly IEntityRepository<EvaluationTemplate> _evaluationTemplateRepository;
        private readonly IEntityRepository<CriteriaTypeTemplate> _criteriaTypeTemplateRepository;
        private readonly IEntityRepository<CriteriaTemplate> _criteriaTemplateRepository;
        private readonly IEntityRepository<QuarterCriteriaTemplate> _quarterCriteriaTemplateRepository;
        private readonly IEntityRepository<CriteriaType> _criteriaTypeRepository;
        private readonly IEntityRepository<Criteria> _criteriaRepository;

        private readonly IEntityRepository<UserQuarterEvaluation> _userQuarterEvaluationRepository;
        private readonly IEntityRepository<CriteriaTypeQuarterEvaluation> _criteriaTypeQuarterEvaluationRepository;
        private readonly IEntityRepository<CriteriaQuarterEvaluation> _criteriaQuarterEvaluationRepository;
        public QuarterEvaluationService(IAuthenLogicService<QuarterEvaluationService> logicService)
        {
            _logicService = logicService;
            _dbContext = logicService.DbContext;
            _quarterEvaluationRepository = _dbContext.GetRepository<QuarterEvaluation>();
            _criteriaTypeStoreRepository = _dbContext.GetRepository<CriteriaTypeStore>();
            _criteriaStoreRepository = _dbContext.GetRepository<CriteriaStore>();
            _evaluationTemplateRepository = _dbContext.GetRepository<EvaluationTemplate>();
            _criteriaTypeTemplateRepository = _dbContext.GetRepository<CriteriaTypeTemplate>();
            _criteriaTemplateRepository = _dbContext.GetRepository<CriteriaTemplate>();
            _quarterCriteriaTemplateRepository = _dbContext.GetRepository<QuarterCriteriaTemplate>();
            _criteriaTypeRepository = _dbContext.GetRepository<CriteriaType>();
            _criteriaRepository = _dbContext.GetRepository<Criteria>();

            _userQuarterEvaluationRepository = _dbContext.GetRepository<UserQuarterEvaluation>();
            _criteriaTypeQuarterEvaluationRepository = _dbContext.GetRepository<CriteriaTypeQuarterEvaluation>();
            _criteriaQuarterEvaluationRepository = _dbContext.GetRepository<CriteriaQuarterEvaluation>();
        }

        public ResultModel<bool> GenerateEvaluation(int inputYear, int inputQuarter)
        {
            DateTime now = DateTime.UtcNow;
            List<ProjectMD> allProject = null;
            List<UserMD> allUser = new List<UserMD>();
            DateTime firstDayOfQuarter = new DateTime();
            DateTime lastDayOfQuarter = new DateTime();
            List<UserMD> normal = null;
            Dictionary<int?, int> leader = null;
            List<int> positionIds = null;
            List<EvaluationTemplate> evaluationTemplates = null;

            var result = _logicService.
                Start().
                ThenAuthorize(Roles.ADMINISTRATOR).
                ThenValidate(c =>
                {
                    if (inputYear < (now.Year - 5) || inputYear > (now.Year + 5) || inputQuarter > 4 || inputQuarter < 1)
                        return new ErrorModel(ErrorType.BAD_REQUEST, "Invalid input year or quarter");
                    firstDayOfQuarter = GetFirstDayOfQuarter(inputYear, inputQuarter);
                    lastDayOfQuarter = GetLastDayOfQuarter(inputYear, inputQuarter);
                    allProject = _logicService.Cache.Projects.GetValues()
                                .Where(project => (project.Status == (int)ProjectStatusEnum.OPEN && project.StartDate < lastDayOfQuarter)
                                || (project.Status == (int)ProjectStatusEnum.CLOSED && project.StartDate < lastDayOfQuarter && project.EndDate > firstDayOfQuarter))
                                .ToList();
                    List<int> positionNotEvaluationIds = _logicService.Cache.Position
                            .GetValues()
                            .Where(x => x.Name == "CEO" || x.Name.Contains("Intern"))
                            .Select(x => x.Id)
                            .ToList();
                        
                    allProject.ForEach(x =>
                    {
                        var employees = _logicService.Cache.Projects.GetUsers(x.Id).Where(employee => !employee.DeletedDate.HasValue && !positionNotEvaluationIds.Contains(employee.PositionId)).ToList();
                        allUser.AddRange(employees);
                    });

                    if (allProject.Count == 0)
                        return new ErrorModel(ErrorType.BAD_REQUEST, "No suitable project for evaluation");

                    normal = allUser.Where(x => x.ProjectRoleId != (int)ProjectRoles.PM).ToList();
                    leader = allUser.Except(normal).Distinct(x => x.ProjectId).ToDictionary(x => x.ProjectId, x => x.Id);

                    var existedQuarterEvaluation = _quarterEvaluationRepository.Query()
                        .Any(x => x.Year == inputYear && x.Quarter == inputQuarter && !x.DeletedDate.HasValue);

                    positionIds = normal.Select(x => x.PositionId).Distinct().ToList();

                    evaluationTemplates = _evaluationTemplateRepository.Query(x => positionIds.Contains(x.PositionId) && x.DeletedBy == null && x.DeletedDate == null)
                                .ToList();

                    if (existedQuarterEvaluation)
                        return new ErrorModel(ErrorType.DUPLICATED, "Evaluation of this quarter has been already done");

                    if (normal.Count == 0)
                        return new ErrorModel(ErrorType.BAD_REQUEST, "No suitable employee for evaluation");

                    if (positionIds.Count > evaluationTemplates.Count)
                    {
                        return new ErrorModel(ErrorType.BAD_REQUEST, "Sending evaluation mail will not proceed because you have not created enough evaluation criteria for all positions");
                    }
                    return null;
                }).
                ThenImplement(currentUser =>
                {
                    try
                    {
                        // get data from database

                        _dbContext.BeginTransaction();
                        _logicService.Logger.LogInformation("start transaction");

                        // get data from database
                        
                        List<Guid> evaluationTemplateIds = evaluationTemplates.Select(x => x.Id).ToList();

                        List<CriteriaTypeTemplate> criteriaTypeTemplates = _criteriaTypeTemplateRepository.Query(x => evaluationTemplateIds.Contains(x.EvaluationTemplateId) && x.DeletedBy == null && x.DeletedDate == null)
                                .ToList();
                        List<Guid> criteriaTypeStoreIds = criteriaTypeTemplates.Select(x => x.CriteriaTypeStoreId).Distinct().ToList();

                        List<CriteriaTemplate> criteriaTemplates = _criteriaTemplateRepository.Query(x => evaluationTemplateIds.Contains(x.EvaluationTemplateId) && x.DeletedBy == null && x.DeletedDate == null)
                                .ToList();
                        List<Guid> criteriaStoreIds = criteriaTemplates.Select(x => x.CriteriaStoreId).Distinct().ToList();

                        List<CriteriaTypeStore> criteriaTypeStores = _criteriaTypeStoreRepository.Query(x => criteriaTypeStoreIds.Contains(x.Id) && x.DeletedBy == null && x.DeletedDate == null)
                                .ToList();

                        List<CriteriaStore> criteriaStores = _criteriaStoreRepository.Query(x => criteriaStoreIds.Contains(x.Id) && x.DeletedBy == null && x.DeletedDate == null)
                                .ToList();


                        List<QuarterCriteriaTemplate> quarterCriteriaTemplates = new List<QuarterCriteriaTemplate>();

                        List<CriteriaType> criteriaTypes = new List<CriteriaType>();
                        List<Criteria> criterias = new List<Criteria>();

                        foreach (var positionId in positionIds)
                        {
                            var evaluationTemplate = evaluationTemplates.Where(x => x.PositionId == positionId).FirstOrDefault();
                            if (evaluationTemplate != null)
                            {
                                #region Create QuarterCriteriaTemplate

                                QuarterCriteriaTemplate quarterCriteriaTemplate = new QuarterCriteriaTemplate()
                                {
                                    Id = new Guid(),
                                    Name = evaluationTemplate.Name,
                                    PositionId = evaluationTemplate.PositionId,
                                    Year = inputYear,
                                    Quarter = inputQuarter,
                                    CreatedDate = now,
                                    CreatedBy = currentUser.Id
                                };
                                quarterCriteriaTemplates.Add(quarterCriteriaTemplate);

                                #endregion

                                #region Create CriteriaType and Criteria

                                List<CriteriaTypeTemplate> listCriteriaTypeTemplate = criteriaTypeTemplates.Where(x => x.EvaluationTemplateId == evaluationTemplate.Id).ToList();
                                if (listCriteriaTypeTemplate.Count > 0)
                                {
                                    foreach (var criteriaTypeTemplate in listCriteriaTypeTemplate)
                                    {
                                        var criteriaTypeStore = criteriaTypeStores.Where(t => t.Id == criteriaTypeTemplate.CriteriaTypeStoreId).FirstOrDefault();
                                        CriteriaType criteriaType = new CriteriaType()
                                        {
                                            Id = new Guid(),
                                            QuarterCriteriaTemplate = quarterCriteriaTemplate,
                                            Name = criteriaTypeStore == null ? "" : criteriaTypeStore.Name,
                                            Description = criteriaTypeStore == null ? "" : criteriaTypeStore.Description,
                                            OrderNo = criteriaTypeTemplate.OrderNo,
                                            CreatedDate = now,
                                            CreatedBy = currentUser.Id
                                        };

                                        List<CriteriaTemplate> listCriteriaTemplate = criteriaTemplates.Where(x => x.EvaluationTemplateId == evaluationTemplate.Id && x.CriteriaTypeTemplateId == criteriaTypeTemplate.Id).ToList();
                                        if (listCriteriaTemplate.Count > 0)
                                        {
                                            foreach (var item in listCriteriaTemplate)
                                            {
                                                var criteriaStore = criteriaStores.Where(t => t.Id == item.CriteriaStoreId).FirstOrDefault();
                                                Criteria criteria = new Criteria()
                                                {
                                                    Id = new Guid(),
                                                    CriteriaType = criteriaType,
                                                    Name = criteriaStore == null ? "" : criteriaStore.Name,
                                                    Description = criteriaStore == null ? "" : criteriaStore.Description,
                                                    OrderNo = item.OrderNo,
                                                    CreatedDate = now,
                                                    CreatedBy = currentUser.Id
                                                };
                                                criterias.Add(criteria);
                                            }
                                            criteriaTypes.Add(criteriaType);
                                        }
                                    }

                                }

                                #endregion


                            }
                        }

                        #region Add QuarterEvaluation

                        List<QuarterEvaluation> quarterEvaluations = normal.Select(x => new QuarterEvaluation
                        {
                            Id = Guid.NewGuid(),
                            Year = inputYear,
                            Quarter = inputQuarter,
                            UserId = x.Id,
                            PositionId = x.PositionId,
                            ProjectId = x.ProjectId ?? 0,
                            ProjectLeaderId = x.ProjectId.HasValue ? leader.GetValueOrDefault(x.ProjectId, 0) : 0,
                            PointAverage = 0,
                            CreatedDate = now,
                            CreatedBy = currentUser.Id,
                        }).ToList();

                        #endregion

                        _quarterEvaluationRepository.InsertRange(quarterEvaluations);
                        _quarterCriteriaTemplateRepository.InsertRange(quarterCriteriaTemplates);
                        _criteriaTypeRepository.InsertRange(criteriaTypes);
                        _criteriaRepository.InsertRange(criterias);
                        int save = _dbContext.Save();
                        _dbContext.CommitTransaction();

                        _logicService.Cache.QuarterCriteriaTemplates.Clear();
                        _logicService.Cache.CriteriaTypes.Clear();
                        _logicService.Cache.Criterias.Clear();
                        return true;
                    }
                    catch (Exception e)
                    {
                        _dbContext.RollbackTransaction();
                        throw new Exception(e.Message);
                    }
                });
            return result;
        }

        public ResultModel<bool> DeleteAllEvaluation()
        {
            var result = _logicService
                .Start()
                .ThenAuthorize(Roles.ADMINISTRATOR)
                .ThenValidate(c =>
                {
                    //List<QuarterEvaluation> quarterEvaluation = _quarterEvaluationRepository.Query(x => !x.DeletedDate.HasValue).ToList();
                    //if (quarterEvaluation.Count == 0)
                    //    return new ErrorModel(ErrorType.NOT_EXIST, "No any record in Quarter Evaluation");
                    return null;
                })
                .ThenImplement(c =>
                {
                    var criteriaQuarterEvaluation = _criteriaQuarterEvaluationRepository.Query().ToList();
                    if (criteriaQuarterEvaluation.Count > 0)
                    {
                        foreach (var item in criteriaQuarterEvaluation)
                        {
                            _criteriaQuarterEvaluationRepository.Delete(item);
                        }
                        
                    }

                    var criteriaTypeQuarterEvaluation = _criteriaTypeQuarterEvaluationRepository.Query().ToList();
                    if (criteriaTypeQuarterEvaluation.Count > 0)
                    {
                        foreach (var item in criteriaTypeQuarterEvaluation)
                        {
                            _criteriaTypeQuarterEvaluationRepository.Delete(item);
                        }
                    }

                    List<UserQuarterEvaluation> userQuarterEvaluation = _userQuarterEvaluationRepository.Query().ToList();
                    if (userQuarterEvaluation.Count > 0)
                    {
                        userQuarterEvaluation.ForEach(x =>
                        {
                            _userQuarterEvaluationRepository.Delete(x);
                        });
                    }
                    List<QuarterEvaluation> quarterEvaluation = _quarterEvaluationRepository.Query().ToList();
                    if (quarterEvaluation.Count > 0)
                    {
                        foreach (var item in quarterEvaluation)
                        {
                            _quarterEvaluationRepository.Delete(item);
                        }
                    }

                    List<Criteria> criterias = _criteriaRepository.Query().ToList();
                    if (criterias.Count > 0)
                    {
                        foreach (var item in criterias)
                        {
                            _criteriaRepository.Delete(item);
                        }              
                    }

                    List<CriteriaType> criteriaTypes = _criteriaTypeRepository.Query().ToList();
                    if (criteriaTypes.Count > 0)
                    {
                        foreach (var item in criteriaTypes)
                        {
                            _criteriaTypeRepository.Delete(item);
                        }
                    }

                    List<QuarterCriteriaTemplate> quarterCriteriaTemplates = _quarterCriteriaTemplateRepository.Query().ToList();
                    if (quarterCriteriaTemplates.Count > 0)
                    {
                        foreach (var item in quarterCriteriaTemplates)
                        {
                            _quarterCriteriaTemplateRepository.Delete(item);
                        }
                    }

                    _dbContext.Save();
                    _logicService.Cache.QuarterCriteriaTemplates.Clear();
                    _logicService.Cache.CriteriaTypes.Clear();
                    _logicService.Cache.Criterias.Clear();
                    return true;
                });
            return result;
        }

        #region Count Quarter
        private DateTime MinusQuarters(DateTime originalDate, int quarters)
        {
            return originalDate.AddMonths(quarters * -3);
        }
        private DateTime AddQuarters(DateTime originalDate, int quarters)
        {
            return originalDate.AddMonths(quarters * 3);
        }
        private DateTime GetFirstDayOfQuarter(int year, int quarter)
        {
            return AddQuarters(new DateTime(year, 1, 1), quarter - 1);
        }
        private DateTime GetLastDayOfQuarter(int year, int quarter)
        {
            return AddQuarters(new DateTime(year, 1, 1), quarter).AddDays(-1);
        }
        #endregion
    }
}

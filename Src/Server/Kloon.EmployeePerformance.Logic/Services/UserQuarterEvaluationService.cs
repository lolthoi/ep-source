using Kloon.EmployeePerformance.DataAccess;
using Kloon.EmployeePerformance.DataAccess.Domain;
using Kloon.EmployeePerformance.Logic.Caches.Data;
using Kloon.EmployeePerformance.Logic.Services.Base;
using Kloon.EmployeePerformance.Models.Common;
using Kloon.EmployeePerformance.Models.Criteria;
using Kloon.EmployeePerformance.Models.QuarterEvaluation;
using System;
using System.Collections.Generic;
using System.Linq;
using Kloon.EmployeePerformance.Logic.Common;

namespace Kloon.EmployeePerformance.Logic.Services
{
    public interface IUserQuarterEvaluationService
    {
        ResultModel<List<QuarterEvaluationModel>> GetAllByPerson(int startYear, int endYear, int projectId);
        ResultModel<UserQuarterEvaluationModel> GetByQuarterEvaluationId(Guid Id);
        ResultModel<UserQuarterEvaluationModel> Create(UserQuarterEvaluationModel userQuarterEvaluationModel);

        ResultModel<UserQuarterEvaluationModel> Update(UserQuarterEvaluationModel userQuarterEvaluationModel);

        ResultModel<List<LeaderEvaluationResultModel>> LeaderEvaluation(int personId);
        ResultModel<List<CriteriaModelQuarter>> EvaluationInfo(int personId, Guid quarterId);
        ResultModel<bool> CreateLeaderEvaluation(Guid quarterId, List<QuarterPoint> model);

        ResultModel<DataSourcePersonalEvaluateModel> GetAvaiableQuarterEvaluations(int userId);
    }
    public class UserQuarterEvaluationService : IUserQuarterEvaluationService
    {
        private readonly IAuthenLogicService<UserQuarterEvaluationService> _logicService;
        private readonly IUnitOfWork<EmployeePerformanceContext> _dbContext;
        private readonly IEntityRepository<UserQuarterEvaluation> _userQuarterEvaluationRespository;
        private readonly IEntityRepository<QuarterEvaluation> _quarterEvaluationRepository;
        private readonly IEntityRepository<User> _users;
        private readonly IEntityRepository<Project> _projects;
        private readonly IEntityRepository<ProjectUser> _projectUserRepository;
        private readonly IEntityRepository<QuarterCriteriaTemplate> _quarterCriteriaTemplates;
        private readonly IEntityRepository<Criteria> _criteriaRepository;
        private readonly IEntityRepository<CriteriaType> _criteriaTypeRepository;
        private readonly IEntityRepository<CriteriaQuarterEvaluation> _criteriaQuarterRepository;
        private readonly IEntityRepository<CriteriaTypeQuarterEvaluation> _criteriaTypeQuarterRepository;

        public UserQuarterEvaluationService(IAuthenLogicService<UserQuarterEvaluationService> logicService)
        {
            _logicService = logicService;
            _dbContext = logicService.DbContext;
            _quarterEvaluationRepository = _dbContext.GetRepository<QuarterEvaluation>();
            _userQuarterEvaluationRespository = _dbContext.GetRepository<UserQuarterEvaluation>();

            _quarterCriteriaTemplates = _dbContext.GetRepository<QuarterCriteriaTemplate>();
            _criteriaRepository = _dbContext.GetRepository<Criteria>();
            _criteriaTypeRepository = _dbContext.GetRepository<CriteriaType>();
            _projectUserRepository = _dbContext.GetRepository<ProjectUser>();
            _criteriaQuarterRepository = _dbContext.GetRepository<CriteriaQuarterEvaluation>();
            _criteriaTypeQuarterRepository = _dbContext.GetRepository<CriteriaTypeQuarterEvaluation>();

            _users = _dbContext.GetRepository<User>();
            _projects = _dbContext.GetRepository<Project>();
        }

        public ResultModel<List<QuarterEvaluationModel>> GetAllByPerson(int startYear, int endYear, int projectId)
        {
            DateTime now = DateTime.Now;
            var currentYear = now.Year;
            var currentQuarter = Math.Ceiling(DateTime.Today.Month / 3m);
            var result = _logicService
                .Start()
                .ThenAuthorize(Roles.ADMINISTRATOR, Roles.USER)
                .ThenValidate(() => null)
                .ThenImplement(currentUser =>
                {
                    List<QuarterEvaluationModel> quarterEvaluationModels = new List<QuarterEvaluationModel>();

                    var quarterEvaluations = _quarterEvaluationRepository.Query(x => x.UserId == currentUser.Id);

                    if (startYear != 0 && endYear != 0)
                    {
                        if (startYear > endYear)
                        {
                            return quarterEvaluationModels;
                        }
                    }

                    if (startYear != 0)
                    {
                        quarterEvaluations = quarterEvaluations.Where(x => x.Year >= startYear);
                    }
                    if (endYear != 0)
                    {
                        quarterEvaluations = quarterEvaluations.Where(x => x.Year <= endYear);
                    }
                    if (projectId != 0)
                    {
                        quarterEvaluations = quarterEvaluations.Where(x => x.ProjectId == projectId);
                    }
                    var data = quarterEvaluations.ToList();

                    if (data.Count > 0)
                    {
                        List<PositionMD> positions = _logicService.Cache.Position.GetValues().ToList();

                        List<int> projectIds = data.Select(x => x.ProjectId).ToList();
                        List<int> projectLeaderIds = data.Select(x => x.ProjectLeaderId).ToList();

                        List<Project> projects = _projects.Query(x => projectIds.Contains(x.Id)).ToList();
                        List<User> userLeaders = _users.Query(z => projectLeaderIds.Contains(z.Id)).ToList();

                        foreach (var item in data)
                        {
                            var project = projects.Where(x => x.Id == item.ProjectId).FirstOrDefault();
                            var projectLeader = userLeaders.Where(u => u.Id == item.ProjectLeaderId).FirstOrDefault();
                            var position = positions.Where(x => x.Id == item.PositionId).FirstOrDefault();
                            QuarterEvaluationModel quarterEvaluationModel = new QuarterEvaluationModel()
                            {
                                Id = item.Id,
                                Quarter = item.Quarter,
                                Year = item.Year,
                                QuarterText = item.Quarter + "/" + item.Year,
                                Position = position == null ? "NA" : position.Name,
                                Leader = projectLeader == null ? "" : projectLeader.FirstName + " " + projectLeader.LastName,
                                Project = project == null ? "" : project.Name,
                                PointAverage = item.PointAverage
                            };
                            quarterEvaluationModels.Add(quarterEvaluationModel);
                        }
                    }

                    if (startYear != 0)
                    {
                        if (startYear == currentYear)
                        {
                            return quarterEvaluationModels;
                        }
                    }

                    if (quarterEvaluationModels.Count < 4)
                    {
                        if (quarterEvaluationModels.Count == 0)
                        {

                            if (endYear != 0)
                            {
                                if (currentYear != endYear)
                                {
                                    currentYear = endYear;
                                    currentQuarter = 4;
                                }
                            }

                            do
                            {
                                if (currentQuarter == 0)
                                {
                                    currentQuarter = 4;
                                    currentYear = currentYear - 1;
                                }
                                QuarterEvaluationModel quarterEvaluationModel = new QuarterEvaluationModel()
                                {
                                    QuarterText = currentQuarter + "/" + currentYear,
                                    Position = "NA",
                                    Leader = "NA",
                                    Project = "NA",
                                    PointAverage = 0
                                };
                                quarterEvaluationModels.Add(quarterEvaluationModel);

                                currentQuarter--;
                            } while (quarterEvaluationModels.Count < 4);
                        }
                        else if (quarterEvaluationModels.Count > 0)
                        {
                            DateTime now = DateTime.Now;
                            var lastOfListQuarterEvaluationModel = quarterEvaluationModels.LastOrDefault();
                            var currentYear = lastOfListQuarterEvaluationModel.Year;
                            var currentQuarter = lastOfListQuarterEvaluationModel.Quarter - 1;

                            do
                            {
                                if (currentQuarter == 0)
                                {
                                    currentQuarter = 4;
                                    currentYear = currentYear - 1;
                                }
                                QuarterEvaluationModel quarterEvaluationModel = new QuarterEvaluationModel()
                                {
                                    QuarterText = currentQuarter + "/" + currentYear,
                                    Position = "NA",
                                    Leader = "NA",
                                    Project = "NA",
                                    PointAverage = 0
                                };
                                quarterEvaluationModels.Add(quarterEvaluationModel);

                                currentQuarter--;
                            } while (quarterEvaluationModels.Count() < 4);
                        }
                    }
                    return quarterEvaluationModels;
                });
            return result;
        }

        public ResultModel<UserQuarterEvaluationModel> Create(UserQuarterEvaluationModel model)
        {
            var now = DateTime.UtcNow;

            var result = _logicService
                .Start()
                .ThenAuthorize(Roles.USER, Roles.ADMINISTRATOR)
                .ThenValidate(currentUser =>
                {
                    var error = Validate(model);
                    if (error != null)
                    {
                        return error;
                    };
                    var quatarEvaluation = _userQuarterEvaluationRespository.Query().Where(t => model.QuarterEvaluationId == t.QuarterEvaluationId).Any();

                    if (quatarEvaluation)
                    {
                        return new ErrorModel(ErrorType.DUPLICATED, "USER_QUARTER_EVALUATION_DUPLICATE");
                    }


                    var quarterEvaluated = _userQuarterEvaluationRespository.Query().Where(t => model.Id == t.Id).Any();
                    if (quarterEvaluated)
                    {
                        return new ErrorModel(ErrorType.NOT_EXIST, "QUARTER IS EVALUATED");

                    }

                    //var quarterEvaluation = _quarterEvaluationRepository.Query().Where(x => x.Id == model.QuarterEvaluationId).FirstOrDefault();

                    //if (quarterEvaluation != null)
                    //{
                    //    var numCreateDays = now - quarterEvaluation.CreatedDate;
                    //    if (numCreateDays.TotalDays >= 7)
                    //    {
                    //        return new ErrorModel(ErrorType.NOT_EXIST, "EVALIATION DATE IS EXPIRED");

                    //    }
                    //}

                    return null;
                })
                .ThenImplement(currentUser =>
                {
                    var userQuarterEvaluation = new UserQuarterEvaluation()
                    {
                        Id = new Guid(),
                        QuarterEvaluationId = model.QuarterEvaluationId,
                        NoteGoodThing = model.NoteGoodThing,
                        NoteBadThing = model.NoteBadThing,
                        NoteOther = model.NoteOther,
                        CreatedDate = now,
                    };

                    _userQuarterEvaluationRespository.Add(userQuarterEvaluation);
                    _dbContext.Save();

                    model.Id = userQuarterEvaluation.Id;
                    return model;
                });

            return result;
        }

        public ResultModel<UserQuarterEvaluationModel> GetByQuarterEvaluationId(Guid Id)
        {
            UserQuarterEvaluation userQuarterEvaluation = null;
            var result = _logicService
                        .Start()
                        .ThenAuthorize(Roles.ADMINISTRATOR, Roles.USER)
                        .ThenValidate(current =>
                        {
                            userQuarterEvaluation = _userQuarterEvaluationRespository
                                .Query(q => q.QuarterEvaluationId == Id && q.DeletedBy == null && q.DeletedDate == null).FirstOrDefault();
                            if (userQuarterEvaluation == null)
                            {
                                return new ErrorModel(ErrorType.NOT_EXIST, "USER_QUARTER_NOT_FOUND");
                            }
                            return null;
                        })
                        .ThenImplement(current =>
                        {
                            var userQuarterEvaluationModel = new UserQuarterEvaluationModel()
                            {
                                Id = userQuarterEvaluation.Id,
                                QuarterEvaluationId = userQuarterEvaluation.QuarterEvaluationId,
                                NoteGoodThing = userQuarterEvaluation.NoteGoodThing,
                                NoteBadThing = userQuarterEvaluation.NoteBadThing,
                                NoteOther = userQuarterEvaluation.NoteOther
                            };
                            return userQuarterEvaluationModel;
                        });
            return result;
        }

        public ResultModel<UserQuarterEvaluationModel> Update(UserQuarterEvaluationModel model)
        {
            var now = DateTime.UtcNow;
            UserQuarterEvaluation userQuarterEvaluation = null;

            var result = _logicService
                .Start()
                .ThenAuthorize(Roles.USER, Roles.ADMINISTRATOR)
                .ThenValidate(x =>
                {

                    var error = Validate(model);
                    if (error != null)
                    {
                        return error;
                    };


                    userQuarterEvaluation = _userQuarterEvaluationRespository.Query()
                            .Where(t => t.Id == model.Id && t.DeletedDate == null && t.DeletedBy == null).FirstOrDefault();
                    if (userQuarterEvaluation == null)
                    {
                        return new ErrorModel(ErrorType.NOT_EXIST, " QUARTER_EVALUATION_NOT_FOUND");
                    }
                    //var numCreateDays = now - userQuarterEvaluation.CreatedDate;
                    //if (numCreateDays.TotalDays >= 3)
                    //{
                    //    return new ErrorModel(ErrorType.NOT_EXIST, " ASSESSMENT_DURATION");
                    //}

                    return null;
                })
                .ThenImplement(x =>
                {
                    userQuarterEvaluation.NoteGoodThing = model.NoteGoodThing;
                    userQuarterEvaluation.NoteBadThing = model.NoteBadThing;
                    userQuarterEvaluation.NoteOther = model.NoteOther;
                    userQuarterEvaluation.ModifiedBy = x.Id;
                    userQuarterEvaluation.ModifiedDate = now;


                    int result = _dbContext.Save();
                    return model;
                });

            return result;

        }

        public ResultModel<List<LeaderEvaluationResultModel>> LeaderEvaluation(int leaderId)
        {
            var result = _logicService.Start()
                .ThenAuthorize(Roles.ADMINISTRATOR, Roles.USER)
                .ThenValidate(x => null)
                .ThenImplement(t =>
                {
                    var firstQuarter = GetYearQuarter(MinusQuarters(DateTime.Now, 3));
                    var firstQuarterYear = MinusQuarters(DateTime.Now, 3).Year;
                    var quarter = new List<QuarterInfo>();
                    for (int i = 0; i < 4; i++)
                    {
                        var item = new QuarterInfo
                        {
                            Quarter = firstQuarter,
                            Year = firstQuarterYear,
                        };
                        quarter.Add(item);

                        firstQuarterYear = firstQuarter + 1 < 5 ? firstQuarterYear : firstQuarterYear + 1;
                        firstQuarter = firstQuarter + 1 < 5 ? firstQuarter + 1 : 1;
                    }
                    var fistDayFilter = GetFirstDayOfQuarter(MinusQuarters(DateTime.Now, 3));
                    var userQuater = _quarterEvaluationRepository.Query().Where(x => x.DeletedDate == null && x.CreatedDate >= fistDayFilter).ToList();
                    var userInPorjectLeader = new List<UserMD>();
                    var projects = _logicService.Cache.Projects.GetValues().Where(x => !x.DeletedDate.HasValue || x.DeletedDate > DateTime.Now.AddYears(-1)).ToList();


                    if (t.Role == Roles.USER)
                    {
                        userQuater = userQuater.Where(x => x.ProjectLeaderId == t.Id).ToList();
                        projects = _logicService.Cache._userProjects.Get(t.Id).Where(a => a.ProjectRoleId == (int)ProjectRoles.PM).ToList();
                    }

                    projects.ForEach(x =>
                    {
                        userInPorjectLeader.AddRange(_logicService.Cache._projectUser.Get(x.Id).Where(a => a.ProjectRoleId != (int)ProjectRoles.PM));
                    });

                    var result = new List<LeaderEvaluationResultModel>();
                    userInPorjectLeader.ForEach(i =>
                    {
                        var item = userQuater.Where(x => x.UserId == i.Id /*&& x.ProjectId == i.ProjectId*/).ToList();
                        var model = new LeaderEvaluationResultModel
                        {
                            UserId = i.Id,
                            PositionId = i.PositionId,
                            ProjectId = i.ProjectId.Value,
                            Value1 = item.Where(x => x.Quarter == quarter[3].Quarter && x.Year == quarter[3].Year).Select(a => new QuarterInfo { Id = a.Id, Quarter = a.Quarter, Year = a.Year, Score = a.PointAverage.ToString(), CreatedDate = a.CreatedDate }).FirstOrDefault()
                                ?? new QuarterInfo { Quarter = quarter[3].Quarter, Year = quarter[3].Year, Score = "-" },
                            Value2 = item.Where(x => x.Quarter == quarter[2].Quarter && x.Year == quarter[2].Year).Select(a => new QuarterInfo { Id = a.Id, Quarter = a.Quarter, Year = a.Year, Score = a.PointAverage.ToString(), CreatedDate = a.CreatedDate }).FirstOrDefault()
                                ?? new QuarterInfo { Quarter = quarter[2].Quarter, Year = quarter[2].Year, Score = "-" },
                            Value3 = item.Where(x => x.Quarter == quarter[1].Quarter && x.Year == quarter[1].Year).Select(a => new QuarterInfo { Id = a.Id, Quarter = a.Quarter, Year = a.Year, Score = a.PointAverage.ToString(), CreatedDate = a.CreatedDate }).FirstOrDefault()
                                ?? new QuarterInfo { Quarter = quarter[1].Quarter, Year = quarter[1].Year, Score = "-" },
                            Value4 = item.Where(x => x.Quarter == quarter[0].Quarter && x.Year == quarter[0].Year).Select(a => new QuarterInfo { Id = a.Id, Quarter = a.Quarter, Year = a.Year, Score = a.PointAverage.ToString(), CreatedDate = a.CreatedDate }).FirstOrDefault()
                                ?? new QuarterInfo { Quarter = quarter[0].Quarter, Year = quarter[0].Year, Score = "-" }
                        };
                        result.Add(model);
                    });

                    result.ForEach(x =>
                    {
                        var position = _logicService.Cache.Users.Get(x.UserId) == null ? 0 : _logicService.Cache.Users.Get(x.UserId).PositionId;
                        x.Name = _logicService.Cache.Users.Get(x.UserId) == null ? string.Empty : _logicService.Cache.Users.Get(x.UserId).FullName;
                        x.Position = _logicService.Cache.Position.Get(position) == null ? string.Empty : _logicService.Cache.Position.Get(position).Name;
                        x.ProjectName = _logicService.Cache.Projects.Get(x.ProjectId) == null ? string.Empty : _logicService.Cache.Projects.Get(x.ProjectId).Name;
                    });
                    return result;
                });
            return result;
        }

        public ResultModel<List<CriteriaModelQuarter>> EvaluationInfo(int userId, Guid quarterId)
        {

            var result = _logicService.Start()
                .ThenAuthorize(Roles.ADMINISTRATOR, Roles.USER)
                .ThenValidate(x =>
                {
                    var quarterEvalution = _quarterEvaluationRepository.Query(x => x.Id == quarterId && x.UserId == userId).FirstOrDefault();
                    if (quarterEvalution == null)
                    {
                        //return new ErrorModel(ErrorType.NOT_EXIST, "NOT_EXIST");
                    }
                    return null;
                })
                .ThenImplement(x =>
                {
                    var criterias = GetCriteriaModels(quarterId);


                    var quarterCiteriaDic = _criteriaQuarterRepository.Query(x => x.QuarterEvaluationId == quarterId)
                    .Distinct(t => t.CriteriaId)
                    .Select(t => new
                    {
                        Id = t.CriteriaId,
                        Point = (double)t.Point
                    })
                    .Union(
                        _criteriaTypeQuarterRepository.Query(x => x.QuarterEvaluationId == quarterId)
                        .Distinct(x => x.CriteriaTypeId)
                        .Select(t => new
                        {
                            Id = t.CriteriaTypeId,
                            Point = t.PointAverage
                        })).ToDictionary(x => x.Id, x => (double?)x.Point);


                    if (quarterCiteriaDic.Count > 0)
                    {
                        var criteriasQuarter = _criteriaRepository.Query().Where(x => quarterCiteriaDic.Keys.Contains(x.Id)).Select(t => new CriteriaModel
                        {
                            Id = t.Id,
                            TypeId = t.CriteriaTypeId,
                            Name = t.Name,
                            OrderNo = t.OrderNo,
                        }).ToList();

                        var data = criteriasQuarter.Union(_criteriaTypeRepository.Query().Where(x => criteriasQuarter.Select(t => t.TypeId).Contains(x.Id)).Select(t => new CriteriaModel
                        {
                            Id = t.Id,
                            Name = t.Name,
                            TypeId = null,
                            OrderNo = t.OrderNo
                        }))
                        .OrderBy(t => t.OrderNo)
                        .ToList();
                        criterias = data;
                    }

                    if (criterias.Count == 0)
                    {
                        return new List<CriteriaModelQuarter>();
                    }
                    var result = criterias.Select(x => new CriteriaModelQuarter
                    {
                        Id = x.Id,
                        Description = x.Description,
                        Name = x.Name,
                        OrderNo = x.OrderNo,
                        QuarterEvaluationId = quarterId,
                        TypeId = x.TypeId,
                        Point = quarterCiteriaDic.GetValueOrDefault(x.Id, null),
                    }).ToList();
                    return result;
                });

            return result;
        }

        public ResultModel<bool> CreateLeaderEvaluation(Guid quarterEvaluationId, List<QuarterPoint> model)
        {
            QuarterEvaluation quarter = null;
            var result = _logicService.Start()
                .ThenAuthorize(Roles.ADMINISTRATOR, Roles.USER)
                .ThenValidate(x =>
                {
                    if (quarterEvaluationId == Guid.Empty)
                    {
                        return new ErrorModel(ErrorType.BAD_REQUEST, "MODEL_INVALID");
                    }
                    if ((x.Role != Roles.ADMINISTRATOR) && _quarterEvaluationRepository.Query().Any(t => t.Id == quarterEvaluationId && t.ProjectLeaderId != x.Id))
                    {
                        return new ErrorModel(ErrorType.BAD_REQUEST, "NOT_THE_PROJECT_LEADER_OR_ADMIN");
                    }
                    //if (quarter != null && (DateTime.Now - quarter.CreatedDate).TotalHours > 168)
                    //{
                    //    return new ErrorModel(ErrorType.BAD_REQUEST, "NOT_INTIME_EVALUATION");
                    //}
                    return null;
                })
                .ThenImplement(x =>
                {
                    var exist = _criteriaQuarterRepository.Query().Any(t => t.QuarterEvaluationId == quarterEvaluationId);
                    var valueDic = model.ToDictionary(x => x.Id, x => x.Point);

                    if (!exist)
                    {
                        quarter = _quarterEvaluationRepository.Query(t => t.Id == quarterEvaluationId).FirstOrDefault();
                        QuarterCriteriaTemplateMD quarterCriteriaTemplate = _logicService.Cache.QuarterCriteriaTemplates.GetValues().Where(x =>
                                x.PositionId == quarter.PositionId &&
                                x.Year == quarter.Year &&
                                x.Quarter == quarter.Quarter &&
                                x.DeletedBy == null && x.DeletedDate == null
                        ).FirstOrDefault();

                        var activeType = _logicService.Cache.CriteriaTypes.GetValues().Where(x => x.QuarterCriteriaTemplateId == quarterCriteriaTemplate.Id && !x.DeletedDate.HasValue).Select(t => t.Id).ToList();
                        var criterias = _logicService.Cache.Criterias.GetValues().Where(x => !x.DeletedDate.HasValue
                            && activeType.Contains(x.CriteriaTypeId)).ToList();

                        var criteriaQuarter = criterias.Select(t => new CriteriaQuarterEvaluation
                        {
                            Id = Guid.NewGuid(),
                            QuarterEvaluationId = quarterEvaluationId,
                            CreatedBy = x.Id,
                            CriteriaId = t.Id,
                            CreatedDate = DateTime.Now,
                            Point = valueDic.GetValueOrDefault(t.Id, 0),
                        }).ToList();

                        _criteriaQuarterRepository.InsertRange(criteriaQuarter);
                    }
                    else
                    {
                        var criteriaQuarter = _criteriaQuarterRepository.Query(t => t.QuarterEvaluationId == quarterEvaluationId).Distinct(x => x.CriteriaId).ToList();
                        criteriaQuarter.ForEach(t =>
                        {
                            if (valueDic.TryGetValue(t.CriteriaId, out int value))
                            {
                                t.Point = value;
                            }
                        });
                        _dbContext.Save();
                    }
                    UpdateCriteriaTypeEvaluation(quarterEvaluationId);
                    return true;
                });

            return result;
        }

        public ResultModel<DataSourcePersonalEvaluateModel> GetAvaiableQuarterEvaluations(int userId)
        {
            var now = DateTime.UtcNow;
            var result = _logicService
                .Start()
                .ThenAuthorize(Roles.USER, Roles.ADMINISTRATOR)
                .ThenValidate(currentUser =>
                {
                    //token and userId not equals
                    if (currentUser.Id != userId)
                    {
                        return new ErrorModel(ErrorType.CONFLICTED, "UNABLE TO ACCESS");
                    }

                    return null;
                })
                .ThenImplement(currentUser =>
                {

                    var allUserRecords = _quarterEvaluationRepository.Query().Where(t => t.UserId == userId).ToList();

                    var allOutdatedUserQuaterEvaluationRecords = _userQuarterEvaluationRespository.Query()
                        .Where(t => allUserRecords.Select(x => x.Id).Contains(t.QuarterEvaluationId)).ToList();

                    var allProjects = _projects.Query().ToList();

                    //All avaiable records can create new and edit.
                    allUserRecords = allUserRecords
                        .Where(t => !allOutdatedUserQuaterEvaluationRecords.Select(x => x.QuarterEvaluationId)
                        .Contains(t.Id)).ToList();

                    if (allUserRecords.Count == 0)
                        return new DataSourcePersonalEvaluateModel
                        {
                            IsAvaibleEvaluate = false,
                            DataSource = new List<QuarterEvaluationModel>(),
                            ProjectSource = new List<ProjectDataSourceModel>(),
                            QuarterSource = new List<QuarterDataSourceModel>(),
                            YearSource = new List<YearDataSourceModel>()
                        };

                    #region PREPARE DATA

                    var dataSource = allUserRecords
                        .Select(t => new QuarterEvaluationModel
                        {
                            Id = t.Id,
                            ProjectLeaderId = t.ProjectLeaderId,
                            Quarter = t.Quarter,
                            Year = t.Year,
                            UserId = t.UserId,
                            ProjectId = t.ProjectId
                        }).ToList();


                    var datasourceProject = new List<ProjectDataSourceModel>();
                    var datasourceQuarter = new List<QuarterDataSourceModel>();
                    var datasourceYear = new List<YearDataSourceModel>();

                    var counter = 0;
                    var groupedRecords = allUserRecords.GroupBy(t => new { t.Year }).ToList();
                    groupedRecords.ForEach(t =>
                    {
                        counter++;
                        var yearModel = new YearDataSourceModel()
                        {
                            Id = counter,
                            Value = t.Key.Year.ToString()
                        };
                        datasourceYear.Add(yearModel);

                        var groupByQuarter = t.GroupBy(t => t.Quarter).ToList();
                        groupByQuarter.ForEach(g =>
                        {
                            var counterQuarter = datasourceQuarter.Count;
                            var quarterModel = new QuarterDataSourceModel()
                            {
                                Id = ++counterQuarter,
                                YearId = counter,
                                Value = g.Key.ToString()
                            };
                            datasourceQuarter.Add(quarterModel);

                            var counterProject = datasourceProject.Count;
                            var projectModel = g.Select(x =>
                                new ProjectDataSourceModel()
                                {
                                    Id = ++counterProject,
                                    YearId = counter,
                                    QuarterId = counterQuarter,
                                    Value = allProjects.Where(t => t.Id == x.ProjectId).FirstOrDefault().Name,
                                    ProjectId = x.ProjectId
                                }
                            ).ToList();
                            datasourceProject.AddRange(projectModel);
                        });
                    });


                    #endregion


                    return new DataSourcePersonalEvaluateModel
                    {
                        IsAvaibleEvaluate = true,
                        DataSource = dataSource,
                        ProjectSource = datasourceProject,
                        QuarterSource = datasourceQuarter,
                        YearSource = datasourceYear
                    };
                });
            return result;
        }

        private void UpdateCriteriaTypeEvaluation(Guid quarterEvaluationId)
        {
            try
            {
                var quarterEvaluation = _quarterEvaluationRepository.Query(x => !x.DeletedDate.HasValue && x.Id == quarterEvaluationId).FirstOrDefault();

                var isExist = _criteriaTypeQuarterRepository.Query().Any(x => x.QuarterEvaluationId == quarterEvaluationId);
                var quarterCriteria = _criteriaQuarterRepository.Query(x => x.QuarterEvaluationId == quarterEvaluationId).ToList();
                var criteriaType = _criteriaTypeRepository.AllIncluding(x => x.Criterias)
                    .ToDictionary(x => x.Id, x => x.Criterias.ToList());
                if (isExist)
                {
                    var criteriaTypeQuarter = _criteriaTypeQuarterRepository.Query(x => x.QuarterEvaluationId == quarterEvaluationId).ToList();

                    criteriaTypeQuarter.ForEach(x =>
                    {
                        if (criteriaType.TryGetValue(x.CriteriaTypeId, out List<Criteria> value))
                        {
                            var data = quarterCriteria.Where(t => value.Select(a => a.Id).Contains(t.CriteriaId));
                            if (data.Any())
                            {
                                x.PointAverage = Math.Round((float)data.Sum(t => t.Point) / data.Count(), 2);
                            }
                        }
                    });

                    if (quarterEvaluation != null)
                    {
                        quarterEvaluation.PointAverage = Math.Round((float)criteriaTypeQuarter.Sum(x => x.PointAverage) / (criteriaTypeQuarter.Count == 0 ? 1 : criteriaTypeQuarter.Count), 2);
                    }
                }
                else
                {
                    var criterias = _criteriaRepository.Query(x => x.DeletedBy == null && x.DeletedDate == null).ToList();
                    var criteriaTypeId = criterias
                        .Where(t => quarterCriteria.Select(x => x.CriteriaId).Contains(t.Id))
                        .GroupBy(x => x.CriteriaTypeId)
                        .Select(x => x.Key)
                        .ToList();

                    criteriaTypeId.ForEach(t =>
                    {
                        var listCriteriaId = criterias.Where(x => x.CriteriaTypeId == t).Select(x => x.Id).ToList();
                        var quarterCriteriaItems = quarterCriteria.Where(x => listCriteriaId.Contains(x.CriteriaId)).ToList();
                        var model = new CriteriaTypeQuarterEvaluation
                        {
                            Id = Guid.NewGuid(),
                            CreatedBy = 1,
                            CreatedDate = DateTime.Now,
                            CriteriaTypeId = t,
                            QuarterEvaluationId = quarterEvaluationId,
                            PointAverage = Math.Round((float)quarterCriteriaItems.Sum(x => x.Point) / (quarterCriteriaItems.Count == 0 ? 1 : quarterCriteriaItems.Count), 2)
                        };
                        _criteriaTypeQuarterRepository.Add(model);
                    });
                    // save to database before update quarter
                    _dbContext.Save();
                    var count = criteriaTypeId.Count == 0 ? 1 : criteriaTypeId.Count;
                    var total = _criteriaTypeQuarterRepository
                        .Query(x => criteriaTypeId.Contains(x.CriteriaTypeId) && x.QuarterEvaluationId == quarterEvaluationId)
                        .Sum(t => t.PointAverage);
                    quarterEvaluation.PointAverage = Math.Round(total / count, 2);
                }

                _dbContext.Save();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        #region PRIVATE METHOD  
        public static ErrorModel Validate(UserQuarterEvaluationModel model)
        {
            if (string.IsNullOrEmpty(model.NoteGoodThing))
            {
                return new ErrorModel(ErrorType.BAD_REQUEST, "INVALID_MODEL_NOTE_GOOD_THING_NULL");
            }
            else
            if (model.NoteGoodThing.Length > 1000)
            {
                return new ErrorModel(ErrorType.BAD_REQUEST, "INVALID_MODEL_NOTE_GOOD_THING_MAX_LENGTH");
            }
            if (string.IsNullOrEmpty(model.NoteBadThing))
            {
                return new ErrorModel(ErrorType.BAD_REQUEST, "INVALID_MODEL_NOTE_BAD_THING_NULL");
            }
            else
           if (model.NoteBadThing.Length > 1000)
            {
                return new ErrorModel(ErrorType.BAD_REQUEST, "INVALID_MODEL_NOTE_BAD_THING_MAX_LENGTH");
            }
            return null;
        }
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
        private List<CriteriaModel> GetCriteriaModels(Guid quarterId)
        {
            var quarterEvalution = _quarterEvaluationRepository.Query(x => x.Id == quarterId).FirstOrDefault();
            if (quarterEvalution != null)
            {
                var quarterCriteriaTemplate = _logicService.Cache.QuarterCriteriaTemplates.GetValues().Where(x =>
                        x.PositionId == quarterEvalution.PositionId &&
                        x.Year == quarterEvalution.Year &&
                        x.Quarter == quarterEvalution.Quarter &&
                        x.DeletedBy == null && x.DeletedDate == null
                ).FirstOrDefault();

                List<CriteriaModel> data = new List<CriteriaModel>();

                List<CriteriaModel> criteriaTypeModels = _logicService.Cache.CriteriaTypes.GetValues()
                            .Where(x => x.DeletedBy == null && x.DeletedDate == null && x.QuarterCriteriaTemplateId == quarterCriteriaTemplate.Id)
                            .Select(x => new CriteriaModel()
                            {
                                Id = x.Id,
                                TypeId = null,
                                Description = x.Description,
                                OrderNo = x.OrderNo,
                                Name = x.Name
                            })
                            .ToList();

                if (criteriaTypeModels.Count > 0)
                {
                    data.AddRange(criteriaTypeModels);
                    List<Guid> criteriaTypeIds = criteriaTypeModels.Select(x => x.Id).ToList();
                    List<CriteriaModel> criteriaModels = _logicService.Cache.Criterias.GetValues()
                            .Where(x => x.DeletedBy == null && x.DeletedDate == null && criteriaTypeIds.Contains(x.CriteriaTypeId))
                            .Select(x => new CriteriaModel()
                            {
                                Id = x.Id,
                                TypeId = x.CriteriaTypeId,
                                Description = x.Description,
                                OrderNo = x.OrderNo,
                                Name = x.Name
                            })
                            .ToList();
                    if (criteriaModels.Count > 0)
                    {
                        data.AddRange(criteriaModels);
                    }
                }
                return data;
            }
            return new List<CriteriaModel>();
        }
        #endregion

    }
}


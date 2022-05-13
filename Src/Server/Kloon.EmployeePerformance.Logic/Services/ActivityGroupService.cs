using Kloon.EmployeePerformance.DataAccess;
using Kloon.EmployeePerformance.DataAccess.Domain;
using Kloon.EmployeePerformance.Logic.Caches.Data;
using Kloon.EmployeePerformance.Logic.Services.Base;
using Kloon.EmployeePerformance.Models.Common;
using Kloon.EmployeePerformance.Models.TimeSheet;
using Kloon.EmployeePerformance.Models.User;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kloon.EmployeePerformance.Logic.Services
{
    public interface IActivityGroupService
    {
        ResultModel<ActivityGroupModel> Create(ActivityGroupModel model);
        ResultModel<List<ActivityGroupModel>> GetAll();
        ResultModel<ActivityGroupModel> GetById(Guid Id);
        ResultModel<ActivityGroupModel> Update(ActivityGroupModel model);
        ResultModel<bool> Delete(Guid Id);
        ResultModel<List<ActivityGroupModel>> GetActivityGroupModelForTimeSheetReport();
        ResultModel<List<ActivityGroupModel>> GetActivityGroupForReportProject();
    }

    public class ActivityGroupService : IActivityGroupService
    {
        private readonly IAuthenLogicService<ActivityGroupService> _logicService;
        private readonly IUnitOfWork<EmployeePerformanceContext> _dbContext;
        private readonly IEntityRepository<TSActivityGroup> _activityGroupRepository;
        private readonly IEntityRepository<TSActivity> _activityRepository;
        private readonly IEntityRepository<ActivityGroupUser> _activityGroupUserRepository;

        public ActivityGroupService(IAuthenLogicService<ActivityGroupService> logicService)
        {
            _logicService = logicService;
            _dbContext = logicService.DbContext;
            _activityGroupRepository = _dbContext.GetRepository<TSActivityGroup>();
            _activityRepository = _dbContext.GetRepository<TSActivity>();
            _activityGroupUserRepository = _dbContext.GetRepository<ActivityGroupUser>();
        }

        public ResultModel<ActivityGroupModel> Create(ActivityGroupModel model)
        {
            var result = _logicService
                .Start()
                .ThenAuthorize(Roles.ADMINISTRATOR)
                .ThenValidate(c =>
                {
                    if (string.IsNullOrEmpty(model.Name))
                        return new ErrorModel(ErrorType.BAD_REQUEST, "Name is required");
                    var existedProject = _logicService.Cache.Projects.GetValues().Where(x => !x.DeletedDate.HasValue && x.Name.Equals(model.Name.Trim(), StringComparison.OrdinalIgnoreCase)).ToList();
                    if (existedProject.Count > 0)
                        return new ErrorModel(ErrorType.DUPLICATED, "Project Name existed");
                    if (model.Name.Length < 2 || model.Name.Length > 50)
                        return new ErrorModel(ErrorType.BAD_REQUEST, "Name character is too short or too long");
                    var existedActivityGroup = _logicService.Cache.TSActivityGroups.GetValues().Where(x => !x.DeletedDate.HasValue).Any(x => x.Name.Equals(model.Name.Trim(), StringComparison.OrdinalIgnoreCase));
                    if (existedActivityGroup)
                        return new ErrorModel(ErrorType.DUPLICATED, "Activity Group Name existed");
                    return null;
                })
                .ThenImplement(c =>
                {
                    TSActivityGroup activityGroup = new()
                    {
                        ProjectId = model.ProjectId,
                        Name = model.Name.Trim(),
                        Description = model.Description,
                        CreatedBy = c.Id,
                        CreatedDate = DateTime.UtcNow,
                    };
                    _activityGroupRepository.Add(activityGroup);
                    _dbContext.Save();
                    model.Id = activityGroup.Id;
                    _logicService.Cache.TSActivityGroups.Clear();
                    return model;
                });
            return result;
        }

        public ResultModel<List<ActivityGroupModel>> GetAll()
        {
            var result = _logicService
                .Start()
                .ThenAuthorize(Roles.ADMINISTRATOR)
                .ThenValidate(c => null)
                .ThenImplement(c =>
                {
                    var query = _activityGroupRepository.Query(x => !x.DeletedDate.HasValue).ToList();
                    List<ActivityGroupModel> listActivityGroupModel = new();
                    listActivityGroupModel = query.Select(t => new ActivityGroupModel
                    {
                        Id = t.Id,
                        ProjectId = t.ProjectId,
                        Name = t.Name,
                        Description = t.Description,
                        Activities = null,
                        ActivityGroupUserModels = null,
                    }).ToList();
                    listActivityGroupModel = listActivityGroupModel.OrderByDescending(x => x.ProjectId.HasValue).ThenBy(x => x.Name).ToList();
                    return listActivityGroupModel;
                });
            return result;
        }

        public ResultModel<ActivityGroupModel> GetById(Guid Id)
        {
            TSActivityGroup activityGroup = new();
            var result = _logicService
                .Start()
                .ThenAuthorize(Roles.ADMINISTRATOR)
                .ThenValidate(c =>
                {
                    activityGroup = _activityGroupRepository.Query(x => !x.DeletedDate.HasValue).Where(x => x.Id == Id).FirstOrDefault();
                    if (activityGroup == null)
                        return new ErrorModel(ErrorType.NOT_EXIST, "Activity Group not found");
                    return null;
                })
                .ThenImplement(c =>
                {
                    var listUser = _logicService.Cache.Users.GetValues().Where(x => !x.DeletedDate.HasValue).ToList();

                    var listActivityDomain = _activityRepository.Query(x => !x.DeletedDate.HasValue && x.TSActivityGroupId == Id).ToList();
                    List<ActivityModel> listActivityModel = new();
                    if (listActivityDomain.Count > 0)
                    {
                        listActivityModel = listActivityDomain.Select(t => new ActivityModel
                        {
                            Id = t.Id,
                            ActivityGroupId = t.TSActivityGroupId,
                            Name = t.Name,
                            Description = t.Description,
                        }).ToList();
                    }

                    var listActivityGroupUserDomain = _activityGroupUserRepository.Query(x => !x.DeletedDate.HasValue && x.TSActivityGroupId == Id).ToList();
                    List<ActivityGroupUserModel> listActivityGroupUserModel = new();
                    if (listActivityGroupUserDomain.Count > 0)
                    {
                        foreach (var item in listActivityGroupUserDomain)
                        {
                            var listUserInGroup = listUser.Where(t => t.Id == item.UserId).ToList();
                            if (listUserInGroup.Count > 0)
                            {
                                ActivityGroupUserModel user = new()
                                {
                                    Id = item.Id,
                                    ActivityGroupId = item.TSActivityGroupId,
                                    UserId = item.UserId,
                                    Role = (TSRole)item.Role,
                                    FullName = listUser.Where(x => x.Id == item.UserId).FirstOrDefault().FullName,
                                    Email = listUser.Where(x => x.Id == item.UserId).FirstOrDefault().Email,
                                };
                                listActivityGroupUserModel.Add(user);
                            }
                        };
                    }
                    ActivityGroupModel groupModel = new()
                    {
                        Id = activityGroup.Id,
                        ProjectId = activityGroup.ProjectId,
                        Name = activityGroup.Name,
                        Description = activityGroup.Description,
                        Activities = listActivityModel,
                        ActivityGroupUserModels = listActivityGroupUserModel,
                    };
                    return groupModel;
                });
            return result;
        }

        public ResultModel<ActivityGroupModel> Update(ActivityGroupModel model)
        {
            TSActivityGroup activityGroup = new();
            var result = _logicService
                .Start()
                .ThenAuthorize(Roles.ADMINISTRATOR)
                .ThenValidate(c =>
                {
                    activityGroup = _activityGroupRepository.Query(x => !x.DeletedDate.HasValue).Where(x => x.Id == model.Id).FirstOrDefault();
                    if (activityGroup == null)
                        return new ErrorModel(ErrorType.NOT_EXIST, "Activity Group not found");
                    if (model.ProjectId != null)
                        return new ErrorModel(ErrorType.BAD_REQUEST, "Inediteable Group");
                    if (string.IsNullOrEmpty(model.Name))
                        return new ErrorModel(ErrorType.BAD_REQUEST, "Name is required");
                    var existedProject = _logicService.Cache.Projects.GetValues().Where(x => !x.DeletedDate.HasValue && x.Name.Equals(model.Name.Trim(), StringComparison.OrdinalIgnoreCase)).ToList();
                    if (existedProject.Count > 0)
                        return new ErrorModel(ErrorType.DUPLICATED, "Project Name existed");
                    if (model.Name.Length < 2 || model.Name.Length > 50)
                        return new ErrorModel(ErrorType.BAD_REQUEST, "Name character is too short or too long");
                    var existedActivityGroup = _logicService.Cache.TSActivityGroups.GetValues().Where(x => !x.DeletedDate.HasValue).Any(x => x.Id != model.Id && x.Name.Equals(model.Name.Trim(), StringComparison.OrdinalIgnoreCase));
                    if (existedActivityGroup)
                        return new ErrorModel(ErrorType.DUPLICATED, "Activity Group Name existed");
                    return null;
                })
                .ThenImplement(c =>
                {
                    activityGroup.Name = model.Name.Trim();
                    activityGroup.Description = model.Description;
                    activityGroup.ModifiedDate = DateTime.UtcNow;
                    activityGroup.ModifiedBy = c.Id;
                    _dbContext.Save();
                    _logicService.Cache.TSActivityGroups.Clear();
                    return model;
                });
            return result;
        }

        public ResultModel<bool> Delete(Guid Id)
        {
            TSActivityGroup activityGroup = new();
            var result = _logicService
                .Start()
                .ThenAuthorize(Roles.ADMINISTRATOR)
                .ThenValidate(c =>
                {
                    activityGroup = _activityGroupRepository.Query(x => !x.DeletedDate.HasValue && x.Id == Id).FirstOrDefault();
                    if (activityGroup == null)
                        return new ErrorModel(ErrorType.NOT_EXIST, "Activity Group not found");
                    var check = _logicService.Cache.TSActivityGroups.GetValues().Where(x => !x.DeletedDate.HasValue && x.Id == Id).FirstOrDefault();
                    if (check.ProjectId != null)
                        return new ErrorModel(ErrorType.BAD_REQUEST, "Undeleteable Group");
                    return null;
                })
                .ThenImplement(c =>
                {
                    List<TSActivity> listActivityDomain = _activityRepository.Query(x => !x.DeletedDate.HasValue && x.TSActivityGroupId == Id).ToList();
                    List<ActivityGroupUser> listActivityGroupUserDomain = _activityGroupUserRepository.Query(x => !x.DeletedDate.HasValue && x.TSActivityGroupId == Id).ToList();
                    if (listActivityDomain.Count > 0)
                    {
                        foreach (var item in listActivityDomain)
                        {
                            item.DeletedBy = c.Id;
                            item.DeletedDate = DateTime.UtcNow;
                        }
                    }
                    if (listActivityGroupUserDomain.Count > 0)
                    {
                        foreach (var item in listActivityGroupUserDomain)
                        {
                            item.DeletedBy = c.Id;
                            item.DeletedDate = DateTime.UtcNow;
                        }
                    }
                    activityGroup.DeletedBy = c.Id;
                    activityGroup.DeletedDate = DateTime.UtcNow;
                    _dbContext.Save();
                    _logicService.Cache.TSActivityGroups.Clear();
                    return true;
                });
            return result;
        }

        public ResultModel<List<ActivityGroupModel>> GetActivityGroupModelForTimeSheetReport()
        {
            var result = _logicService
                .Start()
                .ThenAuthorize(Roles.ADMINISTRATOR, Roles.USER)
                .ThenValidate(currentUser =>
                {
                    return null;
                })
                .ThenImplement(currentUser =>
                {
                    List<ActivityGroupModel> listResult = new();
                    var allGroup = _activityGroupRepository.Query(x => !x.DeletedDate.HasValue).ToList();
                    if (currentUser.Role == Roles.ADMINISTRATOR)
                    {
                        listResult = allGroup.OrderByDescending(x => x.ProjectId.HasValue).ThenBy(x => x.Name)
                           .Select(t => new ActivityGroupModel
                           {
                               Id = t.Id,
                               ProjectId = t.ProjectId,
                               Name = t.Name,
                               Description = t.Description,
                               Activities = null,
                               ActivityGroupUserModels = null,
                           }).ToList();
                    }
                    else
                    {
                        var listProjectId = _logicService.Cache.Users.GetProjects(currentUser.Id).Select(t => t.Id).ToList();
                        var listGroupId = _activityGroupUserRepository.Query(x => !x.DeletedDate.HasValue && x.UserId == currentUser.Id)
                            .Select(x => x.TSActivityGroupId).ToList();
                        var allProjectGroup = allGroup.Where(x => x.ProjectId == null || listProjectId.Contains(x.ProjectId.Value) || listGroupId.Contains(x.Id)).ToList();
                        listResult = allProjectGroup.OrderByDescending(x => x.ProjectId.HasValue).ThenBy(x => x.Name)
                            .Select(t => new ActivityGroupModel
                            {
                                Id = t.Id,
                                ProjectId = t.ProjectId,
                                Name = t.Name,
                                Description = t.Description,
                                Activities = null,
                                ActivityGroupUserModels = null,
                            }).ToList();
                    }
                    return listResult;
                });
            return result;
        }

        public ResultModel<List<ActivityGroupModel>> GetActivityGroupForReportProject()
        {
            var result = _logicService
                .Start()
                .ThenAuthorize(Roles.ADMINISTRATOR, Roles.USER)
                .ThenValidate(currentUser =>
                {
                    return null;
                })
                .ThenImplement(currentUser =>
                {
                    List<ActivityGroupModel> listResult = new();
                    var allGroup = _activityGroupRepository.Query(x => !x.DeletedDate.HasValue).ToList();
                    if (currentUser.Role == Roles.ADMINISTRATOR)
                    {
                        listResult = allGroup.OrderByDescending(x => x.ProjectId.HasValue).ThenBy(x => x.Name)
                           .Select(t => new ActivityGroupModel
                           {
                               Id = t.Id,
                               ProjectId = t.ProjectId,
                               Name = t.Name,
                               Description = t.Description,
                               Activities = null,
                               ActivityGroupUserModels = null,
                           }).ToList();
                    }
                    else
                    {
                        var ActivityGroupIds = _activityGroupUserRepository.Query(x => !x.DeletedDate.HasValue && x.UserId == currentUser.Id).Select(x => x.TSActivityGroupId).ToList();
                        if (ActivityGroupIds.Count > 0)
                        {
                            listResult = allGroup.Where(x => ActivityGroupIds.Contains(x.Id)).OrderByDescending(x => x.ProjectId.HasValue).ThenBy(x => x.Name)
                               .Select(t => new ActivityGroupModel
                               {
                                   Id = t.Id,
                                   ProjectId = t.ProjectId,
                                   Name = t.Name,
                                   Description = t.Description,
                                   Activities = null,
                                   ActivityGroupUserModels = null,
                               }).ToList();
                        }
                    }
                    return listResult;
                });
            return result;
        }
    }
}

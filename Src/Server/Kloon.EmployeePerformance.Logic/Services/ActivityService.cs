using Kloon.EmployeePerformance.DataAccess;
using Kloon.EmployeePerformance.DataAccess.Domain;
using Kloon.EmployeePerformance.Logic.Caches.Data;
using Kloon.EmployeePerformance.Logic.Services.Base;
using Kloon.EmployeePerformance.Models.Common;
using Kloon.EmployeePerformance.Models.TimeSheet;
using Kloon.EmployeePerformance.Models.TimeSheet.TimeSheetRecord;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kloon.EmployeePerformance.Logic.Services
{
    public interface IActivityService
    {
        ResultModel<ActivityModel> Create(ActivityModel model);
        ResultModel<List<ActivityRecordViewModel>> GetCurrentUserActivity();
        ResultModel<ActivityModel> Update(ActivityModel model);
        ResultModel<bool> Delete(Guid Id);
        ResultModel<List<ActivityRecordViewModel>> GetListActivityName(List<Guid> groupIds);
        ResultModel<List<ActivityRecordViewModel>> GetActivityForReportProject(List<Guid> groupIds);
        ResultModel<List<ActivityRecordViewModel>> GetActivityByUserId(int Id);
    }
    public class ActivityService : IActivityService
    {
        private readonly IAuthenLogicService<ActivityService> _logicService;
        private readonly IUnitOfWork<EmployeePerformanceContext> _dbContext;
        private readonly IEntityRepository<TSActivityGroup> _activityGroupRepository;
        private readonly IEntityRepository<TSActivity> _activityRepository;
        private readonly IEntityRepository<ActivityGroupUser> _activityGroupUsers;

        public ActivityService(IAuthenLogicService<ActivityService> logicService)
        {
            _logicService = logicService;
            _dbContext = logicService.DbContext;
            _activityGroupRepository = _dbContext.GetRepository<TSActivityGroup>();
            _activityRepository = _dbContext.GetRepository<TSActivity>();
            _activityGroupUsers = _dbContext.GetRepository<ActivityGroupUser>();
        }

        public ResultModel<ActivityModel> Create(ActivityModel model)
        {
            var result = _logicService
                .Start()
                .ThenAuthorize(Roles.ADMINISTRATOR)
                .ThenValidate(c =>
                {
                    var isExistedActivityGroup = _logicService.Cache.TSActivityGroups.GetValues().Where(x => !x.DeletedDate.HasValue).Any(x => x.Id == model.ActivityGroupId);
                    if (!isExistedActivityGroup)
                        return new ErrorModel(ErrorType.NOT_EXIST, "Activity Group not found");
                    if (string.IsNullOrEmpty(model.Name))
                        return new ErrorModel(ErrorType.BAD_REQUEST, "Name is required");
                    if (model.Name.Length < 2 || model.Name.Length > 50)
                        return new ErrorModel(ErrorType.BAD_REQUEST, "Name character is too short or too long");
                    var existedActivity = _logicService.Cache.TSActivities.GetValues().Where(x => !x.DeletedDate.HasValue && x.TSActivityGroupId == model.ActivityGroupId)
                        .Any(x => x.Name.Equals(model.Name.Trim(), StringComparison.OrdinalIgnoreCase));
                    if (existedActivity)
                        return new ErrorModel(ErrorType.DUPLICATED, "Activity Name existed");
                    return null;
                })
                .ThenImplement(c =>
                {
                    TSActivity activity = new()
                    {
                        TSActivityGroupId = model.ActivityGroupId,
                        Name = model.Name.Trim(),
                        Description = model.Description,
                        CreatedBy = c.Id,
                        CreatedDate = DateTime.UtcNow,
                    };
                    _activityRepository.Add(activity);
                    _dbContext.Save();
                    model.Id = activity.Id;
                    _logicService.Cache.TSActivities.Clear();
                    _logicService.Cache.TSActivityGroups.Clear();
                    return model;
                });
            return result;
        }

        public ResultModel<ActivityModel> Update(ActivityModel model)
        {
            TSActivity activity = new();
            var result = _logicService
                .Start()
                .ThenAuthorize(Roles.ADMINISTRATOR)
                .ThenValidate(c =>
                {
                    var isExistedActivityGroup = _logicService.Cache.TSActivityGroups.GetValues().Where(x => !x.DeletedDate.HasValue).Any(x => x.Id == model.ActivityGroupId);
                    if (!isExistedActivityGroup)
                        return new ErrorModel(ErrorType.NOT_EXIST, "Activity Group not found");
                    activity = _activityRepository.Query(x => !x.DeletedDate.HasValue && x.Id == model.Id).FirstOrDefault();
                    if (activity == null)
                        return new ErrorModel(ErrorType.NOT_EXIST, "Activity not found");
                    if (string.IsNullOrEmpty(model.Name))
                        return new ErrorModel(ErrorType.BAD_REQUEST, "Name is required");
                    if (model.Name.Length < 2 || model.Name.Length > 50)
                        return new ErrorModel(ErrorType.BAD_REQUEST, "Name character is too short or too long");
                    var existedActivity = _logicService.Cache.TSActivities.GetValues().Where(x => !x.DeletedDate.HasValue && x.TSActivityGroupId == model.ActivityGroupId)
                        .Any(x => x.Id != model.Id && x.Name.Equals(model.Name.Trim(), StringComparison.OrdinalIgnoreCase));
                    if (existedActivity)
                        return new ErrorModel(ErrorType.DUPLICATED, "Activity Name existed");
                    return null;
                })
                .ThenImplement(c =>
                {
                    activity.Name = model.Name.Trim();
                    activity.Description = model.Description;
                    activity.ModifiedBy = c.Id;
                    activity.ModifiedDate = DateTime.UtcNow;
                    _dbContext.Save();
                    _logicService.Cache.TSActivities.Clear();
                    _logicService.Cache.TSActivityGroups.Clear();
                    return model;
                });
            return result;
        }

        public ResultModel<bool> Delete(Guid Id)
        {
            TSActivity activity = new();
            var result = _logicService
                .Start()
                .ThenAuthorize(Roles.ADMINISTRATOR)
                .ThenValidate(c =>
                {
                    activity = _activityRepository.Query(x => !x.DeletedDate.HasValue && x.Id == Id).FirstOrDefault();
                    if (activity == null)
                        return new ErrorModel(ErrorType.NOT_EXIST, "Activity not found");
                    return null;
                })
                .ThenImplement(c =>
                {
                    activity.DeletedBy = c.Id;
                    activity.DeletedDate = DateTime.UtcNow;
                    _dbContext.Save();
                    _logicService.Cache.TSActivities.Clear();
                    _logicService.Cache.TSActivityGroups.Clear();
                    return true;
                });
            return result;
        }

        public ResultModel<List<ActivityRecordViewModel>> GetCurrentUserActivity()
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
                    //Get current user activity group
                    var lstProjectId = currentUser.ProjectRole.Select(t => t.ProjectId).ToList();
                    var currentUserGroupActivityId = _activityGroupRepository
                        .Query(t => lstProjectId.Contains((int)t.ProjectId) || t.ProjectId == null && !t.DeletedBy.HasValue && !t.DeletedDate.HasValue)
                        .Select(t => t.Id)
                        .ToList();
                    if (currentUserGroupActivityId.Count == 0)
                    {
                        return new List<ActivityRecordViewModel>();
                    }

                    var userAllActivity = currentUserGroupActivityId
                        .SelectMany(t => _logicService.Cache.TSActivityGroups.GetActivity(t))
                        .Select(t => new ActivityRecordViewModel
                        {
                            Id = t.Id,
                            DisplayName = t.Name,
                            GroupId = t.TSActivityGroupId,
                            GroupName = _logicService.Cache.TSActivityGroups.Get(t.TSActivityGroupId).Name,
                            DeletedBy = t.DeletedBy,
                            ProjectId = _logicService.Cache.TSActivityGroups.Get(t.TSActivityGroupId).ProjectId,
                        })
                        .ToList();
                    return userAllActivity;
                });
            return result;
        }

        public ResultModel<List<ActivityRecordViewModel>> GetListActivityName(List<Guid> groupIds)
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
                    List<ActivityRecordViewModel> listResult = new();
                    var allGroup = _activityGroupRepository.Query(x => !x.DeletedDate.HasValue).ToList();
                    //Check params
                    if (groupIds != null && groupIds.Count > 0)
                    {
                        allGroup = allGroup.Where(x => groupIds.Contains(x.Id)).ToList();
                    }
                    //Check Role
                    if (currentUser.Role == Roles.USER)
                    {
                        var listManagerGroupId = _activityGroupUsers.Query(x => !x.DeletedBy.HasValue && x.UserId == currentUser.Id).Select(t => t.TSActivityGroupId).ToList();
                        var listProjectId = _logicService.Cache.Users.GetProjects(currentUser.Id).Select(t => t.Id).ToList();
                        if (listManagerGroupId.Count == 0)
                        {
                            if (listProjectId.Count > 0)
                            {
                                var allProjectGroupId = allGroup.Where(x => x.ProjectId == null || listProjectId.Contains(x.ProjectId.Value))
                                    .OrderByDescending(x => x.ProjectId.HasValue).ThenBy(x => x.Name).Select(t => t.Id).ToList();
                                if (allProjectGroupId.Count == 0)
                                    return new List<ActivityRecordViewModel>();
                                listResult = allProjectGroupId.SelectMany(t => _logicService.Cache.TSActivityGroups.GetActivity(t).Where(x => !x.DeletedBy.HasValue && !x.DeletedDate.HasValue))
                                    .Select(t => new ActivityRecordViewModel
                                    {
                                        Id = t.Id,
                                        DisplayName = t.Name,
                                        GroupId = t.TSActivityGroupId,
                                        GroupName = _logicService.Cache.TSActivityGroups.Get(t.TSActivityGroupId).Name
                                    }).ToList();
                            }
                            else
                            {
                                var commonGroupId = allGroup.Where(x => x.ProjectId == null)
                                    .OrderByDescending(x => x.ProjectId.HasValue).ThenBy(x => x.Name).Select(t => t.Id).ToList();
                                if (commonGroupId.Count == 0)
                                    return new List<ActivityRecordViewModel>();
                                listResult = commonGroupId.SelectMany(t => _logicService.Cache.TSActivityGroups.GetActivity(t).Where(x => !x.DeletedBy.HasValue && !x.DeletedDate.HasValue))
                                    .Select(t => new ActivityRecordViewModel
                                    {
                                        Id = t.Id,
                                        DisplayName = t.Name,
                                        GroupId = t.TSActivityGroupId,
                                        GroupName = _logicService.Cache.TSActivityGroups.Get(t.TSActivityGroupId).Name
                                    }).ToList();
                            }
                        }
                        else
                        {
                            var managerGroupId = allGroup.Where(x => x.ProjectId == null || listManagerGroupId.Contains(x.Id) || listProjectId.Contains(x.ProjectId.Value))
                                .OrderByDescending(x => x.ProjectId.HasValue).ThenBy(x => x.Name).Select(t => t.Id).ToList();
                            if (managerGroupId.Count == 0)
                                return new List<ActivityRecordViewModel>();
                            listResult = managerGroupId.SelectMany(t => _logicService.Cache.TSActivityGroups.GetActivity(t).Where(x => !x.DeletedBy.HasValue && !x.DeletedDate.HasValue))
                                .Select(t => new ActivityRecordViewModel
                                {
                                    Id = t.Id,
                                    DisplayName = t.Name,
                                    GroupId = t.TSActivityGroupId,
                                    GroupName = _logicService.Cache.TSActivityGroups.Get(t.TSActivityGroupId).Name
                                }).ToList();
                        }
                    }
                    else
                    {
                        var allGroupId = allGroup.OrderByDescending(x => x.ProjectId.HasValue).ThenBy(x => x.Name).Select(t => t.Id).ToList();
                        if (allGroupId.Count == 0)
                            return new List<ActivityRecordViewModel>();
                        listResult = allGroupId.SelectMany(t => _logicService.Cache.TSActivityGroups.GetActivity(t).Where(x => !x.DeletedBy.HasValue && !x.DeletedDate.HasValue))
                            .Select(t => new ActivityRecordViewModel
                            {
                                Id = t.Id,
                                DisplayName = t.Name,
                                GroupId = t.TSActivityGroupId,
                                GroupName = _logicService.Cache.TSActivityGroups.Get(t.TSActivityGroupId).Name
                            }).ToList();
                    }
                    return listResult;
                });
            return result;
        }

        public ResultModel<List<ActivityRecordViewModel>> GetActivityForReportProject(List<Guid> groupIds)
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
                      List<ActivityRecordViewModel> listResult = new();
                      var allGroup = _activityGroupRepository.Query(x => !x.DeletedDate.HasValue).ToList();
                      //Check params
                      if (groupIds != null && groupIds.Count > 0)
                      {
                          allGroup = allGroup.Where(x => groupIds.Contains(x.Id)).ToList();
                      }
                      //Check Role
                      if (currentUser.Role == Roles.USER)
                      {
                          var listManagerGroupId = _activityGroupUsers.Query(x => !x.DeletedBy.HasValue && x.UserId == currentUser.Id).Select(t => t.TSActivityGroupId).ToList();
                          if (listManagerGroupId.Count > 0)
                          {
                              var managerGroupId = allGroup.Where(x => listManagerGroupId.Contains(x.Id))
                                  .OrderByDescending(x => x.ProjectId.HasValue).ThenBy(x => x.Name).Select(t => t.Id).ToList();
                              if (managerGroupId.Count == 0)
                                  return new List<ActivityRecordViewModel>();
                              listResult = managerGroupId.SelectMany(t => _logicService.Cache.TSActivityGroups.GetActivity(t).Where(x => !x.DeletedBy.HasValue && !x.DeletedDate.HasValue))
                                  .Select(t => new ActivityRecordViewModel
                                  {
                                      Id = t.Id,
                                      DisplayName = t.Name,
                                      GroupId = t.TSActivityGroupId,
                                      GroupName = _logicService.Cache.TSActivityGroups.Get(t.TSActivityGroupId).Name
                                  }).ToList();
                          }
                      }
                      else
                      {
                          var allGroupId = allGroup.OrderByDescending(x => x.ProjectId.HasValue).ThenBy(x => x.Name).Select(t => t.Id).ToList();
                          if (allGroupId.Count == 0)
                              return new List<ActivityRecordViewModel>();
                          listResult = allGroupId.SelectMany(t => _logicService.Cache.TSActivityGroups.GetActivity(t).Where(x => !x.DeletedBy.HasValue && !x.DeletedDate.HasValue))
                              .Select(t => new ActivityRecordViewModel
                              {
                                  Id = t.Id,
                                  DisplayName = t.Name,
                                  GroupId = t.TSActivityGroupId,
                                  GroupName = _logicService.Cache.TSActivityGroups.Get(t.TSActivityGroupId).Name
                              }).ToList();
                      }
                      return listResult;
                  });
            return result;
        }

        public ResultModel<List<ActivityRecordViewModel>> GetActivityByUserId(int userId)
        {
            UserMD userMD = null;
            var result = _logicService
                .Start()
                .ThenAuthorize(Roles.ADMINISTRATOR)
                .ThenValidate(c =>
                {
                    userMD = _logicService.Cache.Users.Get(userId);
                    if (userMD == null)
                        return new ErrorModel(ErrorType.NOT_EXIST, "User not found");
                    return null;
                })
                .ThenImplement(c =>
                {
                    List<ActivityRecordViewModel> listResult = new();
                    var allGroup = _activityGroupRepository.Query(x => !x.DeletedDate.HasValue).ToList();

                    var listProjectId = _logicService.Cache.Users.GetProjects(userId).Select(t => t.Id).ToList();

                    if (listProjectId.Count > 0)
                    {
                        var allProjectGroupId = allGroup.Where(x => x.ProjectId == null || listProjectId.Contains(x.ProjectId.Value))
                            .OrderByDescending(x => x.ProjectId.HasValue).ThenBy(x => x.Name).Select(t => t.Id).ToList();
                        if (allProjectGroupId.Count == 0)
                            return new List<ActivityRecordViewModel>();
                        listResult = allProjectGroupId.SelectMany(t => _logicService.Cache.TSActivityGroups.GetActivity(t).Where(x => !x.DeletedBy.HasValue && !x.DeletedDate.HasValue))
                            .Select(t => new ActivityRecordViewModel
                            {
                                Id = t.Id,
                                DisplayName = t.Name,
                                GroupId = t.TSActivityGroupId,
                                GroupName = _logicService.Cache.TSActivityGroups.Get(t.TSActivityGroupId).Name
                            }).ToList();
                    }
                    else
                    {
                        var commonGroupId = allGroup.Where(x => x.ProjectId == null)
                            .OrderByDescending(x => x.ProjectId.HasValue).ThenBy(x => x.Name).Select(t => t.Id).ToList();
                        if (commonGroupId.Count == 0)
                            return new List<ActivityRecordViewModel>();
                        listResult = commonGroupId.SelectMany(t => _logicService.Cache.TSActivityGroups.GetActivity(t).Where(x => !x.DeletedBy.HasValue && !x.DeletedDate.HasValue))
                            .Select(t => new ActivityRecordViewModel
                            {
                                Id = t.Id,
                                DisplayName = t.Name,
                                GroupId = t.TSActivityGroupId,
                                GroupName = _logicService.Cache.TSActivityGroups.Get(t.TSActivityGroupId).Name
                            }).ToList();
                    }
                    return listResult;
                });
            return result;
        }
    }
}

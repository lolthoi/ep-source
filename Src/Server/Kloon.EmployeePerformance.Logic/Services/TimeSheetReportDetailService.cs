using Kloon.EmployeePerformance.DataAccess;
using Kloon.EmployeePerformance.DataAccess.Domain;
using Kloon.EmployeePerformance.Logic.Caches.Data;
using Kloon.EmployeePerformance.Logic.Services.Base;
using Kloon.EmployeePerformance.Models.Common;
using Kloon.EmployeePerformance.Models.TimeSheet.TimeSheetRecord;
using Kloon.EmployeePerformance.Models.TimeSheet.TimeSheetReport;
using Kloon.EmployeePerformance.Models.User;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kloon.EmployeePerformance.Logic.Services
{
    public interface ITimeSheetReportDetailService
    {
        ResultModel<PaginationSet<TimeSheetReportDetailModel>> GetAllRecordPaging(TimeSheetReportDetailRouter model);
        ResultModel<List<TimeSheetReportUserModel>> GetAllUserRecord(FilterParamUserRecord model);
        ResultModel<List<TimeSheetReportProjectModel>> GetAllProjectRecord(FilterParamProjectRecord model);
        ResultModel<List<TimeSheetReportUserActivityModel>> GetAllUserActivityRecord(FilterParamUserActivity model);
        ResultModel<List<TimeSheetReportProjectActivityModel>> GetAllProjectActivityRecord(FilterParamProjectActivity model);
    }

    public class TimeSheetReportDetailService : ITimeSheetReportDetailService
    {
        private readonly IAuthenLogicService<TimeSheetReportDetailService> _logicService;
        private readonly IUnitOfWork<EmployeePerformanceContext> _dbContext;
        private readonly IEntityRepository<ActivityGroupUser> _activityGroupUsers;
        private readonly IEntityRepository<TSActivityGroup> _tSActivityGroups;
        private readonly IEntityRepository<TSActivity> _tSActivities;
        private readonly IEntityRepository<TSRecord> _tsRecords;
        public TimeSheetReportDetailService(IAuthenLogicService<TimeSheetReportDetailService> logicService)
        {
            _logicService = logicService;
            _dbContext = logicService.DbContext;
            _activityGroupUsers = _dbContext.GetRepository<ActivityGroupUser>();
            _tSActivityGroups = _dbContext.GetRepository<TSActivityGroup>();
            _tSActivities = _dbContext.GetRepository<TSActivity>();
            _tsRecords = _dbContext.GetRepository<TSRecord>();
        }
        public ResultModel<PaginationSet<TimeSheetReportDetailModel>> GetAllRecordPaging(TimeSheetReportDetailRouter model)
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
                    var activityGroupUsers = _activityGroupUsers.Query(x => x.UserId == currentUser.Id && x.DeletedDate == null && x.DeletedBy == null).ToList();
                    var query = _tsRecords.Query(x => x.DeletedBy == null && x.DeletedDate == null && x.StartTime >= model.StartDate && x.EndTime <= model.EndDate).AsEnumerable();

                    //Check role
                    if (currentUser.Role == Roles.USER)
                    {
                        List<Guid> tSAcitivityGroupIds = activityGroupUsers.Select(x => x.TSActivityGroupId).ToList();
                        var userRecord = new List<TSRecord>();
                        var groupRecord = new List<TSRecord>();
                        var projectRecord = new List<TSRecord>();
                        if (tSAcitivityGroupIds.Count > 0)
                        {
                            var listProject = _logicService.Cache.TSActivityGroups.GetValues().Where(x => tSAcitivityGroupIds.Contains(x.Id) && x.ProjectId != null).Select(x => x.ProjectId).ToList();
                            if (listProject.Count > 0)
                            {
                                List<UserMD> listUser = new();
                                foreach (var item in listProject)
                                {
                                    var projectUsers = _logicService.Cache.Projects.GetUsers(item.Value).ToList();
                                    listUser.AddRange(projectUsers);
                                }
                                var listUserId = listUser.Select(x => x.Id).Distinct().ToList();
                                projectRecord = query.Where(x => listUserId.Contains(x.UserId)).ToList();
                            }
                            var tsActivityIds = _tSActivities.Query(x => x.DeletedBy == null && x.DeletedDate == null && tSAcitivityGroupIds.Contains(x.TSActivityGroupId))
                                .Select(x => x.Id).Distinct().ToList();
                            groupRecord = query.Where(x => tsActivityIds.Contains(x.TSActivityId)).ToList();
                        }
                        userRecord = query.Where(x => x.UserId == currentUser.Id).ToList();
                        query = userRecord.Union(groupRecord).Union(projectRecord).AsEnumerable();
                    }
                    //Check params
                    if (model.UserIds != null && model.UserIds.Count > 0)
                    {
                        query = query.Where(x => model.UserIds.Contains(x.UserId));
                    }
                    if (model.ProjectIds != null && model.ProjectIds.Count > 0)
                    {
                        List<UserMD> listUser = new();
                        foreach (var item in model.ProjectIds)
                        {
                            var projectUser = _logicService.Cache.Projects.GetUsers(item).ToList();
                            listUser.AddRange(projectUser);
                        }
                        var listUserId = listUser.Select(x => x.Id).Distinct().ToList();
                        query = query.Where(x => listUserId.Contains(x.UserId));
                    }
                    if (model.TSAcitivityGroupIds != null && model.TSAcitivityGroupIds.Count > 0)
                    {
                        var tsActivityIds = _tSActivities.Query(x => x.DeletedBy == null && x.DeletedDate == null && model.TSAcitivityGroupIds.Contains(x.TSActivityGroupId))
                            .Select(x => x.Id).Distinct().ToList();
                        query = query.Where(x => tsActivityIds.Contains(x.TSActivityId));
                    }
                    if (model.TaskIds != null && model.TaskIds.Count > 0)
                    {
                        var tsActivityIds = _tSActivities.Query(x => !x.DeletedBy.HasValue && model.TaskIds.Contains(x.Id)).Select(x => x.Id).Distinct().ToList();
                        if (tsActivityIds.Count > 0)
                        {
                            query = query.Where(x => tsActivityIds.Contains(x.TSActivityId));
                        }
                    }

                    //Count time each record 
                    var totalTime = query.Select(x => (x.EndTime - x.StartTime).Ticks).Sum();
                    var users = _logicService.Cache.Users.GetValues();
                    var tsActityGroups = _logicService.Cache.TSActivityGroups.GetValues();
                    var tsActites = _logicService.Cache.TSActivities.GetValues();
                    var record = query
                        .Select(t =>
                        {
                            var user = users.Where(x => x.Id == t.UserId).FirstOrDefault();
                            var tsActity = tsActites.Where(x => x.Id == t.TSActivityId).FirstOrDefault();
                            var tsActityGroup = tsActity == null ? null : tsActityGroups.Where(x => x.Id == tsActity.TSActivityGroupId).FirstOrDefault();
                            return new TimeSheetReportDetailModel()
                            {
                                BacklogId = t.BacklogId != null ? t.BacklogId.Trim() : null,
                                TaskId = t.TaskId != null ? t.TaskId.Trim() : null,
                                TimeSheetRecordId = t.Id,
                                TimeSheetRecordName = t.Name,
                                StartDate = t.StartTime,
                                StartHour = t.StartTime.ToString("HH:mm"),
                                EndDate = t.EndTime,
                                EndHour = t.EndTime.ToString("HH:mm"),
                                DateRecord = t.StartTime.Date.ToString("dd'/'MM'/'yyyy"),
                                UserId = t.UserId,
                                FirstName = user == null ? "" : user.FirstName,
                                LastName = user == null ? "" : user.LastName,
                                TSActivityId = tsActity == null ? Guid.Empty : tsActity.Id,
                                TSActivityName = tsActity == null ? "" : tsActity.Name,
                                TSAcitivityGroupId = tsActityGroup == null ? Guid.Empty : tsActityGroup.Id,
                                TSAcitivityGroupName = tsActityGroup == null ? "" : tsActityGroup.Name,
                                TotalTime = new TimeSpan(totalTime)
                            };
                        }).OrderBy(x => x.StartDate).ToList();

                    //paging handle
                    record = record.Skip((model.Page - 1) * model.PageSize).Take(model.PageSize).ToList();
                    int totalRow = query.Count();
                    var paging = new PaginationSet<TimeSheetReportDetailModel>()
                    {
                        Items = record,
                        Page = model.Page,
                        TotalCount = totalRow,
                        TotalPages = (int)Math.Ceiling((decimal)totalRow / model.PageSize)
                    };
                    return paging;
                });
            return result;
        }
        public ResultModel<List<TimeSheetReportUserModel>> GetAllUserRecord(FilterParamUserRecord model)
        {
            var result = _logicService
                .Start()
                .ThenAuthorize(Roles.ADMINISTRATOR, Roles.USER)
                .ThenValidate(c =>
                {
                    return null;
                }).ThenImplement(c =>
                {
                    var allUser = _logicService.Cache.Users.GetValues().ToList();
                    var now = DateTime.UtcNow;
                    if (model.PIds != null && model.PIds.Count > 0)
                    {
                        List<UserMD> listUser = new();
                        foreach (var item in model.PIds)
                        {
                            var users = _logicService.Cache.Projects.GetUsers(item).ToList();
                            listUser.AddRange(users);
                        }
                        var listUserId = listUser.Select(x => x.Id).Distinct().ToList();
                        allUser = allUser.Where(x => listUserId.Contains(x.Id)).ToList();
                    }
                    if (model.UIds != null && model.UIds.Count > 0)
                    {
                        allUser = allUser.Where(x => model.UIds.Contains(x.Id)).ToList();
                    }
                    var allRecord = _tsRecords.Query(x => !x.DeletedDate.HasValue && !x.DeletedBy.HasValue).ToList();
                    if (model.Year != null && model.Year <= now.Year && model.Year > (now.Year - 5))
                    {
                        allRecord = allRecord.Where(x => x.StartTime.Year == model.Year && x.EndTime.Year == model.Year).ToList();
                    }
                    else
                    {
                        allRecord = allRecord.Where(x => x.StartTime.Year == now.Year && x.EndTime.Year == now.Year).ToList();
                    }
                    var groupByUser = allRecord.GroupBy(x => x.UserId).Where(x => allUser.Select(i => i.Id).Contains(x.Key)).Select(t => t).ToList();
                    List<TimeSheetReportUserModel> ListUserRecord = new();
                    foreach (var item in groupByUser)
                    {
                        List<MonthlyRecord> monthRecords = new();
                        var groupByMonth = item.GroupBy(x => x.StartTime.Month).Select(t => t).ToList();
                        foreach (var month in groupByMonth)
                        {
                            var time = Math.Round(month.Select(x => (x.EndTime - x.StartTime).TotalHours).Sum(), 2);
                            MonthlyRecord record = new()
                            {
                                Month = month.Key,
                                TimeRecord = time
                            };
                            monthRecords.Add(record);
                        }
                        TimeSheetReportUserModel recordModel = new()
                        {
                            UserId = item.Key,
                            FullName = allUser.FirstOrDefault(x => x.Id == item.Key).FullName,
                            Email = allUser.FirstOrDefault(x => x.Id == item.Key).Email,
                            January = monthRecords.Where(x => x.Month == 1).ToList().Count == 1 ? monthRecords.Where(x => x.Month == 1).FirstOrDefault().TimeRecord : null,
                            February = monthRecords.Where(x => x.Month == 2).ToList().Count == 1 ? monthRecords.Where(x => x.Month == 2).FirstOrDefault().TimeRecord : null,
                            March = monthRecords.Where(x => x.Month == 3).ToList().Count == 1 ? monthRecords.Where(x => x.Month == 3).FirstOrDefault().TimeRecord : null,
                            April = monthRecords.Where(x => x.Month == 4).ToList().Count == 1 ? monthRecords.Where(x => x.Month == 4).FirstOrDefault().TimeRecord : null,
                            May = monthRecords.Where(x => x.Month == 5).ToList().Count == 1 ? monthRecords.Where(x => x.Month == 5).FirstOrDefault().TimeRecord : null,
                            June = monthRecords.Where(x => x.Month == 6).ToList().Count == 1 ? monthRecords.Where(x => x.Month == 6).FirstOrDefault().TimeRecord : null,
                            July = monthRecords.Where(x => x.Month == 7).ToList().Count == 1 ? monthRecords.Where(x => x.Month == 7).FirstOrDefault().TimeRecord : null,
                            August = monthRecords.Where(x => x.Month == 8).ToList().Count == 1 ? monthRecords.Where(x => x.Month == 8).FirstOrDefault().TimeRecord : null,
                            September = monthRecords.Where(x => x.Month == 9).ToList().Count == 1 ? monthRecords.Where(x => x.Month == 9).FirstOrDefault().TimeRecord : null,
                            October = monthRecords.Where(x => x.Month == 10).ToList().Count == 1 ? monthRecords.Where(x => x.Month == 10).FirstOrDefault().TimeRecord : null,
                            November = monthRecords.Where(x => x.Month == 11).ToList().Count == 1 ? monthRecords.Where(x => x.Month == 11).FirstOrDefault().TimeRecord : null,
                            December = monthRecords.Where(x => x.Month == 12).ToList().Count == 1 ? monthRecords.Where(x => x.Month == 12).FirstOrDefault().TimeRecord : null,
                        };
                        ListUserRecord.Add(recordModel);
                    }
                    var groupByNoRecordUser = allUser.Where(x => !allRecord.Select(i => i.UserId).Contains(x.Id)).Select(t => t).ToList();
                    foreach (var item in groupByNoRecordUser)
                    {
                        TimeSheetReportUserModel recordModel = new()
                        {
                            UserId = item.Id,
                            FullName = item.FullName,
                            Email = item.Email,
                        };
                        ListUserRecord.Add(recordModel);
                    }
                    if (c.Role == Roles.USER)
                    {
                        var listManager = _activityGroupUsers.Query(x => !x.DeletedDate.HasValue && !x.DeletedBy.HasValue && x.UserId == c.Id).ToList();
                        var allGroup = _tSActivityGroups.Query(x => !x.DeletedDate.HasValue && !x.DeletedBy.HasValue).ToList();
                        if (listManager.Count > 0)
                        {
                            List<int> userIds = new();
                            foreach (var item in listManager)
                            {
                                var project = allGroup.FirstOrDefault(x => x.Id == item.TSActivityGroupId && x.ProjectId != null);
                                if (project != null)
                                {
                                    List<UserMD> users = _logicService.Cache.Projects.GetUsers(project.ProjectId.Value).ToList();
                                    var listUserContains = users.Where(x => !userIds.Contains(x.Id)).Select(t => t.Id).ToList();
                                    userIds.AddRange(listUserContains);
                                }
                                else
                                {
                                    var listActivityId = _logicService.Cache.TSActivities.GetValues()
                                        .Where(x => !x.DeletedDate.HasValue && !x.DeletedBy.HasValue && x.TSActivityGroupId == item.TSActivityGroupId)
                                        .Select(t => t.Id).ToList();
                                    var listUserId = allRecord.Where(x => listActivityId.Contains(x.TSActivityId)).Select(t => t.UserId).ToList();
                                    var listUserContains = listUserId.Where(x => !userIds.Contains(x)).Select(t => t).ToList();
                                    userIds.AddRange(listUserContains);
                                }
                            }
                            ListUserRecord = ListUserRecord.Where(x => userIds.Contains(x.UserId)).Select(t => t).OrderBy(x => x.FullName).ToList();
                        }
                        else { ListUserRecord = ListUserRecord.Where(t => t.UserId == c.Id).ToList(); }
                    }
                    else { ListUserRecord = ListUserRecord.OrderBy(x => x.FullName).ToList(); }
                    return ListUserRecord;
                });
            return result;
        }
        public ResultModel<List<TimeSheetReportProjectModel>> GetAllProjectRecord(FilterParamProjectRecord model)
        {
            var result = _logicService
                .Start()
                .ThenAuthorize(Roles.ADMINISTRATOR, Roles.USER)
                .ThenValidate(c =>
                {
                    return null;
                }).ThenImplement(c =>
                {
                    var now = DateTime.Now;
                    var allGroup = _tSActivityGroups.Query(x => !x.DeletedDate.HasValue && !x.DeletedBy.HasValue).ToList();
                    if (model.GIds != null && model.GIds.Count > 0)
                    {
                        allGroup = allGroup.Where(x => model.GIds.Contains(x.Id)).ToList();
                    }
                    var allActivity = _tSActivities.Query(x => !x.DeletedDate.HasValue && !x.DeletedBy.HasValue).ToList();
                    var allRecord = _tsRecords.Query(x => !x.DeletedDate.HasValue && !x.DeletedBy.HasValue).ToList();
                    if (model.Year != null && model.Year <= now.Year && model.Year > (now.Year - 3))
                    {
                        allRecord = allRecord.Where(x => x.StartTime.Year == model.Year && x.EndTime.Year == model.Year).ToList();
                    }
                    else
                    {
                        allRecord = allRecord.Where(x => x.StartTime.Year == now.Year && x.EndTime.Year == now.Year).ToList();
                    }
                    var groupRecordByActivity = allRecord.GroupBy(x => x.TSActivityId).Select(t => t).ToList();
                    List<TimeSheetReportProjectModel> listProjectRecord = new();

                    foreach (var group in allGroup)
                    {
                        var listActivityId = allActivity.Where(x => x.TSActivityGroupId == group.Id).Select(t => t.Id).ToList();
                        if (listActivityId.Count > 0)
                        {
                            var listRecord = groupRecordByActivity.Where(x => listActivityId.Contains(x.Key)).Select(t => t).ToList();
                            List<MonthlyRecord> monthRecords = new();
                            if (listRecord.Count > 0)
                            {
                                foreach (var item in listRecord)
                                {
                                    var groupByMonth = item.GroupBy(x => x.StartTime.Month).Select(t => t).ToList();
                                    foreach (var month in groupByMonth)
                                    {
                                        var time = Math.Round(month.Select(x => (x.EndTime - x.StartTime).TotalHours).Sum(), 2);
                                        MonthlyRecord record = new()
                                        {
                                            Month = month.Key,
                                            TimeRecord = time
                                        };
                                        monthRecords.Add(record);
                                    }
                                }
                            }
                            var groupTimeRecordByMonth = monthRecords.GroupBy(x => x.Month).Select(t => t).ToList();
                            List<MonthlyRecord> monthTotalRecord = new();
                            foreach (var month in groupTimeRecordByMonth)
                            {
                                var totalProjectTime = Math.Round((decimal)month.Select(x => x.TimeRecord).Sum(), 2);
                                MonthlyRecord record = new()
                                {
                                    Month = month.Key,
                                    TimeRecord = (double)totalProjectTime,
                                };
                                monthTotalRecord.Add(record);
                            }
                            TimeSheetReportProjectModel project = new()
                            {
                                GroupId = group.Id,
                                ProjectId = group.ProjectId,
                                Name = group.Name,
                                January = monthTotalRecord.Where(x => x.Month == 1).ToList().Count == 1 ? monthTotalRecord.Where(x => x.Month == 1).FirstOrDefault().TimeRecord : null,
                                February = monthTotalRecord.Where(x => x.Month == 2).ToList().Count == 1 ? monthTotalRecord.Where(x => x.Month == 2).FirstOrDefault().TimeRecord : null,
                                March = monthTotalRecord.Where(x => x.Month == 3).ToList().Count == 1 ? monthTotalRecord.Where(x => x.Month == 3).FirstOrDefault().TimeRecord : null,
                                April = monthTotalRecord.Where(x => x.Month == 4).ToList().Count == 1 ? monthTotalRecord.Where(x => x.Month == 4).FirstOrDefault().TimeRecord : null,
                                May = monthTotalRecord.Where(x => x.Month == 5).ToList().Count == 1 ? monthTotalRecord.Where(x => x.Month == 5).FirstOrDefault().TimeRecord : null,
                                June = monthTotalRecord.Where(x => x.Month == 6).ToList().Count == 1 ? monthTotalRecord.Where(x => x.Month == 6).FirstOrDefault().TimeRecord : null,
                                July = monthTotalRecord.Where(x => x.Month == 7).ToList().Count == 1 ? monthTotalRecord.Where(x => x.Month == 7).FirstOrDefault().TimeRecord : null,
                                August = monthTotalRecord.Where(x => x.Month == 8).ToList().Count == 1 ? monthTotalRecord.Where(x => x.Month == 8).FirstOrDefault().TimeRecord : null,
                                September = monthTotalRecord.Where(x => x.Month == 9).ToList().Count == 1 ? monthTotalRecord.Where(x => x.Month == 9).FirstOrDefault().TimeRecord : null,
                                October = monthTotalRecord.Where(x => x.Month == 10).ToList().Count == 1 ? monthTotalRecord.Where(x => x.Month == 10).FirstOrDefault().TimeRecord : null,
                                November = monthTotalRecord.Where(x => x.Month == 11).ToList().Count == 1 ? monthTotalRecord.Where(x => x.Month == 11).FirstOrDefault().TimeRecord : null,
                                December = monthTotalRecord.Where(x => x.Month == 12).ToList().Count == 1 ? monthTotalRecord.Where(x => x.Month == 12).FirstOrDefault().TimeRecord : null,
                            };
                            listProjectRecord.Add(project);
                        }
                    }

                    if (c.Role == Roles.USER)
                    {
                        var listManager = _activityGroupUsers.Query(x => !x.DeletedDate.HasValue && !x.DeletedBy.HasValue && x.UserId == c.Id).ToList();
                        if (listManager.Count > 0)
                        {
                            var listGroupId = listManager.Select(t => t.TSActivityGroupId).ToList();
                            listProjectRecord = listProjectRecord.Where(x => listGroupId.Contains(x.GroupId)).OrderByDescending(x => x.ProjectId.HasValue).ThenBy(x => x.Name).ToList();
                        }
                    }
                    else { listProjectRecord = listProjectRecord.OrderByDescending(x => x.ProjectId.HasValue).ThenBy(x => x.Name).ToList(); }
                    return listProjectRecord;
                });
            return result;
        }
        public ResultModel<List<TimeSheetReportUserActivityModel>> GetAllUserActivityRecord(FilterParamUserActivity model)
        {
            var result = _logicService
                .Start()
                .ThenAuthorize(Roles.ADMINISTRATOR, Roles.USER)
                .ThenValidate(c =>
                {
                    return null;
                }).ThenImplement(c =>
                {
                    List<TimeSheetReportUserActivityModel> listResult = new();
                    //Init raw data
                    var allUser = _logicService.Cache.Users.GetValues().ToList();
                    var allRecord = _tsRecords.Query(x => !x.DeletedDate.HasValue && !x.DeletedBy.HasValue && x.StartTime >= model.StartDate && x.EndTime <= model.EndDate).ToList();
                    List<ActivityRecordViewModel> allActivityView = new();
                    var listAllGroupId = _logicService.Cache.TSActivityGroups.GetValues().Where(x => !x.DeletedDate.HasValue && !x.DeletedBy.HasValue).Select(t => t.Id).ToList();
                    if (listAllGroupId.Count > 0)
                    {
                        allActivityView = listAllGroupId
                           .SelectMany(t => _logicService.Cache.TSActivityGroups.GetActivity(t))
                           .Select(t => new ActivityRecordViewModel
                           {
                               Id = t.Id,
                               DisplayName = t.Name,
                               GroupId = t.TSActivityGroupId,
                               GroupName = _logicService.Cache.TSActivityGroups.Get(t.TSActivityGroupId).Name,
                           }).ToList();
                    }
                    //check param and role
                    var activityGroupUserManager = _activityGroupUsers.Query(x => x.UserId == c.Id && x.DeletedDate == null && x.DeletedBy == null && x.Role == 1).ToList();
                    if (c.Role == Roles.USER && activityGroupUserManager.Count > 0)
                    {
                        List<Guid> tSAcitivityGroupIds = activityGroupUserManager.Select(x => x.TSActivityGroupId).ToList();
                        var listProject = _logicService.Cache.TSActivityGroups.GetValues().Where(x => tSAcitivityGroupIds.Contains(x.Id) && x.ProjectId != null).Select(x => x.ProjectId).ToList();
                        if (listProject.Count > 0)
                        {
                            List<UserMD> listUser = new();
                            foreach (var item in listProject)
                            {
                                var users = _logicService.Cache.Projects.GetUsers(item.Value).ToList();
                                listUser.AddRange(users);
                            }
                            var listUserId = listUser.Select(x => x.Id).Distinct().ToList();
                            allRecord = allRecord.Where(x => listUserId.Contains(x.UserId)).ToList();
                        }
                        else
                        {
                            var tsActivityIds = _tSActivities.Query(x => x.DeletedBy == null && x.DeletedDate == null && tSAcitivityGroupIds.Contains(x.TSActivityGroupId))
                                .Select(x => x.Id).Distinct().ToList();
                            var groupRecord = allRecord.Where(x => tsActivityIds.Contains(x.TSActivityId)).ToList();
                            var userRecord = allRecord.Where(x => x.UserId == c.Id).ToList();
                            allRecord = userRecord.Union(groupRecord).ToList();
                        }
                    }
                    if (c.Role == Roles.USER && activityGroupUserManager.Count == 0)
                    {
                        return listResult;
                    }

                    if (model.UIds != null && model.UIds.Count > 0)
                    {
                        allUser = allUser.Where(x => model.UIds.Contains(x.Id)).ToList();
                    }
                    if (model.PIds != null && model.PIds.Count > 0)
                    {
                        List<UserMD> listUser = new();
                        foreach (var item in model.PIds)
                        {
                            var users = _logicService.Cache.Projects.GetUsers(item).ToList();
                            listUser.AddRange(users);
                        }
                        var listUserId = listUser.Select(x => x.Id).Distinct().ToList();
                        allUser = allUser.Where(x => listUserId.Contains(x.Id)).ToList();
                    }
                    if (model.ActivityIds != null && model.ActivityIds.Count > 0)
                    {
                        allRecord = allRecord.Where(x => model.ActivityIds.Contains(x.TSActivityId)).ToList();
                    }

                    //Implement
                    var groupByUser = allRecord.GroupBy(x => x.UserId).Where(x => allUser.Select(i => i.Id).Contains(x.Key)).Select(t => t).ToList();
                    foreach (var item in groupByUser)
                    {
                        var groupByActivity = item.GroupBy(x => x.TSActivityId).Select(t => t).ToList();

                        foreach (var activity in groupByActivity)
                        {
                            var totalHour = Math.Round(activity.Select(x => (x.EndTime - x.StartTime).TotalHours).Sum(), 2);
                            TimeSheetReportUserActivityModel recordModel = new()
                            {
                                UserId = item.Key,
                                FullName = allUser.FirstOrDefault(x => x.Id == item.Key).FullName,
                                ActivityId = activity.Key,
                                DisplayName = allActivityView.Any(x => x.Id == activity.Key) ? allActivityView.FirstOrDefault(x => x.Id == activity.Key).DisplayName : null,
                                GroupName = allActivityView.Any(x => x.Id == activity.Key) ? allActivityView.FirstOrDefault(x => x.Id == activity.Key).GroupName : null,
                                TotalHour = totalHour,
                            };
                            listResult.Add(recordModel);
                        }
                    }
                    return listResult;
                });
            return result;
        }
        public ResultModel<List<TimeSheetReportProjectActivityModel>> GetAllProjectActivityRecord(FilterParamProjectActivity model)
        {
            var result = _logicService
                .Start()
                .ThenAuthorize(Roles.ADMINISTRATOR, Roles.USER)
                .ThenValidate(c =>
                {
                    return null;
                }).ThenImplement(c =>
                {
                    List<TimeSheetReportProjectActivityModel> listResult = new();
                    //Init raw data
                    var allGroup = _logicService.Cache.TSActivityGroups.GetValues().Where(x => !x.DeletedDate.HasValue && !x.DeletedBy.HasValue).ToList();
                    var allActivity = _logicService.Cache.TSActivities.GetValues().Where(x => !x.DeletedDate.HasValue && !x.DeletedBy.HasValue).ToList();
                    var allRecord = _tsRecords.Query(x => !x.DeletedDate.HasValue && !x.DeletedBy.HasValue && x.StartTime >= model.StartDate && x.EndTime <= model.EndDate).ToList();

                    //check param 
                    if (model.GroupIds != null && model.GroupIds.Count > 0)
                    {
                        allGroup = allGroup.Where(x => model.GroupIds.Contains(x.Id)).ToList();
                    }
                    if (model.ActivityIds != null && model.ActivityIds.Count > 0)
                    {
                        allRecord = allRecord.Where(x => model.ActivityIds.Contains(x.TSActivityId)).ToList();
                    }

                    //Implement
                    var groupRecordByActivity = allRecord.GroupBy(x => x.TSActivityId).Select(t => t).ToList();
                    foreach (var group in allGroup)
                    {
                        var listActivityId = allActivity.Where(x => x.TSActivityGroupId == group.Id).ToList();
                        if (listActivityId.Count > 0)
                        {
                            foreach (var activity in listActivityId)
                            {
                                var listRecord = groupRecordByActivity.Where(x => x.Key == activity.Id).Select(t => t).ToList();
                                if (listRecord.Count > 0)
                                {
                                    double time = 0;
                                    foreach (var record in listRecord)
                                    {
                                        time += Math.Round(record.Select(x => (x.EndTime - x.StartTime).TotalHours).Sum(), 2);
                                    }
                                    TimeSheetReportProjectActivityModel projectModel = new()
                                    {
                                        GroupId = group.Id,
                                        GroupName = group.Name,
                                        ActivityId = activity.Id,
                                        ActivityName = activity.Name,
                                        TotalHour = time,
                                        ProjectId = group.ProjectId,
                                    };
                                    listResult.Add(projectModel);
                                }
                            }
                        }
                    }

                    //check role
                    var activityGroupUserManager = _activityGroupUsers.Query(x => x.UserId == c.Id && x.DeletedDate == null && x.DeletedBy == null && x.Role == 1).ToList();
                    if (c.Role == Roles.USER && activityGroupUserManager.Count > 0)
                    {
                        List<Guid> tSAcitivityGroupIds = activityGroupUserManager.Select(x => x.TSActivityGroupId).ToList();
                        listResult = listResult.Where(x => tSAcitivityGroupIds.Contains(x.GroupId)).ToList();
                    }
                    if (c.Role == Roles.USER && activityGroupUserManager.Count == 0)
                    {
                        listResult = new List<TimeSheetReportProjectActivityModel>();
                    }
                    return listResult;
                });
            return result;
        }
    }
}

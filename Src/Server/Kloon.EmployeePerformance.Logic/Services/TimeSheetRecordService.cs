using Kloon.EmployeePerformance.DataAccess;
using Kloon.EmployeePerformance.DataAccess.Domain;
using Kloon.EmployeePerformance.Logic.Services.Base;
using Kloon.EmployeePerformance.Models.Common;
using Kloon.EmployeePerformance.Models.TimeSheet.TimeSheetRecord;
using System;
using System.Linq;

namespace Kloon.EmployeePerformance.Logic.Services
{
    public interface ITimeSheetRecordService
    {
        ResultModel<PaginationSet<TimeSheetRecordModel>> GetAll(string searchText, int page, int pageSize);

        ResultModel<TimeSheetRecordModel> Create(TimeSheetRecordModel model);

        ResultModel<TimeSheetRecordModel> Update(TimeSheetRecordModel model);

        ResultModel<bool> Delete(Guid recordId);
    }

    public class TimeSheetRecordService : ITimeSheetRecordService
    {
        private readonly IAuthenLogicService<UserQuarterEvaluationService> _logicService;
        private readonly IUnitOfWork<EmployeePerformanceContext> _dbContext;
        private readonly IEntityRepository<TSRecord> _tsRecordRespository;


        public TimeSheetRecordService(IAuthenLogicService<UserQuarterEvaluationService> logicService)
        {
            _logicService = logicService;
            _dbContext = logicService.DbContext;
            _tsRecordRespository = _dbContext.GetRepository<TSRecord>();
        }

        public ResultModel<TimeSheetRecordModel> Create(TimeSheetRecordModel model)
        {
            var now = DateTime.UtcNow;
            var result = _logicService
                .Start()
                .ThenAuthorizeTimeSheet(model)
                .ThenValidate(current =>
                {
                    var error = ValidateTimeSheetRecordModel(model);
                    if (error != null)
                    {
                        return error;
                    }

                    //check valid timesheetlock
                    var isAdmin = current.Role == Roles.ADMINISTRATOR ? true : false;
                    var isLock = _logicService.Cache.WorkSpaceSetting.GetIsLockTimeSheet();
                    if (isLock && !isAdmin)
                    {
                        var lockAfter = _logicService.Cache.WorkSpaceSetting.GetLockAfter();
                        var lockValeByDate = _logicService.Cache.WorkSpaceSetting.GetLockValueByDate();

                        TimeZoneInfo tzone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
                        var lockDate = TimeZoneInfo.ConvertTimeFromUtc(now, tzone).AddDays(-lockValeByDate);

                        var timeSheetLockDateTime = new DateTime(lockDate.Year, lockDate.Month, lockDate.Day, lockAfter.Hours, lockAfter.Minutes, 0);

                        //TaskRecord StartTime < lockedtime count on DateTime.NowUTC => not allow to create timesheetrecord that day. 
                        if (model.StartTime <= timeSheetLockDateTime)
                        {
                            return new ErrorModel(ErrorType.BAD_REQUEST, "TIMESHEET_LOCKED");
                        }
                    }

                    return null;
                })
               .ThenImplement(current =>
               {
                   var newTimeSheet = new TSRecord()
                   {
                       Id = Guid.NewGuid(),
                       Name = model.Name.Trim(),
                       TSActivityId = model.TSActivityId,
                       UserId = model.UserId,
                       TaskId = model.TaskId,
                       BacklogId = model.BacklogId,
                       StartTime = model.StartTime,
                       EndTime = model.EndTime,
                       CreatedBy = current.Id,
                       CreatedDate = now
                   };
                   _tsRecordRespository.Add(newTimeSheet);
                   _dbContext.Save();
                   model.Id = newTimeSheet.Id;
                   return model;
               });
            return result;
        }

        public ResultModel<TimeSheetRecordModel> Update(TimeSheetRecordModel model)
        {
            var now = DateTime.UtcNow;
            TSRecord record = null;
            var result = _logicService
                .Start()
                .ThenAuthorizeTimeSheet(model)
                .ThenValidate(current =>
                {
                    var error = ValidateTimeSheetRecordModel(model);
                    if (error != null)
                    {
                        return error;
                    }

                    //Check if record exist
                    record = _tsRecordRespository.Query(t => t.Id == model.Id).FirstOrDefault();
                    if (record == null)
                    {
                        return new ErrorModel(ErrorType.NOT_EXIST, "INVALID_RECORD_NOT_FOUND.");
                    }

                    //check valid timesheetlock
                    var isAdmin = current.Role == Roles.ADMINISTRATOR ? true : false;
                    var isLock = _logicService.Cache.WorkSpaceSetting.GetIsLockTimeSheet();
                    if (isLock && !isAdmin)
                    {
                        var lockAfter = _logicService.Cache.WorkSpaceSetting.GetLockAfter();
                        var lockValeByDate = _logicService.Cache.WorkSpaceSetting.GetLockValueByDate();

                        TimeZoneInfo tzone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
                        var lockDate = TimeZoneInfo.ConvertTimeFromUtc(now, tzone).AddDays(-lockValeByDate);
                        var timeSheetLockDateTime = new DateTime(lockDate.Year, lockDate.Month, lockDate.Day, lockAfter.Hours, lockAfter.Minutes, 0);

                        //Convert startdate,lockdate with no time and minutes
                        var dtRecordStartTime = record.StartTime;
                        var dtModelStartTime = model.StartTime;
                        var dtLockDate = timeSheetLockDateTime;
                        dtRecordStartTime = dtRecordStartTime.AddHours(-dtRecordStartTime.Hour).AddMinutes(-dtRecordStartTime.Minute);
                        dtModelStartTime = dtModelStartTime.AddHours(-dtModelStartTime.Hour).AddMinutes(-dtModelStartTime.Minute);
                        dtLockDate = dtLockDate.AddHours(-dtLockDate.Hour).AddMinutes(-dtLockDate.Minute);

                        //Compare >0: date 1 later than 2, 0: same date; < 0: date1 earlier than date 2
                        var isLockedRecord = DateTime.Compare(dtRecordStartTime, dtLockDate) > 0
                            && DateTime.Compare(dtModelStartTime, dtLockDate) > 0 ? false : true;

                        //DateTimeNow bigger than datetime allow to edit => LOCKED
                        if (isLockedRecord && DateTime.Now >= timeSheetLockDateTime)
                        {
                            return new ErrorModel(ErrorType.BAD_REQUEST, "TIMESHEET_LOCKED");
                        }
                    }

                    return null;
                })
               .ThenImplement(current =>
               {
                   record.Name = model.Name.Trim();
                   record.TSActivityId = model.TSActivityId;
                   record.UserId = model.UserId;
                   record.TaskId = model.TaskId;
                   record.BacklogId = model.BacklogId;
                   record.StartTime = model.StartTime;
                   record.EndTime = model.EndTime;
                   record.ModifiedBy = current.Id;
                   record.ModifiedDate = now;
                   _tsRecordRespository.Edit(record);
                   _dbContext.Save();
                   return model;
               });
            return result;
        }

        public ResultModel<bool> Delete(Guid recordId)
        {
            var now = DateTime.UtcNow;
            TSRecord record = null;
            var result = _logicService
                .Start()
                .ThenAuthorize(Roles.USER, Roles.ADMINISTRATOR)
                .ThenValidate(current =>
                {
                    //Check if record exist
                    record = _tsRecordRespository.Query(t => t.Id == recordId).FirstOrDefault();
                    if (record == null)
                    {
                        return new ErrorModel(ErrorType.NOT_EXIST, "INVALID_RECORD_NOT_FOUND.");
                    }

                    var isAdmin = current.Role == Roles.ADMINISTRATOR ? true : false;
                    if (!isAdmin && record.UserId != current.Id)
                    {
                        return new ErrorModel(ErrorType.NOT_EXIST, "No role.");
                    }

                    //check valid timesheetlock
                    var isLock = _logicService.Cache.WorkSpaceSetting.GetIsLockTimeSheet();
                    if (isLock && !isAdmin)
                    {
                        var lockAfter = _logicService.Cache.WorkSpaceSetting.GetLockAfter();
                        var lockValeByDate = _logicService.Cache.WorkSpaceSetting.GetLockValueByDate();

                        TimeZoneInfo tzone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
                        var lockDate = TimeZoneInfo.ConvertTimeFromUtc(now, tzone).AddDays(-lockValeByDate);
                        var timeSheetLockDateTime = new DateTime(lockDate.Year, lockDate.Month, lockDate.Day, lockAfter.Hours, lockAfter.Minutes, 0);

                        //TaskRecord StartTime < lockedtime count on DateTime.NowUTC => not allow to create timesheetrecord that day. 
                        if (record.StartTime < timeSheetLockDateTime && DateTime.Now > timeSheetLockDateTime)
                        {
                            return new ErrorModel(ErrorType.BAD_REQUEST, "TIMESHEET_LOCKED");
                        }
                    }

                    return null;
                })
               .ThenImplement(current =>
               {
                   _tsRecordRespository.Delete(record);
                   _dbContext.Save();
                   return true;
               });
            return result;
        }

        public ResultModel<PaginationSet<TimeSheetRecordModel>> GetAll(string searchText, int page, int pageSize)
        {
            var result = _logicService
                .Start()
                .ThenAuthorize(Roles.ADMINISTRATOR, Roles.USER)
                .ThenValidate(current => null)
                .ThenImplement(current =>
                {
                    var query = _tsRecordRespository.Query(t => t.UserId == current.Id);
                    if (!string.IsNullOrWhiteSpace(searchText))
                    {
                        query = query.Where(t => t.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase));
                    }

                    var record = query.OrderByDescending(x => x.StartTime)
                                      .Select(t => new TimeSheetRecordModel
                                      {
                                          Id = t.Id,
                                          Name = t.Name,
                                          TSActivityId = t.TSActivityId,
                                          BacklogId = t.BacklogId != null ? t.BacklogId.Trim() : null,
                                          TaskId = t.TaskId != null ? t.TaskId.Trim() : null,
                                          StartTime = t.StartTime,
                                          EndTime = t.EndTime
                                      }).ToList();

                    var total = query.Count();

                    record = record
                        .Skip((page - 1) * pageSize)
                        .Take(pageSize)
                        .ToList();

                    var paginationResult = new PaginationSet<TimeSheetRecordModel>
                    {
                        Page = page,
                        TotalCount = total,
                        TotalPages = (int)Math.Ceiling((decimal)total / pageSize),
                        Items = record
                    };
                    return paginationResult;

                });
            return result;
        }

        #region PRIVATE METHOD

        private ErrorModel ValidateTimeSheetRecordModel(TimeSheetRecordModel model)
        {
            #region Name
            if (string.IsNullOrEmpty(model.Name))
            {
                return new ErrorModel(ErrorType.BAD_REQUEST, "INVALID_MODEL_NAME_NULL");
            }
            if (model.Name.Length > 200)
            {
                return new ErrorModel(ErrorType.BAD_REQUEST, "INVALID_MODEL_TASK_NAME_MAX_LENGTH");
            }
            #endregion

            #region Backlog - Task
            if (!string.IsNullOrEmpty(model.BacklogId) && model.BacklogId.Length > 50)
                return new ErrorModel(ErrorType.BAD_REQUEST, "INVALID_MODEL_BACKLOG_ID_MAX_LENGTH");
            if (!string.IsNullOrEmpty(model.TaskId) && model.TaskId.Length > 50)
                return new ErrorModel(ErrorType.BAD_REQUEST, "INVALID_MODEL_TASK_ID_MAX_LENGTH");
            #endregion

            #region StartTime, EndTime

            if (model.StartTime > model.EndTime)
            {
                return new ErrorModel(ErrorType.BAD_REQUEST, "Task StartTime is greater than EndTime. Please check again");
            }

            #endregion StartDate, EndDate

            return null;
        }

        #endregion PRIVATE METHOD
    }
}

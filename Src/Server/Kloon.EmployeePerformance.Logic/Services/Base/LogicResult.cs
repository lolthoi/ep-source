using Kloon.EmployeePerformance.DataAccess.Domain;
using Kloon.EmployeePerformance.Models.Common;
using Kloon.EmployeePerformance.Models.TimeSheet.TimeSheetRecord;
using Kloon.EmployeePerformance.Models.User;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kloon.EmployeePerformance.Logic.Services.Base
{
    public class LogicResult
    {
        public readonly LoginUserModel.LoginUser CurrentUser;
        public readonly ICommonService Services;

        public bool IsValid { get; private set; } = true;

        private ErrorModel _error;
        public ErrorModel Error
        {
            get
            {
                return _error;
            }
            set
            {
                _error = value;
                IsValid = _error == null;
            }
        }

        public LogicResult(LoginUserModel.LoginUser currentUser, ICommonService commonService)
        {
            CurrentUser = currentUser;
            Services = commonService;
        }

        public ValidationAuthenResult ThenAuthorize(params Roles[] roles)
        {
            if (roles != null && roles.Length != 0)
            {
                if (CurrentUser == null)
                {
                    Error = new ErrorModel(ErrorType.NOT_AUTHORIZED, "Not Authorized");
                }
                else
                {
                    var user = Services.Cache.Users.Get(CurrentUser.Id);
                    //if (user.DeletedBy != null && user.DeletedDate != null)
                    //{
                    //    Error = new ErrorModel(ErrorType.NOT_EXIST, "Role not found");
                    //}
                    if (!roles.Contains(CurrentUser.Role))
                    {
                        Error = new ErrorModel(ErrorType.NO_ROLE, "No Role");
                    }
                }
            }
            return new ValidationAuthenResult(this);
        }

        public ValidationAuthenResult ThenAuthorizeTimeSheet(TimeSheetRecordModel model)
        {
            if (model == null)
            {
                Error = new ErrorModel(ErrorType.NOT_EXIST, "Activity not found");
            }

            //check valid TSActivityId
            var tsActivity = Services.Cache.TSActivities.Get(model.TSActivityId);
            if (tsActivity == null)
            {
                Error = new ErrorModel(ErrorType.NOT_EXIST, "Activity not found");
            }

            var isAdmin = CurrentUser.Role == Roles.ADMINISTRATOR ? true : false;
            if (!isAdmin)
            {
                //Deny create/edit another person timesheet 
                if (model.UserId != CurrentUser.Id)
                {
                    Error = new ErrorModel(ErrorType.NO_ROLE, "No Role");
                }

                //check user valid Timesheet activity, this is dumb. Please maintain this cuz i dont have time.
                var lstProjectOfUser = CurrentUser.ProjectRole.Select(t => t.ProjectId).ToList();
                var lstGroupOfUser = Services.Cache.TSActivityGroups
                    .GetValues()
                    .Where(t => t.ProjectId == null || lstProjectOfUser.Contains((int)t.ProjectId))
                    .ToList();
                var lstAvaiableActivities = lstGroupOfUser
                    .Select(t => Services.Cache.TSActivityGroups.GetActivity(t.Id))
                    .Aggregate(new List<Caches.Data.TSActivityMD>(), (total, next) => (List<Caches.Data.TSActivityMD>)total.Concat(next).ToList())
                    .Select(t => t.Id).ToList();

                if (!lstAvaiableActivities.Contains(model.TSActivityId))
                {
                    Error = new ErrorModel(ErrorType.NO_ROLE, "User dont have permission with this activity");
                }
            }
            else
            {
                var lstProjectOfUser = Services.Cache.Users.GetProjects(model.UserId)
                    .Select(t => t.Id)
                    .ToList();
                var lstGroupOfUser = Services.Cache.TSActivityGroups
                    .GetValues()
                    .Where(t => t.ProjectId == null || lstProjectOfUser.Contains((int)t.ProjectId))
                    .ToList();
                var lstAvaiableActivities = lstGroupOfUser
                    .Select(t => Services.Cache.TSActivityGroups.GetActivity(t.Id))
                    .Aggregate(new List<Caches.Data.TSActivityMD>(), (total, next) => (List<Caches.Data.TSActivityMD>)total.Concat(next).ToList())
                    .Select(t => t.Id).ToList();

                if (!lstAvaiableActivities.Contains(model.TSActivityId))
                {
                    Error = new ErrorModel(ErrorType.NO_ROLE, "User dont have permission with this activity");
                }
            }

            return new ValidationAuthenResult(this);
        }
    }

    public class ValidationResult
    {
        protected readonly LogicResult _result;
        public ValidationResult(LogicResult logicResult)
        {
            _result = logicResult;
        }

        public virtual ImplementResult ThenValidate(Func<ErrorModel> func)
        {
            if (_result.IsValid)
            {
                try
                {
                    _result.Error = func.Invoke();
                }
                catch (Exception ex)
                {
                    _result.Services.Logger.LogError("VALIDATEDATA: " + ex.ToString());
                    _result.Error = new ErrorModel(ErrorType.INTERNAL_ERROR, ex.Message);
                }
            }
            return new ImplementResult(_result);
        }
    }

    public class ImplementResult
    {
        protected readonly LogicResult _result;
        public ImplementResult(LogicResult result)
        {
            _result = result;
        }
        public ResultModel<T> ThenImplement<T>(Func<T> func)
        {
            if (_result.IsValid)
            {
                try
                {
                    var data = func.Invoke();
                    return new ResultModel<T>(data);
                }
                catch (Exception ex)
                {
                    _result.Services.Logger.LogError("IMPLEMENT: " + ex.ToString());
                    _result.Error = new ErrorModel(ErrorType.INTERNAL_ERROR, ex.Message);
                }
            }
            return new ResultModel<T>(_result.Error);
        }
    }
}

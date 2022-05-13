using Kloon.EmployeePerformance.DataAccess.Domain;
using Kloon.EmployeePerformance.Models.Common;
using Kloon.EmployeePerformance.Models.User;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kloon.EmployeePerformance.Logic.Services.Base
{
    public class ValidationAuthenResult : ValidationResult
    {
        public ValidationAuthenResult(LogicResult result) 
            : base(result)
        {

        }
 
        public ValidationResult ThenAuthorizeUser(int? userId)
        {
            if (_result.IsValid)
            {
                var user = _result.Services.Cache.Users.Get(userId.Value);

                if (user == null)
                {
                    _result.Error = new ErrorModel(ErrorType.NOT_EXIST, "User not found");
                    if (_result.Error != null)
                    {
                        _result.Services.Logger.LogError("User not found");
                    }
                }

                if (user.DeletedBy != null && user.DeletedDate != null)
                {
                    _result.Error = new ErrorModel(ErrorType.NOT_EXIST, "User not found");
                    if (_result.Error != null)
                    {
                        _result.Services.Logger.LogError("User not found");
                    }
                }

                if (_result.CurrentUser.Role == Roles.USER && _result.CurrentUser.Id != userId)
                {
                    _result.Error = new ErrorModel(ErrorType.NO_DATA_ROLE, "Data does not exits");
                    if (_result.Error != null)
                    {
                        _result.Services.Logger.LogError("Data does not exits");
                    }
                }
            }
            return this;
        }

        public ValidationAuthenResult ThenAuthorizeProject(int? projectId, params ProjectRoles[] projectRoles)
        {
            if (_result.IsValid)
            {
                if (projectId != null && projectId <= 0)
                {
                    _result.Error = new ErrorModel(ErrorType.BAD_REQUEST, "Project not found");
                    if (_result.Error != null)
                    {
                        _result.Services.Logger.LogError("Project not found");
                    }
                    return this;
                }

                if (_result.CurrentUser.Role == Roles.USER)
                {
                    var currentProject = _result.CurrentUser.ProjectRole.Where(x => x.ProjectId == projectId).FirstOrDefault();
                    if (currentProject == null)
                    {
                        _result.Error = new ErrorModel(ErrorType.BAD_REQUEST, "Data does not exist");
                        if (_result.Error != null)
                        {
                            _result.Services.Logger.LogError("Data does not exits");
                        }
                        return this;
                    }
                    if (projectRoles != null && projectRoles.Length != 0)
                    {
                        if (!projectRoles.Contains(currentProject.ProjectRoleId))
                        {
                            _result.Error = new ErrorModel(ErrorType.NO_ROLE, "No Role Project");
                            if (_result.Error != null)
                            {
                                _result.Services.Logger.LogError("No Role Project");
                            }
                        }
                        return this;
                    }
                }          
            }
            return this;
        }

        public ImplementAuthenResult ThenValidate(Func<LoginUserModel.LoginUser, ErrorModel> func)
        {
            if (_result.IsValid)
            {
                try
                {
                    if (func != null)
                    {
                        _result.Error = func.Invoke(_result.CurrentUser);
                        if (_result.Error != null)
                        {
                            _result.Services.Logger.LogError(_result.Error.Message);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _result.Services.Logger.LogError("VALIDATEDATA: " + ex.ToString());
                    _result.Error = new ErrorModel(ErrorType.INTERNAL_ERROR, ex.Message);
                }
            }
            return new ImplementAuthenResult(_result);
        }

        public override ImplementAuthenResult ThenValidate(Func<ErrorModel> func)
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
            return new ImplementAuthenResult(_result);
        }

    }
    public class ImplementAuthenResult : ImplementResult
    {
        public ImplementAuthenResult(LogicResult result)
            : base(result)
        {

        }

        public ResultModel<T> ThenImplement<T>(Func<LoginUserModel.LoginUser, T> func)
        {
            if (_result.IsValid)
            {
                try
                {
                    var data = func.Invoke(_result.CurrentUser);
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

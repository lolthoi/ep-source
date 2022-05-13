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
using System.Text;
using System.Threading.Tasks;

namespace Kloon.EmployeePerformance.Logic.Services
{
    public interface IActivityGroupUserService
    {
        ResultModel<ActivityGroupUserModel> Create(ActivityGroupUserModel model);
        ResultModel<ActivityGroupUserModel> Update(ActivityGroupUserModel model);
        ResultModel<ActivityGroupUserModel> GetById(Guid Id);
        ResultModel<bool> Delete(Guid Id);
        ResultModel<int> CheckManagerByUserId(int Id);
    }
    public class ActivityGroupUserService : IActivityGroupUserService
    {
        private readonly IAuthenLogicService<ActivityGroupUserService> _logicService;
        private readonly IUnitOfWork<EmployeePerformanceContext> _dbContext;
        private readonly IEntityRepository<TSActivityGroup> _activityGroupRepository;
        private readonly IEntityRepository<ActivityGroupUser> _activityGroupUserRepository;

        public ActivityGroupUserService(IAuthenLogicService<ActivityGroupUserService> logicService)
        {
            _logicService = logicService;
            _dbContext = logicService.DbContext;
            _activityGroupRepository = _dbContext.GetRepository<TSActivityGroup>();
            _activityGroupUserRepository = _dbContext.GetRepository<ActivityGroupUser>();
        }

        public ResultModel<ActivityGroupUserModel> Create(ActivityGroupUserModel model)
        {
            var result = _logicService
                .Start()
                .ThenAuthorize(Roles.ADMINISTRATOR)
                .ThenValidate(c =>
                {
                    var isExistedActivityGroup = _logicService.Cache.TSActivityGroups.GetValues().Where(x => !x.DeletedDate.HasValue).Any(x => x.Id == model.ActivityGroupId);
                    if (!isExistedActivityGroup)
                        return new ErrorModel(ErrorType.NOT_EXIST, "Activity Group not found");
                    var listExistedUser = _logicService.Cache.Users.GetValues().ToList();
                    var isExistedUser = listExistedUser.Any(x => !x.DeletedDate.HasValue && x.Id == model.UserId);
                    if (!isExistedUser)
                        return new ErrorModel(ErrorType.NOT_EXIST, "UserId not found");
                    if (!Enum.IsDefined(typeof(TSRole), model.Role))
                        return new ErrorModel(ErrorType.BAD_REQUEST, "Invalid TSRole");
                    var existedGroupUser = _activityGroupUserRepository.Query(x => !x.DeletedDate.HasValue).Any(x => x.UserId == model.UserId && x.TSActivityGroupId == model.ActivityGroupId);
                    if (existedGroupUser)
                        return new ErrorModel(ErrorType.BAD_REQUEST, "Duplicated Group User");
                    return null;
                })
                .ThenImplement(c =>
                {
                    ActivityGroupUser user = new()
                    {
                        TSActivityGroupId = model.ActivityGroupId,
                        UserId = model.UserId,
                        Role = (int)model.Role,
                        CreatedBy = c.Id,
                        CreatedDate = DateTime.UtcNow,
                    };
                    _activityGroupUserRepository.Add(user);
                    _dbContext.Save();
                    model.Id = user.Id;
                    return model;
                });
            return result;
        }

        public ResultModel<ActivityGroupUserModel> Update(ActivityGroupUserModel model)
        {
            ActivityGroupUser user = new();
            var result = _logicService
                .Start()
                .ThenAuthorize(Roles.ADMINISTRATOR)
                .ThenValidate(c =>
                {
                    var isExistedActivityGroup = _logicService.Cache.TSActivityGroups.GetValues().Where(x => !x.DeletedDate.HasValue).Any(x => x.Id == model.ActivityGroupId);
                    if (!isExistedActivityGroup)
                        return new ErrorModel(ErrorType.NOT_EXIST, "Activity Group not found");
                    user = _activityGroupUserRepository.Query(x => !x.DeletedDate.HasValue && x.Id == model.Id).FirstOrDefault();
                    if (user == null)
                        return new ErrorModel(ErrorType.NOT_EXIST, "Activity Group User not found");
                    var listExistedUser = _logicService.Cache.Users.GetValues().ToList();
                    var isExistedUser = listExistedUser.Any(x => !x.DeletedDate.HasValue && x.Id == model.UserId);
                    if (!isExistedUser)
                        return new ErrorModel(ErrorType.NOT_EXIST, "UserId not found");
                    if (!Enum.IsDefined(typeof(TSRole), model.Role))
                        return new ErrorModel(ErrorType.BAD_REQUEST, "Invalid TSRole");
                    var existedGroupUser = _activityGroupUserRepository.Query(x => !x.DeletedDate.HasValue).Any(x => x.Id != model.Id && x.UserId == model.UserId && x.TSActivityGroupId == model.ActivityGroupId);
                    if (existedGroupUser)
                        return new ErrorModel(ErrorType.BAD_REQUEST, "Duplicated Group User");
                    return null;
                })
                .ThenImplement(c =>
                {
                    user.TSActivityGroupId = model.ActivityGroupId;
                    user.UserId = model.UserId;
                    user.Role = (int)model.Role;
                    user.ModifiedBy = c.Id;
                    user.ModifiedDate = DateTime.UtcNow;
                    _dbContext.Save();
                    return model;
                });
            return result;
        }

        public ResultModel<ActivityGroupUserModel> GetById(Guid Id)
        {
            ActivityGroupUser user = new();
            var result = _logicService
                .Start()
                .ThenAuthorize(Roles.ADMINISTRATOR)
                .ThenValidate(c =>
                {
                    user = _activityGroupUserRepository.Query(x => !x.DeletedDate.HasValue && x.Id == Id).FirstOrDefault();
                    if (user == null)
                        return new ErrorModel(ErrorType.NOT_EXIST, "Activity Group User not found");
                    return null;
                })
                .ThenImplement(c =>
                {
                    var listUser = _logicService.Cache.Users.GetValues().Where(x => !x.DeletedDate.HasValue).ToList();
                    ActivityGroupUserModel model = new()
                    {
                        Id = user.Id,
                        ActivityGroupId = user.TSActivityGroupId,
                        UserId = user.UserId,
                        Role = (TSRole)user.Role,
                        FullName = listUser.Where(x => x.Id == user.UserId).FirstOrDefault().FullName,
                        Email = listUser.Where(x => x.Id == user.UserId).FirstOrDefault().Email,
                    };
                    return model;
                });
            return result;
        }

        public ResultModel<bool> Delete(Guid Id)
        {
            ActivityGroupUser user = new();
            var result = _logicService
                .Start()
                .ThenAuthorize(Roles.ADMINISTRATOR)
                .ThenValidate(c =>
                {
                    user = _activityGroupUserRepository.Query(x => !x.DeletedDate.HasValue && x.Id == Id).FirstOrDefault();
                    if (user == null)
                        return new ErrorModel(ErrorType.NOT_EXIST, "Activity Group User not found");
                    return null;
                })
                .ThenImplement(c =>
                {
                    user.DeletedBy = c.Id;
                    user.DeletedDate = DateTime.UtcNow;
                    _dbContext.Save();
                    return true;
                });
            return result;
        }

        public ResultModel<int> CheckManagerByUserId(int Id)
        {
            ActivityGroupUser user = new();
            var result = _logicService
                .Start()
                .ThenAuthorize(Roles.ADMINISTRATOR, Roles.USER)
                .ThenValidate(c =>
                {
                    return null;
                })
                .ThenImplement(c =>
                {
                    int res = 0;
                    user = _activityGroupUserRepository.Query(x => !x.DeletedDate.HasValue && x.UserId == Id).FirstOrDefault();
                    if (user == null)
                    {
                        return res;
                    }
                    else
                    {
                        if (user.Role == 1)
                            res = 1;
                        if (user.Role == 2)
                            res = 2;
                    }
                    return res;
                });
            return result;
        }
    }
}

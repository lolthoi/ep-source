using Kloon.EmployeePerformance.DataAccess;
using Kloon.EmployeePerformance.DataAccess.Domain;
using Kloon.EmployeePerformance.Logic.Caches.Data;
using Kloon.EmployeePerformance.Logic.Services.Base;
using Kloon.EmployeePerformance.Models.Common;
using Kloon.EmployeePerformance.Models.WorkSpaceSetting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kloon.EmployeePerformance.Logic.Services
{
    public interface IWorkSpaceSettingService
    {
        ResultModel<WorkSpaceSettingModel> GetAllWorkSpaceSettings();
        ResultModel<WorkSpaceSettingModel> Edit(WorkSpaceSettingModel model);
    }
    public class WorkSpaceSettingService : IWorkSpaceSettingService
    {
        private readonly IAuthenLogicService<WorkSpaceSettingService> _logicService;
        private readonly IUnitOfWork<EmployeePerformanceContext> _dbContext;
        private readonly IEntityRepository<AppSetting> _appSettings;
        private readonly List<string> _ids;

        public WorkSpaceSettingService(IAuthenLogicService<WorkSpaceSettingService> logicService)
        {
            _logicService = logicService;
            _dbContext = _logicService.DbContext;
            _appSettings = _dbContext.GetRepository<AppSetting>();
            _ids = new List<string>()
            {
                WorkSpaceSettingCommon.ISLOCKTIMESHEET,
                WorkSpaceSettingCommon.LOCKAFTER,
                WorkSpaceSettingCommon.LOCKVALUEBYDATE
            };
        }
        public ResultModel<WorkSpaceSettingModel> GetAllWorkSpaceSettings()
        {
            IQueryable<WorkSpaceSettingMD> appSettings = null;
            var result = _logicService
                .Start()
                .ThenAuthorize(Roles.ADMINISTRATOR, Roles.USER)
                .ThenValidate(currentUser =>
                {
                    return null;
                })
                .ThenImplement(currentUser =>
                {
                    appSettings = _logicService.Cache.WorkSpaceSetting.GetValues().AsQueryable();
                    var record = new WorkSpaceSettingModel()
                    {
                        IsLockTimeSheet = appSettings
                            .Where(x => x.Id == WorkSpaceSettingCommon.ISLOCKTIMESHEET)
                            .Select(x => x.Value).FirstOrDefault(),
                        LockAfter = appSettings
                            .Where(x => x.Id == WorkSpaceSettingCommon.LOCKAFTER)
                            .Select(x => x.Value).FirstOrDefault(),
                        LockValueByDate = appSettings
                            .Where(x => x.Id == WorkSpaceSettingCommon.LOCKVALUEBYDATE)
                            .Select(x => x.Value)
                            .FirstOrDefault(),
                    };
                    if (!string.IsNullOrEmpty(record.LockAfter))
                    {
                        var tick2 = TimeSpan.FromTicks(long.Parse(record.LockAfter));
                        record.LockAfter = tick2.ToString(@"hh\:mm\:ss");
                    }
                    return record;
                });
            return result;
        }

        public ResultModel<WorkSpaceSettingModel> Edit(WorkSpaceSettingModel models)
        {
            AppSetting isLockTimeSheet = null;
            AppSetting lockAfter = null;
            AppSetting lockValueByDate = null;

            var result = _logicService
                .Start()
                .ThenAuthorize(Roles.ADMINISTRATOR)
                .ThenValidate(currentUser =>
                {
                    if (models == null)
                    {
                        return new ErrorModel(ErrorType.BAD_REQUEST, "Please fill in the required fields");
                    }
                    if (models.IsLockTimeSheet == true.ToString())
                    {
                        if (string.IsNullOrEmpty(models.LockAfter) || string.IsNullOrEmpty(models.LockValueByDate))
                        {
                            return new ErrorModel(ErrorType.BAD_REQUEST, "All Values are not allowed to be null");
                        }
                    }

                    isLockTimeSheet = _appSettings.Query(x => x.Id == WorkSpaceSettingCommon.ISLOCKTIMESHEET).FirstOrDefault();
                    lockAfter = _appSettings.Query(x => x.Id == WorkSpaceSettingCommon.LOCKAFTER).FirstOrDefault();
                    lockValueByDate = _appSettings.Query(x => x.Id == WorkSpaceSettingCommon.LOCKVALUEBYDATE).FirstOrDefault();

                    if (isLockTimeSheet == null || lockAfter == null || lockValueByDate == null)
                    {
                        return new ErrorModel(ErrorType.BAD_REQUEST, "Work Space Setting not found.");
                    }
                    return null;
                })
                .ThenImplement(currentUser =>
                {
                    isLockTimeSheet.Value = models.IsLockTimeSheet;

                    var tick2 = TimeSpan.Parse(models.LockAfter);
                    lockAfter.Value = tick2.Ticks.ToString();

                    lockValueByDate.Value = models.LockValueByDate;

                    _dbContext.Save();
                    _logicService.Cache.WorkSpaceSetting.Clear();
                    return models;
                });
            return result;
        }      
    }
}

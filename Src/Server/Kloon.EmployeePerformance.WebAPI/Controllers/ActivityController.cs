using Kloon.EmployeePerformance.Logic.Services;
using Kloon.EmployeePerformance.Models.TimeSheet;
using Kloon.EmployeePerformance.Models.TimeSheet.TimeSheetRecord;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

namespace Kloon.EmployeePerformance.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ActivityController : ControllerBase
    {
        private readonly IActivityService _activityService;
        public ActivityController(IActivityService activityService)
        {
            _activityService = activityService;
        }

        [HttpPost]
        public ActionResult<ActivityModel> Create(ActivityModel model)
        {
            return _activityService.Create(model).ToResponse();
        }

        [HttpGet("/api/activity/currentuser")]
        public ActionResult<List<ActivityRecordViewModel>> GetCurrentUserActivity()
        {
            return _activityService.GetCurrentUserActivity().ToResponse();
        }

        [HttpPost("GetListActivityName")]
        public ActionResult<List<ActivityRecordViewModel>> GetListActivityName([FromBody] List<Guid> groupIds)
        {
            return _activityService.GetListActivityName(groupIds).ToResponse();
        }

        [HttpGet("User/{id:int}")]
        public ActionResult<List<ActivityRecordViewModel>> GetActivityByUserId(int id)
        {
            return _activityService.GetActivityByUserId(id).ToResponse();
        }
        [HttpPost("ActivityForReportProject")]
        public ActionResult<List<ActivityRecordViewModel>> ActivityForReportProject([FromBody] List<Guid> groupIds)
        {
            return _activityService.GetActivityForReportProject(groupIds).ToResponse();
        }
        [HttpPut]
        public ActionResult<ActivityModel> Update(ActivityModel model)
        {
            return _activityService.Update(model).ToResponse();
        }

        [HttpDelete("{id:Guid}")]
        public ActionResult<bool> Delete(Guid id)
        {
            return _activityService.Delete(id).ToResponse();
        }
    }
}

using Kloon.EmployeePerformance.Logic.Services;
using Kloon.EmployeePerformance.Models.TimeSheet;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

namespace Kloon.EmployeePerformance.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ActivityGroupController : ControllerBase
    {
        private readonly IActivityGroupService _activityGroupService;
        public ActivityGroupController(IActivityGroupService activityGroupService)
        {
            _activityGroupService = activityGroupService;
        }
        [HttpPost]
        public ActionResult<ActivityGroupModel> Create(ActivityGroupModel model)
        {
            return _activityGroupService.Create(model).ToResponse();
        }
        [HttpGet]
        public ActionResult<List<ActivityGroupModel>> GetAll()
        {
            return _activityGroupService.GetAll().ToResponse();
        }

        [HttpGet("ActivityGroupTimeSheetReport")]
        public ActionResult<List<ActivityGroupModel>> GetActivityGroupModelForTimeSheetReport()
        {
            return _activityGroupService.GetActivityGroupModelForTimeSheetReport().ToResponse();
        }
        [HttpGet("ActivityGroupForReportProject")]
        public ActionResult<List<ActivityGroupModel>> GetActivityGroupForReportProject()
        {
            return _activityGroupService.GetActivityGroupForReportProject().ToResponse();
        }
        [HttpGet("{id:Guid}")]
        public ActionResult<ActivityGroupModel> GetById(Guid id)
        {
            return _activityGroupService.GetById(id).ToResponse();
        }
        [HttpPut]
        public ActionResult<ActivityGroupModel> Update(ActivityGroupModel model)
        {
            return _activityGroupService.Update(model).ToResponse();
        }
        [HttpDelete("{id:Guid}")]
        public ActionResult<bool> Delete(Guid id)
        {
            return _activityGroupService.Delete(id).ToResponse();
        }
    }
}

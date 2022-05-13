using Kloon.EmployeePerformance.Logic.Services;
using Kloon.EmployeePerformance.Models.TimeSheet;
using Microsoft.AspNetCore.Mvc;
using System;

namespace Kloon.EmployeePerformance.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ActivityGroupUserController : ControllerBase
    {
        private readonly IActivityGroupUserService _activityGroupUserService;
        public ActivityGroupUserController(IActivityGroupUserService activityGroupUserService)
        {
            _activityGroupUserService = activityGroupUserService;
        }
        [HttpPost]
        public ActionResult<ActivityGroupUserModel> Create(ActivityGroupUserModel model)
        {
            return _activityGroupUserService.Create(model).ToResponse();
        }
        [HttpGet("{id:Guid}")]
        public ActionResult<ActivityGroupUserModel> GetById(Guid id)
        {
            return _activityGroupUserService.GetById(id).ToResponse();
        }
        [HttpPut]
        public ActionResult<ActivityGroupUserModel> Update(ActivityGroupUserModel model)
        {
            return _activityGroupUserService.Update(model).ToResponse();
        }
        [HttpDelete("{id:Guid}")]
        public ActionResult<bool> Delete(Guid id)
        {
            return _activityGroupUserService.Delete(id).ToResponse();
        }
        [HttpGet("{uid:int}")]
        public ActionResult<int> CheckManagerByUserId(int uid)
        {
            return _activityGroupUserService.CheckManagerByUserId(uid).ToResponse();
        }
    }
}

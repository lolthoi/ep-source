using Kloon.EmployeePerformance.Logic.Services;
using Kloon.EmployeePerformance.Models.WorkSpaceSetting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Kloon.EmployeePerformance.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WorkSpaceSettingController : ControllerBase
    {
        private readonly IWorkSpaceSettingService _workSpaceSettingService;
        public WorkSpaceSettingController(IWorkSpaceSettingService workSpaceSettingService)
        {
            _workSpaceSettingService = workSpaceSettingService;
        }

        [HttpGet]
        public ActionResult<WorkSpaceSettingModel> GetAll()
        {
            return _workSpaceSettingService.GetAllWorkSpaceSettings().ToResponse();
        }
        [HttpPut]
        public ActionResult<WorkSpaceSettingModel> Edit(WorkSpaceSettingModel model)
        {
            return _workSpaceSettingService.Edit(model).ToResponse();
        }
    }
}

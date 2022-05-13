using Kloon.EmployeePerformance.Logic.Services;
using Kloon.EmployeePerformance.Models.Common;
using Kloon.EmployeePerformance.Models.TimeSheet.TimeSheetReport;
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
    public class TimeSheetReportDetailController : ControllerBase
    {
        private ITimeSheetReportDetailService _timeSheetReportDetailService;
        public TimeSheetReportDetailController(ITimeSheetReportDetailService timeSheetReportDetailService)
        {
            _timeSheetReportDetailService = timeSheetReportDetailService;
        }

        [HttpGet("GetAllRecordPaging")]
        public ActionResult<PaginationSet<TimeSheetReportDetailModel>> GetAllRecordPaging([FromQuery] TimeSheetReportDetailRouter model)
        {
            model.EndDate = new DateTime(model.EndDate.Year, model.EndDate.Month, model.EndDate.Day, 23, 59, 59);
            return _timeSheetReportDetailService.GetAllRecordPaging(model).ToResponse();
        }

        [HttpPost("GetAllUserRecord")]
        public ActionResult<List<TimeSheetReportUserModel>> GetAllUserRecord([FromBody] FilterParamUserRecord model)
        {
            return _timeSheetReportDetailService.GetAllUserRecord(model).ToResponse();
        }

        [HttpPost("GetAllProjectRecord")]
        public ActionResult<List<TimeSheetReportProjectModel>> GetAllProjectRecord([FromBody] FilterParamProjectRecord model)
        {
            return _timeSheetReportDetailService.GetAllProjectRecord(model).ToResponse();
        }

        [HttpPost("GetAllUserActivityRecord")]
        public ActionResult<List<TimeSheetReportUserActivityModel>> GetAllUserActivityRecord([FromBody] FilterParamUserActivity model)
        {
            return _timeSheetReportDetailService.GetAllUserActivityRecord(model).ToResponse();
        }
        [HttpPost("GetAllProjectActivityRecord")]
        public ActionResult<List<TimeSheetReportProjectActivityModel>> GetAllProjectActivityRecord([FromBody] FilterParamProjectActivity model)
        {
            return _timeSheetReportDetailService.GetAllProjectActivityRecord(model).ToResponse();
        }
    }
}
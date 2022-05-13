using Kloon.EmployeePerformance.Logic.Services;
using Kloon.EmployeePerformance.Models.Common;
using Kloon.EmployeePerformance.Models.TimeSheet.TimeSheetRecord;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Kloon.EmployeePerformance.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TimeSheetRecordController
    {
        private readonly ITimeSheetRecordService _tsRecordService;

        public TimeSheetRecordController(ITimeSheetRecordService tsRecordService)
        {
            _tsRecordService = tsRecordService;
        }

        [HttpGet]
        public ActionResult<PaginationSet<TimeSheetRecordModel>> GetAll(string searchText, int? page, int? pageSize)
        {
            var pageVal = page ?? 0;
            var pageSizeVal = pageSize ?? 10;
            return _tsRecordService.GetAll(searchText, pageVal, pageSizeVal).ToResponse();
        }

        [HttpPost]
        public ActionResult<TimeSheetRecordModel> Create(TimeSheetRecordModel model)
        {
            return _tsRecordService.Create(model).ToResponse();
        }

        [HttpPut]
        public ActionResult<TimeSheetRecordModel> Update(TimeSheetRecordModel model)
        {
            return _tsRecordService.Update(model).ToResponse();
        }
        [HttpDelete("{id:Guid}")]
        public ActionResult<bool> Delete(Guid id)
        {
            return _tsRecordService.Delete(id).ToResponse();
        }
    }
}

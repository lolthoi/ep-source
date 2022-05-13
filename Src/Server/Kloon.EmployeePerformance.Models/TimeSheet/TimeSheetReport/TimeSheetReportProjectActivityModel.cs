using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kloon.EmployeePerformance.Models.TimeSheet.TimeSheetReport
{
    public class TimeSheetReportProjectActivityModel
    {
        public Guid GroupId { get; set; }
        public string GroupName { get; set; }
        public Guid ActivityId { get; set; }
        public string ActivityName { get; set; }
        public double? TotalHour { get; set; }
        public int? ProjectId { get; set; }
    }
    public class FilterParamProjectActivity
    {
        public List<Guid> GroupIds { get; set; }
        public List<Guid> ActivityIds { get; set; }
        public DateTime? StartDate { get; set; } = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        public DateTime? EndDate { get; set; } = DateTime.Now;
    }
}

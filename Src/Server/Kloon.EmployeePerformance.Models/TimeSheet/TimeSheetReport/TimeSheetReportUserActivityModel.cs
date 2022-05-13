using System;
using System.Collections.Generic;

namespace Kloon.EmployeePerformance.Models.TimeSheet.TimeSheetReport
{
    public class TimeSheetReportUserActivityModel
    {
        public int UserId { get; set; }
        public string FullName { get; set; }
        public Guid ActivityId { get; set; }
        public string DisplayName { get; set; }
        public string GroupName { get; set; }
        public string ActivityName
        {
            get
            {
                return GroupName + ": " + DisplayName;
            }
        }
        public double? TotalHour { get; set; }
    }
    public class FilterParamUserActivity
    {
        public List<int> UIds { get; set; }
        public List<int> PIds { get; set; }
        public List<Guid> ActivityIds { get; set; }
        public DateTime? StartDate { get; set; } = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        public DateTime? EndDate { get; set; } = DateTime.Now;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kloon.EmployeePerformance.Models.TimeSheet.TimeSheetReport
{
    public class TimeSheetReportDetailRouter
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 5;
        public List<int> UserIds { get; set; } = null;
        public List<int> ProjectIds { get; set; } = null;
        public List<Guid> TSAcitivityGroupIds { get; set; } = null;
        public List<Guid> TaskIds { get; set; } = null;
        public DateTime StartDate { get; set; } = DateTime.Now.AddDays(-7);
        public DateTime EndDate { get; set; } = DateTime.Now;
    }
}

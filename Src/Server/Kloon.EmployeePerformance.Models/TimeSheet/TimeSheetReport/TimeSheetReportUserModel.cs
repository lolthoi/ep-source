using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kloon.EmployeePerformance.Models.TimeSheet.TimeSheetReport
{
    public class TimeSheetReportUserModel
    {
        public int UserId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public double? January { get; set; }
        public double? February { get; set; }
        public double? March { get; set; }
        public double? April { get; set; }
        public double? May { get; set; }
        public double? June { get; set; }
        public double? July { get; set; }
        public double? August { get; set; }
        public double? September { get; set; }
        public double? October { get; set; }
        public double? November { get; set; }
        public double? December { get; set; }
    }

    public class MonthlyRecord
    {
        public int Month { get; set; }
        public double? TimeRecord { get; set; } 
    }

    public class FilterParamUserRecord
    {
        public List<int> UIds { get; set; }
        public List<int> PIds { get; set; }
        public int? Year { get; set; }
    }
}

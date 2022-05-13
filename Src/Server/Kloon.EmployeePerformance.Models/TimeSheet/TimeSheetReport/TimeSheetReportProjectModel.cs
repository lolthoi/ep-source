using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kloon.EmployeePerformance.Models.TimeSheet.TimeSheetReport
{
    public class TimeSheetReportProjectModel
    {
        public Guid GroupId { get; set; }
        public int? ProjectId { get; set; }
        public string Name { get; set; }
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
    public class FilterParamProjectRecord
    {
        public List<Guid> GIds { get; set; }
        public int? Year { get; set; }
    }
}

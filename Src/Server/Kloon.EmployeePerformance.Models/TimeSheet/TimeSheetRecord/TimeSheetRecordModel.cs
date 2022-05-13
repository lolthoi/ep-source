using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kloon.EmployeePerformance.Models.TimeSheet.TimeSheetRecord
{
    public class TimeSheetRecordModel
    {
        public Guid Id { get; set; }
        public Guid TSActivityId { get; set; }
        public int UserId { get; set; }
        public string Name { get; set; }
        public string BacklogId { get; set; }
        public string TaskId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kloon.EmployeePerformance.Models.TimeSheet.TimeSheetReport
{
    public class TimeSheetReportDetailModel
    {
        public int UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public string BacklogId { get; set; }
        public string TaskId { get; set; }
        public Guid TSAcitivityGroupId { get; set; }
        public Guid TSActivityId { get; set; }
        public string TSAcitivityGroupName { get; set; }
        public string TSActivityName { get; set; }

        public Guid TimeSheetRecordId { get; set; }
        public string TimeSheetRecordName { get; set; }

        public DateTime StartDate { get; set; }
        public string StartHour { get; set; }
        public DateTime EndDate { get; set; }
        public string EndHour { get; set; }
        public string DateRecord { get; set; }

        public TimeSpan TotalTime { get; set; }

        public string NameTask
        {
            get
            {
                return TSAcitivityGroupName + ": " + TSActivityName;
            }
        }

        public TimeSpan Duration
        {
            get
            {
                return EndDate - StartDate;
            }
        }

        public string FullName
        {
            get
            {
                return FirstName + " " + LastName;
            }
        }
    }
}

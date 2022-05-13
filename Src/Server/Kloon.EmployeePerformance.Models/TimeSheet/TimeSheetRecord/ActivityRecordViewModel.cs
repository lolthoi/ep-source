using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kloon.EmployeePerformance.Models.TimeSheet.TimeSheetRecord
{
    public class ActivityRecordViewModel
    {
        public Guid Id { get; set; }
        public string DisplayName { get; set; }
        public Guid GroupId { get; set; }
        public string GroupName { get; set; }
        public int? DeletedBy { get; set; }
        public int? ProjectId { get; set; }
        public string ActivityName
        {
            get
            {
                return GroupName + " : " + DisplayName;
            }
        }
    }
}

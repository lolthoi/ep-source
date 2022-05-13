using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kloon.EmployeePerformance.Models.TimeSheet
{
    public class ActivityModel
    {
        public Guid Id { get; set; }
        public Guid ActivityGroupId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}

using System;
using System.Collections.Generic;

namespace Kloon.EmployeePerformance.Models.TimeSheet
{
    public class ActivityGroupModel
    {
        public Guid Id { get; set; }
        public int? ProjectId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<ActivityModel> Activities { get; set; }
        public List<ActivityGroupUserModel> ActivityGroupUserModels { get; set; }
    }
}

using Kloon.EmployeePerformance.Models.Common;
using System;

namespace Kloon.EmployeePerformance.Models.TimeSheet
{
    public class ActivityGroupUserModel
    {
        public Guid Id { get; set; }
        public Guid ActivityGroupId { get; set; }
        public int UserId { get; set; }
        public TSRole Role { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
    }
}

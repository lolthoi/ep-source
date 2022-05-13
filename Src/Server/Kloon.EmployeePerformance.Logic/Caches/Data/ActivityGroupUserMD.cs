using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kloon.EmployeePerformance.Logic.Caches.Data
{
    public class ActivityGroupUserMD
    {
        public Guid Id { get; set; }
        public Guid TSAcitivityGroupId { get; set; }
        public int UserId { get; set; }
        public int Role { get; set; }
        public int? DeletedBy { get; set; }
        public DateTime? DeletedDate { get; set; }
    }
}

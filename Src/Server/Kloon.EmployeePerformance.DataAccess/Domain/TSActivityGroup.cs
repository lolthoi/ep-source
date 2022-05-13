using Kloon.EmployeePerformance.DataAccess.Extentions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kloon.EmployeePerformance.DataAccess.Domain
{
    public class TSActivityGroup : AuditedEntity<Guid>, IDeleteable
    {
        public int? ProjectId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int? DeletedBy { get; set; }
        public DateTime? DeletedDate { get; set; }

        public virtual ICollection<TSActivity> TSActivities { get; set; }
        public virtual ICollection<ActivityGroupUser> ActivityGroupUsers { get; set; }

    }
}

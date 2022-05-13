using Kloon.EmployeePerformance.DataAccess.Extentions;
using System;

namespace Kloon.EmployeePerformance.DataAccess.Domain
{
    public class ActivityGroupUser : AuditedEntity<Guid>, IDeleteable
    {
        public Guid TSActivityGroupId { get; set; }
        public int UserId { get; set; }
        public int Role { get; set; }
        public int? DeletedBy { get; set; }
        public DateTime? DeletedDate { get; set; }

        public virtual TSActivityGroup TSActivityGroup { get; set; }
    }
}

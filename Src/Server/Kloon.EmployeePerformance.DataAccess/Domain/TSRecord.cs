using Kloon.EmployeePerformance.DataAccess.Extentions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kloon.EmployeePerformance.DataAccess.Domain
{
    public class TSRecord : AuditedEntity<Guid>, IDeleteable
    {
        public Guid TSActivityId { get; set; }
        public int UserId { get; set; }
        public string Name { get; set; }
        public string BacklogId { get; set; }
        public string TaskId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        public int? DeletedBy { get; set; }
        public DateTime? DeletedDate { get; set; }

        public virtual TSActivity TSActivity { get; set; }
    }
}

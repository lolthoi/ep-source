using Kloon.EmployeePerformance.DataAccess.Extentions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kloon.EmployeePerformance.DataAccess.Domain
{
    public class TSActivity : AuditedEntity<Guid>, IDeleteable
    {
        public Guid TSActivityGroupId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int? DeletedBy { get; set; }
        public DateTime? DeletedDate { get; set; }

        public virtual TSActivityGroup TSActivityGroup { get; set; }
        public virtual ICollection<TSRecord> TSRecords { get; set; }
    }
}

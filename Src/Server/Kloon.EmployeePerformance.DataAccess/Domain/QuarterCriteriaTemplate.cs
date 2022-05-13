using Kloon.EmployeePerformance.DataAccess.Extentions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kloon.EmployeePerformance.DataAccess.Domain
{
    public class QuarterCriteriaTemplate : AuditedEntity<Guid>, IDeleteable
    {
        public string Name { get; set; }
        public int Year { get; set; }
        public int Quarter { get; set; }
        public int PositionId { get; set; }
        public virtual ICollection<CriteriaType> CriteriaTypes { get; set; }
        public int? DeletedBy { get; set; }
        public DateTime? DeletedDate { get; set; }
    }
}

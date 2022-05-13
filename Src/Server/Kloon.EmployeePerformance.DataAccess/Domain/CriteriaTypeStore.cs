using Kloon.EmployeePerformance.DataAccess.Extentions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kloon.EmployeePerformance.DataAccess.Domain
{
    public class CriteriaTypeStore : AuditedEntity<Guid>, IDeleteable
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int OrderNo { get; set; }
        public virtual ICollection<CriteriaStore> CriteriaStores { get; set; }
        public int? DeletedBy { get; set; }
        public DateTime? DeletedDate { get; set; }
    }
}

using Kloon.EmployeePerformance.DataAccess.Domain;
using Kloon.EmployeePerformance.DataAccess.Extentions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kloon.EmployeePerformance.DataAccess.Domain
{
    public class Criteria : AuditedEntity<Guid>, IDeleteable
    {
        public Guid CriteriaTypeId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int OrderNo { get; set; }
        public virtual CriteriaType CriteriaType { get; set; }
        public virtual ICollection<CriteriaQuarterEvaluation> CriteriaQuarterEvaluations { get; set; }
        public int? DeletedBy { get; set; }
        public DateTime? DeletedDate { get; set; }
    }
}

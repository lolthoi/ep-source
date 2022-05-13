using Kloon.EmployeePerformance.DataAccess.Extentions;
using Kloon.EmployeePerformance.DataAccess.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kloon.EmployeePerformance.DataAccess.Domain
{
    public class CriteriaType : AuditedEntity<Guid>, IDeleteable
    {
        public Guid? QuarterCriteriaTemplateId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int OrderNo { get; set; }
        public virtual QuarterCriteriaTemplate QuarterCriteriaTemplate { get; set; }
        public virtual ICollection<Criteria> Criterias { get; set; }
        public virtual ICollection<CriteriaTypeQuarterEvaluation> CriteriaTypeQuarterEvaluations { get; set; }
        public int? DeletedBy { get; set; }
        public DateTime? DeletedDate { get; set; }
    }
}

using Kloon.EmployeePerformance.DataAccess.Extentions;
using System;
using System.Collections.Generic;

namespace Kloon.EmployeePerformance.DataAccess.Domain
{
    public class EvaluationTemplate : AuditedEntity<Guid>, IDeleteable
    {
        public string Name { get; set; }
        public int PositionId { get; set; }
        public virtual ICollection<CriteriaTypeTemplate> CriteriaTypeTemplates { get; set; }
        public int? DeletedBy { get; set; }
        public DateTime? DeletedDate { get; set; }
    }
}

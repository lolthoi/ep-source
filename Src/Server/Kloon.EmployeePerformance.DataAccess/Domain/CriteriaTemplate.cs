using Kloon.EmployeePerformance.DataAccess.Extentions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kloon.EmployeePerformance.DataAccess.Domain
{
    public class CriteriaTemplate : AuditedEntity<Guid>, IDeleteable
    {
        public Guid CriteriaTypeTemplateId { get; set; }
        public Guid EvaluationTemplateId { get; set; }
        public Guid CriteriaStoreId { get; set; }
        public int OrderNo { get; set; }
        public virtual CriteriaTypeTemplate CriteriaTypeTemplate { get; set; }
        public int? DeletedBy { get; set; }
        public DateTime? DeletedDate { get; set; }
    }
}

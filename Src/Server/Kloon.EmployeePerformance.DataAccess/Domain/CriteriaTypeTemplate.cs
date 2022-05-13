using Kloon.EmployeePerformance.DataAccess.Extentions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kloon.EmployeePerformance.DataAccess.Domain
{
    public class CriteriaTypeTemplate : AuditedEntity<Guid>, IDeleteable
    {
        public Guid CriteriaTypeStoreId { get; set; }
        public Guid EvaluationTemplateId { get; set; }
        public int OrderNo { get; set; }
        public virtual EvaluationTemplate EvaluationTemplate { get; set; }
        public virtual ICollection<CriteriaTemplate> CriteriaTemplates { get; set; }
        public int? DeletedBy { get; set; }
        public DateTime? DeletedDate { get; set; }
    }
}

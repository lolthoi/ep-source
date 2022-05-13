using Kloon.EmployeePerformance.DataAccess.Extentions;
using Kloon.EmployeePerformance.DataAccess.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kloon.EmployeePerformance.DataAccess.Domain
{
    public class CriteriaQuarterEvaluation : AuditedEntity<Guid>
    {
        public Guid QuarterEvaluationId { get; set; }
        public Guid CriteriaId { get; set; }
        public long Point { get; set; }
        public virtual Criteria Criteria { get; set; }
        public virtual QuarterEvaluation QuarterEvaluation { get; set; }
    }
}

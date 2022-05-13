using Kloon.EmployeePerformance.DataAccess.Extentions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kloon.EmployeePerformance.DataAccess.Domain
{
    public class CriteriaTypeQuarterEvaluation : AuditedEntity<Guid>
    {
        public Guid QuarterEvaluationId { get; set; }
        public Guid CriteriaTypeId { get; set; }
        public double PointAverage { get; set; }
        public virtual CriteriaType CriteriaType { get; set; }
        public virtual QuarterEvaluation QuarterEvaluation { get; set; }
    }
}

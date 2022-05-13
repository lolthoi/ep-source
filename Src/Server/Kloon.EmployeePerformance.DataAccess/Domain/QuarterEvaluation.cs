using Kloon.EmployeePerformance.DataAccess.Extentions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kloon.EmployeePerformance.DataAccess.Domain
{
    public class QuarterEvaluation : AuditedEntity<Guid>, IDeleteable
    {
        public int Year { get; set; }
        public int Quarter { get; set; }
        public int UserId { get; set; }
        public int PositionId { get; set; }
        public int ProjectId { get; set; }
        public int ProjectLeaderId { get; set; }
        public double PointAverage { get; set; }
        public string NoteByLeader { get; set; }
        public virtual UserQuarterEvaluation UserQuarterEvaluation { get; set; }
        public virtual ICollection<CriteriaQuarterEvaluation> CriteriaQuarterEvaluations { get; set; }
        public virtual ICollection<CriteriaTypeQuarterEvaluation> CriteriaTypeQuarterEvaluations { get; set; }
        public int? DeletedBy { get; set; }
        public DateTime? DeletedDate { get; set; }
    }
}

using Kloon.EmployeePerformance.DataAccess.Extentions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kloon.EmployeePerformance.DataAccess.Domain
{
    public class UserQuarterEvaluation: AuditedEntity<Guid>, IDeleteable
    {
        public Guid QuarterEvaluationId { get; set; }
        public string NoteGoodThing { get; set; }
        public string NoteBadThing { get; set; }
        public string NoteOther { get; set; }

        public virtual QuarterEvaluation QuarterEvaluation { get; set; }
        public int? DeletedBy { get ; set ; }
        public DateTime? DeletedDate { get ; set ; }
    }
}

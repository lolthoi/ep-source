using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kloon.EmployeePerformance.Models.QuarterEvaluation
{
    public class QuarterEvaluationIdModel
    {
        public Guid QuarterEvaluationId { get; set; }
        public bool IsEnableEvaluationButton { get; set; }
    }
}

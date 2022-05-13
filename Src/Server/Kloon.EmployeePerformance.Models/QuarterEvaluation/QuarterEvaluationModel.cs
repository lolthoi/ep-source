using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kloon.EmployeePerformance.Models.QuarterEvaluation
{
    public class QuarterEvaluationModel
    {
        public Guid Id { get; set; }
        public int Year { get; set; }
        public int Quarter { get; set; }
        public int UserId { get; set; }
        public int PositionId { get; set; }
        public int ProjectId { get; set; }
        public int ProjectLeaderId { get; set; }
        public double PointAverage { get; set; }
        public string NoteByLeader { get; set; }

        #region View Get All
        public string QuarterText { get; set; }
        public string Position { get; set; }
        public string Leader { get; set; }
        public string Project { get; set; }

        #endregion
    }
}

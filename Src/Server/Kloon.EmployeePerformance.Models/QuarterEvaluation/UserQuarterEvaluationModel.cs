using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kloon.EmployeePerformance.Models.QuarterEvaluation
{
    public class UserQuarterEvaluationModel
    {
        public Guid Id { get; set; }
        public Guid QuarterEvaluationId { get; set; }
        public string NoteGoodThing { get; set; }
        public string NoteBadThing { get; set; }
        public string NoteOther { get; set; }


    }

    public class LeaderEvaluationResultModel
    {
        public int UserId { get; set; }
        public string Name { get; set; }
        public string Position { get; set; }
        public int PositionId { get; set; }
        public string ProjectName { get; set; }
        public int ProjectId { get; set; }
        public QuarterInfo Value1 { get; set; }
        public QuarterInfo Value2 { get; set; }
        public QuarterInfo Value3 { get; set; }
        public QuarterInfo Value4 { get; set; }
    }
    public class QuarterInfo
    {
        public Guid Id { get; set; }
        public string Score { get; set; }
        public int Quarter { get; set; }
        public int Year { get; set; }
        public DateTime? CreatedDate { get; set; }
        public bool NeedEvaluate
        {
            get
            {
                var result = true; //discard business, lock evaluation after 7 day
                //if(CreatedDate != null) {
                //    result = (DateTime.Now - CreatedDate).Value.TotalHours < 168; // 7 day = 168 hr: evaluate before 7day after created
                //}
                return result;
            }
        }
    }

    public class QuarterPoint
    {
        public Guid Id { get; set; }
        public int Point { get; set; }
    }
}

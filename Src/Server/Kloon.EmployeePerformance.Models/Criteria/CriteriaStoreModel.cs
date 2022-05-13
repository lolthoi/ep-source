using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kloon.EmployeePerformance.Models.Criteria
{
    public class CriteriaStoreModel
    {
        public Guid Id { get; set; }
        public int OrderNo { get; set; }
        public Guid? TypeId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }

    public class CriteriaModelQuarter
    {
        public Guid Id { get; set; }
        public int OrderNo { get; set; }
        public Guid? TypeId { get; set; }
        public Guid QuarterEvaluationId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public double? Point { set; get; }
    }
}

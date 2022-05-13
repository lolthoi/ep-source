using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kloon.EmployeePerformance.Logic.Caches.Data
{
    public partial class QuarterCriteriaTemplateMD
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public int Year { get; set; }
        public int Quarter { get; set; }
        public int PositionId { get; set; }
        public int? DeletedBy { get; set; }
        public DateTime? DeletedDate { get; set; }
    }
}

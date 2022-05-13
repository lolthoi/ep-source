using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kloon.EmployeePerformance.Models.Criteria
{
    public class CriteriaModel
    {
        public Guid Id { get; set; }
        public int OrderNo { get; set; }
        public Guid? TypeId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}

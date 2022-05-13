using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kloon.EmployeePerformance.Logic.Caches.Data
{
    public class TSActivityGroupMD
    {
        public Guid Id { get; set; }
        public int? ProjectId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int? DeletedBy { get; set; }
        public DateTime? DeletedDate { get; set; }
    }
}

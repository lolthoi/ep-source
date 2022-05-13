using Kloon.EmployeePerformance.DataAccess.Extentions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kloon.EmployeePerformance.DataAccess.Domain
{
    public class Position : AuditedEntity<int>, IDeleteable
    {
        public string Name { get; set; }
        public virtual ICollection<User> Users { get; set; }
        public int? DeletedBy { get; set; }
        public DateTime? DeletedDate { get; set; }

    }
}

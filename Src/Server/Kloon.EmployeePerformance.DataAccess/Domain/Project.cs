using Kloon.EmployeePerformance.DataAccess.Extentions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kloon.EmployeePerformance.DataAccess.Domain
{
    public class Project : AuditedEntity<int>, IDeleteable
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int Status { get; set; }
        public virtual ICollection<ProjectUser> ProjectUsers { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? DeletedBy { get; set; }
        public DateTime? DeletedDate { get; set; }
    }
}

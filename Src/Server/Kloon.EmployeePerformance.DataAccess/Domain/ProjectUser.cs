using Kloon.EmployeePerformance.DataAccess.Extentions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kloon.EmployeePerformance.DataAccess.Domain
{
    public class ProjectUser : AuditedEntity<Guid>, IDeleteable
    {
        public int ProjectId { get; set; }
        public int UserId { get; set; }
        public int ProjectRoleId { get; set; }
        public virtual User User { get; set; }
        public virtual Project Project { get; set; }
        public int? DeletedBy { get; set; }
        public DateTime? DeletedDate { get; set; }
    }
}

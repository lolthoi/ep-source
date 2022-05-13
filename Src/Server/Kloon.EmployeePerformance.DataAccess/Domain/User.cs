using Kloon.EmployeePerformance.DataAccess.Extentions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kloon.EmployeePerformance.DataAccess.Domain
{
    public class User : AuditedEntity<int>, IDeleteable
    {
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int PositionId { get; set; }
        public int? Sex { get; set; }
        public DateTime? DoB { get; set; }
        public string PhoneNo { get; set; }
        public int RoleId { get; set; }
        public string PasswordHash { get; set; }
        public string PasswordSalt { get; set; }
        public string PasswordResetCode { get; set; }

        public virtual Position Position { get; set; }
        public virtual ICollection<ProjectUser> ProjectUsers { get; set; }

        public int? DeletedBy { get; set; }
        public DateTime? DeletedDate { get; set; }

        public bool Status { get; set; }

        
    }
}

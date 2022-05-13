using System;

namespace Kloon.EmployeePerformance.Logic.Caches.Data
{
    public partial class UserMD
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int PositionId { get; set; }
        public int Sex { get; set; }
        public DateTime? DoB { get; set; }
        public string PhoneNo { get; set; }
        public int RoleId { get; set; }

        public int? DeletedBy { get; set; }
        public DateTime? DeletedDate { get; set; }
        public bool Status { get; set; }

        public string FullName
        {
            get
            {
                return FirstName + " " + LastName;
            }
        }
    }

    public partial class UserMD
    {
        public int? ProjectId { get; set; }
        public int? ProjectRoleId { get; set; }
        public Guid? ProjectUserId { get; set; }
    }
}

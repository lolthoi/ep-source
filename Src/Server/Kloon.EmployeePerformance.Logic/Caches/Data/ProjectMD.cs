using System;

namespace Kloon.EmployeePerformance.Logic.Caches.Data
{
    public partial class ProjectMD
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Status { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? DeletedBy { get; set; }
        public DateTime? DeletedDate { get; set; }
    }

    public partial class ProjectMD
    {
        public int? ProjectRoleId { get; set; }
    }
}

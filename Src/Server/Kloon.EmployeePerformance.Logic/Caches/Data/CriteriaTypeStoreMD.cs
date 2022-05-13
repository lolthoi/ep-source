using System;

namespace Kloon.EmployeePerformance.Logic.Caches.Data
{
    public partial class CriteriaTypeStoreMD
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int OrderNo { get; set; }
        public int? DeletedBy { get; set; }
        public DateTime? DeletedDate { get; set; }
    }
}

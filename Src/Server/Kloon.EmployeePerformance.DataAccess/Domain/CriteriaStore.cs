using Kloon.EmployeePerformance.DataAccess.Extentions;
using System;

namespace Kloon.EmployeePerformance.DataAccess.Domain
{
    public class CriteriaStore : AuditedEntity<Guid>, IDeleteable
    {
        public Guid CriteriaTypeId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int OrderNo { get; set; }
        public virtual CriteriaTypeStore CriteriaTypeStore { get; set; }
        public int? DeletedBy { get; set; }
        public DateTime? DeletedDate { get; set; }
    }
}

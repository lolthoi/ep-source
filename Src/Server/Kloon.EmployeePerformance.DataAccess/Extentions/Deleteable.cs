using System;

namespace Kloon.EmployeePerformance.DataAccess.Extentions
{
    public interface IDeleteable
    {
        int? DeletedBy { get; set; }
        DateTime? DeletedDate { get; set; }
    }
    public class Deleteable : IDeleteable
    {
        public int? DeletedBy { get; set; }
        public DateTime? DeletedDate { get; set; }
    }
}

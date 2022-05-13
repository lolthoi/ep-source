using System.ComponentModel.DataAnnotations;

namespace Kloon.EmployeePerformance.DataAccess.Domain
{
    public class AppSetting
    {
        [Key]
        public string Id { get; set; }
        public string Value { get; set; }
        public bool Status { get; set; }

        public byte[] RowVersion { get; set; }
    }
}

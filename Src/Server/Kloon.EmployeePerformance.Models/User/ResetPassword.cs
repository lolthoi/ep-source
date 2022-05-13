using System.ComponentModel.DataAnnotations;

namespace Kloon.EmployeePerformance.Models.User
{
    public class ResetPassword
    {
        public string Password { get; set; }
        public string NewPassword { get; set; }

        public string ConfirmPassword { get; set; }

        public string Code { get; set; }

        public bool Status { get; set; } = false;
    }
}

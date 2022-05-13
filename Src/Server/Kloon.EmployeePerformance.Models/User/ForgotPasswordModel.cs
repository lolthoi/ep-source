using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kloon.EmployeePerformance.Models.User
{
    public class ForgotPasswordModel
    {
        public string Email { get; set; }
        public string Url { get; set; }
        public string Code { get; set; }
        public bool IsSuccess { get; set; } = false;
    }
}

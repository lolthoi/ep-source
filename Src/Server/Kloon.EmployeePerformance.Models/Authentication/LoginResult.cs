using Kloon.EmployeePerformance.Models.Common;
using Kloon.EmployeePerformance.Models.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kloon.EmployeePerformance.Models.Authentication
{
    public class LoginResult
    {
        public int Id { get; set; }
        public bool IsSuccessful { get; set; }
        public string Token { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public Roles AppRole { get; set; }
        public List<ProjectRoleModel> ProjectRoles { get; set; }
        public string FullName
        {
            get
            {
                return FirstName + " " + LastName;
            }
        }
    }
}

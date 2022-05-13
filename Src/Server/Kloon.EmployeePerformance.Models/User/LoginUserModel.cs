using Kloon.EmployeePerformance.Models.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kloon.EmployeePerformance.Models.User
{
    public class LoginUserModel
    {
        public string Key { get; set; }
        public LoginUser User { get; set; }

        public class LoginUser
        {
            public int Id { get; set; }
            public string Email { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public int? PositionId { get; set; }
            public Roles Role { get; set; }
            //public List<int> ProjectIds { get; set; }
            public List<ProjectRoleModel> ProjectRole { get; set; }
            public string FullName
            {
                get
                {
                    return FirstName + " " + LastName; 
                }
            }
        }
    }

    public class ProjectRoleModel
    {
        public int ProjectId { get; set; }
        public ProjectRoles ProjectRoleId { get; set; }
    }
}

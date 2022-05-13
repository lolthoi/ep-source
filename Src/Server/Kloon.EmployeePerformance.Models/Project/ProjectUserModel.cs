using Kloon.EmployeePerformance.Models.Common;
using Kloon.EmployeePerformance.Models.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kloon.EmployeePerformance.Models.Project
{
    public class ProjectUserModel
    {
        public Guid Id { get; set; }
        public int UserId { get; set; }
        public int ProjectId { get; set; }
        public string ProjectName { get; set; }

        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public SexEnum Sex { get; set; } = SexEnum.MALE;
        public ProjectRoles ProjectRoleId { get; set; } = ProjectRoles.MEMBER;
        public string FullName
        {
            get
            {
                return FirstName + " " + LastName;
            }
        }
        public string ProjectRole 
        {
            get
            {
                if (ProjectRoleId == ProjectRoles.MEMBER)
                    return "Member";
                else if (ProjectRoleId == ProjectRoles.QA)
                    return "QA(Quality Assurance)";
                else if (ProjectRoleId == ProjectRoles.PM)
                    return "Project Manager";
                return "";
            }
        }
    }
}

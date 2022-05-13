using Kloon.EmployeePerformance.Models.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kloon.EmployeePerformance.Models.User
{
    public class UserModel
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int PositionId { get; set; }
        public SexEnum Sex { get; set; }
        public DateTime? DoB { get; set; }
        public string PhoneNo { get; set; }
        public int RoleId { get; set; }
        public bool Status { get; set; }
        public string FullNameEmail
        { 
            get 
            {
                return FirstName + " " + LastName +" (" + Email + ")"; 
            }
        }
        public string FullName
        {
            get
            {
                return FirstName + " " + LastName;
            }
        }
    }
}

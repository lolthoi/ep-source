using Kloon.EmployeePerformance.Models.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kloon.EmployeePerformance.Models.Project
{
    public class ProjectModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public ProjectStatusEnum Status { get; set; }
        //public int? DeletedBy { get; set; }
        //public DateTime? DeletedDate { get; set; }

        public string StatusText
        {
            get
            {
                if (Status == ProjectStatusEnum.OPEN)
                    return "Open";
                else if (Status == ProjectStatusEnum.PENDING)
                    return "Pending";
                else if (Status == ProjectStatusEnum.CLOSED)
                    return "Closed";
                return "";
            }
        }
    }
}

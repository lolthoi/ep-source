using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kloon.EmployeePerformance.Models.Common
{
    public class PaginationSet<T> where T : class
    {
        public int Page { get; set; }

        public int Count
        {
            get { return null != Items ? Items.Count() : 0; }
        }

        public int TotalPages { get; set; }
        public int TotalCount { get; set; }

        public IEnumerable<T> Items { get; set; }
    }
}

using System.Collections.Generic;

namespace Kloon.EmployeePerformance.DataAccess
{
    public class ConnectionSettings
    {
        public bool UseConnectionStrings { get; set; }

        public Dictionary<string, string> ConnectionStrings { get; set; }
    }
}

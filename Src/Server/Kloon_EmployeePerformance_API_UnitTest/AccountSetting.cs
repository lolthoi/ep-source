using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kloon_EmployeePerformance_UnitTest
{
    public class AccountSetting
    {
        public Account Admin { get; set; }

        public Account User { get; set; }


        public class Account
        {
            public string UserName { get; set; }
            public string Password { get; set; }
        }
    }
}

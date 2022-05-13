using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kloon_EmployeePerformance_UnitTest
{
    public class InvalidModel<T> where T : class
    {
        public InvalidModel(string key, T value)
        {
            Key = key;
            Value = value;
        }
        public string Key { get; set; }
        public T Value { get; set; }
    }
}

using Kloon.EmployeePerformance.Models.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Kloon.EmployeePerformance.WebAPI
{
    public static class ResultExtensions
    {
        public static T ToResponse<T>(this ResultModel<T> result)
        {
            if (result.Error == null)
            {
                return result.Data;
            }
            throw new LogicException(result.Error);
        }
    }
}

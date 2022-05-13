using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Kloon.EmployeePerformance.WebAPI
{
    public class HttpResponseException : Exception
    {
        public int Status { get; set; } = 500;

        public object Value { get; set; }
    }
    public class HttpResponseExceptionFilter : IActionFilter, IOrderedFilter
    {
        public int Order { get; } = int.MaxValue - 10;

        public void OnActionExecuting(ActionExecutingContext context) { }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            if (context.Exception == null)
                return;

            if (context.Exception is LogicException logicEx)
            {
                context.Result = new ObjectResult(logicEx.Message)
                {
                    StatusCode = (int)logicEx.StatusCode,
                };
                context.ExceptionHandled = true;
                return;
            }

            if (context.Exception is HttpResponseException exception)
            {
                context.Result = new ObjectResult(exception.Value)
                {
                    StatusCode = exception.Status,
                };
                context.ExceptionHandled = true;
                return;
            }

            context.Result = new ObjectResult(context.Exception.Message)
            {
                StatusCode = 500,
            };
            context.ExceptionHandled = true;
        }
    }
}

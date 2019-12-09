using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Filters;

namespace WideWorldImporters.API.Filters
{
    public class CustomResultFilter : Attribute, IResultFilter
    {
        public void OnResultExecuted(ResultExecutedContext context)
        {
            Console.WriteLine("Result Executed");
        }

        public void OnResultExecuting(ResultExecutingContext context)
        {
            Console.WriteLine("Result Executing");
            
            context.HttpContext.Response.Headers.Add("author", "dw");
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;

namespace WideWorldImporters.API.Filters
{
    public class CustomActionFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {

            Console.WriteLine($"Action:{context.ActionDescriptor.DisplayName} Executing");
            //base.OnActionExecuting(context);
        }

        public override void OnActionExecuted(ActionExecutedContext context)
        {
            Console.WriteLine($"Action:{context.ActionDescriptor.DisplayName} Executed");
            //base.OnActionExecuted(context);
        }
    }
}

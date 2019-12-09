using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace WideWorldImporters.API.Filters
{
    public class CustomAuthorizeActionFilterAttribute : Attribute, IActionFilter
    {
        public void OnActionExecuted(ActionExecutedContext context)
        {
            // 取出session值
            var bytes = new byte[100];
            if (context.HttpContext.Session.TryGetValue("sessionId", out bytes))
            {
                if (System.Text.Encoding.UTF8.GetString(bytes) == "admin")
                {
                    context.HttpContext.Response.WriteAsync("验证成功");
                }

            }
            else
            {
                context.Result = new ContentResult() { Content = "验证失败" };
            }
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            context.HttpContext.Response.WriteAsync("验证中。。。。");
        }
    }
}

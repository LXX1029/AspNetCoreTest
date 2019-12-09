using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WideWorldImporters.API.DealException
{
    /// <summary>
    /// 自定义异常过滤
    /// </summary>
    public class CustomerExceptionFilter : Attribute, IExceptionFilter
    {

        private readonly ILogger logger = null;
        private readonly IWebHostEnvironment environment = null;
        /// <summary>
        /// 注入服务
        /// </summary>
        /// <param name="logger">日志</param>
        /// <param name="environment">程序托管运行环境</param>
        public CustomerExceptionFilter(ILogger<CustomerExceptionFilter> logger, IWebHostEnvironment environment)
        {
            this.logger = logger;
            this.environment = environment;
        }

        public void OnException(ExceptionContext context)
        {
            Exception exception = context.Exception;
            string error = string.Empty;
            void ReadException(Exception ex) // 获取异常
            {
                error += $"{ex.Message}|{ex.StackTrace}|{ex.InnerException}";
                if (ex.InnerException != null)
                {
                    ReadException(ex.InnerException);
                }
            }
            ReadException(context.Exception);
            logger.LogError(error);
            ContentResult result = new ContentResult
            {
                StatusCode = 500,
                ContentType = "text/json;charset=utf-8"
            };
            //if (environment.IsDevelopment()) // 开发环境
            //{
            //    var json = new { message = exception.Message, detail = error };
            //    result.Content = JsonConvert.SerializeObject(json); // 设置响应结果正文
            //}
            //else
            //{
            //    result.Content = "服务器挂了。。。";
            //}
            context.Result = result;
            context.ExceptionHandled = true;
        }
    }
}

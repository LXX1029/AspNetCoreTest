using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WideWorldImporters.API.Models;

namespace WideWorldImporters.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            IHost host = CreateWebHostBuilder(args).Build();
            host.Run();
        }

        public static IHostBuilder CreateWebHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webHost =>
            {
                webHost.UseStartup<Startup>();
            });
        //.UseKestrel()                       // 使用kestrel
        //.UseUrls("http://localhost:7555")   // 自定义发布地址
        //.UseIISIntegration()
        //.UseContentRoot(Directory.GetCurrentDirectory())
        //.UseStartup<Startup>();
    }
}

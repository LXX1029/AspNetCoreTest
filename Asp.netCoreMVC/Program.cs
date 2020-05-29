using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Asp.netCoreMVC
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {

                    webBuilder.ConfigureServices((context, service) =>
                    {
                        context.HostingEnvironment.ContentRootPath = Directory.GetCurrentDirectory();
                        service.Configure<KestrelServerOptions>(options =>
                        {
                            options.Limits.MinResponseDataRate = null;
                            options.Listen(IPAddress.Any, int.Parse(context.Configuration["Port"]), listen =>
                            {
                                listen.UseHttps("AspNetCore.pfx", "123456");

                            });
                        });
                    });
                    //webBuilder.ConfigureKestrel((context, options) =>
                    //{
                    //    options.Listen(IPAddress.Any, int.Parse(context.Configuration["Port"]), listen =>
                    //    {
                    //        listen.UseHttps("", "");
                    //    });
                    //});
                    webBuilder.UseKestrel();
                    webBuilder.UseStartup<Startup>();
                });
    }
}

using System;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WideWorldImporters.API.Controllers;
using WideWorldImporters.API.Models;
using Microsoft.OpenApi.Models;

using System.Reflection;
using System.IO;
using NLog.Extensions.Logging;
using WideWorldImporters.API.Filters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace WideWorldImporters.API
{
    public class Startup
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Startup"/> class.
        /// </summary>
        /// <param name="configuration"></param>
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }



        //public static readonly SymmetricSecurityKey symmetricKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes("need_to_get_this_from_enviroment"));


        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {


            #region 添加DbContext配置
            services.AddDbContext<WideWorldImportersDbContext>(options =>
            {
                //Configuration["AppSettings:ConnectionString"]
                string str = Configuration.GetSection("connectionStr").Value;
                //string str = this.Configuration.GetConnectionString("connectionStr");

                options.UseSqlServer(str);
            });


            services.AddIdentity<User, IdentityRole>().AddEntityFrameworkStores<WideWorldImportersDbContext>();
            // 注入仓储
            services.AddScoped<IWarehouseRepository, WarehouseRepository>();

            #endregion
            services.AddResponseCompression();



            #region cookie验证
            //services.Configure<CookiePolicyOptions>(options =>
            //{
            //    // 设置配置基本需要
            //    options.CheckConsentNeeded = context => true;
            //    options.MinimumSameSitePolicy = Microsoft.AspNetCore.Http.SameSiteMode.None;
            //});
            //services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie();
            #endregion

            #region JWT

            //    services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            //.AddJwtBearer(o =>
            //{
            //    o.TokenValidationParameters = new TokenValidationParameters
            //    {
            //        NameClaimType = "admin",
            //        RoleClaimType = "role",
            //        ValidIssuer = "admin",
            //        ValidAudience = "api",
            //        IssuerSigningKey = symmetricKey,

            //    };
            //});


            #endregion

            #region 注入服务
            // 注入控制器日志
            services.AddScoped<ILogger, Logger<WarehouseController>>();
            //services.AddSingleton(typeof(ILogger<T>), typeof(Logger<T>));
            #endregion

            // 注入异常日志过滤器到容器[在Controller上使用  [ServiceFilter(typeof(CustomerExceptionFilter))]类过滤exception]
            //services.AddScoped<CustomerExceptionFilter>();

            //services.AddMvc(options =>
            //{
            //    options.EnableEndpointRouting = false;
            //}).SetCompatibilityVersion(CompatibilityVersion.Version_3_0);



            #region 允许跨域访问
            //services.AddCors(options =>
            //   {
            //       options.AddPolicy("any", builder =>
            //       {
            //           builder.AllowAnyOrigin()
            //           .AllowAnyMethod().AllowAnyHeader().AllowCredentials();
            //       });
            //   });
            #endregion

            #region 缓存
            // 启用本地缓存
            services.AddMemoryCache();

            #region RedisCache
            services.AddDataProtection();
            services.AddDistributedRedisCache(options =>
            {
                options.Configuration = this.Configuration["redisCacheConnection"];
                options.InstanceName = "RedisName1";

            });

            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromSeconds(50);
                options.Cookie.HttpOnly = true;
            });
            #endregion

            #region ServerCache
            services.AddDistributedSqlServerCache(options =>
            {
                options.ConnectionString = this.Configuration["sqlserverCacheConnection"];
                options.SchemaName = "dbo";
                options.TableName = "CacheT";
                options.DefaultSlidingExpiration = TimeSpan.FromSeconds(10); // 滑动过期时间，时间内再次请求自动延长过期时间
            });
            #endregion

            #endregion

            #region IOptions配置
            services.Configure<MyTestOptions>(Configuration);
            #endregion

            #region url路径小写
            services.AddRouting(options =>
                {
                    options.LowercaseUrls = true;
                });
            #endregion


            #region Swagger
            services.AddSwaggerGen(m =>
            {
                m.SwaggerDoc("v1", new OpenApiInfo()
                {
                    Title = "WideWorldImporters.API",
                    Version = "v1.0",
                    Description = "WideWorldImporters API说明",
                    Contact = new OpenApiContact
                    {
                        Name = ".net core",
                        Email = "123121.fox.com",
                        //Url = string.Empty,
                    }
                });
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                m.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFile));
            });
            #endregion

            #region 配置NLog
            services.AddLogging(options =>
                {
                    options.AddNLog();   // 使用NLog作为log provider，使用nlog.config记录日志文件。
                });
            #endregion

            services.AddControllers(options =>
            {
                options.CacheProfiles.Add("Default", new Microsoft.AspNetCore.Mvc.CacheProfile
                {
                    Duration = 60,
                    //Location = Microsoft.AspNetCore.Mvc.ResponseCacheLocation.
                });
                options.Filters.Add(new CustomActionFilter());
                options.Filters.Add(new CustomResultFilter());
            });
        }


        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory factory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            #region 配置Nlog
            #endregion
            #region Mvc默认路由规则
            //app.UseMvc();
            //app.UseMvcWithDefaultRoute(); 
            #endregion

            #region Swagger中间件

            app.UseSwagger().UseSwaggerUI(m =>
            {
                m.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
            });
            #endregion

            #region 静态文件  可以使用UseFileServer中间件替代UseDefaultFiles/UseStaticFiles
            // 启用默认文件路径
            //app.UseDefaultFiles();
            // 启用静态文件中间件，使用web根目录内的文件,请求路径：http://<server>/images/1.gif
            //app.UseStaticFiles();

            // 使用web根目录外的文件，非wwwroot文件夹
            //app.UseStaticFiles(new StaticFileOptions()
            //{
            //    FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "MyStaticFiles")),
            //    RequestPath = "/MyStaticFiles",
            //    OnPrepareResponse = ctx =>
            //    {
            //        //ctx.Context.Response.Headers.Add("header1", "自定义header");
            //    },

            //});
            #endregion

            #region UseFileServer
            app.UseFileServer(); // 当前内容目录为 wwwroot目录
            app.UseFileServer(enableDirectoryBrowsing: true);  // enableDirectoryBrowsing 启用目录浏览
                                                               //app.UseFileServer(new FileServerOptions()
                                                               //{
                                                               //    FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot")),
                                                               //    RequestPath = "/root",
                                                               //    EnableDirectoryBrowsing = true,
                                                               //});
            #endregion

            #region 目录浏览
            // wwwroot中的文件
            //app.UseDirectoryBrowser();
            // 自定义目录浏览路径
            //app.UseDirectoryBrowser(new DirectoryBrowserOptions()
            //{
            //    FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "MyStaticFiles", "images")),
            //    RequestPath = "/myimages"
            //});
            #endregion


            #region Rout
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
            #endregion

            //app.UseAuthentication();  // 启用验证
            app.UseAuthorization();

            app.Run(async context =>
            {
                await context.Response.WriteAsync("来自 run 中间件的消息");
            });

            //string str = Configuration.GetSection("connectionStr").Value;
            //if (string.IsNullOrEmpty(str)) return;
            //using (var ctx = new WideWorldImportersDbContext(str))
            //{
            //    // 判断数据库是否创建，未创建则进行创建（注意：此创建不会应用自动创建的Migrations文件）。
            //    if (ctx.Database.EnsureCreated())
            //    {

            //        Console.WriteLine("-------》数据库已创建");
            //    }
            //    else
            //    {
            //        // 判断是否存在未应用的更改，存在则执行数据库迁移
            //        if (ctx.Database.GetPendingMigrations().Any())
            //        {
            //            ctx.Database.SetCommandTimeout(TimeSpan.FromMinutes(30));
            //            Console.WriteLine("-------》正在迁移数据库");
            //            ctx.Database.Migrate();
            //            Console.WriteLine("-------》完成数据库迁移");
            //        }
            //    }
            //}
        }
    }
}

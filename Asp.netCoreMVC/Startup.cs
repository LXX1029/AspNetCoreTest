using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using System.Threading.Tasks;
using Asp.netCoreMVC.ApiHandler;
using Asp.netCoreMVC.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Asp.netCoreMVC
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc(options =>
            {
                options.EnableEndpointRouting = false;
            });

            services.AddEntityFrameworkSqlite().AddDbContext<FlowerDbContext>(options =>
            {
                string connectionStr = this.Configuration["connectionString"];
                options.UseSqlite(connectionStr);
            });


            services.AddIdentity<User, IdentityRole>()
                .AddEntityFrameworkStores<FlowerDbContext>()
                .AddDefaultTokenProviders();
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromSeconds(10);
            });
            services.AddAuthentication(options =>
            {
                options.DefaultScheme = MyHandler.ApiScheme;
            }).AddScheme<ApiAuthenticationSchemeOption, MyHandler>(MyHandler.ApiScheme, options =>
            {
                if (int.TryParse(this.Configuration.GetSection("Api.ExpireDate").Value, out int time))
                    options.ExpireTime = TimeSpan.FromMinutes(time);
            });


            services.AddSingleton(HtmlEncoder.Create(UnicodeRanges.All));
            services.AddScoped<IEmployeeService, EmployeeService>();

            var config1 = this.Configuration.GetSection("UserLoginSettingOption");
            services.Configure<UserLoginSettingOption>(config1);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, RoleManager<IdentityRole> roleManager)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }


            //app.UseWelcomePage();
            #region  资源文件
            // 默认wwwroot文件中的资源，可通过FileProvider指定其它文件目录。
            //app.UseDefaultFiles();
            //app.UseStaticFiles();
            //app.UseFileServer();
            #endregion
            app.UseSession();
            app.UseAuthentication();

            // 默认路由格式
            app.UseMvcWithDefaultRoute();
            //app.UseMvc(builder =>
            //{
            //    builder.MapRoute("Default", "{controller=home}/{action=index}/{id?}");
            //});



            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/", async context =>
                {
                    //throw new Exception("自定义错误");
                    await context.Response.WriteAsync(Configuration["message"]);
                });
            });


            app.Run(async context =>
            {
                await context.Response.WriteAsync("默认 Response");
            });

            InitialDb(app, roleManager).Wait();



        }
        public async Task InitialDb(IApplicationBuilder app, RoleManager<IdentityRole> roleManager)
        {

            using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var db = serviceScope.ServiceProvider.GetService<FlowerDbContext>().Database;
                if (db.GetPendingMigrations().Any())
                {
                    db.Migrate();
                }
            }

            try
            {
                await roleManager.CreateAsync(new IdentityRole(Roles.SuperAdmin));
            }
            catch { }
            try
            {
                await roleManager.CreateAsync(new IdentityRole(Roles.Admin));
            }
            catch { }
            try
            {
                await roleManager.CreateAsync(new IdentityRole(Roles.User));
            }
            catch { }
        }
    }
}

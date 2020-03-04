using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HostFiltering;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Peshitta.Infrastructure.Sqlite;
using SQLitePCL;
using System;
using System.IO;

namespace BibleAPI
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<DbSqlContext>(options => {
                var dbString = Configuration.GetConnectionString("DB");
                ////var dbConnectionStringBuilder = new System.Data.Common.DbConnectionStringBuilder
                ////{
                ////    ConnectionString = dbString
                ////};
                ////if (dbConnectionStringBuilder.ContainsKey("filename"))
                ////{
                ////    dbString = (string)dbConnectionStringBuilder["filename"];
                ////}
                ////if (dbString.StartsWith("./") || dbString.StartsWith(".\\"))
                ////{
                ////    dbString = Path.Combine(Directory.GetCurrentDirectory(), dbString.Substring(2));
                ////}
                ////if (!File.Exists(dbString))
                ////{
                ////    throw new FileNotFoundException(dbString);
                ////}
                ////dbConnectionStringBuilder["filename"] = dbString;
                //Batteries.Init();
                options.UseSqlite(dbString); });
            services.AddControllers();
            //services
            //    .AddHostFiltering(options => { })

            //    .PostConfigure<HostFilteringOptions>(options =>
            //    {
            //        if (options.AllowedHosts == null || options.AllowedHosts.Count == 0)
            //        {
            //            // "AllowedHosts": "localhost;127.0.0.1;[::1]"
            //            var hosts = Configuration["AllowedHosts"]?.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            //            // Fall back to "*" to disable.

            //            options.AllowedHosts = (hosts?.Length > 0 ? hosts : new[] { "*" });
            //        }

            //    })
            services.AddLogging()
            .AddOptions();
            //.AddTransient<INHGRepository, NHGRepository>()
            //.AddTransient<INHGService, NHGService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }
            //app.UseDefaultFiles("/index.html");
            app.UseStaticFiles();
            app.UseRouting(); 

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/", async context =>
                {
                    if (env.IsDevelopment())
                    {
                        await context.Response.WriteAsync("Peshitta.Api works");
                    }
                    else
                    {
                        context.Response.Redirect("/peshitta/index.html");
                    }
                });
                endpoints.MapControllers();
            });
        }
    }
}

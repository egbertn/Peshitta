using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using peshitta.nl.Api.Helpers;
using peshitta.nl.Api.Models;
using Peshitta.Infrastructure;
using Peshitta.Infrastructure.Sqlite;
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace BibleAPI
{
  public class Startup
  {
    private const string CORS = "mypolicy";
    public Startup(IConfiguration configuration)
    {
      Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
      services.AddCors(options =>
      {
        options.AddPolicy(CORS,
        builder =>
        {
          builder.WithOrigins("http://localhost:8999",
                              "https://www.peshitta.nl").SetIsOriginAllowed(_ => true).WithHeaders("Content-Type", "accept").WithMethods("GET", "POST", "OPTIONS").SetPreflightMaxAge(TimeSpan.FromSeconds(3600)); ;
        });
      });
      // Add framework services.
      var mp = Configuration.GetValue<string>("MediaPath");
      var options = new Options { MediaPath = mp };
      services.AddSingleton(options);
      services.AddScoped<BijbelRepository>()
      .AddDbContext<DbSqlContext>(options =>
      {
        var dbString = Configuration.GetConnectionString("DB");
        options.UseSqlite(dbString);
      });


      services.AddSingleton<PathHelper>();
      services.AddControllers();
      services.AddLogging()
          .AddOptions();

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


      app.UseStaticFiles();

      app.UseRouting().UseCors(CORS);

      app.UseAuthorization();
      app.UseEndpoints(endpoints =>
      {
        endpoints.MapGet("/", context =>
              {
                if (env.IsDevelopment())
                {
                  context.Response.WriteAsync("Peshitta.Api works");
                }
                else
                {
                  context.Response.Redirect("/index.html");
                }
                return Task.CompletedTask;
              });
        endpoints.MapControllers();
      });
    }
  }
}

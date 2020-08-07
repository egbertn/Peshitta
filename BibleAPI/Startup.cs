using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Peshitta.Infrastructure;
using Peshitta.Infrastructure.Sqlite;
using System;
using System.IO;
using System.Reflection;

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
          builder.WithOrigins("http://localhost:9000",
                              "https://www.peshitta.nl").SetIsOriginAllowed(_ => true).WithHeaders("Content-Type", "accept").WithMethods("GET", "POST", "OPTIONS").SetPreflightMaxAge(TimeSpan.FromSeconds(3600)); ;
        });
      });
      // Add framework services.
      services.AddTransient<BijbelRepository>()
      .AddDbContext<DbSqlContext>(options =>
      {
        var dbString = Configuration.GetConnectionString("DB");
        options.UseSqlite(dbString);
      });

      services.AddSwaggerGen(c =>
      {
        c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
        {
          Version = "v1",
          Title = "Bijbel API"
        });
        // Set the comments path for the Swagger JSON and UI.
        var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        c.IncludeXmlComments(xmlPath);
      });

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
      //app.UseDefaultFiles("/index.html");
      app.UseSwagger();
      app.UseSwaggerUI(c =>
      {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Bijbel API V1");

      });

      app.UseStaticFiles();

      app.UseRouting().UseCors(CORS);

      app.UseAuthorization();
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
              context.Response.Redirect("/index.html");
            }
          });
        endpoints.MapControllers();
      });
    }
  }
}

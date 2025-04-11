using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using peshitta.nl.Api.Helpers;
using peshitta.nl.Api.Models;
using Peshitta.Infrastructure;
using Peshitta.Infrastructure.Sqlite;

namespace BibleAPI
{
  public class Program
  {
    private const string CORS = "mypolicy";
    public static void Main(string[] args)
    {
      var builder = WebApplication.CreateBuilder(args);
      var services = builder.Services;
      var configuration = builder.Configuration;
      var env = builder.Environment;
      services.AddCors(options =>
      {
        options.AddPolicy(CORS,
        builder =>
        {
          builder.WithOrigins("http://localhost:8999",
                              "https://peshitta.nl").SetIsOriginAllowed(_ => true).WithHeaders("Content-Type", "accept").WithMethods("GET", "POST", "OPTIONS").SetPreflightMaxAge(TimeSpan.FromSeconds(3600)); ;
        });
      });
      services.AddLogging(options => { options.AddSimpleConsole(opt => opt.SingleLine = true); });
      // Add framework services.
      var mp = configuration.GetValue<string>("MediaPath");
      var options = new Options { MediaPath = mp };
      services.AddSingleton(options);
      services.AddResponseCompression();
      services.AddScoped<BijbelRepository>()
      .AddDbContext<DbSqlContext>(options =>
      {
        var dbString = configuration.GetConnectionString("DB");
        options.UseSqlite(dbString);
      });

      services.AddSingleton<PathHelper>();
      services.AddControllers();
      services.AddLogging().AddOptions();

      var app = builder.Build();

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
      app.UseResponseCompression();
      app.MapControllers();
      app.MapFallbackToFile("index.html");
      app.Run();
    }

  }
}

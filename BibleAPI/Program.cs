using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace BibleAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }
        //todo environment ASPNETCORE_FORWARDEDHEADERS_ENABLED
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}

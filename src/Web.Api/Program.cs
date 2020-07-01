using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

namespace Web.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Task.Run(async () =>
            {
              var host = CreateWebHostBuilder(args).Build();
              host.Run();
            }).GetAwaiter().GetResult();
        }            

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .UseSerilog()
                .UseUrls("http://0.0.0.0:5100");
    }
}

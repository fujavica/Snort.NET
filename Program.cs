using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using snortdb;
using System.Collections;

namespace razor
{
    public static class ProgramData
    {
        public static string snort_conf = Startup.Configuration.GetSection("Sources:snort_conf").Value;
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            string path = AppContext.BaseDirectory;
            string path2 = Directory.GetCurrentDirectory();
            var configuration = new ConfigurationBuilder()
            .AddCommandLine(args)
            .Build();
            CreateWebHostBuilder(args).UseConfiguration(configuration).Build().Run();
            ;
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory()); //AppContext.BaseDirectory
             //   .UseUrls("http://*:5000");

    }
}

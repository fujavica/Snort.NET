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
        public static string rootpath;
        public static void Main(string[] args)
        {
            rootpath = AppContext.BaseDirectory;
            #if DEBUG
                    rootpath = Directory.GetCurrentDirectory();
            #endif

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
                .UseContentRoot(rootpath) //AppContext.BaseDirectory
                .UseUrls("http://*:5000");

    }
}

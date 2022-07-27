using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JackHenryTwitterFeed
{

    public static class TwitterMemory
    {
        public static JackHenryTwitterService.TwitterInboundService _TwitterInboundService = null; 
    }
    public class Program
    {
        public static void Main(string[] args)
        {
            // This starts the inbound service on one background thread
            TwitterMemory._TwitterInboundService = new JackHenryTwitterService.TwitterInboundService();
            // Then we continue to start the web site
            CreateHostBuilder(args).Build().Run();
        }


        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}

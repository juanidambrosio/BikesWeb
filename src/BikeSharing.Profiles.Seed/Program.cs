using BikeSharing.Profiles.Seed.Data;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BikeSharing.Profiles.Seed
{
    public class Program
    {
        public static IConfiguration Configuration { get; private set; }

        public static void Main(string[] args)
        {
            var builder = new ConfigurationBuilder();
            builder.AddJsonFile("appsettings.json")
            .AddJsonFile("appsettings.{env.EnvironmentName}.json", optional: true)
            .AddEnvironmentVariables()
            .AddCommandLine(args);
            Configuration = builder.Build();

            Console.WriteLine("+++ Begin Seed");

            var context = new ProfilesDbContext();
            context.Seed();
        }
    }
}

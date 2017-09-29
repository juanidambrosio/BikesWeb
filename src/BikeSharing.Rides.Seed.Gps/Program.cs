using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace BikeSharing.Rides.Seed.Gps
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CultureInfo.CurrentCulture = new CultureInfo("en-US");
            CultureInfo.CurrentUICulture = new CultureInfo("en-US");
            var builder = new ConfigurationBuilder();
            builder.AddJsonFile("config.json");
            builder.AddEnvironmentVariables();
            builder.AddCommandLine(args);
            var config = builder.Build();
            var constr = config["ConnectionStrings:DefaultConnection"];
            Console.WriteLine(constr);

            if (string.IsNullOrEmpty(config["output"]))
            {
                Console.WriteLine("Must use --output with the path where to generate the files");
                return;
            }

            string dataPath = config["output"];
            string apiKey = config["apikey"];

            var gpsApiClient = new GpsApiClient(dataPath, apiKey);

            var count = int.Parse(config["routescount"]);
            var fileGenerator = new BingMapsResponseFileGenerator(constr, gpsApiClient);
            fileGenerator.RoutesCount = count;

            var generatedFileCount = fileGenerator.GenerateAsync().Result;

            Console.WriteLine($"+++ {generatedFileCount} files generated");
        }
    }
}

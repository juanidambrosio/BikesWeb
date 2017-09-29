using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using TinyCsvParser;
using BikeSharing.Rides.Seed.Parsers;

using System.Text;
using TinyCsvParser.Tokenizer.RegularExpressions;
using TinyCsvParser.Mapping;
using System.Globalization;
using BikeSharing.Rides.Seed.Data.Entities;

namespace BikeSharing.Rides.Seed
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

            if (string.IsNullOrEmpty(config["input"]))
            {
                Console.WriteLine("Must use --input with input csv file");
                return;
            }
            
            var dbImporter = new RidesImporter(constr, config.GetSection("dates"));
            dbImporter.LoadStations();
            dbImporter.LoadBikes();
            var rides = ParseFile(config["input"]);
            dbImporter.ProcessData(rides);
        }

        private static IEnumerable<CsvMappingResult<Ride>> ParseFile(string file)
        {
            Console.WriteLine("+++ Starting reading from file {0}", file);
            var options = new CsvParserOptions(true, new QuotedStringTokenizer(','));
            var parser = new CsvParser<Ride>(options, new RideMapping());
            var result = parser.ReadFromFile(file, Encoding.UTF8);
            var rides = result.ToList();
            Console.WriteLine("+++ File parsed {0} entries found", rides.Count);
            return rides;
        }
    }
}

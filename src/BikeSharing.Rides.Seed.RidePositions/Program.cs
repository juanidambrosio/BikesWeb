using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Data.SqlClient;
using BikeSharing.Rides.Seed.Data.Entities;
using BikeSharing.Rides.Seed.Data;
using CsvHelper;

namespace BikeSharing.Rides.Seed.RidePositions
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
                Console.WriteLine("Must use --input with path to folder with the route files");
                return;
            }

            var inputDirectory = new DirectoryInfo(config["input"]);
            if (!inputDirectory.Exists)
            {
                Console.WriteLine("The input path does not exists");
                return;
            }
            var miniDirectory = new DirectoryInfo(config["mini"]);
            if (!miniDirectory.Exists)
            {
                miniDirectory.Create();
            }

            var files = inputDirectory.GetFiles("*.route");
            CreateMiniFiles(config["mini"], files);
            /*
            var rideDataService = new RideDataService(constr);
            var stations = file.Name.Split('-');
            var startStation = int.Parse(stations[0]);
            var endStation = int.Parse(stations[1]);
            var rides = rideDataService.GetRides(startStation, endStation);

            if (!rides.Any())
            {
                Console.WriteLine($"No ride found for stations {startStation}-{endStation}");
                continue;
            }
            */
        }

        private static void CreateMiniFiles(string miniPath, IEnumerable<FileInfo> files)
        {
            foreach (var file in files)
            {
                string json = File.ReadAllText(file.FullName);
                var response = JsonConvert.DeserializeObject<BingResponse>(json);
                if (response == null)
                {
                    Console.WriteLine($"Invalid route file {file.FullName}");
                    return;
                }

                var rset = response.ResourceSets.First();
                for (var setidx = 0; setidx < rset.Resources.Count; setidx++)
                {
                    var res = rset.Resources[setidx];
                    var positions = GetPointsOfResource(res);
                    CreateMiniFile(miniPath, file.Name, setidx, positions);

                }
            }
        }

        private static void CreateMiniFile(string miniPath, string name, int setidx, IEnumerable<RidePosition> positions)
        {
            var separator = Path.DirectorySeparatorChar;
            var fname = $"{miniPath}{separator}{setidx}_{name}.mini";
            Console.WriteLine($"Creating MiniFile {fname}");
            if (File.Exists(fname))
            {
                Console.WriteLine($"MiniFile already exists {fname}");
                return;
            }

            using (var fs = new FileStream(fname, FileMode.Create, FileAccess.Write))
            using (var wr = new StreamWriter(fs))
            {
                var writer = new CsvWriter(wr);
                foreach (var pos in positions)
                {
                    writer.WriteField(pos.Latitude);
                    writer.WriteField(pos.Longitude);
                    writer.NextRecord();
                }
            }

            Console.WriteLine($"MiniFile {fname} written");

        }

        private static IEnumerable<RidePosition> GetPointsOfResource(Resource r)
        {

            var positions = new List<RidePosition>();

            for (int i = 0; i < r.RoutePath.Line.Coordinates.Count; i++)
            {
                var coordinate = r.RoutePath.Line.Coordinates[i];
                var ridePosition = new RidePosition
                {
                    RideId = 0,
                    Latitude = double.Parse(coordinate[0], CultureInfo.InvariantCulture),
                    Longitude = double.Parse(coordinate[1], CultureInfo.InvariantCulture),
                    TS = DateTime.MinValue
                };

                positions.Add(ridePosition);
            }
            return positions;
        }
    }

}

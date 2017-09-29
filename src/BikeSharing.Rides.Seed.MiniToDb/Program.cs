using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BikeSharing.Rides.Seed.Data.Entities;
using Microsoft.Extensions.Configuration;
using BikeSharing.Rides.Seed.Data;

namespace BikeSharing.Rides.Seed.MiniToDb
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
            var inputDirectory = new DirectoryInfo(config["mini"]);
            if (!inputDirectory.Exists)
            {
                Console.WriteLine("The Mini path does not exists");
                return;
            }


            var constr = config["ConnectionStrings:DefaultConnection"];
            Console.WriteLine($"Using const {constr}");
            var db = new RideDataService(constr);

            Console.WriteLine("Reading mini files");
            var miniParser = new MiniParser(config["mini"]);
            miniParser.ReadAllFiles();

            var miniGroups = miniParser.GetGroups();


            Console.WriteLine($"Mini files read. Found {miniGroups.Count()} groups");
            foreach (var miniGroup in  miniGroups)
            {
                var rides = db.GetRidesWithNoPoints(miniGroup.From, miniGroup.To);
                Console.WriteLine($"Found {rides.Count()} rides.");
                foreach (var ride in rides)
                {
                    var points = miniParser.GetPositions(ride.StartStationId, ride.EndStationId);
                    if (points == null || !points.Any())
                    {
                        Console.WriteLine($"No points found for {ride.StartStationId}-{ride.EndStationId}");
                        continue;
                    }
                    Console.WriteLine($"Processing points for ride {ride.Id}");
                    ProcessPointsForRide(ride, points, db);
                    Console.WriteLine($"Processed points for ride {ride.Id}");
                }

            }


        }

        private static void ProcessPointsForRide(Ride ride, IEnumerable<RidePosition> points, RideDataService db)
        {
            var numPoints = points.Count();
            var duration = ride.Duration;
            var timeBetweenPoints = (double)duration / (double)numPoints;

            var start = ride.StartTime;
            var current = start;
            var pointsToInsert = new List<RidePosition>();

            foreach (var point in points)
            {
                pointsToInsert.Add(new RidePosition()
                {
                    RideId = ride.Id,
                    Latitude = point.Latitude,
                    Longitude = point.Longitude,
                    TS = current
                });

                current = current.AddSeconds(timeBetweenPoints);
            }

            db.AddRidePositions(pointsToInsert);
        }

    }
}

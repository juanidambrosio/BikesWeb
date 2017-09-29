using BikeSharing.Rides.Seed.Data;
using BikeSharing.Rides.Seed.Data.Entities;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace BikeSharing.Rides.Seed.Gps
{
    public class BingMapsResponseFileGenerator
    {

        private readonly string _constr;
        private readonly GpsApiClient _gpsApiClient;
        private readonly RouteDataService _routeDataService;

        public BingMapsResponseFileGenerator(string constr, GpsApiClient gpsApiClient)
        {
            _constr = constr;
            _gpsApiClient = gpsApiClient;
            _routeDataService = new RouteDataService(constr);
            RoutesCount = -1;
        }

        public int RoutesCount { get; set; }

        public async Task<int> GenerateAsync()
        {
            var routes = _routeDataService.GetAll();

            if (RoutesCount > 0)
            {
                routes = routes.Take(RoutesCount);
            }
            var routesList = routes.ToList();
            Console.WriteLine($"+++ Generating {routesList.Count} routes");
            int routesAdded = 0;

            foreach (var route in routesList)
            {

                await _gpsApiClient.AddRouteFile(
                   id1: route.StartStationId,
                   id2: route.EndStationId,
                   fromLatitude: (double)route.StartLatitude,
                   fromLongitude: (double)route.StartLongitude,
                   toLatitude: (double)route.EndLatitude,
                   toLongitude: (double)route.EndLongitude);
                routesAdded++;
            }

            Console.WriteLine($"+++ Generated {routesAdded} routes.");
            return routesAdded;
        }
    }
}

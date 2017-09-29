using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BikeSharing.Rides.Seed.Data.Entities
{
    public class Route
    {
        public int StartStationId { get; set; }
        public int EndStationId { get; set; }

        public decimal StartLatitude { get; set; }
        public decimal StartLongitude { get; set; }
        public decimal EndLatitude { get; set; }
        public decimal EndLongitude { get; set; }


        public static string GetSelectAllSql()
        {
            return @"
                    SELECT
                     routes.StartStationId, 
                     routes.EndStationId,
                     startStations.Latitude as StartLatitude,
                     startStations.Longitude as StartLongitude,
                     endStations.Latitude as EndLatitude,
                     endStations.Longitude as EndLongitude
                     FROM (
	                    SELECT StartStationId, EndStationId from rides
	                    group by StartStationId, EndStationId
                     ) routes
                    INNER JOIN stations startStations ON startStations.Id = routes.StartStationId
                    INNER JOIN stations endStations ON endStations.Id = routes.EndStationId
                    ";
        }

    }
}

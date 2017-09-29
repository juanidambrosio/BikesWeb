using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace BikeSharing.Rides.Seed.Data.Entities
{
    public class Ride
    {
        public int Id { get; set; }
        public int Duration { get; set; }

        public DateTime StartTime { get; set; }
        public DateTime StopTime { get; set; }

        public int StartStationId { get; set; }
        public string StartStationName { get; set; }

        public double StartStationLatitude { get; set; }
        public double StartStationLongitude { get; set; }

        public int EndStationId { get; set; }
        public string EndStationName { get; set; }
        public double EndStationLatitude { get; set; }
        public double EndStationLongitude { get; set; }

        public int BikeId { get; set; }
        public string UserType { get; set; }
        public int? BirthYear { get; set; }
        public int Gender { get; set; }

        public int? EventType { get; set; }

        public string GetInsertSql()
        {
            return @"insert into rides 
                        (Duration, Start, Stop, StartStationId, EndStationId, BikeId, UserId, EventType, EventId) 
                        values
                        (@duration, @start, @stop, @startStationId, @endStationId, @bikeId, @userId, NULL, NULL)";
        }

        public IEnumerable<SqlParameter> GetParameters(int forceMonth = -1)
        {
            yield return new SqlParameter("@duration", Duration);
            yield return new SqlParameter("@start", forceMonth > 0 ? ForceMonth(StartTime, forceMonth) : StartTime);
            yield return new SqlParameter("@stop", forceMonth > 0 ? ForceMonth(StopTime, forceMonth) : StopTime);
            yield return new SqlParameter("@startStationId", StartStationId);
            yield return new SqlParameter("@endStationId", EndStationId);
            yield return new SqlParameter("@bikeId", BikeId);
        }

        private static DateTime ForceMonth(DateTime dt, int monthToForce)
        {
            return new DateTime(dt.Year, monthToForce, dt.Day,
                 dt.Hour, dt.Minute, dt.Second);
        }



    }

}

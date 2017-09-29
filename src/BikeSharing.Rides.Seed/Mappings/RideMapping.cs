using BikeSharing.Rides.Seed.Data.Entities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TinyCsvParser.Mapping;
using TinyCsvParser.TypeConverter;

namespace BikeSharing.Rides.Seed.Parsers
{
    public class RideMapping : CsvMapping<Ride>
    {
        public RideMapping()
        {
            MapProperty(0, x => x.Duration);

            MapProperty(1, x => x.StartTime);
            MapProperty(2, x => x.StopTime);

            MapProperty(3, x => x.StartStationId);
            MapProperty(4, x => x.StartStationName);

            MapProperty(5, x => x.StartStationLatitude);
            MapProperty(6, x => x.StartStationLongitude);

            MapProperty(7, x => x.EndStationId);
            MapProperty(8, x => x.EndStationName);
            MapProperty(9, x => x.EndStationLatitude);
            MapProperty(10, x => x.EndStationLongitude);

            MapProperty(11, x => x.BikeId);
            MapProperty(12, x => x.UserType);
            MapProperty(13, x => x.BirthYear)
                .WithCustomConverter(new YearTypeConverter());
            MapProperty(14, x => x.Gender);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BikeSharing.Rides.Seed.Data.Entities
{
    public class RidePosition
    {
        public int RideId { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public DateTime TS { get; set; }
    }
}

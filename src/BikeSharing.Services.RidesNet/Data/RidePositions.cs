using System;
using System.Collections.Generic;

namespace BikeSharing.Services.RidesNet.Data
{
    public partial class RidePositions
    {
        public long Id { get; set; }
        public int RideId { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public DateTime Ts { get; set; }

        public virtual Rides Ride { get; set; }
    }
}

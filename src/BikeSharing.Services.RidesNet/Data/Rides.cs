using System;
using System.Collections.Generic;

namespace BikeSharing.Services.RidesNet.Data
{
    public partial class Rides
    {
        public Rides()
        {
            RidePositions = new HashSet<RidePositions>();
        }

        public int Id { get; set; }
        public int? Duration { get; set; }
        public DateTime? Start { get; set; }
        public DateTime? Stop { get; set; }
        public int StartStationId { get; set; }
        public int? EndStationId { get; set; }
        public int BikeId { get; set; }
        public int UserId { get; set; }
        public int? EventType { get; set; }
        public int? EventId { get; set; }
        public int? GeoDistance { get; set; }
        public string EventName { get; set; }

        public virtual ICollection<RidePositions> RidePositions { get; set; }
        public virtual Stations EndStation { get; set; }
        public virtual Stations StartStation { get; set; }
    }
}

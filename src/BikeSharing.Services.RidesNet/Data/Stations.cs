using System;
using System.Collections.Generic;

namespace BikeSharing.Services.RidesNet.Data
{
    public partial class Stations
    {
        public Stations()
        {
            RidesEndStation = new HashSet<Rides>();
            RidesStartStation = new HashSet<Rides>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public decimal Slots { get; set; }

        public virtual ICollection<Rides> RidesEndStation { get; set; }
        public virtual ICollection<Rides> RidesStartStation { get; set; }
    }
}

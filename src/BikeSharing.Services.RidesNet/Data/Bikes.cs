using System;
using System.Collections.Generic;

namespace BikeSharing.Services.RidesNet.Data
{
    public partial class Bikes
    {
        public int Id { get; set; }
        public string SerialNumber { get; set; }
        public DateTime InCirculationSince { get; set; }
    }
}

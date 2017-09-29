using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BikeSharing.Rides.Seed.RidePositions
{
    public class BingResponse
    {
        public List<ResourceSet> ResourceSets { get; set; }
    }

    public class ResourceSet
    {
        public List<Resource> Resources { get; set; }
    }

    public class Resource
    {
        public List<string> BBox { get; set; }

        public RoutePath RoutePath { get; set; }
    }

    public class RoutePath {
        public Line Line { get; set; }
    }

    public class Line
    {
        public List<List<string>> Coordinates { get; set; }
    }

    //public class Location
    //{

    //}
}

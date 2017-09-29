using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace BikeSharing.Rides.Seed.Gps
{
    public class GpsApiClient
    {

        private readonly string _path;
        private readonly string _apiKey;


        public GpsApiClient(string dataPath, string apiKey)
        {
            _apiKey = apiKey;
            _path = dataPath;
        } 

        public async Task AddRouteFile(
            int id1,
            int id2,
            double fromLatitude, 
            double fromLongitude, 
            double toLatitude, 
            double toLongitude)
        {
           

            var separator = Path.DirectorySeparatorChar;

            var name = $"{_path}{separator}{id1}-{id2}.route";

            if (File.Exists(name))
            {
                Console.WriteLine($"Avoiding {name} as it already exists");
                return;
            }


            using (var fs = new FileStream(name, FileMode.Create, FileAccess.ReadWrite))
            using (var sw = new StreamWriter(fs))
            {
                var route = await GetBingMapsRoute(fromLatitude, fromLongitude, toLatitude, toLongitude);
                await sw.WriteAsync(route);
                Console.WriteLine($"File {name} generated.");
            }

        }

        private async Task<string> GetBingMapsRoute(double fromLatitude, double fromLongitude, double toLatitude, double toLongitude)
        {
            var client = new HttpClient() {
                BaseAddress = new Uri("http://dev.virtualearth.net/REST/v1/")
            };
            var slat1 = fromLatitude.ToString(CultureInfo.InvariantCulture);
            var slon1 = fromLongitude.ToString(CultureInfo.InvariantCulture);
            var slat2 = toLatitude.ToString(CultureInfo.InvariantCulture);
            var slon2 = toLongitude.ToString(CultureInfo.InvariantCulture);
            var url = $"Routes/Driving?maxSolns=3&routeAttributes=routePath&wp.1={slat1},{slon1}&wp.2={slat2},{slon2}&key={_apiKey}";

            var response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return json;
            }

            return null;

        }
    }
}

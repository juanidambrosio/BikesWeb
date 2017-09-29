using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http.Headers;

namespace BikeSharing.Client.Kiosk.UWP.BikeSharingServices
{
    public class RideServices
    {
        private string _baseUrl;

        public RideServices(string baseUrl)
        {
            this._baseUrl = baseUrl;
        }

        public async Task<CheckoutConfirmation> CheckoutBikes(int bikeStationId, int userId, uint numberBikes)
        {
            try
            {
                string url = $"{_baseUrl}/api/stations/{bikeStationId}/checkout";
                var client = new HttpClient();
                client.Timeout = TimeSpan.FromSeconds(3);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var request = new HttpRequestMessage(HttpMethod.Put, url);
                request.Content = new StringContent($"{{\"userId\": {userId}, \"endStationId\": {bikeStationId}}}", Encoding.UTF8, "application/json");
                var result = await client.SendAsync(request);
                if (result.IsSuccessStatusCode)
                {
                    var text = await result.Content.ReadAsStringAsync();
                    var bikeId = JsonConvert.DeserializeObject<int>(text);
                    return new CheckoutConfirmation(ResultType.Succeeded, "OK", DateTime.Now, bikeStationId, userId, new List<int> { bikeId }, 7.62m);
                }
                else
                {
                    Debug.WriteLine($"[Backend]: network issue: {result.ReasonPhrase}, {await result.Content.ReadAsStringAsync()}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Backend]: an error occurred: {ex.ToString()}");
            }

            //TODO (yumeng): remove test data before publishing code.
            return new CheckoutConfirmation(
                ResultType.Succeeded,
                "OK",
                DateTime.Now,
                bikeStationId,
                userId,
                new List<int> { 2468 },
                7.62m);

            return null;
        }
    }
}

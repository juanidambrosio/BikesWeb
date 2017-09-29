using BikeSharing.Clients.CogServicesKiosk.Models;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace BikeSharing.Clients.CogServicesKiosk.Data
{
    public class UserLookupService
    {
        private string PROFILES_SERVICE_BASE_URI = "http://bikesharing360-profiles-dev.azurewebsites.net";

        private static UserLookupService _instance;
        public static UserLookupService Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new UserLookupService();
                return _instance;
            }
        }

        public async Task<UserProfile> GetUserByFaceID(Guid id)
        {
            try
            {
                string url = $"{PROFILES_SERVICE_BASE_URI}/api/Profiles/byfaceid";
                var client = new HttpClient();
                var request = new HttpRequestMessage(HttpMethod.Post, url);
                request.Content = new TextPlainContent(id.ToString(), Encoding.UTF8);
                var result = await client.SendAsync(request);
                if (result.IsSuccessStatusCode)
                {
                    var text = await result.Content.ReadAsStringAsync();
                    var profile = JsonConvert.DeserializeObject<UserProfile>(text);
                    return profile;
                }
                else
                    return null;
            }
            catch(Exception ex)
            {
                Debug.WriteLine("Error lookup up customer database: " + ex.ToString());
                return null;
            }
        }

        /// StringContnet has a Content-Type of "text/plain;charset=utf-8", which 
        /// our custom input formatter won't handle.
        private class TextPlainContent : StringContent
        {
            public TextPlainContent(string content, Encoding encoding)
                : base(content, encoding)
            {
                Headers.ContentType = new MediaTypeHeaderValue("text/plain");
            }
        }
    }
}

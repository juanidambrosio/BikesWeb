using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System;
using System.Diagnostics;

namespace BikeSharing.Client.Kiosk.UWP.BikeSharingServices
{
    public class UserProfileServices
    {
        private string _baseUrl;

        public UserProfileServices(string baseUrl)
        {
            this._baseUrl = baseUrl;
        }

        public async Task<UserProfile> GetUserProfileByFaceProfileIdAsync(string faceProfileId)
        {
            try
            {
                string url = $"{_baseUrl}/api/Profiles/byfaceid";
                var client = new HttpClient();
                client.Timeout = TimeSpan.FromSeconds(3);
                var request = new HttpRequestMessage(HttpMethod.Post, url);
                request.Content = new TextPlainContent(faceProfileId, Encoding.UTF8);
                var result = await client.SendAsync(request);
                if (result.IsSuccessStatusCode)
                {
                    var text = await result.Content.ReadAsStringAsync();
                    var profile = JsonConvert.DeserializeObject<UserProfile>(text);
                    return profile;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Backend]: an error occurred: {ex.ToString()}");
            }

            //TODO (yumeng): demo-only, remove before publishing
            return new UserProfile
            {
                UserId = 24,
                FirstName = "Lara",
                LastName = "Rubbelke",
                VoiceProfileId = Guid.Empty,
                VoiceSecretPhrase = "Be yourself everyone else is already taken",
            };

            return null;
        }

        // StringContnet has a Content-Type of "text/plain;charset=utf-8", which 
        // our custom input formatter won't handle.
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

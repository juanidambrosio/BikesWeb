using System;

namespace BikeSharing.Clients.Core
{
    public static class GlobalSettings
    {
        /* CONFIGURATION FOR PUBLIC ENDPOINTS */
        public const string AuthenticationEndpoint = "http://bikesharing-services-profilespublic.azurewebsites.net/";
        public const string EventsEndpoint = "http://bikesharing-services-eventspublic.azurewebsites.net/";
        public const string IssuesEndpoint = "http://bikesharing-services-feedbackpublic.azurewebsites.net/";
        public const string RidesEndpoint = "http://bikesharing-services-ridespublic.azurewebsites.net/";

        public const string HockeyAppAPIKeyForAndroid = "f316ccb2a99d4700bc7165ad85d0a5e3";
        public const string HockeyAppAPIKeyForiOS = "25777b4e815a42b5a2b6d778f30210c0";

        public const string OpenWeatherMapAPIKey = "2073f7f815da70af1fc44ff80cd55034";

        public const string SkypeBotAccount = "skype:28:5a71685d-c3d7-4169-9d8f-b7aa8b9b4d9d?chat";

        public const string BingMapsAPIKey = "40mgB3YEUPMAewdi02Hm~HM0JyrqUSuQqAz9WoBg0Iw~Ar-hXz2S6oaN-ZJRM3VzCSS1lpPhUlNmParkTwh6zuaG7AcSb9j2N0Oe2wRpr5qz";

        public static string City => "New York";

        public static int TenantId = 1;

        public static float EventLatitude = 40.730610f;
        public static float EventLongitude = -73.935242f;
    }
}

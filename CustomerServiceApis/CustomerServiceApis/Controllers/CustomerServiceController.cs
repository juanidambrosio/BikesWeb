using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Data.SqlClient;
using CustomerServiceApis.Models;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using System.Threading.Tasks;
using CustomerServiceApis.Models;
using System.Xml;

namespace CustomerServiceApis.Controllers
{
    public class CustomerServiceController : ApiController
    {
        private const string _bingmapappid = "AnFdkPU2iJh-hws2VacLRfbVHv-Axxv5xbshHY7vcymXVddNfKlBAhbvRvlhERIJ";
        private const string _bingmapstaticmapwithrouteurl = "http://dev.virtualearth.net/REST/v1/Imagery/Map/Road/{4}%2C{5}/15/Routes/Walking?waypoint.1={2},{3};64;{6}&waypoint.2={0},{1};64;{7}&mapSize=400,300&format=png&key=" + _bingmapappid;
        private const string _bingmapstaticmapwith1pointsurl = "http://dev.virtualearth.net/REST/V1/Imagery/Map/Road/{0}%2C{1}/13?mapSize=400,300&format=png&pushpin={0},{1};64;{2}&key=" + _bingmapappid;
        private const string _bingmapbaseurl = "http://dev.virtualearth.net";
        private const string _bingmapgetroutetime = "REST/v1/Routes?wayPoint.1={0},{1}&waypoint.2={2},{3}&key=" + _bingmapappid;
        private const string _bingmappointtoaddrurl = "http://dev.virtualearth.net/REST/v1/Locations/{0},{1}?o=xml&key=" + _bingmapappid;

        private static string connstr = "Server=tcp:bikesshare360.database.windows.net,1433;Initial Catalog=bikesharing-services-profiles;Persist Security Info=False;User ID=MyBikes;Password=BikesShare360@1;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
        [Route("getavailableemployee")]
        [HttpGet]
        public Employee GetAvailableEmployee()
        {
            string queryString = "SELECT * FROM EMPLOYEES ORDER BY ID DESC";
            using (SqlConnection connection = new SqlConnection(connstr))
            {
                SqlCommand command = new SqlCommand(queryString, connection);
                connection.Open();
                SqlDataReader reader = command.ExecuteReader();
                try
                {
                    if (reader.Read())
                    {
                        Employee employee = new Employee();
                        employee.id = reader["Skype"].ToString();
                        employee.name = reader["Name"].ToString();
                        employee.serviceUrl = reader["ServiceUrl"].ToString();
                        employee.conversationId = reader["ConversationId"].ToString();
                        return employee;
                    }
                    else
                    {
                        Employee employee = new Employee();
                        //employee.id = "29:1QwL9e77CWAGkB03TNiy9S-uOPIRUtIK6Lsv5aUO4pJM";
                        employee.id = "8:orgid:eca6d4d4-89be-4b73-8699-6e70c9cbe6bf";
                        employee.conversationId = "19:eca6d4d4-89be-4b73-8699-6e70c9cbe6bf_5a71685d-c3d7-4169-9d8f-b7aa8b9b4d9d@unq.gbl.spaces";
                        employee.name = "Skype Team";
                        employee.serviceUrl = @"https://skype.botframework.com";
                        return employee;
                    }
                }
                finally
                {
                    // Always call Close when done reading.
                    reader.Close();
                }
            }
        }

        [Route("getaddress")]
        [HttpGet]
        public string GetAddress(double latitude, double longitude)
        {
            string requesturl = string.Format(_bingmappointtoaddrurl, latitude.ToString(), longitude.ToString());
            XmlDocument response = GetXmlResponse(requesturl);
            if (response == null)
            {
                return "";
            }
            var addresses = response.GetElementsByTagName("FormattedAddress");
            string ret = "";
            if (addresses.Count > 0)
            {
                ret = addresses[0].InnerText.ToString();
            }
            return ret;
        }

        [Route("getinformation")]
        [HttpGet]
        public Employee GetInformation(string id)
        {
            string queryString = "SELECT * FROM EMPLOYEES WHERE Skype='" + id + "'";
            using (SqlConnection connection = new SqlConnection(connstr))
            {
                SqlCommand command = new SqlCommand(queryString, connection);
                connection.Open();
                SqlDataReader reader = command.ExecuteReader();
                try
                {
                    if (reader.Read())
                    {
                        Employee employee = new Employee();
                        employee.id = reader["Skype"].ToString();
                        employee.name = reader["Name"].ToString();
                        employee.serviceUrl = reader["ServiceUrl"].ToString();
                        employee.conversationId = reader["ConversationId"].ToString();
                        employee.location = new GeoLocation();
                        employee.location.latitude = double.Parse(reader["Latitude"].ToString());
                        employee.location.longitude = double.Parse(reader["Longitude"].ToString());
                        employee.customer = new BotUser();
                        employee.customer.id = reader["CustomerSkype"].ToString();
                        employee.customer.name = reader["CustomerName"].ToString();
                        employee.customer.serviceUrl = reader["CustomerServiceUrl"].ToString();
                        employee.customer.location = new GeoLocation();
                        employee.customer.location.latitude = double.Parse(reader["CustomerLatitude"].ToString());
                        employee.customer.location.longitude = double.Parse(reader["CustomerLongitude"].ToString());
                        employee.customer.location.name = reader["CustomerAddr"].ToString();
                        return employee;
                    }
                    else
                    {
                        Employee employee = new Employee();
                        employee.id = "29:1QwL9e77CWAGkB03TNiy9S-uOPIRUtIK6Lsv5aUO4pJM";
                        //employee.id = "8:orgid:eca6d4d4-89be-4b73-8699-6e70c9cbe6bf";
                        employee.name = "Skype Team";
                        employee.serviceUrl = @"https://skype.botframework.com";
                        employee.location = new GeoLocation();
                        employee.location.latitude = 40.7130200;
                        employee.location.longitude = -74.0057100;
                        employee.customer = new BotUser();
                        employee.customer.id = "29:1LWS1S2LigpdBCKwy1F7kuKBfvO3RvUmuiSaYfnjv2z4";
                        employee.customer.name = "Xiang Yan";
                        employee.customer.serviceUrl = @"https://skype.botframework.com";
                        employee.customer.location = new GeoLocation();
                        employee.customer.location.latitude = 40.7207259535789;
                        employee.customer.location.longitude = -74.0059641748667;
                        employee.customer.location.name = "Spring Studios";
                        return employee;
                    }
                }
                finally
                {
                    // Always call Close when done reading.
                    reader.Close();
                }
            }
        }

        [Route("register")]
        [HttpPost]
        // POST api/values
        public void Register([FromBody]Employee value)
        {
            try
            {
                using (SqlConnection openCon = new SqlConnection(connstr))
                {
                    SqlCommand cmd = new SqlCommand("DELETE FROM EMPLOYEES WHERE skype = '" + value.id + "'");
                    cmd.Connection = openCon;
                    openCon.Open();
                    cmd.ExecuteNonQuery();
                    cmd.CommandText = "INSERT into EMPLOYEES (skype,name,serviceurl,conversationid,latitude,longitude) VALUES ('"
                        + value.id + "','" + value.name + "', '" + value.serviceUrl + "','" + value.conversationId + "',"
                        + 40.720862 + ", " + -74.005947 + ")";
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception) { }
        }

        [Route("saveinformation")]
        [HttpPost]
        public void SaveInformation([FromBody]Employee value)
        {
            try
            {
                using (SqlConnection openCon = new SqlConnection(connstr))
                {
                    SqlCommand cmd = new SqlCommand("UPDATE EMPLOYEES set CustomerSkype='"
                        + value.customer.id + "', CustomerName='"
                        + value.customer.name + "', CustomerServiceUrl = '"
                        + value.customer.serviceUrl + "', CustomerLatitude="
                        + value.customer.location.latitude + ",CustomerLongitude="
                        + value.customer.location.longitude + ", CustomerAddr='" + 
                        value.customer.location.name + "' WHERE Skype = '" + value.id + "'");
                    cmd.Connection = openCon;
                    openCon.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception) { }
        }

        [Route("filecase")]
        [HttpGet]
        public string FileCase(string userid, string incidenttype, double lat, double lon)
        {
            //todo add record to database
            return "83723";
        }

        [Route("locatebike")]
        [HttpGet]
        public GeoLocation LocateBike(string bikeid, string datetime = "")
        {
            GeoLocation ret = new GeoLocation();
            if (datetime == "")
            {
                ret.latitude = 40.722567290067673;
                ret.longitude = -73.9976117759943;
                ret.name = "Chipotle Mexican Grill";
            }
            else
            {
                ret.latitude = 40.720725953578949;
                ret.longitude = -74.005964174866676;
                ret.name = "Spring Studios";
            }
            return ret;
        }

        [Route("getmapwithroute")]
        [HttpGet]
        public string GetMapWithRoute(double latitude1, 
            double longitude1, string name1, double latitude2,
            double longitude2, string name2)
        {
            double midlat = (latitude1 + latitude2) / 2;
            double midlon = (longitude1 + longitude2) / 2;
            return string.Format(_bingmapstaticmapwithrouteurl,
                       latitude1.ToString(), longitude1.ToString(),
                       latitude2.ToString(), longitude2.ToString(),
                       midlat.ToString(), midlon.ToString(), name1.ToLower(),
                       name2.ToLower());
        }

        [Route("getmapwith1pin")]
        [HttpGet]
        public string GetMapWith1Pin(double latitude,
            double longitude, string name)
        {
            return string.Format(_bingmapstaticmapwith1pointsurl,
                       latitude.ToString(), longitude.ToString(), name);
        }

        [Route("geteta")]
        [HttpGet]
        public async Task<int> GetETA(double latitude1, double longitude1, 
            double latitude2, double longitude2)
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("http://dev.virtualearth.net");

            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            string parameter = string.Format(_bingmapgetroutetime, latitude1.ToString(), longitude1.ToString(), latitude2.ToString(), longitude2.ToString());
            HttpResponseMessage response = client.GetAsync("REST/v1/Routes?wayPoint.1=40.720862,-74.005947&waypoint.2=40.713078,-74.005711&key=AnFdkPU2iJh-hws2VacLRfbVHv-Axxv5xbshHY7vcymXVddNfKlBAhbvRvlhERIJ").Result;
            if (response.IsSuccessStatusCode)
            {
                JsonSerializerSettings settings = new JsonSerializerSettings();
                String responseString = await response.Content.ReadAsStringAsync();
                var responseElement = JsonConvert.DeserializeObject<Rootobject>(responseString, settings);
                if (responseElement == null || responseElement.resourceSets == null
                    || responseElement.resourceSets.Length < 1
                    || responseElement.resourceSets[0].resources == null
                    || responseElement.resourceSets[0].resources.Length < 1)
                    return -1;
                return responseElement.resourceSets[0].resources[0].travelDurationTraffic;
            }
            return -1;
        }

        private static XmlDocument GetXmlResponse(string requestUrl)
        {
            try
            {
                HttpWebRequest request = WebRequest.Create(requestUrl) as HttpWebRequest;
                HttpWebResponse response = request.GetResponse() as HttpWebResponse;

                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(response.GetResponseStream());
                return (xmlDoc);

            }
            catch (Exception e)
            {
                var roo = e.Message;
                return null;
            }
        }
    }
}

using System;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.PowerBI.Api.V1;
using Microsoft.PowerBI.Security;
using Microsoft.Rest;
using Microsoft.PowerBI.Api.V1.Models;
using System.Collections.Generic;
using BikeSharing_DashBoardSite.Models;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using System.Web;

namespace paas_demo.Controllers
{
    public class DashboardController : Controller
    {
        private readonly string workspaceCollection;
        private readonly string workspaceId;
        private readonly string accessKey;
        private readonly string apiUrl;
        private readonly string bikeReportId;
        private readonly string StationsUrl;
        private readonly string RidesUrl;
        private readonly string IssuesUrl;
        private readonly string SubscriptionUrl;
        private readonly string UserUrl;

        public DashboardController()
        {
            this.workspaceCollection = ConfigurationManager.AppSettings["powerbi:WorkspaceCollection"];
            this.workspaceId = ConfigurationManager.AppSettings["powerbi:WorkspaceId"];
            this.accessKey = ConfigurationManager.AppSettings["powerbi:AccessKey"];
            this.apiUrl = ConfigurationManager.AppSettings["powerbi:ApiUrl"];
            this.bikeReportId = ConfigurationManager.AppSettings["powerbi:BikesReportId"];

            this.StationsUrl = ConfigurationManager.AppSettings["bikes:StationsUrl"];
            string privateWebUrl = ConfigurationManager.AppSettings["bikes:PrivateWebUrl"];
            this.RidesUrl = privateWebUrl;
            this.IssuesUrl = $"{privateWebUrl}{ConfigurationManager.AppSettings["bikes:IssuesRoute"]}";
            this.SubscriptionUrl = $"{privateWebUrl}{ConfigurationManager.AppSettings["bikes:SubscriptionRoute"]}";
            this.UserUrl = $"{privateWebUrl}{ConfigurationManager.AppSettings["bikes:UserRoute"]}";
        }

        public async Task<ActionResult> Index(string id)
        {
            ViewBag.StationsUrl = string.Format(@"{0}?id={1}", this.StationsUrl, id);
            ViewBag.RidesUrl = this.RidesUrl;
            ViewBag.IssuesUrl = this.IssuesUrl;
            ViewBag.SubscriptionUrl = this.SubscriptionUrl;

            await getUserInfo(id);

            using (var client = this.CreatePowerBIClient())
            {
                var reportsResponse = client.Reports.GetReports(this.workspaceCollection, this.workspaceId);
                var report = reportsResponse.Value.FirstOrDefault(r => r.Id == this.bikeReportId);
                var embedToken = PowerBIToken.CreateReportEmbedToken(this.workspaceCollection, this.workspaceId, report.Id);

                var viewModel = new DashBoardIndexViewModel
                {
                    Report = report,
                    AccessToken = embedToken.Generate(this.accessKey)
                };

                return View(viewModel);
            }
        }

        private async Task getUserInfo(string id)
        {
            using (var client = new HttpClient())
            {
                var uri = new Uri(this.UserUrl + "/" + id);
                try
                {
                    var response = await client.GetAsync(uri);
                    if (!response.IsSuccessStatusCode)
                    {
                        ViewBag.UserName = null;
                        ViewBag.UserImage = null;
                        return;
                    }

                    string result = await response.Content.ReadAsStringAsync();
                    JObject json = JObject.Parse(result);

                    ViewBag.UserName = json.GetValue("name");
                    ViewBag.UserImage = json.GetValue("image");
                }
                catch(Exception)
                {
                    ViewBag.UserName = null;
                    ViewBag.UserImage = null;
                }
            }
                
        }

 
        private IPowerBIClient CreatePowerBIClient()
        {
            var credentials = new TokenCredentials(accessKey, "AppKey");
            var client = new PowerBIClient(credentials)
            {
                BaseUri = new Uri(apiUrl)
            };

            return client;
        }
    }
}
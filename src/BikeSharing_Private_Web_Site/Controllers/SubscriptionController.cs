using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using BikeSharing_Private_Web_Site.Services;
using Microsoft.Extensions.Options;
using BikeSharing_Private_Web_Site.Configuration;
using BikeSharing_Private_Web_Site.Services.Pagination;
using Microsoft.AspNetCore.Authorization;

namespace BikeSharing_Private_Web_Site.Controllers
{
    [Authorize]
    public class SubscriptionController : Controller
    {
        private readonly IOptions<NavigationSettings> _settings;

        public SubscriptionController(IOptions<NavigationSettings> navSettings)
        {
            _settings = navSettings;

            ViewBag.StationUrl = navSettings.Value.StationUrl;
            ViewBag.DashboardUrl = navSettings.Value.DashboardUrl;
        }

        public IActionResult Index(int page)
        {
            ViewBag.StationUrl = _settings.Value.StationUrl;
            ViewBag.DashboardUrl = _settings.Value.DashboardUrl;

            var vm = new Models.SubscriptionViewModels.IndexViewModel()
            {
                PaginationInfo = new PaginationInfo()
                {
                    ActualPage = page,
                    ItemsPerPage = 20
                }
            };

            return View(vm);
        }
    }
}
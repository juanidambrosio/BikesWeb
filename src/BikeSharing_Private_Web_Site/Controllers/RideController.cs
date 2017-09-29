using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using BikeSharing_Private_Web_Site.Services;
using BikeSharing_Private_Web_Site.Models.RideViewModels;
using BikeSharing_Private_Web_Site.Services.Pagination;
using Microsoft.Extensions.Options;
using BikeSharing_Private_Web_Site.Configuration;
using Microsoft.AspNetCore.Authorization;

namespace BikeSharing_Private_Web_Site.Controllers
{
    [Authorize]
    public class RideController : Controller
    {
        IOptions<NavigationSettings> _settings;

        public RideController(IOptions<NavigationSettings> settings)
        {
            _settings = settings;

            ViewBag.StationUrl = settings.Value.StationUrl;
            ViewBag.DashboardUrl = settings.Value.DashboardUrl;
        }

        [HttpGet]
        public IActionResult Index(int page)
        {
            ViewBag.StationUrl = _settings.Value.StationUrl;
            ViewBag.DashboardUrl = _settings.Value.DashboardUrl;

            var vm = new IndexViewModel()
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
using AzureIndegoLib;
using MyBikes.Web.Models.Home;
using System;
using System.Web.Mvc;

namespace MyBikes.Web.Controllers
{
    public class HomeController : Controller
    {
        public string _homeCity = "LosAngeles";
        public DateTime _searchTime = DateTime.Now;


        // GET: Home
        public ActionResult Index()
        {
            return View();
        }
        
        public ActionResult GetClients()
        {
            var ticks = DateTime.Now.Ticks;
            var cityRouter = (int)(ticks % 2);

            var cityName = CityLookup.Lookup(cityRouter);

            return View("ClientsView", new ClientsViewModel { CityName = cityName });
        }
    }


}
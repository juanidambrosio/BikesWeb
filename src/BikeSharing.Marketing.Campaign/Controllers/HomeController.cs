﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using BikeSharing.Web.Configuration; 

namespace BikeSharing.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IOption<Links> _links;

        public HomeController (IOption<Links> links)
        {
            _links = links;
        }
        public IActionResult Index()
        {
            ViewBag.PrivateWebsite = _links.Value.PrivateWebsite;
            return View();
        }
    }
}

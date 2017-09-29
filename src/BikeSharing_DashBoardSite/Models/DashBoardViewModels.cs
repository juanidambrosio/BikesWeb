using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using Microsoft.PowerBI.Api.V1.Models;

namespace BikeSharing_DashBoardSite.Models
{
    public class DashBoardIndexViewModel
    {
        public Report Report { get; set; }

        public string AccessToken { get; set; }
    }
}
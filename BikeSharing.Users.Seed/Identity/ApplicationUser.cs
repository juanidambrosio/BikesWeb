using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace BikeSharing.Users.Seed.Identity
{
    public class ApplicationUser : IdentityUser
    {
        public string Name { get; set; }
        public string ProfileImage { get; set; }
    }
}

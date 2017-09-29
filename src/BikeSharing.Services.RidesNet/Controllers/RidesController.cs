using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using BikeSharing.Services.RidesNet.Data;
using Microsoft.EntityFrameworkCore;

namespace BikeSharing.Services.RidesNet.Controllers
{
    [Route("api/[controller]")]
    public class RidesController : Controller
    {
        private readonly RidesDbContext _db;
        public RidesController(RidesDbContext db)
        {
            _db = db;
        }

        // GET api/values
        [HttpGet]
        public async Task<IActionResult> Get(int from = 0, int qty = 15000)
        {
            var data = await _db.Rides.Skip(from).Take(qty).ToListAsync();
            return Ok(data);
        }
    }
}

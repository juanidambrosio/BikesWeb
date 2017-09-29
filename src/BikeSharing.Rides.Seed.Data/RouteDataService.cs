using BikeSharing.Rides.Seed.Data.Entities;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace BikeSharing.Rides.Seed.Data
{
    public class RouteDataService
    {
        private readonly string _constr;

        public RouteDataService(string constr)
        {
            _constr = constr;
        }

        public IEnumerable<Route> GetAll() {
            var routes = new List<Route>();

            using (var con = new SqlConnection(_constr))
            {
                con.Open();
                var cmd = new SqlCommand(Route.GetSelectAllSql(), con);
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    var route = new Route()
                    {
                        StartStationId = (int)reader["StartStationId"],
                        EndStationId = (int)reader["EndStationId"],
                        StartLatitude = (decimal)reader["StartLatitude"],
                        StartLongitude = (decimal)reader["StartLongitude"],
                        EndLatitude = (decimal)reader["EndLatitude"],
                        EndLongitude = (decimal)reader["EndLongitude"],
                    };

                    routes.Add(route);
                }
            }
            return routes;
        }

    }
}

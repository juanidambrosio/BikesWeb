using BikeSharing.Rides.Seed.Data.Entities;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace BikeSharing.Rides.Seed.Data
{
    public class RideDataService
    {
        private readonly string _constr;

        public RideDataService(string constr)
        {
            _constr = constr;
        }

        public IEnumerable<Ride> GetRidesWithNoPoints(int startStation, int endStation) {

            var rides = new List<Ride>();
            using (var con = new SqlConnection(_constr))
            {
                con.Open();

                var sql = $@"select rp.id as rpid,
                    r.Id, r.Duration, r.Start, r.Stop, r.StartStationId, r.EndStationId, r.BikeId, r.UserId, r.EventType, r.EventId
                    from rides r
                    left outer join ridePositions rp
					on r.Id = rp.RideId
					where rp.Id IS NULL
                    and r.startStationId={startStation} and r.endStationId={endStation}";

                var cmd = new SqlCommand(sql, con);

                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    var ride = new Ride
                    {
                        Id = (int)reader["id"],
                        Duration = (int)reader["Duration"],
                        StartStationId = (int)reader["startStationId"],
                        EndStationId = (int)reader["endStationId"],
                        StartTime = (DateTime)reader["Start"],
                        StopTime = (DateTime)reader["Stop"]
                    };

                    rides.Add(ride);
                }
            }

            return rides;
        }

        public void AddRidePositions(IEnumerable<RidePosition> ridePositions)
        {
            using (var con = new SqlConnection(_constr))
            {
                con.Open();
                var sql = $@"insert into ridePositions ([RideId], [Latitude], [Longitude], [TS])
                values (@rideid, @lat, @long, @ts)";
                using (var cmd = new SqlCommand(sql, con))
                {
                    cmd.Parameters.Clear();
                    cmd.Parameters.Add(new SqlParameter() { ParameterName = "@rideid" });
                    cmd.Parameters.Add(new SqlParameter() { ParameterName = "@lat" });
                    cmd.Parameters.Add(new SqlParameter() { ParameterName = "@long" });
                    cmd.Parameters.Add(new SqlParameter() { ParameterName = "@ts" });

                    foreach (var ridePosition in ridePositions)
                    {
                        cmd.Parameters["@rideid"].Value = ridePosition.RideId;
                        cmd.Parameters["@lat"].Value = ridePosition.Latitude;
                        cmd.Parameters["@long"].Value = ridePosition.Longitude;
                        cmd.Parameters["@ts"].Value = ridePosition.TS;
                        cmd.ExecuteNonQuery();
                    }

                    
                }

            }
        }
    }
}

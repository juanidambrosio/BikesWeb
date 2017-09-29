using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace BikeSharing.Rides.Seed.Data.Entities
{
    public class Bike
    {
        public int Id { get; set; }
        public string SerialNumber { get; set; }
        public DateTime InCirculationSince { get; set; }
        public int? StationId { get; set; }

        public string GetInsertSql()
        {
            return @"insert into bikes 
                        (Id, SerialNumber, InCirculationSince, StationId) 
                        values
                        (@Id, @SerialNumber, @InCirculationSince, @StationId)";
        }

        public IEnumerable<SqlParameter> GetParameters()
        {
            yield return new SqlParameter("@Id", Id);
            yield return new SqlParameter("@SerialNumber", SerialNumber);
            yield return new SqlParameter("@InCirculationSince", InCirculationSince);
            if (StationId == null)
            {
                yield return new SqlParameter("@StationId", DBNull.Value);
            }
            else
            {
                yield return new SqlParameter("@StationId", StationId);
            }
        }
    }
}

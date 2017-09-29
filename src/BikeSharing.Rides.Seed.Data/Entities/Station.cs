using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace BikeSharing.Rides.Seed.Data.Entities
{
    public class Station
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public decimal Slots { get; set; }

        public string GetInsertSql()
        {
            return @"insert into stations 
                        (Id, Name, Latitude, Longitude, Slots) 
                        values
                        (@Id, @Name, @Latitude, @Longitude, @Slots) ";
        }

        public IEnumerable<SqlParameter> GetParameters()
        {
            yield return new SqlParameter("@Id", Id);
            yield return new SqlParameter("@Name", Name);
            yield return new SqlParameter("@Latitude", Latitude);
            yield return new SqlParameter("@Longitude", Longitude);
            yield return new SqlParameter("@Slots", Slots);
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureIndegoLib
{
    public class CityLookup
    {
        private static Dictionary<int, City> _IdCityDictionary = new Dictionary<int, City>();
        public static string _searchCity = "LosAngeles";
        public static DateTime _searchTime;

        static CityLookup()
        {
            _IdCityDictionary.Add(1, new City
            {
                Id = 1,
                Name = "Los Angeles",
                Zip = 90001
            });

            _IdCityDictionary.Add(2, new City
            {
                Id = 2,
                Name = "West Lafayette",
                Zip = 47906
            });

            _IdCityDictionary.Add(3, new City
            {
                Id = 3,
                Name = "Seattle",
                Zip = 98101
            });

            _IdCityDictionary.Add(4, new City
            {
                Id = 4,
                Name = "Chicago",
                Zip = 60007
            });

            _IdCityDictionary.Add(5, new City
            {
                Id = 1,
                Name = "Kenmore",
                Zip = 98028
            });

        }


        public static string Lookup(int id)
        {
            try
            {
                var searchTime = DateTime.Now;
                var searchCity = _searchCity;
                var city = _IdCityDictionary[id];
                return city.Name;
            }
            catch(KeyNotFoundException e)
            {
                throw new CityNotFoundException("city code " + id + " not found");
            }
        }
    }
}


using BikeSharing.Rides.Seed.Parsers;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TinyCsvParser;
using TinyCsvParser.Mapping;
using TinyCsvParser.Tokenizer.RegularExpressions;
using System.Data;
using BikeSharing.Rides.Seed.Data.Entities;
using Microsoft.Extensions.Configuration;

namespace BikeSharing.Rides.Seed
{
    public class RidesImporter
    {
        private Dictionary<int, Station> _stations;
        private Dictionary<int, Bike> _bikes;
        private List<User> _users;

        private readonly string _constr;
        private readonly Random _random;
        private object _user;

        private int _maxDay = -1;
        private int _maxHour = -1;
        private int _forceMonth = -1;

        public RidesImporter(string constr, IConfiguration datesConfig)
        {
            _users = new List<User>();
            _constr = constr;
            _stations = new Dictionary<int, Station>();
            _bikes = new Dictionary<int, Bike>();
            _random = new Random();

            _maxDay = datesConfig["maxDay"] != null ? int.Parse(datesConfig["maxDay"]) : _maxDay;
            _maxHour = datesConfig["maxHour"] != null ? int.Parse(datesConfig["maxHour"]) : _maxHour;
            _forceMonth = datesConfig["forceMonth"] != null ? int.Parse(datesConfig["forceMonth"]) : _forceMonth;
        } 

        public void LoadStations()
        {
            Console.WriteLine("+++ Reading Stations");
            using (var con = new SqlConnection(_constr))
            {
                con.Open();
                var cmd = new SqlCommand("SELECT * from [dbo].[stations]", con);
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    var station = new Station()
                    {
                        Id = (int)reader["Id"],
                        Latitude = (decimal)reader["Latitude"],
                        Longitude = (decimal)reader["Longitude"],
                        Name = (string)reader["Name"],
                        Slots = (decimal)reader["Slots"],
                    };

                    _stations.Add(station.Id, station);
                }
            }

            Console.WriteLine("+++ Reading Stations Done. {0} found.", _stations.Count);
        }

        public void LoadBikes()
        {
            Console.WriteLine("+++ Reading Bikes");
            using (var con = new SqlConnection(_constr))
            {
                con.Open();
                var cmd = new SqlCommand("SELECT * from [dbo].[bikes]", con);
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    var bike = new Bike()
                    {
                        Id = (int)reader["Id"],
                        SerialNumber = (string)reader["SerialNumber"],
                        InCirculationSince = (DateTime)reader["InCirculationSince"],
                        StationId = reader["StationId"] as int?
                    };

                    _bikes.Add(bike.Id, bike);
                }
            }

            Console.WriteLine("+++ Reading Bikes Done. {0} found.", _bikes.Count);
        }

        public void ProcessData(IEnumerable<CsvMappingResult<Ride>> rawRides)
        {
            var rides = rawRides.Where(r => r.IsValid)
                .Select(r => r.Result);
            if (_maxDay > 0)
            {
                rides = rides.Where(r => r.StartTime.Date.Day <= _maxDay && r.StopTime.Date.Day <= _maxDay);
            }

            var currentDay = DateTime.MinValue.Date;
            var idx = 0;
            using (var con = new SqlConnection(_constr))
            {
                con.Open();
                foreach (var ride in rides)
                {
                    var startDay = ride.StartTime.Date;
                    if (_forceMonth > 0)
                    {
                        startDay = new DateTime(startDay.Year, _forceMonth, startDay.Day); 
                    }
                    var endDay = ride.StopTime.Date;
                    if (_forceMonth > 0)
                    {
                        endDay = new DateTime(endDay.Year, _forceMonth, endDay.Day);
                    }

                    if (startDay != currentDay)
                    {
                        foreach (var user in _users)
                        {
                            user.ClearAllDayIntervals(currentDay);
                        }
                        currentDay = startDay;
                        Console.WriteLine("Start processing day {0} (forced month {1})", 
                            currentDay, _forceMonth > 0 ? _forceMonth.ToString() : "none");
                    }

                    if (startDay.Day == _maxDay  && _maxHour > 0 && 
                        (ride.StartTime.Hour > _maxHour || ride.StopTime.Hour > _maxHour))
                    {
                        continue;
                    }

                    var sql = ride.GetInsertSql();
                    using (var cmd = new SqlCommand(sql, con))
                    {
                        if (!StationExists(ride.StartStationId))
                        {
                            CreateStartStation(ride, con);
                        }

                        if (!StationExists(ride.EndStationId))
                        {
                            CreateEndStation(ride, con);
                        }

                        if (!BikeExists(ride.BikeId))
                        {
                            CreateBike(ride, con);
                        }

                        cmd.Parameters.AddRange(ride.GetParameters(_forceMonth).ToArray());
                        cmd.Parameters.Add(new SqlParameter("@userId", GetUserId(ride)));
                        cmd.ExecuteNonQuery();
                    }
                    idx++;
                    if (idx % 5000 == 0)
                    {
                        Console.WriteLine("{0} processed. Continuing...", idx);
                    }
                }
            }
        }

        private int GetUserId(Ride ride)
        {
            var existing = _users.FirstOrDefault(u =>
                u.Gender == ride.Gender &&
                u.Birthyear == ride.BirthYear &&
                u.UserType == ride.UserType
                && u.IsValidInterval(ride.StartTime, ride.StopTime));

            if (existing != null)
            {
                existing.AddInterval(ride.StartTime, ride.StopTime);
                return existing.Id;
            }

            int nextId = _users.Count + 1;
            var newUser = new User()
            {
                Gender = ride.Gender,
                Birthyear = ride.BirthYear,
                UserType = ride.UserType,
                Id = nextId
            };
            newUser.AddInterval(ride.StartTime, ride.StopTime);
            _users.Add(newUser);
            return nextId;
        }

        private bool StationExists(int stationId)
        {
            return _stations.ContainsKey(stationId);
        }

        private bool BikeExists(int bikeId)
        {
            return _bikes.ContainsKey(bikeId);
        }

        private void CreateEndStation(Ride ride, SqlConnection con)
        {
            var station = new Station()
            {
                Id = ride.EndStationId,
                Name = ride.EndStationName,
                Latitude = (decimal)ride.EndStationLatitude,
                Longitude = (decimal)ride.EndStationLongitude
            };

            InsertStation(station, con);
            _stations.Add(station.Id, station);
        }

        private void CreateStartStation(Ride ride, SqlConnection con)
        {
            var station = new Station()
            {
                Id = ride.StartStationId,
                Name = ride.StartStationName,
                Latitude = (decimal)ride.StartStationLatitude,
                Longitude = (decimal)ride.StartStationLongitude
            };

            InsertStation(station, con);
            _stations.Add(station.Id, station);
        }

        private void InsertStation(Station station, SqlConnection con)
        {
            station.Slots = (byte)_random.Next(30, 90);

            var sql = station.GetInsertSql();
            using (var cmd = new SqlCommand(sql, con))
            {
                cmd.Parameters.AddRange(station.GetParameters().ToArray());
                cmd.ExecuteNonQuery();
            }

            Console.WriteLine("Station {0} created.", station.Name);
        }

        private void CreateBike(Ride ride, SqlConnection con)
        {
            var bike = new Bike()
            {
                Id = ride.BikeId,
                SerialNumber = Guid.NewGuid().ToString().Substring(0, 15),
                InCirculationSince = DateTime.Now.AddMonths(-2),
                StationId = GetAvailableStation()
            };

            InsertBike(bike, con);
            _bikes.Add(bike.Id, bike);
        }

        private void InsertBike(Bike bike, SqlConnection con)
        {
            var sql = bike.GetInsertSql();
            using (var cmd = new SqlCommand(sql, con))
            {
                cmd.Parameters.AddRange(bike.GetParameters().ToArray());
                cmd.ExecuteNonQuery();
            }
        }

        private int? GetAvailableStation()
        {
            var freeSlotsEnsured = 15;

            var station = _stations.Values.OrderBy(s => _bikes.Values.Count(b => b.StationId == s.Id))
                .FirstOrDefault(s => s.Slots - freeSlotsEnsured > _bikes.Values.Count(b => b.StationId == s.Id));

            if (station != null)
            {
                return station.Id;
            }

            return null;
        }
    }
}

using BikeSharing.Rides.Seed.Data.Entities;
using CsvHelper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BikeSharing.Rides.Seed.MiniToDb
{
    public class MiniParser
    {
        private readonly string _miniPath;
        private readonly DirectoryInfo _miniDir;

        private Dictionary<string, List<List<RidePosition>>> _pos;

        public MiniParser(string inputPath)
        {
            _miniPath = inputPath;
            _miniDir = new DirectoryInfo(_miniPath);
            _pos = new Dictionary<string, List<List<RidePosition>>>();
        }


        private void AddInfoFromMiniFiles(string key, int from, int to)
        {
            var positionsLists = new List<List<RidePosition>>();
                var files = _miniDir.GetFiles($"*_{from}-{to}.route.mini");
            foreach (var file in files)
            {
                var filePositions = new List<RidePosition>();
                using (var fs = new FileStream(file.FullName, FileMode.Open, FileAccess.Read))
                using (var rd = new StreamReader(fs))
                {
                    var reader = new CsvReader(rd);
                    while(reader.Read())
                    {
                        var lat = reader.GetField<double>(0);
                        var lon = reader.GetField<double>(1);
                        filePositions.Add(new RidePosition() { Latitude = lat, Longitude = lon });
                    }

                    if (filePositions.Any())
                    {
                        positionsLists.Add(filePositions);
                    }
                }

            }

 
             _pos.Add(key, positionsLists);

        }

        public IEnumerable<RidePosition> GetPositions(int from, int to)
        {
            var key = $"{from}|{to}";
            return GetRandomPositions(_pos[key]);
        }

        public IEnumerable<MiniFileGroup> GetGroups()
        {
            foreach (var key in _pos.Keys)
            {
                var toks = key.Split('|');
                yield return new MiniFileGroup()
                {
                    From = int.Parse(toks[0]),
                    To = int.Parse(toks[1])
                };
            }
        }

        public void ReadAllFiles()
        {
            var files = _miniDir.GetFiles("0_*.mini");
            foreach (var file in files)
            {
                var strFromTo = file.Name.Substring(file.Name.IndexOf('_') + 1);
                strFromTo = strFromTo.Replace(".route.mini", "");
                var fromto = strFromTo.Split('-');
                var from = int.Parse(fromto[0]);
                var to = int.Parse(fromto[1]);
                AddInfoFromMiniFiles($"{from}|{to}", from, to);
            }
        }

        private IEnumerable<RidePosition> GetRandomPositions(List<List<RidePosition>> list)
        {
            if (!list.Any()) { return null; }
            var random = new Random();
            var idx = random.Next(0, list.Count);
            return list[idx];
        }
    }
}

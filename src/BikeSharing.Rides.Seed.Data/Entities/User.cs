using System;
using System.Collections.Generic;

namespace BikeSharing.Rides.Seed.Data.Entities
{
    public class User
    {
        class RideIntervals
        {
            public DateTime From { get; set; }
            public DateTime To { get; set; }
        }

        private List<RideIntervals> _intervals;

        public User()
        {
            _intervals = new List<RideIntervals>();
        }

        public int Id { get; set; }

        public int? Birthyear { get; set; }

        public int Gender { get; set; }

        public string UserType { get; set; }

        public bool IsValidInterval(DateTime from, DateTime to)
        {
            // An interval of time is valid only if is not partially or totally overlapped with
            // other intervals
            // Two interval overlapps if one starts in the middle or end in the middle of another
            foreach (var interval in _intervals)
            {
                if (from >= interval.From && from <= interval.To) return false;
                if (to >= interval.From && to <= interval.To) return false;
            }

            return true;
        }

        public void AddInterval(DateTime from, DateTime to)
        {
            _intervals.Add(new RideIntervals() { From = from, To = to });
        }

        public void ClearAllDayIntervals(DateTime currentDay)
        {
            var save = new List<RideIntervals>();
            foreach (var interval in _intervals)
            {
                var fday = interval.From.Date;
                var tday = interval.To.Date;

                if (fday != currentDay || tday != currentDay)
                {
                    save.Add(interval);
                }
            }

            _intervals.Clear();
            _intervals.AddRange(save);
        }
    }
}
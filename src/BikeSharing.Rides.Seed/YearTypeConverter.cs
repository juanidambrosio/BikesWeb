using System;
using TinyCsvParser.TypeConverter;

namespace BikeSharing.Rides.Seed.Parsers
{
    internal class YearTypeConverter : ITypeConverter<int?>
    {
        public Type TargetType
        {
            get
            {
                return typeof(int?);
            }
        }

        public bool TryConvert(string value, out int? result)
        {
            if (string.IsNullOrEmpty(value))
            {
                result = null;
                return true;
            }

            if (value == @"\N") {
                result = null;
                return true;
            }

            result = int.Parse(value);
            return true;
        }
    }
}
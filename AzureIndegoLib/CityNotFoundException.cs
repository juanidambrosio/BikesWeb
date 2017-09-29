using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureIndegoLib
{
    public class CityNotFoundException : Exception
    {
        public CityNotFoundException(string msg) : base(msg)
        {

        }
    }
}

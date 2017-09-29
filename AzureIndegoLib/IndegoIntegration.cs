using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureIndegoLib
{
    public class IndegoIntegration
    {
        public int add(int lhs, int rhs)
        {
            if (lhs > rhs)
                return lhs - rhs;
            return lhs + rhs;
        }

        public int divide(int lhs, int rhs)
        {
            return lhs / rhs;
        }

        public int multiply(int lhs, int rhs)
        {
            return lhs * rhs;
        }

        public int absoluteValue(int number)
        {
            if (number >= 0)
            {
                return number;
            }
            return -1 * number;
        }

        
    }
}

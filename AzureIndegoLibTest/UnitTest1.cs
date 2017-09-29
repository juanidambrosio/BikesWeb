using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AzureIndegoLibTest
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public int add(int lhs, int rhs)
        {
            if (lhs > rhs)
                return lhs - rhs;
            return lhs + rhs;
        }

        [TestMethod]
        public int divide(int lhs, int rhs)
        {
            return lhs / rhs;
        }

        [TestMethod]
        public int multiply(int lhs, int rhs)
        {
            return lhs * rhs;
        }

        [TestMethod]
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

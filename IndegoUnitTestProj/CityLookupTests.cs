using AzureIndegoLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IndegoUnitTestProj
{
    [TestClass]
    public class CityLookupTests
    {
        [TestMethod]
        [TestCategory("Unit")]
        public void CityLookupTestLA()
        {
            var name = CityLookup.Lookup(1);

            Assert.AreEqual("Los Angeles", name);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void CityLookupTestWestLafayette()
        {
            var name = CityLookup.Lookup(2);

            Assert.AreEqual("West Lafayette", name);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void CityLookupTestSeattle()
        {
            var name = CityLookup.Lookup(3);

            Assert.AreEqual("Seattle", name);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [ExpectedException(typeof(CityNotFoundException))]
        public void CityNotFoundTest()
        {
            var name = CityLookup.Lookup(999);

        }
    }
}

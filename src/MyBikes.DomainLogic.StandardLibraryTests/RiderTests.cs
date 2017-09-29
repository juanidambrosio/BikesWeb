using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using MyBikes.DomainLogic.StandardLibrary;

namespace MyBikes.DomainLogic.StandardLibraryTests
{
    [TestClass()]
    public class RiderTests
    {
        [TestMethod()]
        public void ReceiveDamageTest()
        {
            Rider rider = new Rider(10);
            rider.ReceiveDamage(3);
            Assert.AreEqual(7, rider.CurrentHitPoints);

        }
    }
}

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AzureIndegoLib;

namespace IndegoUnitTestProj
{
    [TestClass]
    public class UnitTest1
    {

        [TestMethod]
        /**
         * Unit tests which tests the add method and makes sure the answer is correct
         **/
        public void AddTest()
        {
            var integrator = new IndegoIntegration();
            var answer = integrator.add(3, 5);

            Assert.AreEqual(8, answer, "Answer should be 8");
        }

        [TestMethod]
        public void DivideTest()
        {
            var integrator = new IndegoIntegration();
            var answer = integrator.divide(6, 3);

            Assert.AreEqual(2, answer, "Answer should be 2");
        }

        [TestMethod]
        public void MultiplyTest()
        {
            var integrator = new IndegoIntegration();
            var answer = integrator.multiply(3, 2);

            Assert.AreEqual(6, answer, "Answer should be 6");
        }

        [TestMethod]
        public void AbsoluteValuePositiveNumberTest()
        {
            var integrator = new IndegoIntegration();
            var answer = integrator.absoluteValue(5);

            Assert.AreEqual(5, answer, "Answer should be 5");
        }

        [TestMethod]
        public void AbsoluteValueNegativeNumberTest()
        {
            var integrator = new IndegoIntegration();
            var answer = integrator.absoluteValue(-5);

            Assert.AreEqual(5, answer, "Answer should be 5");
        }
    }
}

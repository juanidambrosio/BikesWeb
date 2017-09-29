using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AzureIndegoLib;
using Microsoft.QualityTools.Testing.Fakes;
using AzureIndegoLib.Fakes;
using System.Fakes;

namespace IndegoUnitTestProj
{
    [TestClass]
    public class IndegoTests
    {
 
        [TestMethod]
        [TestCategory("Unit")]

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
        [TestCategory("Unit")]
        public void DivideTest()
        {
            var integrator = new IndegoIntegration();
            var answer = integrator.divide(6, 3);

            Assert.AreEqual(2, answer, "Answer should be 2");
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void MultiplyTest()
        {
            var integrator = new IndegoIntegration();
            var answer = integrator.multiply(3, 2);

            Assert.AreEqual(6, answer, "Answer should be 6");
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void AbsoluteValuePositiveNumberTest()
        {
            var integrator = new IndegoIntegration();
            var answer = integrator.absoluteValue(5);

            Assert.AreEqual(5, answer, "Answer should be 5");
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void AbsoluteValueNegativeNumberTest()
        {
            var integrator = new IndegoIntegration();
            var answer = integrator.absoluteValue(-5);

            Assert.AreEqual(5, answer, "Answer should be 5");
        }

        
        

    }
}

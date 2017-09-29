using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AzureIndegoLib;
using Microsoft.QualityTools.Testing.Fakes;
using AzureIndegoLib.Fakes;
using System.Fakes;

namespace IndegoUnitTestProj
{
    [TestClass]
    public class ReportGeneratorTests
    {
        [TestMethod]
        [TestCategory("Unit")]
        public void GetReportNameTest()
        {
            using (ShimsContext.Create())
            {
               
                ShimUserRepository.AllInstances
                        .GetUserInt32 = (self, userId) => new User
                        {
                            Id = userId,
                            FirstName = "Abel",
                            LastName = "Wang",
                            CityCode = 1
                        };

                ShimDateTime.NowGet = () => new DateTime(2017, 12, 12);

                var reportName = ReportGenerator.GetReportName(2342);

                Assert.AreEqual("Wang_Abel_Los Angeles_12/12/2017 12:00 AM", reportName);
            }
        }
    }
}

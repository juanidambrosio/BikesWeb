// <copyright file="HealthCalculatorTest.cs">Copyright ©  2016</copyright>
using System;
using Microsoft.Pex.Framework;
using Microsoft.Pex.Framework.Validation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MyBikes.DomainLogic;
using Microsoft.QualityTools.Testing.Fakes;
using System.IO.Fakes;

namespace MyBikes.DomainLogic.Tests
{
    /// <summary>This class contains parameterized unit tests for HealthCalculator</summary>
    [PexClass(typeof(HealthCalculator))]
    [PexAllowedExceptionFromTypeUnderTest(typeof(InvalidOperationException))]
    [PexAllowedExceptionFromTypeUnderTest(typeof(ArgumentException), AcceptExceptionSubtypes = true)]
    [TestClass]
    public partial class HealthCalculatorTest
    {
        /// <summary>Test stub for WeirdHealthIndex(Int32, Int32)</summary>
        [PexMethod]
        [PexAllowedException(typeof(DivideByZeroException))]
        public int WeirdHealthIndexTest(
            [PexAssumeUnderTest]HealthCalculator target,
            int lhs,
            int rhs
        )
        {
            int result = target.WeirdHealthIndex(lhs, rhs);
            return result;
            // TODO: add assertions to method HealthCalculatorTest.WeirdHealthIndexTest(HealthCalculator, Int32, Int32)
        }

        [PexMethod]
        [PexAllowedException(typeof(ArgumentNullException))]
        public int GetNumberOfFilesAndFolders([PexAssumeUnderTest]HealthCalculator target, string path)
        {
            using (ShimsContext.Create())
            {
                ShimDirectory.GetFilesString = (fullPath) => new string[10];
                ShimDirectory.GetDirectoriesString = (fullPath) => new string[3];

                int result = target.GetNumberOfFilesAndFolders(path);
                return result;
            }
            
            // TODO: add assertions to method HealthCalculatorTest.GetNumberOfFilesAndFolders(HealthCalculator, String)
        }
    }
}

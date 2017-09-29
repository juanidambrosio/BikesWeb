using Microsoft.VisualStudio.TestTools.UnitTesting;
using MyBikes.DomainLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyBikes.DomainLogic.Tests
{
    [TestClass()]
    public class TrainerTests
    {
        [TestMethod()]
        public void EmptyTrainerMustHaveCurrentMilesToZero()
        {
            var trainer = new Trainer(100);
            Assert.AreEqual(0, trainer.MilesTravelled);

        }

        [TestMethod]
        public void AddingWorkoutAddsMilesTravelled()
        {
            var trainer = new Trainer(100);
            trainer.RegisterWorkout(10, TimeSpan.FromMinutes(20));
            Assert.AreEqual(1, trainer.MilesTravelled);
        }

        [TestMethod()]
        public void TrainerTest()
        {
            Assert.Fail();
        }
    }
}
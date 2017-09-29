using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Trainer.Tests
{
    [TestClass()]
    public class TrainerTests
    {
        Trainer StubTrainer;
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
            Assert.AreEqual(10, trainer.MilesTravelled);
        }

        [TestMethod]
        public void GetBestWorkoutSpeedNoWorkouts()
        {
            var trainer = new Trainer(10);
            var expectedWorkout = trainer.GetWorkoutWithBestSpeed();
            Assert.AreEqual(null, expectedWorkout);
        }
        [TestMethod]
        public void GetBestWorkoutSpeedOneWorkouts()
        {
            var trainer = new Trainer(10);
            trainer.RegisterWorkout(10, TimeSpan.FromMinutes(20));
            var expectedWorkout = trainer.GetWorkoutWithBestSpeed();
            Assert.AreEqual("Workout: 10 Miles, 20 Minutes", expectedWorkout.ToString());
        }
        [TestMethod]
        public void GetBestWorkoutSpeedTwoWorkouts()
        {
            var trainer = new Trainer(10);
            trainer.RegisterWorkout(10, TimeSpan.FromMinutes(20));
            trainer.RegisterWorkout(5, TimeSpan.FromMinutes(20));
            var expectedWorkout = trainer.GetWorkoutWithBestSpeed();
            Assert.AreEqual("Workout: 10 Miles, 20 Minutes", expectedWorkout.ToString());
        }

        //[TestMethod]
        //public void GetWorkoutWithMostMilesTraveled()
        //{
        //    var trainer = new Trainer(10);
        //    trainer.RegisterWorkout(10, TimeSpan.FromMinutes(20));
        //    trainer.RegisterWorkout(5, TimeSpan.FromMinutes(20));
        //    var expectedWorkout = trainer.GetMostMilesTraveled();
        //    Assert.AreEqual("Workout: 10 Miles, 20 Minutes", expectedWorkout.ToString());
        //}

        private void CreateTrainerWith3Workouts()
        {
            StubTrainer = new Trainer(100);
            StubTrainer.RegisterWorkout(15, TimeSpan.FromMinutes(60));
            StubTrainer.RegisterWorkout(1, TimeSpan.FromMinutes(10));
            StubTrainer.RegisterWorkout(10, TimeSpan.FromMinutes(30));

        }

        [TestMethod]
        public void GettingWorkoutIntensityForEasyWorkouts()
        {
            CreateTrainerWith3Workouts();
            int intensity = StubTrainer.GetWorkoutIntensityCount("Easy");
            Assert.AreEqual(1, intensity);
        }

        [TestMethod]
        public void GettingWorkoutIntensityForHardWorkouts()
        {
            CreateTrainerWith3Workouts();
            int intensity = StubTrainer.GetWorkoutIntensityCount("Hard");
            Assert.AreEqual(1, intensity);
        }

        [TestMethod]
        public void GettingWorkoutIntensityForMediumWorkouts()
        {
            CreateTrainerWith3Workouts();
            int intensity = StubTrainer.GetWorkoutIntensityCount("Medium");
            Assert.AreEqual(1, intensity);
        }


    }
}
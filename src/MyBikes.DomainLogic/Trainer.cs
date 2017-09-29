using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyBikes.DomainLogic
{
    
    public class WorkOut
    {
        public int Miles { get; }
        public TimeSpan Duration { get; }


        public WorkOut(int miles, TimeSpan duration)
        {
            Miles = miles;
            Duration = duration;
        }

        public override string ToString()
        {
            return "Workout: " + Miles + " Miles, " + Duration.TotalMinutes + " Minutes";
        }
    }

    public class Trainer
    {
        public int Goal { get; }
        public int MilesTravelled
        {
            get
            {
                int count = 0;
                foreach (var work in _workOuts)
                {
                    count += work.Miles;
                }
                return count;
            }
        }

        private List<WorkOut> _workOuts;

        public Trainer(int goal)
        {
            _workOuts = new List<WorkOut>();
            Goal = goal; ;
        }

        public void RegisterWorkout(int miles, TimeSpan duration)
        {
            _workOuts.Add(new WorkOut(miles, duration));
        }

        public bool HasMetGoal()
        {
            if (MilesTravelled == Goal)
            {
                return true;
            }
            return false;
        }

        public double GetMilesPerMinute(WorkOut workout)
        {
            return workout.Miles / workout.Duration.TotalMinutes;
        }

        public WorkOut GetWorkoutWithBestSpeed()
        {
            double bestSpeed = 0;
            WorkOut bestWorkout = null;
            foreach (var workout in _workOuts)
            {
                double workoutSpeed = GetMilesPerMinute(workout);

               //estWorkout = (workoutSpeed > bestSpeed) ? workout : bestWorkout;
               //estSpeed = (workoutSpeed > bestSpeed) ? workoutSpeed : bestSpeed;

                // start with this out
                if (workoutSpeed > bestSpeed)
                {
                    bestWorkout = workout;
                    bestSpeed = workoutSpeed;
                }
            }

            return bestWorkout;
        }

        public WorkOut GetMostMilesTraveled()
        {
            var mostMiles = 0;
            WorkOut FurthestWorkout = null;
            foreach (var workout in _workOuts)
            {
                if (workout.Miles > mostMiles)
                {
                    FurthestWorkout = workout;
                    mostMiles = workout.Miles;
                }
            }
            return FurthestWorkout;
        }

        public static string GetWorkoutIntensity(WorkOut workout)
        {
            if (workout == null)
            {
                return "No Workout";
            }
            if ((workout.Duration <= TimeSpan.FromMinutes(30)
                    && workout.Miles >= 10)
                    || workout.Miles >= 20)
            {
                return "Hard";
            }
            else if (workout.Duration < TimeSpan.FromMinutes(30)
                        || workout.Miles < 10)
            {
                return "Easy";
            }
            else
            {
                return "Medium";
            }
        }


        public int GetWorkoutIntensityCount(String desiredIntensity)
        {
            int intensityCount = 0;
            foreach (var workout in _workOuts)
            {
                string intensity = GetWorkoutIntensity(workout);
                if (desiredIntensity == intensity)
                {
                    intensityCount++;
                }
            }
            return intensityCount;
        }
    }

}

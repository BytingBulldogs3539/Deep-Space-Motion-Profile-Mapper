using System;
using System.Collections.Generic;

namespace MotionProfile.Spline
{
    public class VelocityGenerator
    {
        // Mechanism characteristics
        private double maxVel;
        private double maxAccel;
        private double maxJerk;

        // Calculated constants
        private double timeToMaxAchievableVel;
        private double timeToMaxAchievableVelCoast;

        private double timeToMaxAchievableAccel;
        private double timeToMaxAchievableAccelCoast;
        private double timeToAccelCoastDeccel;

        private double displacementAccelCoastDeccel;
        private double timeToAccelCoastDeccelCoast;

        public VelocityGenerator(double maxVel, double maxAccel, double maxJerk)
        {
            this.maxVel = Math.Abs(maxVel);
            this.maxAccel = Math.Abs(maxAccel);
            this.maxJerk = Math.Abs(maxJerk);
        }

        public List<double[]> generateMotionProfile(double distance, double dt)
        {
            calculateConstants(distance);
            double timeToComplete = timeToAccelCoastDeccelCoast + timeToAccelCoastDeccel;
            List<double[]> points = new List<double[]>();

            // vel at time t is found from algebraic integration, while position is found from numerical integration
            double v1 = maxJerk * timeToMaxAchievableAccel * timeToMaxAchievableAccel / 2; // vel at maxAccel
            double v2 = v1 + maxJerk * timeToMaxAchievableAccel * timeToMaxAchievableAccelCoast; // vel after accel coast
            double v3 = v2 + v1; // vel after accel-coast-deccel
                                 // skipping a portion since velocity is constant during this period
            double v4 = v3 - v1; // vel after accel-coast-deccel-zero-accel
            double v5 = v3 - v2; // vel after accel-coast-deccel-zero-accel-coast

            double currTime = 0;
            double currPos = 0;

            // {time, vel, pos}
            double[] currPoint = new double[3];

            while (currTime <= timeToComplete)
            {
                currPoint[0] = currTime;

                if (currTime > timeToComplete - timeToMaxAchievableAccel)
                {
                    double t6 = timeToComplete - timeToMaxAchievableAccel;
                    currPoint[1] = v5 + lookUpAccel(currTime - t6) * (currTime - t6) / 2 + lookUpAccel(timeToComplete - timeToMaxAchievableAccel) * (currTime - t6);
                }
                else if (currTime > timeToAccelCoastDeccelCoast + timeToMaxAchievableAccel)
                {
                    currPoint[1] = v4 + (lookUpAccel(currTime) * (currTime - (timeToAccelCoastDeccelCoast + timeToMaxAchievableAccel)));
                }
                else if (currTime > timeToAccelCoastDeccelCoast)
                {
                    currPoint[1] = v3 + (lookUpAccel(currTime) * (currTime - timeToAccelCoastDeccelCoast)) / 2;
                }
                else if (currTime > timeToAccelCoastDeccel)
                {
                    currPoint[1] = v3;
                }
                else if (currTime > timeToMaxAchievableAccel + timeToMaxAchievableAccelCoast)
                {
                    double t2 = timeToMaxAchievableAccel + timeToMaxAchievableAccelCoast;
                    currPoint[1] = v2 + (currTime - t2) * (lookUpAccel(currTime) + lookUpAccel(t2)) / 2;
                }
                else if (currTime > timeToMaxAchievableAccel)
                {
                    currPoint[1] = v1 + lookUpAccel(currTime) * (currTime - timeToMaxAchievableAccel);
                }
                else if (currTime >= 0)
                {
                    currPoint[1] = lookUpAccel(currTime) * currTime / 2;
                }

                currPoint[2] = currPos += currPoint[1] * dt;

                double[] currPoints2 = { currPoint[0], currPoint[1], currPoint[2] };
                points.Add(currPoints2);

                currTime += dt;
            }
            return points;
        }

        // default 10ms dt
        public List<double[]> generateMotionProfile(double distance)
        {
            return generateMotionProfile(distance, 0.010);
        }

        private void calculateConstants(double distance)
        {
            timeToMaxAchievableVel = Math.Min(Math.Sqrt(distance / maxAccel), maxVel / maxAccel);
            double maxAchieveableVel = timeToMaxAchievableVel * maxAccel;
            timeToMaxAchievableVelCoast = distance / (maxAccel * timeToMaxAchievableVel) - timeToMaxAchievableVel;

            timeToMaxAchievableAccel = Math.Min(Math.Min(Math.Sqrt(maxAchieveableVel / maxJerk), maxAccel / maxJerk),
                    (timeToMaxAchievableVel + timeToMaxAchievableVelCoast) / 2);
            timeToMaxAchievableAccelCoast = Math.Min(timeToMaxAchievableVel + timeToMaxAchievableVelCoast - 2 * timeToMaxAchievableAccel,
                    maxAchieveableVel / (maxJerk * timeToMaxAchievableAccel) - timeToMaxAchievableAccel);
            timeToAccelCoastDeccel = 2 * timeToMaxAchievableAccel + timeToMaxAchievableAccelCoast;

            displacementAccelCoastDeccel = timeToAccelCoastDeccel * maxJerk * timeToMaxAchievableAccel * (timeToMaxAchievableAccel + timeToMaxAchievableAccelCoast) / 2;
            timeToAccelCoastDeccelCoast = timeToAccelCoastDeccel +
                    (distance - 2 * displacementAccelCoastDeccel) / (maxJerk * timeToMaxAchievableAccel * (timeToMaxAchievableAccel + timeToMaxAchievableAccelCoast));
        }

        private double lookUpAccel(double time)
        {
            if (time > timeToAccelCoastDeccelCoast + timeToAccelCoastDeccel || time <= 0) return 0;
            if (time > timeToAccelCoastDeccelCoast + timeToAccelCoastDeccel - timeToMaxAchievableAccel) return -maxJerk * (timeToAccelCoastDeccelCoast + timeToAccelCoastDeccel - time);
            if (time > timeToAccelCoastDeccelCoast + timeToMaxAchievableAccel) return -maxJerk * timeToMaxAchievableAccel;
            if (time > timeToAccelCoastDeccelCoast) return -maxJerk * (time - timeToAccelCoastDeccelCoast);
            if (time > timeToAccelCoastDeccel) return 0;
            if (time > timeToMaxAchievableAccel + timeToMaxAchievableAccelCoast) return maxJerk * (timeToAccelCoastDeccel - time);
            if (time > timeToMaxAchievableAccel) return maxJerk * timeToMaxAchievableAccel;
            if (time > 0) return maxJerk * time;
            else return 0;
        }

        // Getters and Setters
        public double getMaxVel()
        {
            return maxVel;
        }

        public void setMaxVel(double maxVel)
        {
            this.maxVel = maxVel;
        }

        public double getMaxAccel()
        {
            return maxAccel;
        }

        public void setMaxAccel(double maxAccel)
        {
            this.maxAccel = maxAccel;
        }

        public double getMaxJerk()
        {
            return maxJerk;
        }

        public void setMaxJerk(double maxJerk)
        {
            this.maxJerk = maxJerk;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MotionProfile
{
    /// <summary>
    /// Used to calculate the velocity of the robot.
    /// </summary>
    class VelocityMap
    {
        public float vMax = 4;

        public double T1 = .4;
        public double T2 = .2;
        public double time = .01;
        public int FL1 = 40;
        public bool instVelocity = false;

        private int FL2 = 20;

        private double _distance = 100;
        private double _rampDistance = 1;

        private List<double> _filter1 = new List<double>();
        private List<double> _filter2 = new List<double>();

        private List<float> velocity = new List<float>();
        private List<float> position = new List<float>();

        private MotionProfile.Spline.CubicSpline  spline;

        //used to return the slowest that the robot will be going.
        public float getMinVelocity()
        {
            return velocity.Skip(1).First();
        }
        //used to set the over all distance of the path.
        public void setLength(float distance)
        {
            _distance = distance;
            buildFilter1();
            buildFilter2();

            velocity.Clear();
            position.Clear();
            velocity.Add(0);
            position.Add(0);
            for (int i = 1; i < FL2 + FL1; i++)
            {
                velocity.Add((float)((_filter1[i] + _filter2[i]) / (1 + FL2) * vMax));
                position.Add((float)((velocity[i] + velocity[i - 1]) / 2.0 * time + position.Last()));
            }

            _rampDistance = position.Last();

            if (position.Last() * 2 > _distance) _rampDistance = _distance / 2;

            spline = new Spline.CubicSpline(position.ToArray(), velocity.ToArray());
        }
        /// <summary>
        /// Returns the velocity the robot should be going at x distance
        /// </summary>
        public float getVelocity(float distance)
        {
            float[] d = {distance };

            if (distance < _rampDistance)
            { 
                // ramp up
                if (instVelocity )
                    return vMax;
                
                return spline.Eval(d, false).First();

            }
            else if (distance > this._distance - _rampDistance)
            {
                //ramp down
                d[0] = (float)(this._distance - distance);
                return spline.Eval(d, false).First();
            }

            //steady state
            return (float)vMax ;

        }

        public void buildFilter1()
        {

            _filter1.Clear();

            for (int i = 0; i <= FL2+FL1; i++)
            {
                double a = Math.Min(1, (time*i / this.time) / FL1);
                _filter1.Add(Math.Max(0, a));
            }
        }
        public void buildFilter2()
        {
            _filter2.Clear();
            for (int i = 1; i <= FL1+FL2 ; i++)
            {
                double aPos = _filter1.Skip(Math.Max(0,i-FL2)).Take(Math.Min(FL2,i)).Sum() ;
                _filter2.Add(aPos);

            }
        }

    }
}

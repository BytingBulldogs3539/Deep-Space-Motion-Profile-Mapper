using System;
using System.Collections.Generic;
using static MotionProfile.ControlPoint;

namespace MotionProfile.Spline
{
    public class VelocityGenerator
    {
        private double max_jerk = 10000;
        private double max_vel = 1000;
        private double max_acc = 2500;
        private double p_target = 100;
        private ControlPointDirection direction;
        private double dt=.01;
        private S_Curve[] s_curve = new S_Curve[7];
        public VelocityGenerator(double max_vel, double max_acc, double max_jerk, ControlPointDirection direction, double dt)
        {

            this.max_vel = max_vel;
            this.max_acc = max_acc;
            this.max_jerk = max_jerk;
            this.direction = direction;
            this.dt = dt;

            s_curve[0] = new S_Curve(0.0, 0.0, max_jerk, 0.0, 0.0, 0.0); // curve1
            s_curve[1] = new S_Curve(0.0, 0.0, 0.0, 0.0, 0.0, 0.0); // curve2
            s_curve[2] = new S_Curve(0.0, 0.0, -max_jerk, 0.0, 0.0, 0.0); // curve3
            s_curve[3] = new S_Curve(0.0, 0.0, 0.0, 0.0, 0.0, 0.0); // curve4
            s_curve[4] = new S_Curve(0.0, 0.0, -max_jerk, 0.0, 0.0, 0.0); // curve5
            s_curve[5] = new S_Curve(0.0, 0.0, 0.0, 0.0, 0.0, 0.0); // curve6
            s_curve[6] = new S_Curve(0.0, 0.0, max_jerk, 0.0, 0.0, 0.0); // curve7
        }

        public List<VelocityPoint> GeneratePoints(double distance)
        {
            List<VelocityPoint> list = new List<VelocityPoint>();
            this.p_target = distance;
            recalculate_s_curve();
            for (double time = 0; time < s_curve[6].t0 + s_curve[6].t; time += dt)
            {
                VelocityPoint point = new VelocityPoint();
                if (direction == ControlPointDirection.FORWARD)
                {
                    point.Pos = s_curve_pos(s_curve, time);
                    point.Vel = s_curve_vel(s_curve, time);
                    point.Acc = s_curve_acc(s_curve, time);
                    point.Jerk = s_curve_jerk(s_curve, time);
                }
                if (direction == ControlPointDirection.REVERSE)
                {
                    point.Pos = -s_curve_pos(s_curve, time);
                    point.Vel = -s_curve_vel(s_curve, time);
                    point.Acc = -s_curve_acc(s_curve, time);
                    point.Jerk = -s_curve_jerk(s_curve, time);
                }
                point.Time = time;
                list.Add(point);
            }
            return list;
        }

        
        private void recalculate_s_curve()
        {

            double t1 = 0;
            double t2 = 0;

            // Compute a constant jerk S-curve profile with starting
            // and ending velocity of 0 using max_jrk, max_acc, and
            // max_vel.  The total distance travelled by the curve will
            // be "p_target".  Maximum velocity may be reduced in order
            // to reach p_target.

            double p = 0;

            double test_vel_min = 0;
            double test_vel_max = max_vel;
            double test_vel = test_vel_max; // Start at max vel - probably the solution
            while ((test_vel_max - test_vel_min) > 5) // Solve to within 5 velocity units
            {
                if (Math.Pow(max_acc, 2) / max_jerk > test_vel)
                {
                    t1 = Math.Pow(test_vel / max_jerk, 0.5);
                    t2 = 0;
                }
                else
                {
                    t1 = max_acc / max_jerk;
                    t2 = (test_vel - max_acc * t1) / max_acc;
                }

                s_curve[0].t = s_curve[2].t = s_curve[4].t = s_curve[6].t = t1;
                s_curve[1].t = s_curve[5].t = t2;
                calculate_s_curve(s_curve);

                p = s_curve_pos(s_curve, s_curve[6].t0 + s_curve[6].t);

                if (p > p_target)
                {
                    // Need to reduce velocity
                    test_vel_max = test_vel;
                    test_vel = (test_vel_max + test_vel_min) / 2.0;
                }
                else
                {
                    if (p > (p_target - 5))
                    {
                        break;
                    }
                    else
                    {
                        // Increase velocity
                        test_vel_min = test_vel;
                        test_vel = (test_vel_max + test_vel_min) / 2.0;
                    }
                }
            }

            // Adjust the constant velocity section to reach the target position
            double t = (p_target - p) / test_vel;
            s_curve[3].t = Math.Max(t, 0);
            calculate_s_curve(s_curve);
        }
        private void calculate_s_curve(S_Curve[] curve)
        {
            S_Curve last_curve = curve[0];
            for (var i = 1; i < 7; i++)
            {
                curve[i].t0 = last_curve.t0 + last_curve.t;
                curve[i].a = s_curve_part_acc(last_curve, last_curve.t);
                curve[i].v = s_curve_part_vel(last_curve, last_curve.t);
                curve[i].p = s_curve_part_pos(last_curve, last_curve.t);
                last_curve = curve[i];
            }
        }
        private double s_curve_pos(S_Curve[] curve, double time)
        {
            int i = s_curve_index(curve, time);
            return s_curve_part_pos(curve[i], time - curve[i].t0);
        }
        private double s_curve_vel(S_Curve[] curve, double time)
        {
            int i = s_curve_index(curve, time);
            return s_curve_part_vel(curve[i], time - curve[i].t0);
        }
        private double s_curve_acc(S_Curve[] curve, double time)
        {
            int i = s_curve_index(curve, time);
            return s_curve_part_acc(curve[i], time - curve[i].t0);
        }
        private double s_curve_jerk(S_Curve[] curve, double time)
        {
            int i = s_curve_index(curve, time);
            return s_curve_part_jerk(curve[i], time - curve[i].t0);
        }

        private double s_curve_part_pos(S_Curve part, double time)
        {
            return part.p + part.v * time +
            1.0 / 2.0 * part.a * Math.Pow(time, 2) +
            1.0 / 6.0 * part.j * Math.Pow(time, 3);
        }

        private double s_curve_part_vel(S_Curve part, double time)
        {
            return part.v + part.a * time +
            1.0 / 2.0 * part.j * Math.Pow(time, 2);
        }

        private double s_curve_part_acc(S_Curve part, double time)
        {
            return part.a + part.j * time;
        }

        private double s_curve_part_jerk(S_Curve part, double time)
        {
            return part.j;
        }

        private int s_curve_index(S_Curve[] curve, double time)
        {
            int i;
            for (i = 1; i < 7; i++)
            {
                if (curve[i].t0 > time)
                {
                    break;
                }
            }
            return i - 1;
        }
    }
    public class VelocityPoint
    {
        private double pos;
        private double vel;
        private double acc;
        private double jerk;
        private double time;

        public double Pos
        {
            get
            {
                return pos;
            }
            set
            {
                pos = value;
            }
        }
        public double Vel
        {
            get
            {
                return vel;
            }
            set
            {
                vel = value;
            }
        }
        public double Acc
        {
            get
            {
                return acc;
            }
            set
            {
                acc = value;
            }
        }
        public double Jerk
        {
            get
            {
                return jerk;
            }
            set
            {
                jerk = value;
            }
        }
        public double Time
        {
            get
            {
                return time;
            }
            set
            {
                time = value;
            }
        }
    }
}

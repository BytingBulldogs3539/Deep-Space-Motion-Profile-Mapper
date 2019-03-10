using MotionProfile;
using MotionProfile.Spline;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MotionProfile.Spline
{
    class SplinePath
    {
        private static ParametricSpline spline;
        public static List<ControlPointSegment> GenSpline(List<ControlPoint> points, List<VelocityPoint> velocityPoints = null)
        {
            List<ControlPointSegment> splinePoints = new List<ControlPointSegment>();
            splinePoints.Clear();

            List<float> xs = new List<float>();
            List<float> ys = new List<float>();

            foreach (ControlPoint point in points)
            {
                xs.Add(point.X);
                ys.Add(point.Y);
            }

            List<CubicSplinePoint> outxs;
            List<CubicSplinePoint> outys;

            spline = new ParametricSpline(xs.ToArray(), ys.ToArray(), 100, out outxs, out outys);

            spline = new ParametricSpline(xs.ToArray(), ys.ToArray(), (int)spline.distance.Last() / 25, out outxs, out outys);


            if(velocityPoints==null)
            {
                for (int i = 0; i < outxs.Last().ControlPointNum + 1; i++)
                {
                    ControlPointSegment seg = new ControlPointSegment();

                    for (int x = 0; x < outxs.Count; x++)
                    {
                        if (outxs[x].ControlPointNum == i)
                        {
                            seg.points.Add(new SplinePoint(outxs[x].Y, outys[x].Y, i));
                        }

                    }
                    splinePoints.Add(seg);

                }
            }
            else
            {
                ControlPointSegment seg = new ControlPointSegment();
                int lastControlPointNum=0;
                foreach (VelocityPoint point in velocityPoints)
                {
                    Console.WriteLine(point.Pos);
                    SplinePoint spoint = spline.Eval((float)point.Pos);
                    if (spoint.ControlPointNum != lastControlPointNum)
                    {
                        splinePoints.Add(seg);
                        seg = new ControlPointSegment();
                    }
                    seg.points.Add(spoint);
                    lastControlPointNum = spoint.ControlPointNum;

                }
                splinePoints.Add(seg);

            }

            return splinePoints;

        }
        public static double getLength()
        {
            if (spline == null)
            {
                throw new NoSplineGenerated();
            }
            return spline.length;
        }

    }
    public class NoSplineGenerated: Exception
    {

        public NoSplineGenerated()
            : base("NoSplineGenerated: Spline must be generated before distance is found.")
        {
        }
    }
}


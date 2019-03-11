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
        private static double length = 0.0;
        public static List<ControlPointSegment> GenSpline(List<ControlPoint> points, List<VelocityPoint> velocityPoints = null)
        {

            float[] xOrig = new float[0];
            List<ControlPointSegment> splineSegments = new List<ControlPointSegment>();
            splineSegments.Clear();

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
            xOrig = spline.xSpline.xOrig;

            spline = new ParametricSpline(xs.ToArray(), ys.ToArray(), (int)(spline.length), out outxs, out outys);

            List<double> CPDistances = new List<double>();

            double d = 0;

            CPDistances.Add(d);

            for (int i = 0; i < outxs.Last().ControlPointNum + 1; i++)
            {

                for (int x = 1; x < outxs.Count; x++)
                {
                    if (outxs[x].ControlPointNum == i)
                    {
                        d += GetDistance(outxs[x-1].Y, outys[x-1].Y, outxs[x].Y, outys[x].Y);
                    }

                }
                CPDistances.Add(d);

            }

            Console.WriteLine(String.Join(", ", CPDistances.ToArray()));

            xs.Clear();
            ys.Clear();

            foreach (CubicSplinePoint point in outxs)
            {
                xs.Add(point.Y);
            }

            foreach (CubicSplinePoint point in outys)
            {
                ys.Add(point.Y);
            }

            spline = new ParametricSpline(xs.ToArray(), ys.ToArray(), 100, out outxs, out outys);

            

            if (velocityPoints==null)
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
                    splineSegments.Add(seg);

                }
            }
            else
            {
                ControlPointSegment seg = new ControlPointSegment();
                int lastControlPointNum=0;
                double distance=0;




                for (int i = 1; i < velocityPoints.Count; i++)
                {
                    SplinePoint spoint = spline.Eval((float)velocityPoints[i].Pos);
                    SplinePoint lastpoint = spline.Eval((float)velocityPoints[i - 1].Pos);

                    distance += GetDistance(spoint.X, spoint.Y, lastpoint.X, lastpoint.Y);

                    for (int dis = 1; dis < CPDistances.Count; dis++)
                    {
                        if (distance > CPDistances[dis - 1] && distance < CPDistances[dis])
                        {
                            spoint.ControlPointNum = dis - 1;
                            Console.WriteLine(dis - 1);

                            if (spoint.ControlPointNum != lastControlPointNum)
                            {
                                splineSegments.Add(seg);
                                seg = new ControlPointSegment();
                            }
                            seg.points.Add(spoint);
                            lastControlPointNum = dis - 1;


                        }

                    }
                }
                splineSegments.Add(seg);

            }
            return splineSegments;

        }
        private static double GetDistance(double x1, double y1, double x2, double y2)
        {
            return Math.Sqrt(Math.Pow((x2 - x1), 2) + Math.Pow((y2 - y1), 2));
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


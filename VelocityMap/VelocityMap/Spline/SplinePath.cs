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

        public static List<ControlPointSegment> GenSpline(List<ControlPoint> points)
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

            ParametricSpline spline = new ParametricSpline(xs.ToArray(), ys.ToArray(), 100, out outxs, out outys);

            spline = new ParametricSpline(xs.ToArray(), ys.ToArray(), (int)spline.distance.Last()/50, out outxs, out outys);

            for (int i = 0; i < outxs.Last().ControlPointNum+1; i++)
            {
                ControlPointSegment seg = new ControlPointSegment();

                for (int x = 0; x < outxs.Count; x++)
                {
                    if (outxs[x].ControlPointNum == i)
                    {
                        seg.points.Add(new SplinePoint(outxs[x].Y, outys[x].Y));
                    }
                    
                }
                splinePoints.Add(seg);

            }



            return splinePoints;

        }

    }
}


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MotionProfile.Spline
{
    public class OffsetSegment
    {
        public enum Point
        {
            A,
            B,
        };

        public SplinePoint A;
        public SplinePoint B;
        public double length = 0;
        public double dx, dy;

        public OffsetSegment(SplinePoint a, SplinePoint b)
        {
            A = a;
            B = b;

            dx = b.X - a.X;
            dy = b.Y - a.Y;
            length = Math.Sqrt(dx * dx + dy * dy);
        }
        public SplinePoint perp(float offset)
        {
            SplinePoint pt = new SplinePoint(0, 0, B.ControlPointNum);

            if (length > 0)
            {

                pt.X = (float)(B.X + offset * (B.Y - A.Y) / length);
                pt.Y = (float)(B.Y - offset * (B.X - A.X) / length);
            }
            return pt;
        }
    }
}

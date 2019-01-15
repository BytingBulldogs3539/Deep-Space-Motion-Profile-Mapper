using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace MotionProfile
{
    /// <summary>
    /// Part of the path.
    /// </summary>
    public class Segment
    {
        public enum Point
        {
            A,
            B,
        };

        public PointF A;
        public PointF B;
        public double length = 0;
        public double dx, dy;

        public Segment(PointF a, PointF b)
        {
            A = a;
            B = b;

            dx = b.X - a.X;
            dy = b.Y - a.Y;
            length = Math.Sqrt(dx * dx + dy * dy);
        }
        public PointF perp(float offset)
        {
            PointF pt = new PointF(0,0);

            if (length > 0)
            {

                pt.X = (float)(B.X + offset * (B.Y - A.Y) / length);
                pt.Y = (float)(B.Y - offset * (B.X - A.X) / length);
            }
            return pt;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MotionProfile.Spline
{
    class ControlPointSegment
    {
        public List<SplinePoint> points = new List<SplinePoint>();
        public int PathNum;
        public float velocity;
    }
}

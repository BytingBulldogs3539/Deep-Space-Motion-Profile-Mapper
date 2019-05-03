using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MotionProfile.ControlPoint;

namespace MotionProfile.Spline
{
    public class DirectionSegment
    {
        ControlPointDirection direction;
        List<ControlPointSegment> segments;
    }
}

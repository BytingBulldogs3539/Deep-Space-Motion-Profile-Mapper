using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VelocityMap
{
    class ControlPoint
    {
        public Boolean direction = false;
        public int pointNumber;
        public int velocity;
        public System.Drawing.PointF[] point;

        public ControlPoint(int pointNumber)
        {
            this.pointNumber = pointNumber;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MotionProfile.Spline
{
    public class CubicSplinePoint
    {
        float y;
        int Num;

        public float Y
        {
            get
            {
                return y;
            }

            set
            {
                y = value;
            }
        }
        public int ControlPointNum
        {
            get
            {
                return Num;
            }

            set
            {
                Num = value;
            }
        }

        public CubicSplinePoint(float y, int ControlPointNum)
        {
            this.Num = ControlPointNum;
            this.y = y;
        }
    }
}

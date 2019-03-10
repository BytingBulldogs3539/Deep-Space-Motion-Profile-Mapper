using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MotionProfile.Spline
{
    public class SplinePoint
    {
        float x;
        float y;
        int controlPointNum;


        public float X
        {
            get
            {
                return x;
            }

            set
            {
                x = value;
            }
        }

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
                return controlPointNum;
            }
        }


        public SplinePoint(float x, float y, int controlPointNum)
        {
            this.x = x;
            this.y = y;
            this.controlPointNum = controlPointNum;
        }

    }
}

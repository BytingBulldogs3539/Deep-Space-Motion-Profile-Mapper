using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MotionProfile.ControlPoint;

namespace MotionProfile.Spline
{
    public class SplinePoint
    {
        double x;
        double y;
        int controlPointNum;
        ControlPointDirection direction;


        public double X
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

        public double Y
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
            set
            {
                controlPointNum = value;
            }
        }

        public ControlPointDirection Direction
        {
            get
            {
                return direction;
            }

            set
            {
                direction = value;
            }
        }


        public SplinePoint(double x, double y, int controlPointNum)
        {
            this.x = x;
            this.y = y;
            this.controlPointNum = controlPointNum;
        }

    }
}

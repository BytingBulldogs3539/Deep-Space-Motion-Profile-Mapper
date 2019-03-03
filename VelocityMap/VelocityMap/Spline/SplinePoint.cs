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

        public SplinePoint(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

    }
}

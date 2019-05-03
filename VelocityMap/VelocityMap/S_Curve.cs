using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MotionProfile
{
    public class S_Curve
    {
        public double t0;
        public double t;
        public double j;
        public double a;
        public double v;
        public double p;
        public S_Curve(double t0, double t, double j, double a, double v, double p)
        {
            this.t0 = t0;
            this.t = t;
            this.j = j;
            this.a = a;
            this.v = v;
            this.p = p;
        }
    }
}

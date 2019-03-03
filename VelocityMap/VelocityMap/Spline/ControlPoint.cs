using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MotionProfile
{
    public class ControlPoint
    {
        float x;
        float y;
        ControlPointDirection direction;
        int graphIndex;
        float velocity;
        Boolean selected;

        public enum ControlPointDirection
        {
            FORWARD,
            REVERSE
        }
        public ControlPoint(float x, float y, ControlPointDirection direction, Boolean selected = false)
        {
            this.x = x;
            this.y = y;
            this.direction = direction;
            this.selected = selected;
        }
        public ControlPointDirection getDirection()
        {
            return this.direction;
        }

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

        public float Velocity
        {
            get
            {
                return velocity;
            }

            set
            {
                velocity = value;
            }
        }

        public Boolean Selected
        {
            get
            {
                return selected;
            }

            set
            {
                selected = value;
            }
        }

        public ControlPointDirection Direction
        {
            get
            {
                return this.direction;
            }

            set
            {
                direction = value;
            }
        }



        public int getGraphIndex()
        {
            return this.graphIndex;
        }
        public void setGraphIndex(int graphIndex)
        {
            this.graphIndex = graphIndex;
        }
        public Boolean isReverse()
        {
            if (direction == ControlPointDirection.FORWARD)
            {
                return false;
            }
            return true;
        }
        public Boolean isForward()
        {
            if (direction == ControlPointDirection.FORWARD)
            {
                return true;
            }
            return false;
        }
        public String toString()
        {
            return X+", "+ Y + ", " + Direction;
        }
    }
}

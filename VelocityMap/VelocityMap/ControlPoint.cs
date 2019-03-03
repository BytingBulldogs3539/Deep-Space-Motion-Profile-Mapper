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
        Direction direction;
        int graphIndex;

        public enum Direction
        {
            FORWARD,
            REVERSE
        }
        public ControlPoint(float x, float y, Direction direction)
        {
            this.x = x;
            this.y = y;
            this.direction = direction;
        }
        public Direction getDirection()
        {
            return this.direction;
        }
        public float getX()
        {
            return this.x;
        }
        public float getY()
        {
            return this.y;
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
            if (direction == Direction.FORWARD)
            {
                return false;
            }
            return true;
        }
        public Boolean isForward()
        {
            if (direction == Direction.FORWARD)
            {
                return true;
            }
            return false;
        }
    }
}

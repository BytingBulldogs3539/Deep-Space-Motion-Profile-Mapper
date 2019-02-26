using System;

namespace VelocityMap
{
	/// <summary>
	/// This class is used to store all of the individual points used to calculate angle.
	/// </summary>

	public enum Direction
	{
		FORWARD,
		REVERSE
	}

	public class SplinePoint
	{    
		public float x, y;

		public Direction direction;

		public int pointNumber;

		public SplinePoint(float x, float y, Direction direction, int pointNumber)
		{
			this.x = x;
			this.y = y;
			this.direction = direction;
			this.pointNumber = pointNumber;
		}
	}

	class ControlPoint
	{
		public Direction direction = Direction.FORWARD;

		public int velocity;
		
		public System.Drawing.PointF[] point;

		public int[] pointnumbers;

		public ControlPoint()
		{
		}
	}
}
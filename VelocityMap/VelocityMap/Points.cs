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
		private float x, y;

		private Direction direction;

		private int pointNumber;

		public SplinePoint(float x, float y, Direction direction, int pointNumber)
		{
			this.x = x;
			this.y = y;
			this.direction = direction;
			this.pointNumber = pointNumber;
		}

		public float GetX()
		{
			return x;
		}

		public float GetY()
		{
			return y;
		}

		public Direction GetDirection()
		{
			return direction;
		}

		public int GetPointNumber()
		{
			return pointNumber;
		}
	}

	class ControlPoint
	{
		public Direction direction;
		
		public System.Drawing.PointF[] point;

		public int[] pointnumbers;

		public ControlPoint()
		{
			direction = Direction.FORWARD;
		}

		public void SetDirection(Direction direction)
		{
			this.direction = direction;
		}
	}
}
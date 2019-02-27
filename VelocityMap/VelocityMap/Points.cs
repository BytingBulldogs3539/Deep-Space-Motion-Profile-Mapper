using System;
using System.Drawing;

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
		private Direction direction;

		private PointF[] drawingPoints;

		private int[] pointNumbers;

		public ControlPoint()
		{
			direction = Direction.FORWARD;
		}

		public ControlPoint(Direction direction, PointF[] drawingPoints, int[] pointNumbers)
		{
			this.direction = direction;
			this.drawingPoints = drawingPoints;
			this.pointNumbers = pointNumbers;
		}

		public void SetDirection(Direction direction)
		{
			this.direction = direction;
		}

		public Direction GetDirection()
		{
			return direction;
		}

		public int[] GetPointNumbers()
		{
			return pointNumbers;
		}

		public int GetPointNumber(int i)
		{
			return pointNumbers[i];
		}

		public PointF[] GetDrawingPoints()
		{
			return drawingPoints;
		}

		public PointF GetDrawingPoint(int i)
		{
			return drawingPoints[i];
		}
	}
}
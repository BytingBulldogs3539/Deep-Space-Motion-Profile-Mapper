using System;

namespace VelocityMap
{
	/// <summary>
	/// This class is used to store all of the individual points used to calculate angle.
	/// </summary>
	public class Point
	{    
		public float x;
		public float y;
		public Boolean direction;
		public int pointNumber;
		public Point(float x, float y, Boolean direction, int pointNumber)
		{
			this.x = x;
			this.y = y;
			this.direction = direction;
			this.pointNumber = pointNumber;
		}
	}
}
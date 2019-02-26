using System;
using System.Collections.Generic;
using System.Linq;
using VelocityMap;

namespace MotionProfile
{
	class Trajectory : List<Path>
	{
		private double CONVERT = 180.0 / Math.PI;

		/// <summary>
		/// Returns the absolute fastest that the robot will be going.
		/// </summary>
		public double GetMaxVelocity()
		{
			double max = 0;
			foreach (Path p in this)
			{
				if (p.velocityMap.vMax > max)
					max = p.velocityMap.vMax;
			}
			return max;
		}

		/// <summary>
		/// returns a time profile that is ment to match the other profile so you know how long each point should take.
		/// </summary>
		public float[] GetTimeProfile()
		{
			float offset = 0;
			List<float> values = new List<float>();
			foreach (Path p in this)
			{
				foreach (float f in p.GetTimeProfile())
				{
					values.Add(f + offset);
				}
				offset = values.Last();
			}
			return values.ToArray<float>();
		}

		/// <summary>
		/// Returns the distance profile which is the points that the talon should be hitting.
		/// </summary>
		public float[] GetDistanceProfile()
		{
			float offset = 0;
			List<float> values = new List<float>();
			foreach (Path p in this)
			{
				foreach (float f in p.GetDistanceProfile())
				{
					values.Add(f + offset);
				}
				offset = values.Last();
			}
			return values.ToArray<float>();
		}

		/// <summary>
		/// Returns the velocity points that the robot should be hitting.
		/// </summary>
		public float[] GetVelocityProfile()
		{
			List<float> values = new List<float>();
			foreach (Path p in this)
			{
				if (p.direction == Direction.FORWARD)
				{
					values.AddRange(p.GetVelocityProfile());
				}
				else
				{
					foreach (float f in p.GetVelocityProfile())
						values.Add(-f);
				}
			}

			return values.ToArray<float>();
		}

		/// <summary>
		/// Returns the headings that the robot should be hitting.
		/// </summary>
		public float[] GetHeadingProfile()
		{
			List<float> headings = new List<float>();

			List<SplinePoint> pointList = new List<SplinePoint>();


			foreach (ControlPoint p in BuildPath(0))
			{
				for(int i =0; i<p.point.Length; i++)
				{
					System.Drawing.PointF p1 = p.point[i];
					pointList.Add(new SplinePoint(p1.X, p1.Y, p.direction, p.pointnumbers[i]));
				}
			}

			List<ControlPoint> cp = BuildPath(0);
			float startAngle = FindStartAngle(pointList[1].x, pointList[0].x, pointList[1].y, pointList[0].y);

			for (int i = 0; i < (pointList.Count - 2); i++) //for not zeroing the angle after each path.
			{

				if (i == 0)
				{
					headings.Add(FindStartAngle(pointList[i + 1].x, pointList[i].x, pointList[i + 1].y, pointList[i].y));
				}
				else
				{

					headings.Add(FindAngleChange(pointList[i + 1].x, pointList[i].x, pointList[i + 1].y, pointList[i].y, headings[headings.Count - 1], pointList,i));
				}
			}

			for (int i = 0; i < (pointList.Count - 2); i++) //converts the values from raw graph angles to angles the robot can use.
			{

				float angle = headings[i];
				angle = (angle - startAngle);
				angle = -angle;

				headings[i] = angle;
			}

			for (int i = 0; i < (pointList.Count - 2); i++) //converts the values from raw graph angles to angles the robot can use.
			{
				if (i > 0)
				{
					float ang = headings[i];
					float prevAngle = headings[i - 1];
					float angleChange = ang - prevAngle;
					if (angleChange > 300) angleChange -= 360;
					if (angleChange < -300) angleChange += 360;

					float angle = (prevAngle + angleChange);

					headings[i] = angle;
				}
			}
			return headings.ToArray<float>();
		}

		//public List<int> GetControlPointNumberProfile()
		//{
		//	List<int> controlPoints = new List<int>();
		//	foreach (ControlPoint p in BuildPath(0))
		//	{
		//		for (int i = 0; i < p.point.Length; i++)
		//		{
		//			System.Drawing.PointF p1 = p.point[i];
		//			controlPoints.Add(p.pointnumbers[i]);
		//		}
		//	}
		//	return controlPoints;
		//}

		/// <summary>
		/// Returns the velocity profile of the right or left wheel while using the offset from the middle of the robot.
		/// </summary>
		public List<float> GetOffsetVelocityProfile(int offset)
		{
			List<float> values = new List<float>();
			foreach (Path p in this)
				if (p.direction == Direction.FORWARD)
				{
					values.AddRange(p.GetOffsetVelocityProfile(offset));
				}
				else
				{
					foreach (float f in p.GetOffsetVelocityProfile(offset))
						values.Add(-f);
				}

			return values;
		}

		/// <summary>
		/// Returns the distance profile of the right or left wheel while using the offset from the middle of the robot.
		/// </summary>
		public List<float> GetOffsetDistanceProfile(int offset)
		{
			List<float> values = new List<float>();
			foreach (Path p in this)
				if (p.direction == Direction.FORWARD)
				{
					values.AddRange(p.GetOffsetDistanceProfile(offset));
				}
				else
				{
					foreach (float f in p.GetOffsetDistanceProfile(offset))
						values.Add(-f);
				}

			return values;
		}

		//HARDLY USED
		public void Test()
		{
			float dx = 0;
			float dy = 0;
			foreach (Path p in this)
			{
				p.TestCreate(dx,dy);
				dx = p.dx;
				dy = p.dy;
			}
		}
		/// <summary>
		/// create the profile with or without an offset
		/// </summary>
		public void Create(int offset = 0)
		{
			foreach (Path p in this)
			{
				if (offset > 0)
					p.CreateThrottled(offset);
				else
					p.Create();
			}
		}
	
		/// <summary>
		/// Build the main path or the path with a offset.
		/// </summary>
		/// <param name="offset"></param>
		/// <returns></returns>
		public List<ControlPoint> BuildPath(int offset = 0)
		{
			List<ControlPoint> values = new List<ControlPoint>();
			foreach (Path p in this)
			{
				if (offset != 0)
				{
					if (p.direction == Direction.REVERSE)
						offset = -offset;
					ControlPoint p2 = new ControlPoint();
					p2.point = p.BuildOffsetPoints(offset).ToArray<System.Drawing.PointF>();
					p2.direction = p.direction;
					p2.pointnumbers = p.FindPointControlPoints().ToArray();

					values.Add(p2);
				}
				else
				{
					ControlPoint p3 = new ControlPoint();
					p3.point = p.BuildPath().ToArray<System.Drawing.PointF>();
					p3.pointnumbers = p.FindPointControlPoints().ToArray();

					p3.direction = p.direction;
					
					values.Add(p3);
				}
			}
			return values;
		}

		/// <summary>
		/// Find the angle of the first point that is used to calculate the rest of the angles.
		/// </summary>
		private float FindStartAngle(double x2, double x1, double y2, double y1)
		{
			float ang = 0;
			float chx = (float)(x2 - x1);
			float chy = (float)(y2 - y1);
			if (chy == 0)
			{
				if (chx >= 0) ang = 0;
				else ang = 180;
			}
			else if (chy > 0)
			{                         // X AND Y ARE REVERSED BECAUSE OF MOTION PROFILER STUFF
				if (chx > 0)
				{
					// positive x, positive y, 90 - ang, quad 1
					ang = (float)(90 - CONVERT * (Math.Atan(chx / chy)));
					//ang = (float)(CONVERT * Math.Atan(chx / chy));
					//ang = 1; // represents quadrants.
				}
				else
				{
					// positive x, negative y, 90 + ang, quad 2
					ang = (float)(90 - CONVERT * (Math.Atan(chx / chy)));
					//ang = (float)(CONVERT * Math.Atan(chx / chy));
					//ang = 2;
				}
			}
			else
			{
				if (chx > 0)
				{
					// negative x, positive y, 270 + ang, quad 4
					ang = (float)(270 - CONVERT * (Math.Atan(chx / chy)));
					//ang = (float)(CONVERT * Math.Atan(chx / chy));
					//ang = 4;
				}
				else
				{
					// negative x, negative y, 270 - ang, quad 3
					ang = (float)(270 - CONVERT * (Math.Atan(chx / chy)));
					//ang = (float)(CONVERT * Math.Atan(chx / chy));
					//ang = 3;
				}
			}
			return ang;
		}

		/// <summary>
		/// Returns the angle of this point by adding the angle change to the prevAngle.
		/// </summary>
		private float FindAngleChange(double x2, double x1, double y2, double y1, float prevAngle, List<SplinePoint> pointList, int i)
		{
			float ang = 0;
			float chx = (float)(x2 - x1);
			float chy = (float)(y2 - y1);
			if (chy == 0)
			{
				if (chx >= 0) ang = 0;
				else ang = 180;
			}
			else if (chy > 0)
			{                         // X AND Y ARE REVERSED BECAUSE OF MOTION PROFILER STUFF
				if (chx > 0)
				{
					// positive x, positive y, 90 - ang, quad 1
					ang = (float)(90 - CONVERT * (Math.Atan(chx / chy)));
					//ang = (float)(CONVERT * Math.Atan(chx / chy));
					//ang = 1; // represents quadrants.
				}
				else
				{
					// positive x, negative y, 90 + ang, quad 2
					ang = (float)(90 - CONVERT * (Math.Atan(chx / chy)));
					//ang = (float)(CONVERT * Math.Atan(chx / chy));
					//ang = 2;
				}
			}
			else
			{
				if (chx > 0)
				{
					// negative x, positive y, 270 + ang, quad 4
					ang = (float)(270 - CONVERT * (Math.Atan(chx / chy)));
					//ang = (float)(CONVERT * Math.Atan(chx / chy));
					//ang = 4;
				}
				else
				{
					// negative x, negative y, 270 - ang, quad 3
					ang = (float)(270 - CONVERT * (Math.Atan(chx / chy)));
					//ang = (float)(CONVERT * Math.Atan(chx / chy));
					//ang = 3;
				}
			}

			Direction direction;
			if (i == pointList.Count - 1)
			{
				direction = pointList[i].direction;
			}
			else
			{
				direction = pointList[i + 1].direction;
			}

			if (direction == Direction.REVERSE)
			{
				int add = 0;
				if (ang > 0)
					add = -180;
				if (ang < 0)
					add = 180;
				ang = ang + add;
			}

			float angleChange = ang - prevAngle;
			if (angleChange > 300) angleChange -= 360;
			if (angleChange < -300) angleChange += 360;
			return (prevAngle + angleChange);
		}
	}
}
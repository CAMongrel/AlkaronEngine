// Project: Hellspawn, File: CollisionDataConvexHull.cs
// Namespace: AlkaronEngine.Assets.Meshes.Collision, Class: CollisionDataConvexHull
// Path: D:\Projekte\Hellspawn\Code\Hellspawn\Assets\Meshes\Collision, Author: Henning
// Code lines: 217, Size of file: 6,63 KB
// Creation date: 23.04.2010 21:22
// Last modified: 10.05.2010 13:00
// Generated with Commenter by abi.exDream.com

#region Using directives
using AlkaronEngine.Scene;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
#endregion

namespace AlkaronEngine.Assets.Meshes.Collision
{
	/// <summary>
	/// Collision data convex hull
	/// </summary>
	class CollisionDataConvexHull : CollisionData
	{
		/// <summary>
		/// Collision vertices
		/// </summary>
		Vector2[] collisionVertices;

		#region CollisionDataConvexHull
		/// <summary>
		/// Create collision data convex hull
		/// </summary>
		public CollisionDataConvexHull()
		{
			Type = CollisionType.ConvexHull;
		} // CollisionDataConvexHull()
		#endregion

		#region FromStream
		/// <summary>
		/// From stream
		/// </summary>
		/// <param name="reader">Reader</param>
		/// <returns>Collision data convex hull</returns>
		public static CollisionDataConvexHull FromStream(BinaryReader reader)
		{
			CollisionDataConvexHull res = new CollisionDataConvexHull();

			int vertCount = reader.ReadInt32();
			res.collisionVertices = new Vector2[vertCount];
			for (int i = 0; i < vertCount; i++)
			{
				res.collisionVertices[i] = new Vector2(
					reader.ReadSingle(), reader.ReadSingle());
			} // for (int)

			return res;
		} // FromStream(reader)
		#endregion

		#region FromMesh
		/// <summary>
		/// From mesh
		/// </summary>
		/// <param name="points2D">Points 2D</param>
		/// <param name="points3D">Points 3D</param>
		/// <returns>Collision data convex hull</returns>
		public static CollisionDataConvexHull FromMesh(Vector2[] points2D, Vector3[] points3D)
		{
			CollisionDataConvexHull res = new CollisionDataConvexHull();
			res.collisionVertices = GetConvexHull(points2D);
			return res;
		} // FromMesh(points2D, points3D)
		#endregion

		#region GetConvexHull
		/// <summary>
		/// Number points
		/// </summary>
		static int NumPoints;
		/// <summary>
		/// Points
		/// </summary>
		static Vector2[] points;
		/// <summary>
		/// Used points
		/// </summary>
		static int[] usedPoints;
		/// <summary>
		/// Minimum point
		/// </summary>
		static int minPoint, maxPoint, currPoint;

		/// <summary>
		/// Get convex hull
		/// </summary>
		/// <param name="verts">Verts</param>
		/// <returns></returns>
		static Vector2[] GetConvexHull(Vector2[] verts)
		{
			NumPoints = verts.Length;
			points = verts;
			usedPoints = new int[10000];
			for (int i = 0; i < usedPoints.Length; i++)
			{
				usedPoints[i] = -1;
			} // for (int)
			List<Vector2> result = new List<Vector2>();

			jarvis();

			for (int i = 0; i < usedPoints.Length; i++)
			{
				if (usedPoints[i] == -1)
					break;

				result.Add(points[usedPoints[i]]);
			} // for (int)

			return result.ToArray();
		} // GetConvexHull(verts)

		/// <summary>
		/// Jarvis
		/// </summary>
		static void jarvis()
		{
			int maxAngle = 0, minAngle = 0;
			/// <summary>
			/// Maximum point
			/// </summary>
			maxPoint = 0;

			for (int k = 1; k < NumPoints; k++)
				if (points[k].Y > points[maxPoint].Y)
					maxPoint = k;

			//cout<<"Max: "<<points[maxPoint].x<<" "<<points[maxPoint].y<<endl;
			minPoint = 0;

			for (int j = 1; j < NumPoints; j++) //FOR ALL POINTS IN THE SET, FIND MIN POINT
				if (points[j].Y < points[minPoint].Y)
					minPoint = j;

			//cout<<"Min: "<<points[minPoint].x<<" "<<points[minPoint].y<<endl;
			addUsedPoint(minPoint); //ADD MIN POINT TO LIST
			/// <summary>
			/// Curr point
			/// </summary>
			currPoint = minPoint;

			while (currPoint != maxPoint) //BUILD LEFT-HAND SIDE OF CONVEX HULL
			{
				maxAngle = currPoint;

				for (int y = 0; y < NumPoints; y++) //LOOP FOR ALL POINTS IN THE SET, FIND POINT WITH LOWEST RELATIVE ANGLE
				{
					//cout<<"Angles: "<<findAngle(points[currPoint].x,points[currPoint].y,points[maxAngle].x,points[maxAngle].y)<<" "<<findAngle(points[currPoint].x,points[currPoint].y,points[y].x,points[y].y)<<endl;
					if (findAngle(points[currPoint].X, points[currPoint].Y, points[maxAngle].X, points[maxAngle].Y) < findAngle(points[currPoint].X, points[currPoint].Y, points[y].X, points[y].Y) && (notUsed(y) || y == maxPoint) && findAngle(points[currPoint].X, points[currPoint].Y, points[y].X, points[y].Y) <= 270)
					{
						maxAngle = y;
					} // if (findAngle)
				} // for (int)

				currPoint = maxAngle;
				addUsedPoint(currPoint); //ADD NEW POINT TO FINAL PERIMETER LIST

			} // while (currPoint)

			// currentIndex => currently used index + 1
			int currentIndex = getCurrentIndex();
			currPoint = minPoint;

			while (currPoint != maxPoint) //BUILD RIGHT-HAND SIDE OF CONVEX HULL
			{
				minAngle = maxPoint;

				for (int y = 0; y < NumPoints; y++) //LOOP FOR ALL POINTS IN THE SET, FIND POINT WITH GREATEST RELATIVE ANGLE
				{
					//cout<<"Angles: "<<findAngle(points[currPoint].X,points[currPoint].Y,points[minAngle].X,points[minAngle].Y)<<" "<<findAngle(points[currPoint].X,points[currPoint].Y,points[y].X,points[y].Y)<<endl;
					if (findAngle(points[currPoint].X, points[currPoint].Y, points[minAngle].X, points[minAngle].Y) > findAngle(points[currPoint].X, points[currPoint].Y, points[y].X, points[y].Y) && (notUsed(y) || y == maxPoint) && findAngle(points[currPoint].X, points[currPoint].Y, points[y].X, points[y].Y) >= 90)
					{
						minAngle = y;
					} // if (findAngle)
				} // for (int)

				currPoint = minAngle;
				//cout<<"Selected Point: "<<currPoint<<endl<<endl;
				addUsedPoint(currPoint); //ADD NEW POINT TO FINAL PERIMETER LIST
			} // while (currPoint)

			int endIndex = getCurrentIndex();
			int loopEndIndex = currentIndex + ((endIndex - currentIndex) / 2);

			// Reverse list after we had the split
			currPoint = currentIndex;
			for (int i = currentIndex; i < loopEndIndex; i++)
			{
				int tmpVec = usedPoints[i];
				usedPoints[i] = usedPoints[endIndex - (i - currentIndex) - 1];
				usedPoints[endIndex - (i - currentIndex) - 1] = tmpVec;
			} // for (int)
		} // jarvis()


		/// <summary>
		/// Find angle
		/// </summary>
		/// <param name="x1">X 1</param>
		/// <param name="y1">Y 1</param>
		/// <param name="x2">X 2</param>
		/// <param name="y2">Y 2</param>
		/// <returns>Double</returns>
		static double findAngle(double x1, double y1, double x2, double y2)
		{
			double deltaX = x2 - x1;
			double deltaY = y2 - y1;

			if (deltaX == 0 && deltaY == 0)
				return 0;

			double angle = Math.Atan(deltaY / deltaX) * (180.0 / 3.141592);

			//TAKE INTO ACCOUNT QUADRANTS, VALUE: 0°-360°
			if (deltaX >= 0 && deltaY >= 0)
				angle = 90 + angle;
			else if (deltaX >= 0 && deltaY < 0)
				angle = 90 + angle;
			else if (deltaX < 0 && deltaY > 0)
				angle = 270 + angle;
			else if (deltaX < 0 && deltaY <= 0)
				angle = 270 + angle;

			return angle;
		} // findAngle(x1, y1, x2)


		/// <summary>
		/// Not used
		/// </summary>
		/// <param name="y">Y</param>
		/// <returns>Bool</returns>
		static bool notUsed(int y)
		{
			for (int i = 0; i < NumPoints; i++) //FOR ALL POINTS IN THE SET, CHECK IF INDEX HAS ALREADY BEEN ADDED
				if (y == usedPoints[i])
					return false;

			return true;
		} // notUsed()

		/// <summary>
		/// Get current index
		/// </summary>
		/// <returns>Int</returns>
		static int getCurrentIndex()
		{
			int i = 0;
			while (usedPoints[i] != -1) //FIND NEXT FREE SPOT IN USEDPOINTS[], AND STORE INDEX
				i++;

			return i;
		} // getCurrentIndex()

		/// <summary>
		/// Add used point
		/// </summary>
		/// <param name="index">Index</param>
		static void addUsedPoint(int index)
		{
			int i = 0;
			while (usedPoints[i] != -1) //FIND NEXT FREE SPOT IN USEDPOINTS[], AND STORE INDEX
				i++;

			usedPoints[i] = index;
			return;
		} // addUsedPoint(index)
		#endregion

		#region ToStream
		/// <summary>
		/// To stream
		/// </summary>
		/// <param name="writer">Writer</param>
		public override void ToStream(BinaryWriter writer)
		{
			writer.Write(collisionVertices.Length);
			for (int i = 0; i < collisionVertices.Length; i++)
			{
				writer.Write(collisionVertices[i].X);
				writer.Write(collisionVertices[i].Y);
			} // for (int)
		}
		#endregion // ToStream(writer)

		#region CreateComponent
		/// <summary>
		/// Create component
		/// </summary>
		/// <param name="owner">Owner</param>
		/// <returns>Collision component</returns>
		public override CollisionComponent CreateComponent(Actor owner)
		{
			CollisionComponentConvexHull res = new CollisionComponentConvexHull(owner);
			res.CreatePhysicsBody(collisionVertices);
			return res;
		}
		#endregion // CreateComponent(owner)
	} // class CollisionDataConvexHull
} // namespace AlkaronEngine.Assets.Meshes.Collision

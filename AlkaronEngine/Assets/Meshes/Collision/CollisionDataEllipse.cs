// Project: Hellspawn, File: CollisionDataEllipse.cs
// Namespace: AlkaronEngine.Assets.Meshes.Collision, Class: CollisionDataEllipse
// Path: D:\Projekte\Hellspawn\Code\Hellspawn\Assets\Meshes\Collision, Author: Henning
// Code lines: 109, Size of file: 2,97 KB
// Creation date: 23.04.2010 21:22
// Last modified: 10.05.2010 12:59
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
	/// Collision data ellipse
	/// </summary>
	class CollisionDataEllipse : CollisionData
	{
		/// <summary>
		/// Radius x
		/// </summary>
		float radiusX;
		/// <summary>
		/// Radius y
		/// </summary>
		float radiusY;

		/// <summary>
		/// Create collision data ellipse
		/// </summary>
		public CollisionDataEllipse()
		{
			Type = CollisionType.BoundingEllipse;
		} // CollisionDataEllipse()

		/// <summary>
		/// From stream
		/// </summary>
		/// <param name="reader">Reader</param>
		/// <returns>Collision data ellipse</returns>
		public static CollisionDataEllipse FromStream(BinaryReader reader)
		{
			CollisionDataEllipse res = new CollisionDataEllipse();
			res.radiusX = reader.ReadSingle();
			res.radiusY = reader.ReadSingle();
			return res;
		} // FromStream(reader)

		/// <summary>
		/// From mesh
		/// </summary>
		/// <param name="points2D">Points 2D</param>
		/// <param name="points3D">Points 3D</param>
		/// <returns>Collision data ellipse</returns>
		public static CollisionDataEllipse FromMesh(Vector2[] points2D, Vector3[] points3D)
		{
			CollisionDataEllipse res = new CollisionDataEllipse();

			float minX = float.MaxValue;
			float minY = float.MaxValue;
			float maxX = float.MinValue;
			float maxY = float.MinValue;

			for (int i = 0; i < points2D.Length; i++)
			{
				if (points2D[i].X < minX)
					minX = points2D[i].X;
				if (points2D[i].X > maxX)
					maxX = points2D[i].X;

				if (points2D[i].Y < minY)
					minY = points2D[i].Y;
				if (points2D[i].Y > maxY)
					maxY = points2D[i].Y;
			} // for (int)

			res.radiusX = (maxX - minX) * 0.5f;
			res.radiusY = (maxY - minY) * 0.5f;

			return res;
		} // FromMesh(points2D, points3D)

		/// <summary>
		/// To stream
		/// </summary>
		/// <param name="writer">Writer</param>
		public override void ToStream(BinaryWriter writer)
		{
			writer.Write(radiusX);
			writer.Write(radiusY);
		} // ToStream(writer)

		/// <summary>
		/// Create component
		/// </summary>
		/// <param name="owner">Owner</param>
		/// <returns>Collision component</returns>
		public override CollisionComponent CreateComponent(Actor owner)
		{
			CollisionComponentEllipse res = new CollisionComponentEllipse(owner);
			res.CreatePhysicsBody(radiusX, radiusY);
			return res;
		} // CreateComponent(owner)
	} // class CollisionDataEllipse
} // namespace AlkaronEngine.Assets.Meshes.Collision

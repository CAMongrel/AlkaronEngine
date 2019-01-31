// Project: Hellspawn, File: CollisionDataVertices.cs
// Namespace: AlkaronEngine.Assets.Meshes.Collision, Class: CollisionDataVertices
// Path: D:\Projekte\Hellspawn\Code\Hellspawn\Assets\Meshes\Collision, Author: Henning
// Code lines: 97, Size of file: 2,78 KB
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
	/// Collision data vertices
	/// </summary>
	class CollisionDataVertices : CollisionData
	{
		/// <summary>
		/// Collision vertices
		/// </summary>
		Vector2[] collisionVertices;

		/// <summary>
		/// Create collision data vertices
		/// </summary>
		public CollisionDataVertices()
		{
			Type = CollisionType.Vertices;
		} // CollisionDataVertices()

		/// <summary>
		/// From stream
		/// </summary>
		/// <param name="reader">Reader</param>
		/// <returns>Collision data vertices</returns>
		public static CollisionDataVertices FromStream(BinaryReader reader)
		{
			CollisionDataVertices res = new CollisionDataVertices();

			int vertCount = reader.ReadInt32();
			res.collisionVertices = new Vector2[vertCount];
			for (int i = 0; i < vertCount; i++)
			{
				res.collisionVertices[i] = new Vector2(
					reader.ReadSingle(), reader.ReadSingle());
			} // for (int)

			return res;
		} // FromStream(reader)

		/// <summary>
		/// From mesh
		/// </summary>
		/// <param name="points2D">Points 2D</param>
		/// <param name="points3D">Points 3D</param>
		/// <returns>Collision data vertices</returns>
		public static CollisionDataVertices FromMesh(Vector2[] points2D, Vector3[] points3D)
		{
			CollisionDataVertices res = new CollisionDataVertices();

			res.collisionVertices = points2D;

			return res;
		} // FromMesh(points2D, points3D)

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
		} // ToStream(writer)

		/// <summary>
		/// Create component
		/// </summary>
		/// <param name="owner">Owner</param>
		/// <returns>Collision component</returns>
		public override CollisionComponent CreateComponent(Actor owner)
		{
			CollisionComponentVertices res = new CollisionComponentVertices(owner);
			res.CreatePhysicsBody(collisionVertices);
			return res;
		} // CreateComponent(owner)
	} // class CollisionDataVertices
} // namespace AlkaronEngine.Assets.Meshes.Collision

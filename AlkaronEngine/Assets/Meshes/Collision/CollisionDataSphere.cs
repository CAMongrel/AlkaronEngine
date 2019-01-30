// Project: Hellspawn, File: CollisionDataSphere.cs
// Namespace: HellspawnEngine.Assets.Meshes.Collision, Class: CollisionDataSphere
// Path: D:\Projekte\Hellspawn\Code\Hellspawn\Assets\Meshes\Collision, Author: Henning
// Code lines: 83, Size of file: 2,37 KB
// Creation date: 23.04.2010 21:22
// Last modified: 10.05.2010 12:59
// Generated with Commenter by abi.exDream.com

#region Using directives
using HellspawnEngine.Scenes;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
#endregion

namespace AlkaronEngine.Assets.Meshes.Collision
{
	/// <summary>
	/// Collision data sphere
	/// </summary>
	class CollisionDataSphere : CollisionData
	{
		/// <summary>
		/// Radius
		/// </summary>
		float radius;

		/// <summary>
		/// Create collision data sphere
		/// </summary>
		public CollisionDataSphere()
		{
			Type = CollisionType.BoundingSphere;
		} // CollisionDataSphere()

		/// <summary>
		/// From stream
		/// </summary>
		/// <param name="reader">Reader</param>
		/// <returns>Collision data sphere</returns>
		public static CollisionDataSphere FromStream(BinaryReader reader)
		{
			CollisionDataSphere res = new CollisionDataSphere();
			res.radius = reader.ReadSingle();
			return res;
		} // FromStream(reader)

		/// <summary>
		/// From mesh
		/// </summary>
		/// <param name="points2D">Points 2D</param>
		/// <param name="points3D">Points 3D</param>
		/// <returns>Collision data sphere</returns>
		public static CollisionDataSphere FromMesh(Vector2[] points2D, Vector3[] points3D)
		{
			CollisionDataSphere res = new CollisionDataSphere();
			BoundingSphere sph = BoundingSphere.CreateFromPoints(points3D);
			res.radius = sph.Radius;
			return res;
		} // FromMesh(points2D, points3D)

		/// <summary>
		/// To stream
		/// </summary>
		/// <param name="writer">Writer</param>
		public override void ToStream(BinaryWriter writer)
		{
			writer.Write(radius);
		} // ToStream(writer)

		/// <summary>
		/// Create component
		/// </summary>
		/// <param name="owner">Owner</param>
		/// <returns>Collision component</returns>
		public override CollisionComponent CreateComponent(Actor owner)
		{
			CollisionComponentSphere res = new CollisionComponentSphere(owner);
			res.CreatePhysicsBody(radius);
			return res;
		} // CreateComponent(owner)
	} // class CollisionDataSphere
} // namespace HellspawnEngine.Assets.Meshes.Collision

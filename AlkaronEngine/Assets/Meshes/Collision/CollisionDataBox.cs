// Project: Hellspawn, File: CollisionDataBox.cs
// Namespace: AlkaronEngine.Assets.Meshes.Collision, Class: CollisionDataBox
// Path: D:\Projekte\Hellspawn\Code\Hellspawn\Assets\Meshes\Collision, Author: Henning
// Code lines: 91, Size of file: 2,47 KB
// Creation date: 23.04.2010 21:22
// Last modified: 10.05.2010 12:58
// Generated with Commenter by abi.exDream.com

#region Using directives
using AlkaronEngine.Actors;
using AlkaronEngine.Components;
using AlkaronEngine.Scene;
using Microsoft.Xna.Framework;
using System.IO;
#endregion

namespace AlkaronEngine.Assets.Meshes.Collision
{
    /// <summary>
    /// Collision data box
    /// </summary>
    class CollisionDataBox : CollisionData
	{
		/// <summary>
		/// Width
		/// </summary>
		float width;
		/// <summary>
		/// Height
		/// </summary>
		float height;

		/// <summary>
		/// Create collision data box
		/// </summary>
		public CollisionDataBox()
		{
			Type = CollisionType.BoundingBox;
		} // CollisionDataBox()

		/// <summary>
		/// From stream
		/// </summary>
		/// <param name="reader">Reader</param>
		/// <returns>Collision data box</returns>
		public static CollisionDataBox FromStream(BinaryReader reader)
		{
			CollisionDataBox res = new CollisionDataBox();
			res.width = reader.ReadSingle();
			res.height = reader.ReadSingle();
			return res;
		} // FromStream(reader)

		/// <summary>
		/// From mesh
		/// </summary>
		/// <param name="points2D">Points 2D</param>
		/// <param name="points3D">Points 3D</param>
		/// <returns>Collision data box</returns>
		public static CollisionDataBox FromMesh(Vector2[] points2D, Vector3[] points3D)
		{
			CollisionDataBox res = new CollisionDataBox();

			BoundingBox box = BoundingBox.CreateFromPoints(points3D);
			res.width = box.Max.X - box.Min.X;
			res.height = box.Max.Y - box.Min.Y;

			return res;
		} // FromMesh(points2D, points3D)

		/// <summary>
		/// To stream
		/// </summary>
		/// <param name="writer">Writer</param>
		public override void ToStream(BinaryWriter writer)
		{
			writer.Write(width);
			writer.Write(height);
		} // ToStream(writer)

		/// <summary>
		/// Create component
		/// </summary>
		/// <param name="owner">Owner</param>
		/// <returns>Collision component</returns>
		public override CollisionComponent CreateComponent(Actor owner)
		{
			CollisionComponentBox res = new CollisionComponentBox(owner);
			res.CreatePhysicsBody(width, height);
			return res;
		} // CreateComponent(owner)
	} // class CollisionDataBox
} // namespace AlkaronEngine.Assets.Meshes.Collision

// Project: Hellspawn, File: CollisionDataSkeletonBox.cs
// Namespace: HellspawnEngine.Assets.Meshes.Collision, Class: CollisionDataSkeletonBox
// Path: D:\Projekte\Hellspawn\Code\Hellspawn\Assets\Meshes\Collision, Author: Henning
// Code lines: 73, Size of file: 2,16 KB
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
	/// Collision data skeleton box
	/// </summary>
	class CollisionDataSkeletonBox : CollisionData
	{
		/// <summary>
		/// Create collision data skeleton box
		/// </summary>
		public CollisionDataSkeletonBox()
		{
			Type = CollisionType.SkeletonBox;
		} // CollisionDataSkeletonBox()

		/// <summary>
		/// From stream
		/// </summary>
		/// <param name="reader">Reader</param>
		/// <returns>Collision data skeleton box</returns>
		public static CollisionDataSkeletonBox FromStream(BinaryReader reader)
		{
			CollisionDataSkeletonBox res = new CollisionDataSkeletonBox();
			return res;
		} // FromStream(reader)

		/// <summary>
		/// From mesh
		/// </summary>
		/// <param name="points2D">Points 2D</param>
		/// <param name="points3D">Points 3D</param>
		/// <returns>Collision data skeleton box</returns>
		public static CollisionDataSkeletonBox FromMesh(Vector2[] points2D, Vector3[] points3D)
		{
			CollisionDataSkeletonBox res = new CollisionDataSkeletonBox();
			return res;
		} // FromMesh(points2D, points3D)

		/// <summary>
		/// To stream
		/// </summary>
		/// <param name="writer">Writer</param>
		public override void ToStream(BinaryWriter writer)
		{

		} // ToStream(writer)

		/// <summary>
		/// Create component
		/// </summary>
		/// <param name="owner">Owner</param>
		/// <returns>Collision component</returns>
		public override CollisionComponent CreateComponent(Actor owner)
		{
			return new CollisionComponentSkeletonBox(owner);
		} // CreateComponent(owner)
	} // class CollisionDataSkeletonBox
} // namespace HellspawnEngine.Assets.Meshes.Collision

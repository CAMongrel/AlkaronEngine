// Project: Hellspawn, File: CollisionData.cs
// Namespace: HellspawnEngine.Assets.Meshes.Collision, Class: CollisionData
// Path: D:\Projekte\Hellspawn\Code\Hellspawn\Assets\Meshes\Collision, Author: Henning
// Code lines: 31, Size of file: 956 Bytes
// Creation date: 23.04.2010 21:22
// Last modified: 10.05.2010 12:58
// Generated with Commenter by abi.exDream.com

#region Using directives
using System.IO;
using AlkaronEngine.Actors;
#endregion

namespace AlkaronEngine.Assets.Meshes.Collision
{
    /// <summary>
    /// Collision data
    /// </summary>
    public abstract class CollisionData
	{
		/// <summary>
		/// Type
		/// </summary>
		/// <returns>Collision type</returns>
		public CollisionType Type { get; protected set; } // Type

		/// <summary>
		/// To stream
		/// </summary>
		/// <param name="writer">Writer</param>
		public abstract void ToStream(BinaryWriter writer);

		/// <summary>
		/// Create component
		/// </summary>
		/// <param name="owner">Owner</param>
		/// <returns>Collision component</returns>
		public abstract CollisionComponent CreateComponent(BaseActor owner);
	} // class CollisionData
} // namespace HellspawnEngine.Assets.Meshes.Collision

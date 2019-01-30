// Project: Hellspawn

#region Using directives
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.IO;
#endregion

namespace AlkaronEngine.Assets.Meshes
{
    /// <summary>
    /// Mesh asset
    /// </summary>
    public abstract class MeshAsset : Asset
	{
		/// <summary>
		/// Vertex buffer for the mesh.
		/// </summary>
		protected VertexBuffer vertexBuffer = null;
		/// <summary>
		/// Index buffer for the mesh.
		/// </summary>
		protected IndexBuffer indexBuffer = null;

		/// <summary>
		/// 32 bit indices
		/// </summary>
		protected int[] objectIndices;

		/// <summary>
		/// Collision type
		/// </summary>
		/// <returns>Collision type</returns>
		public CollisionType CollisionType
		{
			get
			{
				if (collisionData == null)
					return CollisionType.BoundingBox;

				return collisionData.Type;
			} // get
#if WINDOWS
			set
			{
				CreateRuntimeCollisionData(value);

				PackageManager.LoadPackage(PackageName).SetNeedsSave(true);
			} // set
#endif
		} // CollisionType

		/// <summary>
		/// Collision data
		/// </summary>
		protected CollisionData collisionData;
		/// <summary>
		/// Collision data
		/// </summary>
		/// <returns>Collision data</returns>
#if WINDOWS
		[Browsable(false)]
#endif
		public CollisionData CollisionData
		{
			get
			{
				return collisionData;
			} // get
		} // CollisionData

		/// <summary>
		/// 
		/// </summary>
		protected List<CollisionData> customCollisions;
		/// <summary>
		/// Custom collisions
		/// </summary>
		/// <returns>List</returns>
#if WINDOWS
		[Browsable(false)]
#endif
		public List<CollisionData> CustomCollisions
		{
			get
			{
				return customCollisions;
			} // get
		} // CustomCollisions

		/// <summary>
		/// Node material
		/// </summary>
		protected IMaterial nodeMaterial;
		/// <summary>
		/// Material
		/// </summary>
		/// <returns>String</returns>
		public string Material
		{
			get
			{
				return (nodeMaterial != null ? nodeMaterial.Fullname : "");
			} // get
#if WINDOWS
			set
			{
				SetMaterialByName(value);

				Package.SetNeedsSave(true);
			} // set
#endif
		} // Material

#if WINDOWS
		#region SetMaterialByName
		/// <summary>
		/// Set material by name
		/// </summary>
		protected void SetMaterialByName(string setName)
		{
			string assetType = Path.GetExtension(setName);

			switch (assetType)
			{
				case ".NodeMaterial":
					nodeMaterial =
						AssetManager.Load<NodeMaterial>(setName);
					break;

				case ".MaterialInstance":
					nodeMaterial =
						AssetManager.Load<MaterialInstance>(setName);
					break;

				default:
					nodeMaterial = null;
					return;
			} // switch
		} // SetMaterialByName(setName)
		#endregion 

		/// <summary>
		/// Create bounding sphere
		/// </summary>
		internal abstract void CreateBoundingSphere();

		/// <summary>
		/// Create runtime collision data
		/// </summary>
		/// <param name="collType">Coll type</param>
		internal abstract void CreateRuntimeCollisionData(CollisionType collType);

		#region WriteCollisionData
		/// <summary>
		/// Write collision data
		/// </summary>
		/// <param name="writer">Writer</param>
		/// <param name="collData">Coll data</param>
		protected void WriteCollisionData(BinaryWriter writer, CollisionData collData)
		{
			writer.Write((int)collData.Type);
			collData.ToStream(writer);
		} // WriteCollisionData(writer, collData)
		#endregion // class ColladaModel
#endif

		protected const float EPSILON = 0.000001f;

		#region GetNodeMaterial
		/// <summary>
		/// Get node material
		/// </summary>
		/// <returns>IMaterial</returns>
		public IMaterial GetNodeMaterial()
		{
			return nodeMaterial;
		}
		#endregion // GetNodeMaterial()

		#region ReadCollisionData
		/// <summary>
		/// Creates the collision data from File
		/// </summary>
		protected CollisionData ReadCollisionData(BinaryReader reader)
		{
			CollisionData collData = null;

			CollisionType collType = (CollisionType)reader.ReadInt32();

			switch (collType)
			{
				case CollisionType.BoundingBox:
					collData = CollisionDataBox.FromStream(reader);
					break;

				case CollisionType.BoundingEllipse:
					collData = CollisionDataEllipse.FromStream(reader);
					break;

				case CollisionType.BoundingSphere:
					collData = CollisionDataSphere.FromStream(reader);
					break;

				case CollisionType.ConvexHull:
					collData = CollisionDataConvexHull.FromStream(reader);
					break;

				case CollisionType.SkeletonBox:
					collData = CollisionDataSkeletonBox.FromStream(reader);
					break;

				case CollisionType.Vertices:
					collData = CollisionDataVertices.FromStream(reader);
					break;
			} // switch

			return collData;
		} // ReadCollisionData(reader)
		#endregion
	} // class MeshAsset
} // namespace HellspawnEngine.Assets.Meshes

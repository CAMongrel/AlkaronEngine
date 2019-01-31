// Project: Hellspawn

using AlkaronEngine.Assets;
using AlkaronEngine.Assets.Meshes;
using Microsoft.Xna.Framework;

namespace AlkaronEngine.Scene
{
    /// <summary>
    /// Static mesh actor
    /// </summary>
    class StaticMeshActor : MeshActor
	{
		/// <summary>
		/// Mesh for this actor
		/// </summary>
		protected StaticMesh mesh;

		#region Constructor
		/// <summary>
		/// Create static mesh actor
		/// </summary>
		protected StaticMeshActor()
		{
		} // StaticMeshActor()
		#endregion

		#region StaticMeshActor
		/// <summary>
		/// Create static mesh actor
		/// </summary>
		public StaticMeshActor(BaseScene setOwnerScene)
			: base(setOwnerScene)
		{
			// Static meshes aren't ticked
			bIsTickable = false;
			// Static meshes should have a physics property
			bIsPhysical = true;
			// bIsStatic is already set to true in the base actor
		} // StaticMeshActor(setOwnerScene)
		#endregion

		#region StaticMesh
		/// <summary>
		/// Direct reference to the referenced mesh asset
		/// </summary>
		public StaticMesh StaticMesh
		{
			get
			{
				return mesh as StaticMesh;
			}
		} // StaticMesh
		#endregion

		#region SetStaticMesh
		/// <summary>
		/// Sets a new static mesh asset for this actor
		/// </summary>
		public void SetStaticMesh(StaticMesh setMesh)
		{
			mesh = setMesh;

			UpdateCollisionComponent();

			CreateBoundingSphere();
		} // SetStaticMesh(setMesh)

		/// <summary>
		/// Sets a new static mesh asset using the asset name
		/// </summary>
		/// <param name="AssetName"></param>
		public void SetStaticMesh(string AssetName)
		{
			SetStaticMesh(AssetManager.Load<StaticMesh>(AssetName));
		} // SetStaticMesh(AssetName)
		#endregion

		#region CreateBoundingSphere
		/// <summary>
		/// Creates the bounding sphere based on the mesh.
		/// </summary>
		protected override void CreateBoundingSphere()
		{
			base.CreateBoundingSphere();

			if (mesh != null)
			{
				if (boundingSphere == null)
				{
					boundingSphere = new BoundingSphere(
						worldPosition,
						mesh.BoundingSphere.Radius);
				}
				else
				{
					boundingSphere.Center = worldPosition;
					boundingSphere.Radius = mesh.BoundingSphere.Radius;
				}
			} // if
		} // CreateBoundingSphere()
		#endregion

		#region UpdateCollisionComponent
		/// <summary>
		/// Update collision component
		/// </summary>
		protected override void UpdateCollisionComponent()
		{
			if (StaticMesh == null ||
				StaticMesh.BoundingSphere == null)
				return;

			if (collision == null &&
				mesh.CollisionData != null)
			{
				collision = mesh.CollisionData.CreateComponent(this);
			}
		} // UpdateCollisionComponent()
		#endregion

		#region Hit
		/// <summary>
		/// Ray/BoundingSphere check
		/// </summary>
		internal override bool Hit(Ray castRay, out float distance)
		{
			bool bHit = false;
			float closestDistance = float.MaxValue;

			float maxScale = worldScaling.X;
			if (worldScaling.Y > maxScale)
				maxScale = worldScaling.Y;
			if (worldScaling.Z > maxScale)
				maxScale = worldScaling.Z;

			BoundingSphere bs = new BoundingSphere(
				StaticMesh.BoundingSphere.Center + WorldPosition,
				StaticMesh.BoundingSphere.Radius * maxScale);

			Matrix invWorldMatrix =
				//Matrix.CreateScale(worldScaling) *
				Matrix.CreateTranslation(worldPosition);
			invWorldMatrix = Matrix.Invert(invWorldMatrix);

			float? dist = bs.Intersects(castRay);

			if (dist != null)
			{
				// Trace against the triangles
				// Move the ray into the meshs' local space
				float rayOffset = Vector3.Distance(castRay.Position, worldPosition);
				castRay.Position = Vector3.Transform(castRay.Position, invWorldMatrix);
				//castRay.Position -= worldPosition;
				bHit = mesh.Intersect(castRay, out closestDistance, ref worldScaling);
				closestDistance += rayOffset;
			} // if
			distance = closestDistance;

			return bHit;
		} // Hit(castRay, distance)
		#endregion

		#region PostLoad
		/// <summary>
		/// Post load
		/// </summary>
		public override void PostLoad()
		{
			base.PostLoad();
		} // PostLoad()
		#endregion

		#region Draw
		/// <summary>
		/// Draw
		/// </summary>
		internal override void Draw(Microsoft.Xna.Framework.GameTime gameTime)
		{
			base.Draw(gameTime);

			if (StaticMesh == null)
				return;

			Program.Game.ActiveScene.StaticMeshRenderer.Render(this);

#if (WINDOWS)
			if (Program.Game.CurrentViewport.CurrentRenderPass == RenderPass.Color)
			{
				if (Program.Game.IsEditor)
				{
					if (IsSelected)
					{
						Program.Game.BeginRenderSelectionBuffer(IsSelected);
						Program.Game.ActiveScene.StaticMeshRenderer.RenderImmediate(this);
						Program.Game.EndRenderSelectionBuffer();
					}
				} // if
			} // if
#endif
		} // Draw(gameTime)
		#endregion

#if WINDOWS
		#region DrawEditorIcon
		/// <summary>
		/// Draw editor icon
		/// </summary>
		internal override void DrawEditorIcon(GameTime gameTime)
		{
			if (Program.Game.IsEditor)
			{
				Vector3 pos = Program.Game.ActiveScene.Camera.Project(
						this.WorldPosition);
				Vector2 scrPos = new Vector2(pos.X - 13, pos.Y - 13);

				Program.Game.SpriteBatch.Begin(SpriteBlendMode.AlphaBlend,
					SpriteSortMode.FrontToBack, SaveStateMode.SaveState);
				Program.Game.SpriteBatch.Draw(
					Program.Game.GetSpriteTexture("StaticMeshIcon"), scrPos,
					(IsSelected ? HellspawnGame.SelectionColorXNA :
					Microsoft.Xna.Framework.Graphics.Color.White));
				Program.Game.SpriteBatch.End();
			} // if
		} // DrawEditorIcon(gameTime)
		#endregion
#endif

		#region Clone
		/// <summary>
		/// Clone
		/// </summary>
		public override BasicObject Clone()
		{
			StaticMeshActor result = new StaticMeshActor();
			InternalClone(result);
			return result;
		} // Clone()

		/// <summary>
		/// Internal clone
		/// </summary>
		protected override void InternalClone(AlkaronEngine.Scene.BasicObject newObject)
		{
			base.InternalClone(newObject);

			StaticMeshActor newMesh = newObject as StaticMeshActor;
			newMesh.mesh = this.mesh;
		} // InternalClone(newObject)
		#endregion

#if WINDOWS
		#region GetReferencedAssets
		/// <summary>
		/// Get referenced assets
		/// </summary>
		internal override List<IAsset> GetReferencedAssets()
		{
			List<IAsset> result = base.GetReferencedAssets();
			result.Add(StaticMesh);
			return result;
		} // GetReferencedAssets()
		#endregion
#endif
	} // class StaticMeshActor
} // namespace AlkaronEngine.Scene

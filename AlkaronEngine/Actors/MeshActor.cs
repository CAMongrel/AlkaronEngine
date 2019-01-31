// Project: Hellspawn

#region Using directives
using System.Collections.Generic;
using System.IO;
using AlkaronEngine.Assets;
using AlkaronEngine.Components;
using Microsoft.Xna.Framework;
#endregion

namespace AlkaronEngine.Scene
{
	/// <summary>
	/// Mesh actor
	/// </summary>
	public abstract class MeshActor : Actor
	{
		#region Members
		/// <summary>
		/// Collision component for this mesh
		/// </summary>
		protected CollisionComponent collision;
		/// <summary>
		/// Collision
		/// </summary>
		public CollisionComponent Collision
		{
			get
			{
				/*if (collision == null)
				{
					for (int i = 0; i < AllComponents.Count; i++)
					{
						if (AllComponents[i].Owner == this &&
							AllComponents[i].GetType().IsSubclassOf(typeof(CollisionComponent)))
						{
							collision = AllComponents[i] as CollisionComponent;
							break;
						} // if
					}
					
					if (collision == null)
					{
						// Log warning? Might not be useful ...
					} // if
				} // if
				*/
			
				return collision;
			} // get
		} // Collision

		/// <summary>
		/// 
		/// </summary>
		protected LightComponent lightComp;
		/// <summary>
		/// Light comp
		/// </summary>
		public LightComponent LightComp
		{
			get
			{
				/*if (lightComp == null)
				{
					for (int i = 0; i < AllComponents.Count; i++)
					{
						if (AllComponents[i].Owner == this &&
							AllComponents[i].GetType().IsSubclassOf(typeof(LightComponent)))
						{
							lightComp = AllComponents[i] as LightComponent;
							break;
						} // if
					}

					if (lightComp == null)
					{
						// Log warning? Might not be useful ...
					} // if
				} // if
				*/

				return lightComp;
			} // get
		} // LightComp
		
		/// <summary>
		/// Does this actor override the fog distances?
		/// </summary>
		protected bool overrideFogValues;
		/// <summary>
		/// Override fog values
		/// </summary>
		public bool OverrideFogValues
		{
			get
			{
				return overrideFogValues;
			} // get
			set
			{
				if (overrideFogValues == value)
					return;
				overrideFogValues = value;
			} // set
		} // OverrideFogValues
		
		/// <summary>
		/// Fog start/end
		/// </summary>
		protected Vector2 fogDistanceValues;
		/// <summary>
		/// Fog distance values
		/// </summary>
		public Vector2 FogDistanceValues
		{
			get
			{
				return fogDistanceValues;
			} // get
			set
			{
				if (fogDistanceValues == value)
					return;
				fogDistanceValues = value;
			} // set
		} // FogDistanceValues

		/// <summary>
		/// Fog color
		/// </summary>
		protected Vector4 fogColorValues;
		/// <summary>
		/// Fog color values
		/// </summary>
		public Vector4 FogColorValues
		{
			get
			{
				return fogColorValues;
			} // get
			set
			{
				if (fogColorValues == value)
					return;
				fogColorValues = value;
			} // set
		} // FogColorValues
		#endregion

		#region ctor
		/// <summary>
		/// Create mesh actor
		/// </summary>
		protected MeshActor()
		{
		} // MeshActor()

		/// <summary>
		/// Create mesh actor
		/// </summary>
		internal MeshActor(BaseScene setOwnerScene)
			: base(setOwnerScene)
		{
			lightComp = new LightComponent(this);

			overrideFogValues = false;
			fogDistanceValues = new Vector2();
			fogColorValues = Vector4.One;
		} // MeshActor(setOwnerScene)
		#endregion

		#region Spawned
		/// <summary>
		/// Spawned
		/// </summary>
		protected override void Spawned()
		{
			base.Spawned();

			UpdateLightComponent();
		} // Spawned()
		#endregion

		#region Moved
		/// <summary>
		/// Moved
		/// </summary>
		protected override void Moved()
		{
			base.Moved();
			
            /*
			if (bIsPhysical && 
				collision != null &&
				collision.PhysicsBody != null)
			{
				if (worldLayer > 0)
					collision.CollisionArea = CollisionArea.Foreground;
				else if (worldLayer < 0)
					collision.CollisionArea = CollisionArea.Background;
				else
					collision.CollisionArea = CollisionArea.Game;
			} // if
            */

			CreateBoundingSphere();
			UpdateLightComponent();			
		} // Moved()
		#endregion

		#region Tick
		/// <summary>
		/// Tick
		/// </summary>
		public override bool Tick(GameTime gameTime)
		{
			if (base.Tick(gameTime) == false)
				return false;

            /*
			if (bIsPhysical && 
				collision != null &&
				collision.PhysicsBody != null)
			{
				WorldPosition = new Vector3(
					collision.PhysicsBody.Position.X,
					collision.PhysicsBody.Position.Y,
					worldPosition.Z);
				worldRotation = collision.PhysicsBody.Rotation;
			} // if
            */
			
			return true;
		} // Tick(gameTime)
		#endregion

        /*
		#region FillLightInfo
		/// <summary>
		/// 
		/// </summary>
		internal void FillLightInfo(ref LightInfo lightInfo)
		{
			for (int i = 0; i < lightInfo.Positions.Length; i++)
			{
				lightInfo.Positions[i] = Vector4.Zero;
				lightInfo.Colors[i] = Vector4.Zero;
				//lightInfo.Radii[i] = 0.0f;

				if (lightComp != null &&
					lightComp.InfluencingLights.Count > i)
				{
					lightInfo.Positions[i].X =
						lightComp.InfluencingLights[i].WorldPosition.X;
					lightInfo.Positions[i].Y =
						lightComp.InfluencingLights[i].WorldPosition.Y;
					lightInfo.Positions[i].Z =
						lightComp.InfluencingLights[i].WorldPosition.Z;
					lightInfo.Positions[i].W =
						1.0f / lightComp.InfluencingLights[i].LightRadius;

					lightInfo.Colors[i].X =
						lightComp.InfluencingLights[i].LightColor.X;
					lightInfo.Colors[i].Y =
						lightComp.InfluencingLights[i].LightColor.Y;
					lightInfo.Colors[i].Z =
						lightComp.InfluencingLights[i].LightColor.Z;
					lightInfo.Colors[i].W =
						lightComp.InfluencingLights[i].LinearAttenuation;

					//lightInfo.Radii[i] = 1 / lightComp.InfluencingLights[i].LightRadius;
				}
			}
		}
		#endregion

		#region FillFogInfo
		internal void FillFogInfo(ref FogInfo FogInfo)
		{
			FogInfo.ColorValues = overrideFogValues ? fogColorValues :
				Program.Game.ActiveScene.WorldInfo.FogColor;

			Vector2 fogDist = new Vector2(
				Program.Game.ActiveScene.WorldInfo.FogStart,
				Program.Game.ActiveScene.WorldInfo.FogEnd);
			FogInfo.DistanceValues = 
				overrideFogValues ? fogDistanceValues : fogDist;
		}
		#endregion
		*/

		#region UpdateCollisionComponent
		/// <summary>
		/// Update collision component
		/// </summary>
		protected virtual void UpdateCollisionComponent()
		{
			//
		} // UpdateCollisionComponent()
		#endregion

		#region Draw
		/// <summary>
		/// Draw
		/// </summary>
		internal override void Draw(GameTime gameTime)
		{
			base.Draw(gameTime);

#if (WINDOWS)
			if (IsPhysical && collision != null &&
				Program.Game.ShowPhysicGeometry != CollisionViewMode.None)
			{
				if (Program.Game.ShowPhysicGeometry == CollisionViewMode.All ||
					WorldLayer == 0)
				{
					collision.DrawPhysicsGeom();
				} // if (Program.Game.ShowPhysicGeometry)
			} // if
#endif
		} // Draw(gameTime)
		#endregion

		#region SetScale
		/// <summary>
		/// Set scale
		/// </summary>
		internal override void SetScale(Vector3 newScale)
		{
			base.SetScale(newScale);
			
			collision.SetScale(newScale);
		} // SetScale(newScale)
		#endregion

		#region InternalClone
		/// <summary>
		/// Internal clone
		/// </summary>
		protected override void InternalClone(BasicObject newObject)
		{
			base.InternalClone(newObject);
			
			MeshActor newMesh = newObject as MeshActor;
			newMesh.overrideFogValues = this.overrideFogValues;
			newMesh.fogColorValues = this.fogColorValues;
			newMesh.fogDistanceValues = this.fogDistanceValues;
			newMesh.nodeMaterial = this.nodeMaterial;

			for (int i = 0; i < newMesh.AllComponents.Count; i++)
			{
				if (newMesh.AllComponents[i] == null)
					continue;
			
				if (newMesh.AllComponents[i].GetType().IsSubclassOf(
						typeof(CollisionComponent)))
				{
					newMesh.collision = newMesh.AllComponents[i]
						as CollisionComponent;
				} // if

				if (newMesh.AllComponents[i].GetType() == typeof(LightComponent) ||
					newMesh.AllComponents[i].GetType().IsSubclassOf(
						typeof(LightComponent)))
				{
					newMesh.lightComp = newMesh.AllComponents[i]
						as LightComponent;
				} // if
			} // for
		} // InternalClone(newObject)
		#endregion
		
		#region UpdateLightComponent
		/// <summary>
		/// Update light component
		/// </summary>
		internal virtual void UpdateLightComponent()
		{
			if (lightComp == null)
				return;
				
			lightComp.CacheLightInfluences();
		} // UpdateLightComponent()
		#endregion

		#region SetPosition
		/// <summary>
		/// Set position
		/// </summary>
		internal override void SetPosition(Vector3 newPosition)
		{
			base.SetPosition(newPosition);

			if (bIsPhysical &&
				collision != null &&
				collision.PhysicsBody != null)
			{
				collision.PhysicsBody.Position = 
					new Vector2(newPosition.X, newPosition.Y);
			} // if
		} // SetPosition(newPosition)
		#endregion

		#region PostLoad
		/// <summary>
		/// Post load
		/// </summary>
		public override void PostLoad()
		{
			base.PostLoad();
			
			/*if (collision == null)
				collision = new CollisionComponent(this);*/
			
			UpdateCollisionComponent();
			
			// Refresh the world position. This will correctly
			// update the position of the physics component
			WorldPosition = WorldPosition;
		} // PostLoad()
		#endregion

		#region GetReferencedAssets
		/// <summary>
		/// Get referenced assets
		/// </summary>
		internal override List<IAsset> GetReferencedAssets()
		{
			List<IAsset> result = base.GetReferencedAssets();
			//result.Add(NodeMaterial);
			return result;
		} // GetReferencedAssets()
		#endregion
	} // class MeshActor
} // namespace AlkaronEngine.Scene

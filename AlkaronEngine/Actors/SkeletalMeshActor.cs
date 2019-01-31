// Project: Hellspawn

#region Using directives
using AlkaronEngine.Assets;
using AlkaronEngine.Assets.Meshes;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
#endregion

namespace AlkaronEngine.Scene
{
    /// <summary>
    /// Dynamic mesh with bone support
    /// </summary>
    public class SkeletalMeshActor : MeshActor
	{
		#region Members
		/// <summary>
		/// Mesh for this actor
		/// </summary>
		protected SkeletalMesh mesh;

		/// <summary>
		/// The currently active animation of this skeletal mesh
		/// </summary>
		[NoStore]
		SkeletalMesh.RuntimeAnimation currentAnimation;

		/// <summary>
		/// Should the current animation be looped?
		/// </summary>
		[NoStore]
		bool loopCurrentAnimation;

		/// <summary>
		/// The animation to fall back to when the current non-looped
		/// animation has ended.
		/// </summary>
		[NoStore]
		SkeletalMesh.RuntimeAnimation fallbackAnimation;

		/// <summary>
		/// Current animation frame.
		/// 
		/// </summary>
		[NoStore]
		private float currentAnimationFrame;

		/// <summary>
		/// Framerate of the current skeletal mesh
		/// </summary>
		[NoStore]
		protected float frameRate;

		/// <summary>
		/// Does the current animation prevent other animation from
		/// overwriting it? This will automatically reset to false
		/// if the current animation ends and the fallback animation
		/// kicks in.
		/// </summary>
		[NoStore]
		private bool isCurrentAnimBlocking;

		/// <summary>
		/// Actual current frame index that will be used 
		/// for rendering.
		/// </summary>
		[NoStore]
		private int currentAnimationFrameNum;
		/// <summary>
		/// Current animation frame number
		/// </summary>
		public int CurrentAnimationFrameNum
		{
			get
			{
				return currentAnimationFrameNum;
			}
		} // CurrentAnimationFrameNum

		/// <summary>
		/// The name of the currently active animation
		/// </summary>
		public string CurrentAnimationName
		{
			get
			{
				return
					(currentAnimation != null ?
					 currentAnimation.Name : "");
			}
		} // CurrentAnimationName
		#endregion

		#region SkeletalMeshActor
		/// <summary>
		/// Create skeletal mesh actor
		/// </summary>
		protected SkeletalMeshActor()
		{
		} // SkeletalMeshActor()
		#endregion

		#region SkeletalMeshActor
		/// <summary>
		/// Create skeletal mesh actor
		/// </summary>
		public SkeletalMeshActor(BaseScene setOwnerScene)
			: base(setOwnerScene)
		{
			bIsStatic = false;
			isCurrentAnimBlocking = false;
		} // SkeletalMeshActor(setOwnerScene)
		#endregion

		#region PlayAnimation
		/// <summary>
		/// Plays the specified animation using a default FPS of 30. No blending yet.
		/// </summary>
		public void PlayAnimation(string animName, bool shouldLoop, string fallbackAnimName)
		{
			PlayAnimation(animName, shouldLoop, fallbackAnimName, 30.0f, false);
		} // PlayAnimation(animName, shouldLoop, fallbackAnimName)

		/// <summary>
		/// Plays the specified animation using the specified framerate. No blending yet.
		/// </summary>
		public void PlayAnimation(string animName, bool shouldLoop,
			string fallbackAnimName, float setFrameRate, bool shouldBlockAnim)
		{
			if (isCurrentAnimBlocking == true)
			{
				Log.LogWarning("Can't play animation '" + animName + "', because the " +
					"current animation '" + currentAnimation.Name + "' is blocking.");
				return;
			}

			if (mesh == null)
			{
				Log.LogError("Trying to play animation '" + animName +
					"' on '" + name + "', but the Mesh is null!");
				return;
			}

			SkeletalMesh.RuntimeAnimation anim =
				mesh.GetAnimationByName(animName);
			if (anim == null)
			{
				Log.LogWarning("Trying to play animation '" + animName + "', but that animation " +
					"does not exist in SkeletalMesh '" + this.Name + "'");
				return;
			}

			if (fallbackAnimName != null)
			{
				SkeletalMesh.RuntimeAnimation fallbackAnim =
					mesh.GetAnimationByName(fallbackAnimName);
				if (anim == null)
				{
					Log.LogWarning("Trying to play animation '" + animName + "', but that animation " +
						"does not exist in SkeletalMesh '" + this.Name + "'");
					return;
				}

				fallbackAnimation = fallbackAnim;
			} // if

			frameRate = setFrameRate;

			isCurrentAnimBlocking = shouldBlockAnim;

			currentAnimation = anim;
			currentAnimationFrame = 0;

			AnimationChanged(currentAnimation.Name, false);

			loopCurrentAnimation = shouldLoop;
		} // PlayAnimation(animName, shouldLoop, fallbackAnimName)

		/// <summary>
		/// Calculate the frame rate based on the length of the animation
		/// and the desired time in which it should complete.
		/// 
		/// Example:
		/// Number of Frame of the animation: 90
		/// Default FPS: 30
		/// Desired Completion Time: 1.5 seconds
		/// 
		/// Default playing time would be: 3 seconds (90 / 30)
		/// fpsMod = 3 / 1.5 = 0.5
		/// 
		/// Return value: DefaultFPS * fpsMod = 30 * 1.5 = 60 fps
		/// </summary>
		public float CalculateFPS(string animName, float desiredCompletionTime)
		{
			// Set the default FPS to 30
			const float DefaultFPS = 30.0f;

			if (mesh == null)
			{
				Log.LogError("Trying to play animation '" + animName +
					"' on '" + name + "', but the Mesh is null!");
				return DefaultFPS;
			}

			SkeletalMesh.RuntimeAnimation anim =
				mesh.GetAnimationByName(animName);
			if (anim == null)
			{
				Log.LogWarning("Trying to play animation '" + animName + "', but that animation " +
					"does not exist in SkeletalMesh '" + this.Name + "'");
				return DefaultFPS;
			}

			// Using the default FPS of 30, calculate the default running time
			// of the animation in seconds.
			float defaultPlaytime = (float)anim.NumOfFrames / DefaultFPS;

			// Calculate the factor we have to modify the default FPS to
			// play the animation in the desired completion time
			float fpsMod = defaultPlaytime / desiredCompletionTime;

			// Multiply the default FPS by the fpsMod
			return DefaultFPS * fpsMod;
		} // CalculateFPS(animName, desiredCompletionTime)

		/// <summary>
		/// Gets called when an animation ends and the next animation starts.
		/// "isLooping" is true if the current animation is looped and the same
		/// animation starts again from the beginning.
		/// </summary>
		protected virtual void AnimationChanged(string nextAnimationName, bool isLooping)
		{
			// Does nothing in the base implementation.
		} // AnimationChanged(nextAnimationName, isLooping)
		#endregion

		#region Tick
		/// <summary>
		/// Ticks the actor, updating the animation.
		/// </summary>
		public override bool Tick(GameTime gameTime)
		{
			if (base.Tick(gameTime) == false)
				return false;

			TickAnimation(gameTime);

			return true;
		} // Tick(gameTime)

#if WINDOWS
		/// <summary>
		/// Note: For editor use only
		/// Only ticks the animation of this skeletal mesh
		/// </summary>
		internal void TickAnimOnly(GameTime gameTime)
		{
			TickAnimation(gameTime);
		} // TickAnimOnly(gameTime)
#endif

		/// <summary>
		/// Updates the current animation, taking loop and
		/// frame count into account.
		/// </summary>
		void TickAnimation(GameTime gameTime)
		{
			if (currentAnimation == null)
				return;

			currentAnimationFrame +=
				(float)gameTime.ElapsedGameTime.TotalSeconds * frameRate;
			if (currentAnimationFrame < 0)
				currentAnimationFrame = 0;

			// If the current animation should not be looped and if it has
			// ended, then revert to the fallbackAnimation (and loop the
			// fallback anim)
			if (loopCurrentAnimation == false &&
				(int)currentAnimationFrame > currentAnimation.NumOfFrames)
			{
				// Reset block flag
				isCurrentAnimBlocking = false;

				if (fallbackAnimation != null)
				{
					loopCurrentAnimation = true;
					currentAnimation = fallbackAnimation;
					currentAnimationFrame = 0;

					AnimationChanged(currentAnimation.Name, false);
				}
			} // if

			// If we're looping the current animation and the animation restarts
			// also call the AnimationChanged handler
			if (loopCurrentAnimation == true &&
				(int)currentAnimationFrame > currentAnimation.NumOfFrames)
			{
				AnimationChanged(currentAnimation.Name, true);
				currentAnimationFrame = 0;
			}

			currentAnimationFrameNum = ((int)currentAnimationFrame %
				currentAnimation.NumOfFrames) + currentAnimation.Start;
		} // TickAnimation(gameTime)
		#endregion

		#region UpdateCollisionComponent
		/// <summary>
		/// Update collision component
		/// </summary>
		protected override void UpdateCollisionComponent()
		{
			if (SkeletalMesh == null ||
				SkeletalMesh.BoundingSphere == null)
				return;

			if (collision == null &&
				mesh.CollisionData != null)
			{
				collision = mesh.CollisionData.CreateComponent(this);
			}
		} // UpdateCollisionComponent()
		#endregion

		#region SkeletalMesh
		/// <summary>
		/// Direct reference to the referenced mesh asset
		/// </summary>
		public SkeletalMesh SkeletalMesh
		{
			get
			{
				return mesh as SkeletalMesh;
			}
		} // SkeletalMesh
		#endregion

		#region SetSkeletalMesh
		/// <summary>
		/// Sets a new static mesh asset for this actor
		/// </summary>
		public void SetSkeletalMesh(SkeletalMesh setMesh)
		{
			mesh = setMesh;

			UpdateCollisionComponent();

			InitAnimation();

			CreateBoundingSphere();
		} // SetSkeletalMesh(setMesh)

		/// <summary>
		/// Sets a new static mesh asset using the asset name
		/// </summary>
		/// <param name="AssetName"></param>
		public void SetSkeletalMesh(string AssetName)
		{
			SetSkeletalMesh(AssetManager.Load<SkeletalMesh>(AssetName));
		} // SetSkeletalMesh(AssetName)
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
				}
			} // if
		} // CreateBoundingSphere()
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
				SkeletalMesh.BoundingSphere.Center + WorldPosition,
				SkeletalMesh.BoundingSphere.Radius * maxScale);

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
				bHit = mesh.Intersect(castRay, out closestDistance, ref worldScaling);
				closestDistance += rayOffset;
			} // if
			distance = closestDistance;

			return bHit;
		} // Hit(castRay, distance)
		#endregion

		#region Spawned
		/// <summary>
		/// Spawned
		/// </summary>
		protected override void Spawned()
		{
			base.Spawned();
		} // Spawned()
		#endregion

		#region PostLoad
		/// <summary>
		/// Post load
		/// </summary>
		public override void PostLoad()
		{
			base.PostLoad();

			InitAnimation();
		} // PostLoad()
		#endregion

		#region InitAnimation
		/// <summary>
		/// Init animation
		/// </summary>
		void InitAnimation()
		{
			SkeletalMesh.RuntimeAnimation anim =
				mesh.GetAnimationByName("Idle");

			currentAnimation = anim;
			fallbackAnimation = anim;

			if (currentAnimation == null &&
				mesh.runtimeAnimations.Count > 0)
			{
				currentAnimation = mesh.runtimeAnimations[0];
				fallbackAnimation = currentAnimation;
			}

			currentAnimationFrame = 0;

			frameRate = mesh.FrameRate;

			loopCurrentAnimation = true;
		} // InitAnimation()
		#endregion

		#region Draw
		/// <summary>
		/// Render the skeletal mesh
		/// </summary>
		internal override void Draw(GameTime gameTime)
		{
			base.Draw(gameTime);

			if (SkeletalMesh == null)
				return;

			Program.Game.ActiveScene.SkeletalMeshRenderer.Render(this);

			if (Program.Game.ShowPhysicGeometry != CollisionViewMode.None)
				RenderSkeleton(true, Program.Game.ActiveScreen, null);

#if (WINDOWS)
			if (Program.Game.IsEditor)
			{
				Program.Game.BeginRenderSelectionBuffer(IsSelected);
				Program.Game.ActiveScene.SkeletalMeshRenderer.RenderImmediate(this);
				Program.Game.EndRenderSelectionBuffer();
			}
#endif
		} // Draw(gameTime)
		#endregion

		#region DrawEditorIcon
#if WINDOWS
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
					Program.Game.GetSpriteTexture("SkeletalMeshIcon"), scrPos,
					(IsSelected ? HellspawnGame.SelectionColorXNA :
					Microsoft.Xna.Framework.Graphics.Color.White));
				Program.Game.SpriteBatch.End();
			} // if
		} // DrawEditorIcon(gameTime)
#endif
		#endregion

		#region Clone
		/// <summary>
		/// Clone
		/// </summary>
		public override BasicObject Clone()
		{
			SkeletalMeshActor result = new SkeletalMeshActor();
			InternalClone(result);
			return result;
		} // Clone()

		/// <summary>
		/// Internal clone
		/// </summary>
		protected override void InternalClone(AlkaronEngine.Scene.BasicObject newObject)
		{
			base.InternalClone(newObject);

			SkeletalMeshActor newMesh = newObject as SkeletalMeshActor;
			newMesh.mesh = this.mesh;
		} // InternalClone(newObject)
		#endregion

		#region RenderSkeleton
		/// <summary>
		/// Render skeleton-
		/// </summary>
		public void RenderSkeleton(bool bRenderBoneNames, //Screen drawScreen,
			List<string> setBoneNames)
		{
			// Refresh the current animation
			SkeletalMesh.CurrentAnimationNum = CurrentAnimationFrameNum;
			SkeletalMesh.Update();

			if (bRenderBoneNames && setBoneNames == null)
				setBoneNames = SkeletalMesh.GetBoneNames();

			SkeletalMesh.RenderSkeleton(WorldMatrix);

			if (bRenderBoneNames && setBoneNames != null)
			{
				for (int i = 0; i < setBoneNames.Count; i++)
				{
					Vector3 bonePosition = GetBoneWorldLocation(setBoneNames[i], false);

					Vector3 screenPos = drawScreen.MainScene.Camera.Project(bonePosition);

					//drawScreen.WriteText(setBoneNames[i], screenPos.X, screenPos.Y);
				}
			} // if (boneNames)
		}
		#endregion // RenderSkeleton()

		#region GetBoneWorldLocation
		/// <summary>
		/// Get bone world location
		/// </summary>
		public Vector3 GetBoneWorldLocation(string BoneName, bool bNeedsUpdate)
		{
			if (bNeedsUpdate)
			{
				// Refresh the current animation
				SkeletalMesh.CurrentAnimationNum = CurrentAnimationFrameNum;
				SkeletalMesh.Update();
			} // if (bNeedsUpdate)

			Vector3 localPosition = SkeletalMesh.GetBoneLocation(BoneName);
			return Vector3.Transform(localPosition, WorldMatrix);
		}
		#endregion // GetBoneWorldLocation()

#if WINDOWS
		#region GetReferencedAssets
		/// <summary>
		/// Get referenced assets
		/// </summary>
		internal override List<IAsset> GetReferencedAssets()
		{
			List<IAsset> result = base.GetReferencedAssets();
			result.Add(SkeletalMesh);
			return result;
		} // GetReferencedAssets()
		#endregion
#endif
	} // class SkeletalMeshActor
} // namespace AlkaronEngine.Scene

using Microsoft.Xna.Framework;

namespace AlkaronEngine.Scene
{
    [Spawnable]
	class PointLightActor : LightActor
	{
		protected PointLightActor()
		{
		}

		internal PointLightActor(BaseScene setOwnerScene)
			: base(setOwnerScene)
		{
			bIsPhysical = false;
			bIsStatic = true;

			lightColor = new Vector3(0.3f, 0.3f, 0.3f);
			linearAttenuation = 2.0f;
			LightRadius = 200.0f;
		}

		/// <summary>
		/// Ray/BoundingSphere check
		/// </summary>
		internal override bool Hit(Ray castRay, out float distance)
		{
			bool bHit = false;
			float closestDistance = float.MaxValue;

			BoundingSphere bs = new BoundingSphere(
				WorldPosition, 20.0f);

			float? dist = bs.Intersects(castRay);

			if (dist != null &&
				(float)dist < closestDistance)
			{
				bHit = true;
				closestDistance = (float)dist;
			}
			distance = closestDistance;

			return bHit;
		}

		/// <summary>
		/// Called after the light has changed its position.
		/// </summary>
		protected override void Moved()
		{
			base.Moved();
		}

		public override BasicObject Clone()
		{
			PointLightActor newPointLight = new PointLightActor();
			InternalClone(newPointLight);
			return newPointLight;
		}

		protected override void InternalClone(BasicObject newObject)
		{
			base.InternalClone(newObject);
		}

		/// <summary>
		/// Will the actor be influenced by this light?
		/// </summary>
		internal override bool CanInfluence(Actor actor)
		{
			return boundingSphere.Intersects(actor.BoundingSphere);
			/*float dist = Vector3.DistanceSquared(
				this.WorldPosition, actor.WorldPosition);
				
			return (dist <= lightRadiusSquared);
			*/
		}

		#region Draw
		/// <summary>
		/// Render the light info
		/// </summary>
		internal override void Draw(GameTime gameTime)
		{
			base.Draw(gameTime);

#if (IS_EDITOR)
			if (Program.Game.IsEditor)
			{
				Program.Game.BeginRenderSelectionBuffer(IsSelected);
			}

			if (Program.Game.IsEditor)
			{
				// We don't want the lines in the selection buffer
				Program.Game.EndRenderSelectionBuffer();

				if (IsSelected)
				{
					PrimitiveRenderer.DrawCircle(
						this.WorldPosition, LightRadius, new Vector3(1, 0, 0),
						new Vector4(1, 1, 1, 1), 32);
					PrimitiveRenderer.DrawCircle(
						this.WorldPosition, LightRadius, new Vector3(0, 1, 0),
						new Vector4(1, 1, 1, 1), 32);
					PrimitiveRenderer.DrawCircle(
						this.WorldPosition, LightRadius, new Vector3(0, 0, 1),
						new Vector4(1, 1, 1, 1), 32);
				}
			}
#endif
        }
        #endregion

        #region DrawEditorIcon
#if IS_EDITOR
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
					Program.Game.GetSpriteTexture("LightIcon"), scrPos,
					(IsSelected ? HellspawnGame.SelectionColorXNA :
					Microsoft.Xna.Framework.Graphics.Color.White));
				Program.Game.SpriteBatch.End();
			}
		}
#endif
        #endregion
    }
}

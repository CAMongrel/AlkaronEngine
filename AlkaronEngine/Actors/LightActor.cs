using System;
using System.Collections.Generic;

using System.Text;
using Microsoft.Xna.Framework;

namespace AlkaronEngine.Scene
{
	public abstract class LightActor : Actor
	{
		#region LightColor
		protected Vector3 lightColor;
		/// <summary>
		/// Color of this light
		/// </summary>
		public Vector3 LightColor
		{
			get
			{
				return lightColor;
			}
			set
			{
				lightColor = value;
			}
		}
		#endregion

		#region LinearAttenuation
		protected float linearAttenuation;
		/// <summary>
		/// Linear light influence attenuation
		/// </summary>
		public float LinearAttenuation
		{
			get
			{
				return linearAttenuation;
			}
			set
			{
				linearAttenuation = value;
			}
		}
		#endregion

		#region LightRadius
		protected float lightRadius;
		/// <summary>
		/// Maximum light radius
		/// </summary>
		public float LightRadius
		{
			get
			{
				return lightRadius;
			}
			set
			{
				lightRadius = value;
				lightRadiusSquared = value * value;
				
				if (boundingSphere == null)
					CreateBoundingSphere();
				else
					boundingSphere.Radius = lightRadius;
			}
		}
		protected float lightRadiusSquared;
		/// <summary>
		/// Maximum light radius (Squared)
		/// </summary>
		public float LightRadiusSquared
		{
			get
			{
				return lightRadiusSquared;
			}
		}
		#endregion

		protected LightActor()
		{
		}

		internal LightActor(BaseScene setOwnerScene)
			: base(setOwnerScene)
		{
		}

		protected override void InternalClone(BasicObject newObject)
		{
			base.InternalClone(newObject);
			
			LightActor newLightAct = newObject as LightActor;
			newLightAct.lightColor = this.lightColor;
			newLightAct.lightRadius = this.lightRadius;
			newLightAct.lightRadiusSquared = this.lightRadiusSquared;
			newLightAct.linearAttenuation = this.linearAttenuation;
		}

		public override void Destroyed()
		{
			
		}

		protected override void Moved()
		{
			boundingSphere.Center = worldPosition;
		
			// TODO!!! ownerScene.RefreshAllLightRefs();
		}

		protected override void CreateBoundingSphere()
		{
			base.CreateBoundingSphere();
			
			boundingSphere = 
				new BoundingSphere(worldPosition, lightRadius);
		}
		
		internal abstract bool CanInfluence(Actor actor);
	}
}

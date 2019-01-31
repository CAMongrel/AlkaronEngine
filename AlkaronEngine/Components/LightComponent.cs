using System;
using System.Collections.Generic;

using System.Text;
using AlkaronEngine.Scene;

namespace AlkaronEngine.Components
{
	public class LightComponent : BaseComponent
    {
		public List<LightActor> InfluencingLights;

		protected LightComponent()
		{
		}

		public LightComponent(Actor setOwner)
			: base(setOwner)
		{
			if (setOwner == null)
				throw new ArgumentNullException(nameof(setOwner));

			Owner = setOwner;

			InfluencingLights = new List<LightActor>();
		}
		
		/// <summary>
		/// Refreshes the light influence data for all lights in 
		/// the scene. Called when the owner actor moves.
		/// </summary>
		public void CacheLightInfluences()
		{
			/*for (int i = 0; i < InfluencingLights.Count; i++)
			{
				InfluencingLights[i].DetachComponent(this);
			}*/
		
			InfluencingLights.Clear();

			for (int i = 0; i < Owner.OwnerScene.AllObjects.Count; i++)
			{
				LightActor light = Owner.OwnerScene.AllObjects[i] as LightActor;
				
				if (light == null)
					continue;
			
				if (light.CanInfluence(this.Owner))
				{
					InfluencingLights.Add(light);
					
					//Owner.OwnerScene.AllLights[i].AttachComponent(this);
				}
			}
		}

		/// <summary>
		/// Refreshes the light influence data for a single light.
		/// Called when the light moves.
		/// </summary>
		public void CacheLightInfluence(LightActor light)
		{
			if (InfluencingLights.Contains(light) == false)
				return;

			// If the light still influences this object, do 
			// nothing and return
			if (light.CanInfluence(this.Owner))
				return;
		
			// Otherwise, remove the influence
			//light.DetachComponent(this);
			InfluencingLights.Remove(light);
		}

		public override Component Clone(Actor newOwner)
		{
			LightComponent result = new LightComponent();
			InternalClone(result, newOwner);
			return result;
		}

		protected override void InternalClone(Component newComponent, Actor newOwner)
		{
			base.InternalClone(newComponent, newOwner);
			
			LightComponent newLightComp = newComponent as LightComponent;
			newLightComp.InfluencingLights = 
				new List<LightActor>(this.InfluencingLights.Count);
			for (int i = 0; i < InfluencingLights.Count; i++)
			{
				newLightComp.InfluencingLights.Add(this.InfluencingLights[i]);
			}
		}
	}
}

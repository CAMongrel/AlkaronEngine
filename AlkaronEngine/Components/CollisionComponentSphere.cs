using System;
using System.Collections.Generic;
using System.Text;
using FarseerGames.FarseerPhysics.Factories;

namespace AlkaronEngine.Components
{
	class CollisionComponentSphere : CollisionComponent
	{
		protected CollisionComponentSphere()
		{
		}

		public CollisionComponentSphere(Actor setOwner)
			: base(setOwner)
		{
			collisionType = CollisionType.BoundingSphere;
		}
		
		public override void CreatePhysicsBody(params object[] args)
		{
			if (args.Length != 1 ||
				Owner.IsPhysical == false)
				return;
				
			float radius = (float)args[0];

			physicsBody = BodyFactory.Instance.CreateCircleBody(
				Owner.OwnerScene.PhysicsSim, radius, 1.0f);
			physicsBody.Tag = this;

			physicsGeom = GeomFactory.Instance.CreateCircleGeom(
				Owner.OwnerScene.PhysicsSim, physicsBody, radius, 64, 0);
			physicsGeom.Tag = this;

			physicsBody.MomentOfInertia = float.PositiveInfinity;

			// Static physics?
			physicsBody.IsStatic = Owner.IsStatic;

			base.CreatePhysicsBody(args);
		}

		public override Component Clone(Actor newOwner)
		{
			CollisionComponentSphere result = new CollisionComponentSphere();
			InternalClone(result, newOwner);
			return result;
		}
	}
}

using System;
using System.Collections.Generic;
using System.Text;
using FarseerGames.FarseerPhysics.Factories;
using Microsoft.Xna.Framework;

namespace AlkaronEngine.Components
{
	class CollisionComponentEllipse : CollisionComponent
	{
		protected CollisionComponentEllipse()
		{
		}

		public CollisionComponentEllipse(Actor setOwner)
			: base(setOwner)
		{
			collisionType = CollisionType.BoundingEllipse;
		}

		public override void CreatePhysicsBody(params object[] args)
		{
			if (args.Length != 2 ||
				Owner.IsPhysical == false)
				return;

			float radiusX = (float)args[0];
			float radiusY = (float)args[1];

			physicsBody = BodyFactory.Instance.CreateEllipseBody(
				Owner.OwnerScene.PhysicsSim, radiusX, radiusY, 1.0f);
			physicsBody.Tag = this;

			physicsGeom = GeomFactory.Instance.CreateEllipseGeom(
				Owner.OwnerScene.PhysicsSim, physicsBody, radiusX, radiusY, 64,
				new Vector2(0, radiusY), 0.0f, 0.0f);
			physicsGeom.Tag = this;

			physicsBody.MomentOfInertia = float.PositiveInfinity;

			// Static physics?
			physicsBody.IsStatic = Owner.IsStatic;

			base.CreatePhysicsBody(args);
		}

		public override Component Clone(Actor newOwner)
		{
			CollisionComponentEllipse result = new CollisionComponentEllipse();
			InternalClone(result, newOwner);
			return result;
		}
	}
}

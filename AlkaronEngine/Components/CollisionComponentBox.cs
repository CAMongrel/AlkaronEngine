using System;
using System.Collections.Generic;
using System.Text;
using FarseerGames.FarseerPhysics.Factories;

namespace AlkaronEngine.Components
{
	class CollisionComponentBox : CollisionComponent
	{
		protected CollisionComponentBox()
		{
		}

		public CollisionComponentBox(Actor setOwner)
			: base(setOwner)
		{
			collisionType = CollisionType.BoundingBox;
		}

		public override void CreatePhysicsBody(params object[] args)
		{
			if (args.Length != 2 ||
				Owner.IsPhysical == false)
				return;

			float width = (float)args[0];
			float height = (float)args[1];

			physicsBody = BodyFactory.Instance.CreateRectangleBody(
				Owner.OwnerScene.PhysicsSim, width, height, 1.0f);
			physicsBody.Tag = this;

			physicsGeom = GeomFactory.Instance.CreateRectangleGeom(
				Owner.OwnerScene.PhysicsSim, physicsBody, width, height, 0);
			physicsGeom.Tag = this;

			physicsBody.MomentOfInertia = float.PositiveInfinity;

			// Static physics?
			physicsBody.IsStatic = Owner.IsStatic;

			base.CreatePhysicsBody(args);
		}

		public override Component Clone(Actor newOwner)
		{
			CollisionComponentBox result = new CollisionComponentBox();
			InternalClone(result, newOwner);
			return result;
		}
	}
}

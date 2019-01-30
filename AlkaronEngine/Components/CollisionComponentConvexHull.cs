using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics.Factories;

namespace AlkaronEngine.Components
{
	class CollisionComponentConvexHull : CollisionComponent
	{
		protected CollisionComponentConvexHull()
		{
		}

		public CollisionComponentConvexHull(Actor setOwner)
			: base(setOwner)
		{
			collisionType = CollisionType.ConvexHull;
		}

		public override void CreatePhysicsBody(params object[] args)
		{
			if (args.Length != 1 ||
				Owner.IsPhysical == false)
				return;

			Vector2[] collisionVertices = args[0] as Vector2[];

			Vertices verts = new Vertices(collisionVertices);
			verts = verts.GetConvexHull();

			// Add the first vertex again for easier rendering
			verts.Add(verts[0]);
			
			physicsBody = BodyFactory.Instance.CreatePolygonBody(
				Owner.OwnerScene.PhysicsSim, verts, 1.0f);
			physicsBody.Tag = this;

			physicsGeom = GeomFactory.Instance.CreatePolygonGeom(
				Owner.OwnerScene.PhysicsSim, physicsBody, verts, 0);
			physicsGeom.Tag = this;

			physicsBody.MomentOfInertia = float.PositiveInfinity;

			// Static physics?
			physicsBody.IsStatic = Owner.IsStatic;

			base.CreatePhysicsBody(args);
		}

		public override Component Clone(Actor newOwner)
		{
			CollisionComponentConvexHull result = new CollisionComponentConvexHull();
			InternalClone(result, newOwner);
			return result;
		}
	}
}

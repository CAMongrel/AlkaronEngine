using Microsoft.Xna.Framework;

namespace AlkaronEngine.Components
{
    class CollisionComponentVertices : CollisionComponent
	{
		protected CollisionComponentVertices()
		{
		}

		public CollisionComponentVertices(Actor setOwner)
			: base(setOwner)
		{
			collisionType = CollisionType.Vertices;
		}

		public override void CreatePhysicsBody(params object[] args)
		{
			if (args.Length != 1 ||
				Owner.IsPhysical == false)
            {
                return;
            }

            Vector2[] collisionVertices = args[0] as Vector2[];

			Vertices verts = new Vertices(collisionVertices);
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
			CollisionComponentVertices result = new CollisionComponentVertices();
			InternalClone(result, newOwner);
			return result;
		}
	}
}

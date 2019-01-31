using System;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using AlkaronEngine.Actors;
using BepuPhysics.Collidables;
using AlkaronEngine.Scene;

namespace AlkaronEngine.Components
{
	public enum CollisionType
	{
		BoundingSphere,
		BoundingBox,
		ConvexHull,
		Vertices,
		SkeletonBox,
		BoundingEllipse
	}

	public abstract class CollisionComponent : BaseComponent
	{
        public IShape PhysicsShape { get; protected set; }

        #region Members
        /*protected Body physicsBody;
		/// <summary>
		/// Physics body object. Handles forces.
		/// </summary>
		public Body PhysicsBody
		{
			get
			{
				return physicsBody;
			}
		}

		[NoStore]
		protected Geom physicsGeom;
		/// <summary>
		/// Physics geometry object. Handles collision.
		/// </summary>
		public Geom PhysicsGeom
		{
			get
			{
				return physicsGeom;
			}
		}

		protected int collisionGroup;
		public int CollisionGroup
		{
			get
			{
				return collisionGroup;
			}
			set
			{
				if (collisionGroup == value)
					return;

				collisionGroup = value;

				physicsGeom.CollisionGroup = collisionGroup;
			}
		}

		protected CollisionArea collisionArea;
		public CollisionArea CollisionArea
		{
			get
			{
				return collisionArea;
			}
			set
			{
				if (collisionArea == value)
					return;

				collisionArea = value;
				ApplyCollisionArea();
			}
		}*/

        protected CollisionType collisionType;
		public CollisionType CollisionType
		{
			get
			{
				return collisionType;
			}
		}
		#endregion

        protected CollisionComponent()
		{
		}

		public CollisionComponent(Actor setOwner)
			: base(setOwner)
		{
		}

		/*public override void Destroy()
		{
			physicsBody.Dispose();
			physicsBody = null;

			physicsGeom.Dispose();
			physicsGeom = null;
		}

		#region InternalClone
		protected override void InternalClone(Component newComponent, Actor newOwner)
		{
			base.InternalClone(newComponent, newOwner);

			CollisionComponent newCollComp = newComponent as CollisionComponent;
			newCollComp.collisionArea = this.collisionArea;
			newCollComp.collisionGroup = this.collisionGroup;
			newCollComp.collisionType = this.collisionType;

			newCollComp.physicsBody = BodyFactory.Instance.CreateBody(physicsBody);
			newCollComp.physicsGeom = GeomFactory.Instance.CreateGeom(
				newCollComp.physicsBody, physicsGeom);
		}
		#endregion

		#region ApplyCollisionArea
		void ApplyCollisionArea()
		{
			switch (collisionArea)
			{
				case CollisionArea.Background:
					physicsGeom.CollisionCategories = CollisionCategory.Cat20;
					physicsGeom.CollidesWith = CollisionCategory.Cat20;
					break;

				case CollisionArea.Game:
					physicsGeom.CollisionCategories = CollisionCategory.Cat10;
					physicsGeom.CollidesWith = CollisionCategory.Cat10;
					break;

				case CollisionArea.Foreground:
					physicsGeom.CollisionCategories = CollisionCategory.Cat1;
					physicsGeom.CollidesWith = CollisionCategory.Cat1;
					break;
			}
		}
		#endregion

		#region CreatePhysicsBody
		/// <summary>
		/// Creates the collision component in the physics engine.
		/// </summary>
		public virtual void CreatePhysicsBody(params object[] args)
		{
			physicsGeom.OnCollision = new CollisionEventHandler(OnCollide);
			physicsGeom.OnSeparation = new SeparationEventHandler(OnSeparate);

			physicsGeom.FrictionCoefficient = 0.8f;

			physicsGeom.CollisionGroup = collisionGroup;
			ApplyCollisionArea();
		}
		#endregion

		#region DrawPhysicsGeom
		/// <summary>
		/// Renders the vertices of the physics engine making up the 
		/// collision component.
		/// </summary>
		public void DrawPhysicsGeom()
		{
			if (physicsGeom == null)
				return;

			// Init the shader and begin rendering
			PhysicsEffect.Singleton.Begin();

			// Get the current world vertices from the physics engine for rendering
			Vector2[] verts = physicsGeom.WorldVertices.ToArray();

			int numPrimitives;
			PrimitiveType primType;

			switch (collisionType)
			{
				default:
				// Using TriangleList is not correct for BoundingSphere, but gives 
				// a nice visual effect
				case CollisionType.BoundingSphere:
				case CollisionType.Vertices:
					numPrimitives = verts.Length / 3;
					primType = PrimitiveType.TriangleList;
					break;

				case CollisionType.BoundingBox:
				case CollisionType.ConvexHull:
					numPrimitives = verts.Length - 1;
					primType = PrimitiveType.LineStrip;
					break;
			}

			Program.Game.GraphicsDevice.DrawUserPrimitives<Vector2>(
				primType, verts, 0, numPrimitives);

			PhysicsEffect.Singleton.End();
		}
		#endregion

		#region PostLoad
		public override void PostLoad()
		{
			base.PostLoad();
		}
		#endregion

		#region OnCollide
		bool OnCollide(Geom geometry1, Geom geometry2, ContactList contactList)
		{
			CollisionComponent comp1 = (geometry1.Tag as CollisionComponent);
			CollisionComponent comp2 = (geometry2.Tag as CollisionComponent);

			Actor actor1 = null;
			if (comp1 != null)
				actor1 = comp1.Owner;
			Actor actor2 = null;
			if (comp2 != null)
				actor2 = comp2.Owner;

			return Owner.OnCollide(actor1, actor2, contactList);
		}
		#endregion

		#region OnSeparate
		void OnSeparate(Geom geometry1, Geom geometry2)
		{
			CollisionComponent comp1 = (geometry1.Tag as CollisionComponent);
			CollisionComponent comp2 = (geometry2.Tag as CollisionComponent);

			Actor actor1 = null;
			if (comp1 != null)
				actor1 = comp1.Owner;
			Actor actor2 = null;
			if (comp2 != null)
				actor2 = comp2.Owner;

			Owner.OnSeparate(actor1, actor2);
		}
		#endregion

		#region ApplyForce
		internal void ApplyForce(Vector2 forceVector)
		{
			if (physicsBody == null)
				return;

			physicsBody.ApplyForce(forceVector);
		}
		#endregion

		#region ApplyImpulse
		internal void ApplyImpulse(Vector2 impulseVector)
		{
			if (physicsBody == null)
				return;

			physicsBody.ApplyImpulse(impulseVector);
		}
		#endregion

		#region SetScale
		internal void SetScale(Vector3 newScale)
		{
			physicsGeom.Matrix =
				Matrix.CreateScale(newScale);
		}
		#endregion
        */
	}
}

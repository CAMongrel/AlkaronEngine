using System;
using System.Collections.Generic;
using System.Text;

namespace AlkaronEngine.Components
{
	class CollisionComponentSkeletonBox : CollisionComponent
	{
		protected CollisionComponentSkeletonBox()
		{
		}

		public CollisionComponentSkeletonBox(Actor setOwner)
			: base(setOwner)
		{
			collisionType = CollisionType.SkeletonBox;
		}

		public override void CreatePhysicsBody(params object[] args)
		{
			if (args.Length != 1 ||
				Owner.IsPhysical == false)
				return;
		}

		public override Component Clone(Actor newOwner)
		{
			CollisionComponentSkeletonBox result = new CollisionComponentSkeletonBox();
			InternalClone(result, newOwner);
			return result;
		}
	}
}

// Project: Hellspawn

#region Using directives
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel;
#endregion

namespace AlkaronEngine.Scene
{
	public abstract class Actor : BasicObject
	{
		#region Variables
		/// <summary>
		/// List of all attached components
		/// </summary>
		protected List<Component> AllComponents;
		
		protected BoundingSphere boundingSphere;
		public BoundingSphere BoundingSphere
		{
			get
			{
				return boundingSphere;
			}
		}

		private string currentState;
		/// <summary>
		/// The name of the current state
		/// </summary>
		public string CurrentState 
		{
			get
			{
				return currentState;
			}
		}
		#endregion

		#region Constructor
		/// <summary>
		/// This constructor is called by the engine when the object is deserialized.
		/// </summary>
		protected Actor()
		{
		}

		/// <summary>
		/// This constructor will be called at runtime, when objects are spawned
		/// dynamically.
		/// </summary>
		public Actor(BaseScene setOwnerScene)
			: base(setOwnerScene)
		{
			bIsTickable = true;
			bIsPhysical = true;
			
			AllComponents = new List<Component>();

			currentState = "Idle";
		}
		#endregion

		#region AttachComponent
		internal void AttachComponent(Component comp)
		{
			AllComponents.Add(comp);
		}
		#endregion
		
		#region Count
		/// <summary>
		/// Number of attached components
		/// </summary>
		public int ComponentCount
		{
			get
			{
				return AllComponents.Count;
			}
		}
		#endregion

		#region Tick
		public override bool Tick(GameTime gameTime)
		{
			// Tick the current state (if we have one)
			TickState(gameTime);
			
			return true;
		}
		#endregion

		#region PostLoad
		/// <summary>
		/// Calls PostLoad on all attached components. Automatically called by the engine 
		/// after all objects were deserialized.
		/// </summary>
		public override void PostLoad()
		{
			base.PostLoad();

			if (string.IsNullOrEmpty(currentState))
				currentState = "Idle";

			CreateBoundingSphere();
			
			for (int i = 0; i < AllComponents.Count; i++)
			{
				if (AllComponents[i] == null)
					continue;
			
				AllComponents[i].PostLoad();
			}
		}
		#endregion

		#region State handling
		/// <summary>
		/// Go to state. Does nothing if the actor is already in
		/// the specified state.
		/// </summary>
		public void GoToState(string name)
		{
			if (currentState.ToLower() == name.ToLower())
				return;

			EndState(currentState, name);
			string prev = currentState;
			currentState = name;
			BeginState(currentState, prev);
		} // GoToState(name)

		protected virtual void BeginState(string currentState, string previousState)
		{
			Console.WriteLine("Entering state: " + currentState);
		}

		protected virtual void TickState(GameTime gameTime)
		{
		}

		protected virtual void EndState(string currentState, string nextState)
		{
			Console.WriteLine("Leaving state: " + currentState);
		}
		#endregion

		#region InternalClone
		protected override void InternalClone(BasicObject newObject)
		{
			base.InternalClone(newObject);
			
			Actor newActor = newObject as Actor;
			newActor.boundingSphere = this.boundingSphere;
			
			newActor.AllComponents = new List<Component>(this.AllComponents.Count);
			for (int i = 0; i < this.AllComponents.Count; i++)
			{
				if (this.AllComponents[i] == null)
					newActor.AllComponents.Add(null);
				else
					newActor.AllComponents.Add(this.AllComponents[i].Clone(newActor));
			}
		}
		#endregion
		
		#region CreateBoundingSphere
		/// <summary>
		/// Creates the bounding sphere for this actor.
		/// </summary>
		protected virtual void CreateBoundingSphere()
		{
			boundingSphere = new BoundingSphere(Vector3.Zero, 0.0f);
		}
		#endregion

		#region Spawned
		/// <summary>
		/// Called when the objects gets spawned dynamically,
		/// either in the editor or in game.
		/// </summary>
		protected override void Spawned()
		{
			base.Spawned();
			
			CreateBoundingSphere();
		}
		#endregion
		
		#region RotateAround
		/// <summary>
		/// Rotate around
		/// </summary>
		public void RotateAround(float yaw, float pitch, float roll, Vector3 position)
		{
			Matrix rotMat = Matrix.CreateFromYawPitchRoll(yaw, pitch, roll);
			
			Matrix transformMat = 
				Matrix.CreateTranslation(-position) *  
				rotMat * 
				Matrix.CreateTranslation(position);
				
			worldPosition = Vector3.Transform(worldPosition, transformMat);
		} // RotateAround(yaw, pitch, roll)
		#endregion

		public override void FinalizeDestroy()
		{
			for (int i = 0; i < AllComponents.Count; i++)
			{
				AllComponents[i].Destroy();
			}
			AllComponents.Clear();
		}
	}
}

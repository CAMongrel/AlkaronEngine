// Project: Hellspawn

using System.Collections.Generic;
using AlkaronEngine.Assets;
using Microsoft.Xna.Framework;

namespace AlkaronEngine.Scene
{
    /// <summary>
    /// Basic object
    /// </summary>
    public abstract class BasicObject : ILoadableObject
    {
        #region Variables
        /// <summary>
        /// Will this object be ticked by the scene?
        /// </summary>
        protected bool bIsTickable;

        /// <summary>
        /// Will this object be added to the physics engine?
        /// </summary>
        protected bool bIsPhysical;

        /// <summary>
        /// Static physics?
        /// </summary>
        protected bool bIsStatic;

        /// <summary>
        /// Scene owning this object
        /// </summary>
        protected BaseScene ownerScene;

        /// <summary>
        /// Name of this object
        /// </summary>
        protected string name;

        /// <summary>
        /// World position of the previous tick
        /// </summary>
        [NoStore]
        protected Vector3 prevWorldPosition;

        /// <summary>
        /// Position of this object in the world
        /// </summary>
        protected Vector3 worldPosition;

        /// <summary>
        /// Orientation of this object in the world.
        /// </summary>
        protected Quaternion worldOrientation;

        /// <summary>
        /// Scaling of this object in the world.
        /// </summary>
        protected Vector3 worldScaling;
        #endregion

        #region Properties
        #region IsTickable
        /// <summary>
        /// Will this object be ticked by the scene?
        /// </summary>
        public bool IsTickable
        {
            get
            {
                return bIsTickable;
            }
        } // IsTickable
        #endregion

        #region IsPhysical
        /// <summary>
        /// Will this object be added to this physics engine?
        /// </summary>
        public bool IsPhysical
        {
            get
            {
                return bIsPhysical;
            }
        } // IsPhysical
        #endregion

        #region IsStatic
        /// <summary>
        /// Static physics?
        /// </summary>
        public bool IsStatic
        {
            get
            {
                return bIsStatic;
            }
        } // IsStatic
        #endregion

        #region Name
        /// <summary>
        /// Name of this object
        /// </summary>
        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
            }
        } // Name
        #endregion

        #region OwnerScene
        /// <summary>
        /// Scene owning this object
        /// </summary>
        public BaseScene OwnerScene
        {
            get
            {
                return ownerScene;
            }
        } // OwnerScene
        #endregion

        #region WorldPosition
        /// <summary>
        /// Position of this object in the world
        /// </summary>
        public Vector3 WorldPosition
        {
            get
            {
                return worldPosition;
            }
            set
            {
                SetPosition(value);
            }
        } // WorldPosition
        #endregion

        #region PrevWorldPosition
        /// <summary>
        /// World position of the previous tick
        /// </summary>
        public Vector3 PrevWorldPosition
        {
            get
            {
                return prevWorldPosition;
            }
        } // PrevWorldPosition
        #endregion

        #region WorldScaling
        /// <summary>
        /// Scaling of the object in the world
        /// </summary>
        public Vector3 WorldScaling
        {
            get
            {
                return worldScaling;
            }
            set
            {
                SetScale(value);
            }
        } // WorldScaling
        #endregion

        #region WorldMatrix
        /// <summary>
        /// World matrix for rendering this object
        /// </summary>
        public Matrix WorldMatrix
        {
            get
            {
                return
                    Matrix.CreateScale(worldScaling) *
                    // TODO!!! Matrix.CreateRotationZ(worldRotation) *
                    Matrix.CreateTranslation(WorldPosition);
            }
        } // WorldMatrix
        #endregion
        #endregion

        #region BasicObject
        /// <summary>
        /// Create basic object
        /// </summary>
        protected BasicObject()
        {
        } // BasicObject()
        #endregion

        #region BasicObject
        /// <summary>
        /// ctor
        /// </summary>
        internal BasicObject(BaseScene setOwnerScene)
        {
            ownerScene = setOwnerScene;

            bIsTickable = false;
            bIsPhysical = false;
            bIsStatic = true;

            worldPosition = new Vector3();
            prevWorldPosition = worldPosition;
            worldOrientation = new Quaternion();
            worldScaling = new Vector3(1, 1, 1);

            // Autogenerate name
            name = GetType().Name + "_" + ownerScene.Count;

            ownerScene.SceneGraph.AddObject(this);

            Spawned();
        } // BasicObject(setOwnerScene)
        #endregion

        #region Spawned
        /// <summary>
        /// Called after the object has been spawned dynamically at
        /// runtime; e.g. in game script or in the editor.
        /// Not called when loaded from harddisk.
        /// </summary>
        protected virtual void Spawned()
        {
            prevWorldPosition = worldPosition;
        } // Spawned()
        #endregion

        #region Moved
        /// <summary>
        /// Called after the object has changed its position
        /// </summary>
        protected virtual void Moved()
        {
        } // Moved()
        #endregion

        #region Destroyed
        /// <summary>
        /// Called after the object has been removed from the scene.
        /// "ownerScene" is NO longer valid!
        /// </summary>
        public virtual void Destroyed()
        {
        } // Destroyed()
        #endregion

        #region Tick
        /// <summary>
        /// Tick function
        /// </summary>
        public virtual bool Tick(GameTime gameTime)
        {
            return true;
        } // Tick(gameTime)
        #endregion

        #region Draw
        /// <summary>
        /// Rendering function
        /// </summary>
        internal virtual void Draw(GameTime gameTime)
        {
        } // Draw(gameTime)
        #endregion

        #region SetPosition
        /// <summary>
        /// Sets the world position
        /// </summary>
        /// <param name="newPosition"></param>
        internal virtual void SetPosition(Vector3 newPosition)
        {
            prevWorldPosition = worldPosition;
            worldPosition = newPosition;

            Moved();
        } // SetPosition(newPosition)
        #endregion

        #region SetScale
        /// <summary>
        /// Set scale
        /// </summary>
        internal virtual void SetScale(Vector3 newScale)
        {
            worldScaling = newScale;
        } // SetScale(newScale)
        #endregion

        #region Hit
        /// <summary>
        /// Ray/Object hit detection
        /// </summary>
        internal virtual bool Hit(Ray castRay, out float distance)
        {
            distance = float.MaxValue;
            return false;
        } // Hit(castRay, distance)
        #endregion

        #region ILoadableObject Member
        /// <summary>
        /// Post load
        /// </summary>
        public virtual void PostLoad()
        {
            // obs: Should not be neccessary. The deserialization should
            // create a correctly sized array with all object references
            // already set.
            //ownerScene.AddObject(this);

            if (worldScaling == Vector3.Zero)
                worldScaling = Vector3.One;

            prevWorldPosition = worldPosition;
        } // PostLoad()
        #endregion

        #region Clone
        /// <summary>
        /// Clone
        /// </summary>
        public abstract BasicObject Clone();

        /// <summary>
        /// Internal clone
        /// </summary>
        protected virtual void InternalClone(BasicObject newObject)
        {
            newObject.worldPosition = this.worldPosition;
            newObject.worldOrientation = this.worldOrientation;
            newObject.worldScaling = this.worldScaling;
            newObject.bIsPhysical = this.bIsPhysical;
            newObject.bIsStatic = this.bIsStatic;
            newObject.bIsTickable = this.bIsTickable;
            newObject.name = GetType().Name + "_" + ownerScene.Count;
            newObject.ownerScene = this.ownerScene;
            newObject.prevWorldPosition = this.prevWorldPosition;

            newObject.ownerScene.SceneGraph.AddObject(newObject);
        } // InternalClone(newObject)
        #endregion

        #region Destroy
        /// <summary>
        /// Destroy
        /// </summary>
        public void Destroy()
        {
            if (ownerScene == null)
                return;

            ownerScene.SceneGraph.DestroyObject(this);
        } // Destroy()
        #endregion

        #region GetReferencedAssets
        /// <summary>
        /// Get referenced assets
        /// </summary>
        internal virtual List<IAsset> GetReferencedAssets()
        {
            return new List<IAsset>();
        } // GetReferencedAssets()
        #endregion

        #region ToString
        /// <summary>
        /// To string
        /// </summary>
        public override string ToString()
        {
            return Name;
        } // ToString()
        #endregion

        #region FinalizeDestroy
        /// <summary>
        /// Finalize destroy
        /// </summary>
        public virtual void FinalizeDestroy()
        {
            //
        } // FinalizeDestroy()
        #endregion
    } // class BasicObject
} // namespace AlkaronEngine.Scene

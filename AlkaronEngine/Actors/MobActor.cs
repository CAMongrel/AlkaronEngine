using System;
namespace AlkaronEngine.Actors
{
    public enum MovementType
    {
        None,
        Walking,
        Running,
        Swimming,
        Flying,
        Other
    }

    /// <summary>
    /// Basic actor for all mobile entities (like monsters, players etc.)
    /// Contains basic movement logic for walking, running, swimming, flying
    /// </summary>
    public class MobActor : BaseActor
    {
        public MovementType MovementType { get; private set; }

        public MobActor()
        {
            MovementType = MovementType.None;
        }

        #region Movement
        public virtual void MoveForward()
        {
            if (MovementType == MovementType.None)
            {
                return;
            }
        }

        public virtual void MoveBackward()
        {
            if (MovementType == MovementType.None)
            {
                return;
            }
        }

        public virtual void StrafeLeft()
        {
            if (MovementType == MovementType.None)
            {
                return;
            }
        }

        public virtual void StrafeRight()
        {
            if (MovementType == MovementType.None)
            {
                return;
            }
        }
        #endregion
    }
}

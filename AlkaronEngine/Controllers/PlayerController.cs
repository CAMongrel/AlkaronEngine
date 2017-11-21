using System;
using AlkaronEngine.Actors;
using AlkaronEngine.Input;
using Microsoft.Xna.Framework;

namespace AlkaronEngine.Controllers
{
    public class PlayerController : BaseController
    {
        public PlayerController()
        {
        }

        public override bool OnKeyEvent(Microsoft.Xna.Framework.Input.Keys key, KeyEventType eventType)
        {
            if (PossessedActor == null)
            {
                return false;
            }

            MobActor mob = PossessedActor as MobActor;
            if (mob == null)
            {
                return false;
            }

            return base.OnKeyEvent(key, eventType);
        }
    }
}

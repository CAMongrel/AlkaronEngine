using AlkaronEngine.Actors;
using Veldrid;

namespace AlkaronEngine.Controllers
{
   public class PlayerController : BaseController
   {
      public PlayerController()
      {
      }

      public override bool KeyPressed(Key key)
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

         return base.KeyPressed(key);
      }
   }
}

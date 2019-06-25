﻿using AlkaronEngine.Actors;
using AlkaronEngine.Input;
using System.Numerics;
using Veldrid;

namespace AlkaronEngine.Controllers
{
   public class BaseController
   {
      public BaseActor PossessedActor { get; private set; }

      public BaseController()
      {
      }

      public void Possess(BaseActor actor)
      {
         PossessedActor = actor;
      }

      public virtual bool PointerDown(Vector2 position, PointerType pointerType)
      {
         // Base implementation does nothing
         return false;
      }

      public virtual bool PointerUp(Vector2 position, PointerType pointerType)
      {
         // Base implementation does nothing
         return false;
      }

      public virtual bool PointerMoved(Vector2 position)
      {
         // Base implementation does nothing
         return false;
      }

      public virtual bool PointerWheelChanged(Vector2 position)
      {
         // Base implementation does nothing
         return false;
      }

      public virtual bool KeyPressed(Key key)
      {
         // Base implementation does nothing
         return false;
      }

      public virtual bool KeyReleased(Key key)
      {
         // Base implementation does nothing
         return false;
      }
   }
}

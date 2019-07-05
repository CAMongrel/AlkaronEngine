using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using AlkaronEngine.Util;
using Veldrid;

namespace AlkaronEngine.Gui
{
   public delegate void AnimationStateChange(UIAnimation animation, UIBaseComponent component);

   public enum UIAnimationPriority
   {
      /// <summary>
      /// Defines an animation that provides no game relevant information,
      /// essentially running in the background from a user perspective.
      /// </summary>
      Background,
      /// <summary>
      /// Declares an animation as user relevant. The animation should complete
      /// before the user takes another action. This allows coupling animation
      /// states with game logic.
      /// </summary>
      Foreground,
   }

   public class UIAnimation
   {
      private enum EndState
      {
         Finished,
         Aborted
      }

      /// <summary>
      /// Currently active animations groups. This will delay start of another
      /// animation of an already playing group.
      /// </summary>
      private static HashSet<string> activeGroups;
      /// <summary>
      /// The full list of currently active animations
      /// </summary>
      private static List<UIAnimation> activeAnimations;

      private readonly object lockObj = new object();

      public string Name { get; set; }
      public UIBaseComponent Component { get; set; }
      public double AnimationDelay { get; private set; }
      public double AnimationDuration { get; private set; }

      public UIAnimationPriority Priority { get; private set; }

      public string Group { get; private set; }

      private AnimationStateChange animationFinished;
      private AnimationStateChange animationFinalState;
      private AnimationStateChange animationAborted;

      private Dictionary<string, object> initialValues;
      private Dictionary<string, object> finalValues;

      private DateTimeOffset animationActivateTime;

      public bool IsActive { get; private set; }

      static UIAnimation()
      {
         activeGroups = new HashSet<string>();
         activeAnimations = new List<UIAnimation>();
      }

      public UIAnimation(string setName, double setAnimationDelay, double setAnimationDuration,
                         AnimationStateChange setAnimationFinalState, AnimationStateChange setAnimationFinished = null,
                         AnimationStateChange setAnimationAborted = null,
                         string setAnimationGroup = null, UIAnimationPriority setPriority = UIAnimationPriority.Background)
      {
         Contract.Requires(setAnimationDelay >= 0.0);
         Contract.Requires(setAnimationDuration >= 0.0);

         Name = setName;
         AnimationDelay = setAnimationDelay;
         AnimationDuration = setAnimationDuration;
         Priority = setPriority;

         Group = setAnimationGroup;

         animationFinished = setAnimationFinished;
         animationFinalState = setAnimationFinalState;
         animationAborted = setAnimationAborted;
      }

      public void Activate(double deltaTime)
      {
         lock (lockObj)
         {
            if (IsActive == true || Component == null)
            {
               return;
            }

            if (Group != null && activeGroups.Contains(Group))
            {
               // Group is already active, cannot activate (yet)
               return;
            }

            // Add Group to activeGroups, preventing other animations of the
            // same group to be started until this animation has finished
            activeGroups.Add(Group);

            activeAnimations.Add(this);

            initialValues = new Dictionary<string, object>();
            Component.BackupAnimateableProperties(initialValues);

            animationFinalState?.Invoke(this, Component);

            Dictionary<string, object> initialAndChangedValues = new Dictionary<string, object>();
            Component.BackupAnimateableProperties(initialAndChangedValues);

            finalValues = new Dictionary<string, object>();
            // Remove unchanged values
            foreach (var pair in initialAndChangedValues)
            {
               if (pair.Value.Equals(initialValues[pair.Key]) == false)
               {
                  finalValues.Add(pair.Key, pair.Value);
               }
            }

            Component.RestoreAnimateableProperties(initialValues);

            animationActivateTime = DateTimeOffset.UtcNow;
            IsActive = true;
         }
      }

      private void EnterEndState(EndState endState)
      {
         lock (lockObj)
         {
            if (Group != null)
            {
               activeGroups.Remove(Group);
            }

            activeAnimations.Remove(this);

            // Animation is aborted. Stop it.
            IsActive = false;
            switch (endState)
            {
               case EndState.Aborted:
                  animationAborted?.Invoke(this, Component);
                  Component?.AnimationWasAborted(this);
                  break;

               case EndState.Finished:
                  animationFinished?.Invoke(this, Component);
                  Component.AnimationDidFinish(this);
                  break;
            }
            Component = null;
         }
      }

      public void Abort()
      {
         EnterEndState(EndState.Aborted);
      }

      private object Interpolate(double percentage, object initialValue, object finalValue, Type fieldType)
      {
         if (fieldType.FullName == "System.Int32")
         {
            int valueRange = (int)finalValue - (int)initialValue;
            return (int)initialValue + (int)((double)valueRange * percentage);
         }
         if (fieldType.FullName == "System.Int64")
         {
            long valueRange = (long)finalValue - (long)initialValue;
            return (long)initialValue + (long)((double)valueRange * percentage);
         }
         if (fieldType.FullName == "System.Single")
         {
            float valueRange = (float)finalValue - (float)initialValue;
            return (float)initialValue + (float)((float)valueRange * percentage);
         }
         if (fieldType.FullName == "Microsoft.Xna.Framework.Color")
         {
            RgbaFloat initialColor = (RgbaFloat)initialValue;
            RgbaFloat finalColor = (RgbaFloat)finalValue;

            float r = finalColor.R - initialColor.R;
            float g = finalColor.G - initialColor.G;
            float b = finalColor.B - initialColor.B;
            float a = finalColor.A - initialColor.A;

            return new RgbaFloat((float)(initialColor.R + r * percentage),
                                 (float)(initialColor.G + g * percentage),
                                 (float)(initialColor.B + b * percentage),
                                 (float)(initialColor.A + a * percentage));
         }

         return null;
      }

      private object AnimateValue(double percentage, object initialValue, object finalValue)
      {
         Type valueType = initialValue.GetType();

         return Interpolate(percentage, initialValue, finalValue, valueType);
      }

      public virtual void Update(double deltaTime)
      {
         lock (lockObj)
         {
            if (IsActive == false)
            {
               return;
            }

            double currentAnimationTime = ((DateTimeOffset.UtcNow - animationActivateTime).TotalSeconds - AnimationDelay);
            if (currentAnimationTime < 0.0)
            {
               // We are ahead of the animation delay. Don't do anything just yet.
               return;
            }

            double percentage = Math.Clamp((float)(currentAnimationTime / AnimationDuration), 0.0f, 1.0f);

            Type componentType = Component.GetType();
            foreach (KeyValuePair<string, object> pair in finalValues)
            {
               PropertyInfo pi = componentType.GetRuntimeProperty(pair.Key);
               object result = AnimateValue(percentage, initialValues[pair.Key], pair.Value);
               if (result == null)
               {
                  Log.Warning("Can't animate field '" + pair.Key + "': Unhandled type: " + pi.GetType());
                  continue;
               }
               pi.SetValue(Component, result, null);
            }

            if (currentAnimationTime > AnimationDuration)
            {
               EnterEndState(EndState.Finished);
            }
         }
      }

      public static bool IsAnyAnimationActive()
      {
         return activeAnimations.Count > 0;
      }
      public static bool IsAnyAnimationActiveOfPriority(UIAnimationPriority prio)
		{
         return (from a in activeAnimations where a.Priority == prio select a).FirstOrDefault() != null;
		}
   }
}

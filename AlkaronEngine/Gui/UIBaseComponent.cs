#region Using directives
using AlkaronEngine.Graphics2D;
using AlkaronEngine.Graphics3D;
using AlkaronEngine.Input;
using AlkaronEngine.Util;
using BepuUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Veldrid;
#endregion

namespace AlkaronEngine.Gui
{
    public enum UITextAlignHorizontal
    {
        Center,
        Left,
        Right,
    }

    public enum UITextAlignVertical
    {
        Center,
        Top,
        Bottom,
    }

    public enum UIPositionAnchor
    {
        TopLeft,
        TopCenter,
        TopRight,
        CenterLeft,
        Center,
        CenterRight,
        BottomLeft,
        BottomCenter,
        BottomRight
    }

    public enum UISizeMode
    {
        /// <summary>
        /// Use an absolute size. Unit is pixels.
        /// </summary>
        Fixed,
        /// <summary>
        /// Stretch or shrink the size to match the parent size.
        /// </summary>
        Fit
    }

    public delegate void OnPointerDownInside(UIBaseComponent sender, Vector2 position, double deltaTime);
    public delegate void OnPointerUpInside(UIBaseComponent sender, Vector2 position, double deltaTime);
    public delegate void OnPointerUpOutside(UIBaseComponent sender, Vector2 position, double deltaTime);

    public delegate void ModifyLayout(UIBaseComponent component);

    public abstract class UIBaseComponent : IDisposable
    {
        #region Member
        public float X { get; set; }
        public float Y { get; set; }

        private bool isPerformingLayout;
        private bool suppressPerformLayout;

        private float width;
        public float Width
        {
            get { return width; }
            set { width = value; PerformLayout(); }
        }

        private float height;
        public float Height
        {
            get { return height; }
            set { height = value; PerformLayout(); }
        }

        private UISizeMode widthSizeMode;
        public UISizeMode WidthSizeMode
        {
            get { return widthSizeMode; }
            set { if (widthSizeMode != value) { widthSizeMode = value; PerformLayout(); } }
        }
        private UISizeMode heightSizeMode;
        public UISizeMode HeightSizeMode
        {
            get { return heightSizeMode; }
            set { if (heightSizeMode != value) { heightSizeMode = value; PerformLayout(); } }
        }

        public virtual Vector2 PreferredSize
        {
            get
            {
                Vector2 resultSize = new Vector2(Width, Height);
                for (int i = 0; i < components.Count; i++)
                {
                    Vector2 compSize = components[i].PreferredSize;
                    if (compSize.X > resultSize.X)
                    {
                        resultSize.X = compSize.X;
                    }
                    if (compSize.Y > resultSize.Y)
                    {
                        resultSize.Y = compSize.Y;
                    }
                }
                return resultSize;
            }
        }

        /// <summary>
        /// Gets or sets the rotation.
        /// </summary>
        /// <value>The rotation in degrees</value>
        public float Rotation { get; set; }
        /// <summary>
        /// Returns the accumulated rotation of this component and all parent 
        /// components in degrees.
        /// </summary>
        public float CompositeRotation
        {
            get
            {
                if (ParentComponent == null)
                {
                    return Rotation;
                }

                float result = Rotation + ParentComponent.CompositeRotation;
                while (result < 0)
                {
                    result += 360;
                }
                while (result > 360)
                {
                    result -= 360;
                }
                return result;
            }
        }

        public float Alpha { get; set; }
        public float CompositeAlpha
        {
            get
            {
                if (ParentComponent == null)
                {
                    return Alpha;
                }

                return Alpha * ParentComponent.CompositeAlpha;
            }
        }

        public RgbaFloat BackgroundColor { get; set; }

        public bool UserInteractionEnabled { get; set; }
        public bool Visible { get; set; }
        public bool Focusable { get; set; }

        private UIPositionAnchor positionAnchor;
        /// <summary>
        /// Gets or sets the screen position anchor. Defines the relative meaning of X,Y with regards
        /// to the parent control. 
        /// Default is TopLeft.
        /// Setting the same value again has no effect.
        /// </summary>
        public UIPositionAnchor PositionAnchor
        {
            get { return positionAnchor; }
            set { if (positionAnchor != value) { positionAnchor = value; PerformLayout(); } }
        }

        public Vector2 ScreenPosition
        {
            get
            {
                Vector2 parentScreenPos = Vector2.Zero;
                if (ParentComponent != null)
                {
                    parentScreenPos = ParentComponent.ScreenPosition;
                }

                return new Vector2(RelativeX + parentScreenPos.X, RelativeY + parentScreenPos.Y);
            }
        }

        /// <summary>
        /// Returns the screen position of the ParentComponent or (0,0) if the
        /// component has no parent.
        /// </summary>
        /// <value>The parent screen position  or (0,0) if the component has no parent.</value>
        public Vector2 ParentScreenPosition
        {
            get
            {
                if (ParentComponent == null)
                {
                    return Vector2.Zero;
                }
                return ParentComponent.ScreenPosition;
            }
        }

        /// <summary>
        /// Returns the X position relative to the center of the parent component.
        /// This is the *unrotated* coordinate!
        /// </summary>
        public float RelativeX
        {
            get
            {
                float parentWidth = AlkaronCoreGame.Core.Window.Width;
                if (ParentComponent != null)
                {
                    parentWidth = ParentComponent.Width;
                }

                switch (PositionAnchor)
                {
                    case UIPositionAnchor.TopLeft:
                    case UIPositionAnchor.BottomLeft:
                    case UIPositionAnchor.CenterLeft:
                        return X;
                    case UIPositionAnchor.TopRight:
                    case UIPositionAnchor.BottomRight:
                    case UIPositionAnchor.CenterRight:
                        return parentWidth - (Width + X);
                    case UIPositionAnchor.BottomCenter:
                    case UIPositionAnchor.TopCenter:
                    case UIPositionAnchor.Center:
                        return (parentWidth / 2 - Width / 2) + X;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
        /// <summary>
        /// Returns the Y position relative to the center of the parent component.
        /// This is the *unrotated* coordinate!
        /// </summary>
        public float RelativeY
        {
            get
            {
                float parentHeight = AlkaronCoreGame.Core.Window.Height;
                if (ParentComponent != null)
                {
                    parentHeight = ParentComponent.Height;
                }

                switch (PositionAnchor)
                {
                    case UIPositionAnchor.TopLeft:
                    case UIPositionAnchor.TopCenter:
                    case UIPositionAnchor.TopRight:
                        return Y;
                    case UIPositionAnchor.BottomLeft:
                    case UIPositionAnchor.BottomCenter:
                    case UIPositionAnchor.BottomRight:
                        return parentHeight - (Y + Height);
                    case UIPositionAnchor.CenterRight:
                    case UIPositionAnchor.CenterLeft:
                    case UIPositionAnchor.Center:
                        return (parentHeight / 2 - Height / 2) + Y;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private Dictionary<string, ModifyLayout> customLayoutHooks;

        public List<UIBaseComponent> Components { get { return components; } }
        protected List<UIBaseComponent> components;

        private UIBaseComponent parentComponent;
        public UIBaseComponent ParentComponent
        {
            get { return parentComponent; }
            private set
            {
                parentComponent = value;
                ParentComponentHasChanged();
            }
        }

        //protected IRenderConfiguration renderConfig;

        private List<UIAnimation> animations;
        #endregion

        #region ctor
        public UIBaseComponent()
        {
            widthSizeMode = UISizeMode.Fixed;
            heightSizeMode = UISizeMode.Fixed;
            isPerformingLayout = false;
            suppressPerformLayout = false;
            customLayoutHooks = new Dictionary<string, ModifyLayout>();
            components = new List<UIBaseComponent>();
            animations = new List<UIAnimation>();
            parentComponent = null;

            Focusable = false;
            PositionAnchor = UIPositionAnchor.TopLeft;
            BackgroundColor = new RgbaFloat(1.0f, 1.0f, 1.0f, 0.0f);
            UserInteractionEnabled = true;
            Visible = true;
            Alpha = 1.0f;
            Rotation = 0.0f;
        }
        #endregion

        #region Add/remove components
        /// <summary>
        /// Called before the component is added as a subcomponent.
        /// 
        /// Base implementation returns true
        /// </summary>
        /// <returns>Return false to prevent the component from being added</returns>
        protected virtual bool WillAddComponent(UIBaseComponent newComp)
        {
            return true;
        }

        /// <summary>
        /// Called before the component is removed as a subcomponent.
        /// 
        /// Base implementation returns true
        /// </summary>
        /// <returns>Return false to prevent the component from being removed</returns>
        protected virtual bool WillRemoveComponent(UIBaseComponent newComp)
        {
            return true;
        }

        /// <summary>
        /// Called after the component is added as a subcomponent.
        /// </summary>
        protected virtual void DidAddComponent(UIBaseComponent newComp)
        {
        }

        /// <summary>
        /// Called after the component is removed as a subcomponent.
        /// </summary>
        protected virtual void DidRemoveComponent(UIBaseComponent newComp)
        {
        }

        /// <summary>
        /// Adds the component.
        /// </summary>
        /// <returns><c>true</c>, if component was added, <c>false</c> otherwise.</returns>
        public bool AddComponent(UIBaseComponent newComp)
        {
            if (newComp == null)
            {
                return false;
            }

            if (WillAddComponent(newComp) == false)
            {
                return false;
            }

            if (newComp.ParentComponent != null)
            {
                newComp.ParentComponent.RemoveComponent(newComp);
            }

            newComp.ParentComponent = this;
            components.Add(newComp);
            newComp.PerformLayout();

            DidAddComponent(newComp);

            return true;
        }

        public bool InsertComponent(UIBaseComponent newComp, int atPosition)
        {
            if (newComp == null)
            {
                return false;
            }

            if (WillAddComponent(newComp) == false)
            {
                return false;
            }

            if (newComp.ParentComponent != null)
            {
                newComp.ParentComponent.RemoveComponent(newComp);
            }

            newComp.ParentComponent = this;
            components.Insert(atPosition, newComp);
            newComp.PerformLayout();

            DidAddComponent(newComp);

            return true;
        }

        public bool RemoveComponent(UIBaseComponent comp)
        {
            if (comp == null)
            {
                return false;
            }

            if (WillRemoveComponent(comp) == false)
            {
                return false;
            }

            if (components.Contains(comp))
            {
                components.Remove(comp);
            }
            comp.ParentComponent = null;
            comp.PerformLayout();

            DidRemoveComponent(comp);

            return true;
        }

        /// <summary>
        /// Removes all sub-components from this component, invoking callbacks
        /// for each removed component.
        /// </summary>
        public void ClearComponents()
        {
            for (int i = components.Count - 1; i >= 0; i--)
            {
                RemoveComponent(components[i]);
            }
        }

        protected virtual void ParentComponentHasChanged()
        {
            //
        }

        /// <summary>
        /// Switches the parent component while keeping the screen 
        /// position the same.
        /// </summary>
        /// <param name="newParent">New parent.</param>
        public void SwitchParentComponent(UIBaseComponent newParent)
        {
            Vector2 newLocalPos = newParent.ScreenToLocalCoords(this.ScreenPosition);
            ParentComponent.RemoveComponent(this);
            newParent.AddComponent(this);

            this.X = newLocalPos.X;
            this.Y = newLocalPos.Y;
        }

        /// <summary>
        /// Returns the child component that comes after the given component
        /// Returns null if the given component is not a child of the current
        /// component.
        /// May return the same component as the given component if there is 
        /// only one child component
        /// </summary>
        public UIBaseComponent GetNextChildComponent(UIBaseComponent comp)
        {
            int index = components.IndexOf(comp);
            if (index == -1)
            {
                return null;
            }

            index = (index + 1) % components.Count;
            return components[index];
        }
        #endregion

        #region Positioning
        public Vector2 LocalToScreenCoords(float localX, float localY)
        {
            return ScreenPosition + new Vector2(localX, localY);
        }

        public Vector2 LocalToScreenCoords(Vector2 localCoords)
        {
            return ScreenPosition + localCoords;
        }

        public Vector2 ScreenToLocalCoords(Vector2 screenCoords)
        {
            return screenCoords - ScreenPosition;
        }

        public Vector2 ScreenToLocalCoords(float screenX, float screenY)
        {
            return new Vector2(screenX, screenY) - ScreenPosition;
        }
        #endregion

        #region Layouting
        public void SizeToFit()
        {
            Vector2 prefSize = PreferredSize;
            BulkPerformLayout((comp) =>
            {
                Width = prefSize.X;
                Height = prefSize.Y;
            });
        }

        public bool PerformLayout()
        {
            if (isPerformingLayout == true || suppressPerformLayout == true)
            {
                return false;
            }

            try
            {
                isPerformingLayout = true;

                InternalPerformLayout();
            }
            finally
            {
                isPerformingLayout = false;
            }

            return true;
        }

        protected virtual void InternalPerformLayout()
        {
            // Calculate layoutWidth
            switch (widthSizeMode)
            {
                case UISizeMode.Fixed:
                    // Do not change Width
                    break;
                case UISizeMode.Fit:
                    {
                        float parentWidth = AlkaronCoreGame.Core.Window.Width;
                        if (ParentComponent != null)
                        {
                            parentWidth = ParentComponent.Width;
                        }
                        width = parentWidth;
                    }
                    break;
            }
            // Calculate layoutHeight
            switch (heightSizeMode)
            {
                case UISizeMode.Fixed:
                    // Do not change Height
                    break;
                case UISizeMode.Fit:
                    {
                        float parentHeight = AlkaronCoreGame.Core.Window.Height;
                        if (ParentComponent != null)
                        {
                            parentHeight = ParentComponent.Height;
                        }
                        height = parentHeight;
                    }
                    break;
            }

            try
            {
                foreach (var pair in customLayoutHooks)
                {
                    pair.Value?.Invoke(this);
                }
            }
            catch (Exception ex)
            {
                Log.Error("Exception occured in customLayoutHooks: " + ex);
            }

            // Layout sub-components
            for (int i = 0; i < components.Count; i++)
            {
                components[i].PerformLayout();
            }
        }

        /// <summary>
        /// Allows the caller to modify multiple layout-affecting properties without causing invokes of
        /// multiple PerformLayout() events. 
        /// 
        /// For example, setting Width and Height indivually causes two PerformLayout()s to be invoked.
        /// Using BulkPerformLayout allows the user to modify multiple properties with only one
        /// PerformLayout() call, thus saving CPU power.
        /// </summary>
        /// <param name="modifyLayoutCode">Code to be executed that modifies layout-essential code.</param>
        public void BulkPerformLayout(ModifyLayout modifyLayoutCode)
        {
            try
            {
                suppressPerformLayout = true;

                modifyLayoutCode?.Invoke(this);
            }
            finally
            {
                suppressPerformLayout = false;
                PerformLayout();
            }
        }

        public bool AddLayoutHook(string setUniqueId, ModifyLayout layoutCode)
        {
            if (customLayoutHooks.ContainsKey(setUniqueId))
            {
                return false;
            }

            customLayoutHooks.Add(setUniqueId, layoutCode);
            return true;
        }

        public bool RemoveLayoutHook(string uniqueId)
        {
            if (customLayoutHooks.ContainsKey(uniqueId))
            {
                customLayoutHooks.Remove(uniqueId);
                return true;
            }

            return false;
        }

        public void ClearPerformLayoutHooks()
        {
            customLayoutHooks.Clear();
        }
        #endregion

        #region Game update
        public virtual void Update(double deltaTime)
        {
            for (int i = 0; i < animations.Count; i++)
            {
                if (animations[i].IsActive == false)
                {
                    animations[i].Activate(deltaTime);
                }
            }

            for (int i = 0; i < animations.Count; i++)
            {
                animations[i].Update(deltaTime);
            }

            for (int i = 0; i < components.Count; i++)
            {
                Performance.Push(this.GetType() + ".components[" + i + "] - Update");
                components[i].Update(deltaTime);
                Performance.Pop();
            }
        }
        #endregion

        #region Rendering
        internal void InternalRender(RenderContext renderContext)
        {
            // Call actual draw method
            Draw(renderContext);

            // Iterate through child components
            for (int i = 0; i < components.Count; i++)
            {
                if (components[i].Visible == false)
                {
                    continue;
                }

                components[i].InternalRender(renderContext);
            }

            // Render focus
            if (HasCapturedKeyboardFocus())
            {
                Vector2 screenPos = ScreenPosition;
                RectangleF rect = new RectangleF(screenPos.X, screenPos.Y, Width, Height);

                RgbaFloat focusColor = new RgbaFloat(new Vector4(new Vector3(0.3f), 0.7f));
                RectangleF focusRect = rect;
                int focusInset = 4;

                focusRect.X += focusInset;
                focusRect.Y += focusInset;
                focusRect.Width -= (focusInset * 2);
                focusRect.Height -= (focusInset * 2);

                ScreenQuad.RenderRectangle(renderContext, focusRect, focusColor, 2);
            }
        }

        protected virtual void Draw(RenderContext renderContext)
        {
            // Draw background
            if (BackgroundColor.A > 0)
            {
                float alpha = CompositeAlpha * ((float)BackgroundColor.A / 1.0f);
                RgbaFloat color = new RgbaFloat(BackgroundColor.R, BackgroundColor.G, BackgroundColor.B, alpha);

                ScreenQuad.RenderQuad(renderContext,
                    new Vector2(ScreenPosition.X, ScreenPosition.Y),
                    new Vector2(this.Width, this.Height),
                    ScreenQuad.SingleWhiteTexture,
                    color,
                    CompositeRotation);
            }
        }
        #endregion

        #region Input
        /// <summary>
        /// NOTE: Check only works for positive numbers!
        /// </summary>
        internal static bool InsideRect(Vector2 pos, Rectangle rect)
        {
            return (pos.X >= rect.X && pos.X <= rect.X + rect.Width &&
                    pos.Y >= rect.Y && pos.Y <= rect.Y + rect.Height);
        }

        /// <summary>
        /// Checks if the specified point in local coordinates is inside the 
        /// component or not. 
        /// The default implementation takes the rotation into account. If you
        /// override the implementation you should ensure that you do the same.
        /// </summary>
        /// <param name="position">Position to test. Must be relative to the checked component.</param>
        protected internal virtual bool HitTest(Vector2 position)
        {
            // Rotation happens around the center of the component. Therefore
            // we just rotate the hit test position relative to the center of the
            // component then perform regular (non-rotated) rectangle checks.

            if (Math.Abs(CompositeRotation) < 1.0f)
            {
                // Rotations with less than one degree are treated as unrotated
                return InsideRect(position, new Rectangle(0, 0, (int)Width, (int)Height));
            }
            else
            {
                float halfWidth = Width / 2.0f;
                float halfHeight = Height / 2.0f;

                Vector2 localCenter = new Vector2(halfWidth, halfHeight);
                Vector3 pointInLocal = new Vector3(position - localCenter, 0);
                // pointInLocal now represents a point in a coordinate system with the
                // center of the unrotated rectangle as 0,0

                pointInLocal = Vector3.Transform(pointInLocal, Matrix4x4.CreateRotationZ(MathHelper.ToRadians(-CompositeRotation)));
                // pointInLocal in now rotated according to the inverse rotation of
                // the component

                // Do regular rectangle check
                return InsideRect(new Vector2(pointInLocal.X + halfWidth, pointInLocal.Y + halfHeight), new Rectangle(0, 0, (int)Width, (int)Height));
            }
        }

        protected internal virtual bool PointerDown(Vector2 position, PointerType pointerType, double deltaTime)
        {
            if (UserInteractionEnabled == false)
            {
                return false;
            }

            for (int i = components.Count - 1; i >= 0; i--)
            {
                Vector2 localPosition = new Vector2(position.X - components[i].RelativeX, position.Y - components[i].RelativeY);
                if (components[i].HitTest(localPosition) == false)
                {
                    continue;
                }

                if (components[i].PointerDown(localPosition, pointerType, deltaTime))
                {
                    return true;
                }
            }

            return false;
        }

        protected internal virtual bool PointerUp(Vector2 position, PointerType pointerType, double deltaTime)
        {
            if (UserInteractionEnabled == false)
            {
                return false;
            }

            for (int i = components.Count - 1; i >= 0; i--)
            {
                Vector2 localPosition = new Vector2(position.X - components[i].RelativeX, position.Y - components[i].RelativeY);
                if (components[i].HitTest(localPosition) == false)
                {
                    continue;
                }

                if (components[i].PointerUp(localPosition, pointerType, deltaTime))
                {
                    return true;
                }
            }

            return false;
        }

        protected internal virtual bool PointerMoved(Vector2 position, double deltaTime)
        {
            if (UserInteractionEnabled == false)
            {
                return false;
            }

            for (int i = components.Count - 1; i >= 0; i--)
            {
                Vector2 localPosition = new Vector2(position.X - components[i].RelativeX, position.Y - components[i].RelativeY);
                if (components[i].HitTest(localPosition) == false)
                {
                    continue;
                }

                if (components[i].PointerMoved(localPosition, deltaTime))
                {
                    return true;
                }
            }

            return false;
        }

        protected internal virtual bool KeyReleased(Key key, double deltaTime)
        {
            if (UserInteractionEnabled == false)
            {
                return true;
            }

            return false;
        }

        protected internal virtual bool KeyPressed(Key key, double deltaTime)
        {
            if (UserInteractionEnabled == false ||
                HasCapturedKeyboardFocus() == false)
            {
                return true;
            }

            if (key == Key.Tab)
            {
                UIBaseComponent nextFocus = FindNextFocusable(this);
                if (nextFocus != null &&
                    nextFocus != this)
                {
                    RelinquishFocus();
                    nextFocus.ReceiveFocus(deltaTime);
                }
                return true;
            }

            return false;
        }

        protected void CaptureInput()
        {
            UIWindowManager.CaptureComponent(this);
        }

        protected void ReleaseInput()
        {
            UIWindowManager.CaptureComponent(null);
        }

        public bool HasCapturedInput()
        {
            return UIWindowManager.CapturedComponent == this;
        }

        protected void CaptureKeyboardFocus()
        {
            UIWindowManager.CaptureKeyboardFocus(this);
        }

        protected void ReleaseKeyboardFocus()
        {
            UIWindowManager.CaptureKeyboardFocus(null);
        }

        public bool HasCapturedKeyboardFocus()
        {
            return UIWindowManager.CapturedKeyboardComponent == this;
        }
        #endregion

        #region Focus
        public virtual void ReceiveFocus(double deltaTime)
        {
            if (Focusable)
            {
                CaptureKeyboardFocus();
            }
        }

        public virtual void RelinquishFocus()
        {
            ReleaseKeyboardFocus();
        }

        public UIBaseComponent FindNextFocusable(UIBaseComponent original)
        {
            UIBaseComponent foundComponent = original;

            var parent = original.ParentComponent;
            if (parent == null)
            {
                return null;
            }

            var nextComp = parent.GetNextChildComponent(original);
            while (nextComp != original)
            {
                if (nextComp.Focusable == true)
                {
                    foundComponent = nextComp;
                    break;
                }

                nextComp = parent.GetNextChildComponent(nextComp);
            }

            if (foundComponent == original)
            {
                return null;
            }

            return foundComponent;
        }
        #endregion

        #region Animations
        public void AddAnimation(UIAnimation animation)
        {
            animations.Add(animation);
            animation.Component = this;
        }

        public bool HasAnimation(string name)
        {
            return GetAnimationByName(name) != null;
        }

        public UIAnimation GetAnimationByName(string name)
        {
            return (from anim in animations where anim.Name == name select anim).FirstOrDefault();
        }

        public void AbortAnimation(string name)
        {
            // Unregister animation
            UIAnimation anim = GetAnimationByName(name);
            if (anim == null)
            {
                return;
            }

            anim.Abort();
        }

        internal virtual void AnimationWasAborted(UIAnimation animation)
        {
            // Unregister animation
            animations.Remove(animation);
        }

        internal virtual void AnimationDidFinish(UIAnimation animation)
        {
            // Unregister animation
            animations.Remove(animation);
        }

        internal virtual void BackupAnimateableProperties(Dictionary<string, object> backup)
        {
            backup.Add("X", X);
            backup.Add("Y", Y);
            backup.Add("Width", Width);
            backup.Add("Height", Height);
            backup.Add("BackgroundColor", BackgroundColor);
            backup.Add("Alpha", Alpha);
            backup.Add("Rotation", Rotation);
        }

        internal virtual void RestoreAnimateableProperties(Dictionary<string, object> backup)
        {
            BulkPerformLayout((comp) =>
            {
                if (backup.ContainsKey("X")) { X = (float)backup["X"]; }
                if (backup.ContainsKey("Y")) { Y = (float)backup["Y"]; }
                if (backup.ContainsKey("Width")) { Width = (float)backup["Width"]; }
                if (backup.ContainsKey("Height")) { Height = (float)backup["Height"]; }
                if (backup.ContainsKey("BackgroundColor")) { BackgroundColor = (RgbaFloat)backup["BackgroundColor"]; }
                if (backup.ContainsKey("Alpha")) { Alpha = (float)backup["Alpha"]; }
                if (backup.ContainsKey("Rotation")) { Rotation = (float)backup["Rotation"]; }
            });
        }
        #endregion

        #region IDisposable
        public virtual void Dispose()
        {
            //
        }
        #endregion
    }
}

using System;
using System.Numerics;
using Veldrid;

namespace AlkaronEngine.Gui
{
    public class UIMessageBox : UIWindow
    {
        private float headerHeight = 70;
        private float buttonHeight = 70;

        private UIButton? dismissButton;
        private UILabel headerLabel;
        private UILabel textLabel;
        private UIPanel backgroundPanel;

        public event Action? OnClose;

        public string Header
        {
            get { return headerLabel.Text; }
            set { headerLabel.Text = value; }
        }
        public string Text
        {
            get { return textLabel.Text; }
            set { textLabel.Text = value; }
        }

        public UIMessageBox(bool addDismissButton = true) 
        {
            BackgroundColor = new RgbaFloat(RgbaFloat.White.R, RgbaFloat.White.G, RgbaFloat.White.B, 0.0f);

            if (addDismissButton)
            {
                dismissButton = new UIButton("Ok", AlkaronCoreGame.Core.DefaultFont, UIButtonStyle.Flat);
                dismissButton.OnPointerUpInside += DismissButton_OnPointerUpInside;
            }

            headerLabel = new UILabel("", AlkaronCoreGame.Core.DefaultFont);
            headerLabel.BackgroundColor = new RgbaFloat(RgbaFloat.Grey.R, RgbaFloat.Grey.G, RgbaFloat.Grey.B, 1.0f);

            textLabel = new UILabel("", AlkaronCoreGame.Core.DefaultFont);
            textLabel.TextAlignVertical = UITextAlignVertical.Top;

            backgroundPanel = new UIPanel();
            backgroundPanel.BackgroundColor = new RgbaFloat(RgbaFloat.Grey.R, RgbaFloat.Grey.G, RgbaFloat.Grey.B, 0.9f);

            AddComponent(backgroundPanel);
            backgroundPanel.AddComponent(headerLabel);
            backgroundPanel.AddComponent(textLabel);
            if (dismissButton != null)
            {
                backgroundPanel.AddComponent(dismissButton);
            }
        }

        private void DismissButton_OnPointerUpInside(UIBaseComponent sender, Vector2 position, double gameTime)
        {
            Close();
        }

        public static UIMessageBox Show(string headerText, string messageText, Action? dismissEvent)
        {
            UIMessageBox box = new UIMessageBox(dismissEvent != null);
            if (dismissEvent != null)
            {
                box.OnClose += dismissEvent;
            }
            box.Header = headerText;
            box.Text = messageText;
            box.Show(true);
            return box;
        }

        protected override void DidRemove()
        {
            base.DidRemove();

            OnClose?.Invoke();
        }

        protected override void DidShow()
        {
            base.DidShow();

            BulkPerformLayout((comp) =>
            {
                backgroundPanel.PositionAnchor = UIPositionAnchor.Center;
                backgroundPanel.Width = this.Width * 0.7f;
                backgroundPanel.WidthSizeMode = UISizeMode.Fixed;
                backgroundPanel.Height = this.Height * 0.5f;
                backgroundPanel.HeightSizeMode = UISizeMode.Fixed;

                headerLabel.Width = 1.0f;
                headerLabel.WidthSizeMode = UISizeMode.Fit;
                headerLabel.Height = headerHeight;
                headerLabel.TextAlignVertical = UITextAlignVertical.Center;
                headerLabel.PositionAnchor = UIPositionAnchor.TopCenter;

                if (dismissButton != null)
                {
                    dismissButton.Width = 1.0f;
                    dismissButton.WidthSizeMode = UISizeMode.Fit;
                    dismissButton.Height = buttonHeight;
                    dismissButton.PositionAnchor = UIPositionAnchor.BottomCenter;
                }

                textLabel.Width = 1.0f;
                textLabel.WidthSizeMode = UISizeMode.Fit;
                textLabel.Height = backgroundPanel.Height - (dismissButton?.Height ?? 0) - headerLabel.Height;
                textLabel.TextAlignVertical = UITextAlignVertical.Center;
                textLabel.PositionAnchor = UIPositionAnchor.BottomCenter;
                textLabel.Y = (dismissButton?.Height ?? 0);
            });
        }
    }
}

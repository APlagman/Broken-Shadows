using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Broken_Shadows.UI
{
    public class Button
    {
        public Rectangle Bounds = new Rectangle();
        private Rectangle boundsHover = new Rectangle();

        private Texture2D texDefault;
        private Texture2D texFocus;

        private SpriteFont font;
        private string text = "";
        private Color colorDefault;
        private Color colorFocus;

        private Action OnClick;
        public void Click()
        {
            if (OnClick != null)
            {
                OnClick();
            }
        }

        private bool isFocused;
        private bool enabled = true;

        public bool HasFocus { set { isFocused = value; } get { return isFocused; } }
        public bool Enabled { get { return enabled; } set { enabled = value; } }

        // Constructor for text-only buttons
        public Button(Point Position, string TextKey, SpriteFont Font, Color Default,
            Color MouseOver, Action Callback)
        {
            string Text = Localization.Get().Text(TextKey);
            // Grab the extents of this text so we can align it properly
            Vector2 vTextSize = Font.MeasureString(Text);
            Vector2 vHoverTextSize = Font.MeasureString("< " + Text + " >");

            Bounds.Location = Position;
            Bounds.X -= (int)(vTextSize.X / 2.0f);
            Bounds.Y -= (int)(vTextSize.Y / 2.0f);
            Bounds.Width = (int)vTextSize.X;
            Bounds.Height = (int)vTextSize.Y;

            boundsHover.Location = Position;
            boundsHover.X -= (int)(vHoverTextSize.X / 2.0f);
            boundsHover.Y -= (int)(vHoverTextSize.Y / 2.0f);
            boundsHover.Width = (int)vHoverTextSize.X;
            boundsHover.Height = (int)vHoverTextSize.Y;

            text = Text;
            font = Font;
            colorDefault = Default;
            colorFocus = MouseOver;

            OnClick += Callback;
        }

        // This is for image buttons
        public Button(Point Position, Texture2D DefaultTexture, Texture2D FocusTexture,
            Action Callback)
        {
            texDefault = DefaultTexture;
            texFocus = FocusTexture;

            Bounds.Location = Position;
            Bounds.Width = texDefault.Width;
            Bounds.Height = texDefault.Height;

            OnClick += Callback;
        }

        public void Draw(float deltaTime, SpriteBatch DrawBatch)
        {
            if (HasFocus)
            {
                if (texFocus != null)
                {
                    DrawBatch.Draw(texFocus, Bounds, Color.White);
                }

                if (text != "")
                {
                    DrawBatch.DrawString(font, "< " + text + " >", new Vector2(boundsHover.X, boundsHover.Y), colorFocus);
                }
            }
            else
            {
                if (texDefault != null)
                {
                    DrawBatch.Draw(texDefault, Bounds, Color.White);
                }

                if (text != "")
                {
                    DrawBatch.DrawString(font, text, new Vector2(Bounds.X, Bounds.Y), colorDefault);
                }
            }
#if DEBUG
            // Draw the bounds of the rect
            if (DebugDefines.DrawButtonBounds)
            {
                Vector2 vStart = new Vector2(Bounds.X, Bounds.Y);
                Vector2 vEnd = vStart;
                vEnd.X += Bounds.Width;
                // Top
                Graphics.GraphicsManager.Get().DrawLine(DrawBatch, 1.0f, Color.White, vStart, vEnd);

                // Left
                vEnd = vStart;
                vEnd.Y += Bounds.Height;
                Graphics.GraphicsManager.Get().DrawLine(DrawBatch, 1.0f, Color.White, vStart, vEnd);

                // Bottom
                vStart.Y += Bounds.Height;
                vEnd = vStart;
                vEnd.X += Bounds.Width;
                Graphics.GraphicsManager.Get().DrawLine(DrawBatch, 1.0f, Color.White, vStart, vEnd);

                // Right
                vStart = new Vector2(Bounds.X, Bounds.Y);
                vStart.X += Bounds.Width;
                vEnd = vStart;
                vEnd.Y += Bounds.Height;
                Graphics.GraphicsManager.Get().DrawLine(DrawBatch, 1.0f, Color.White, vStart, vEnd);
            }
#endif
        }
    }
}

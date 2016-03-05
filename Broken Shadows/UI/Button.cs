using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Broken_Shadows.UI
{
    public class Button
    {
        public Rectangle _bounds = new Rectangle();
        Rectangle _boundsHover = new Rectangle();

        Texture2D _texDefault;
        Texture2D _texFocus;

        SpriteFont _font;
        string _text = "";
        Color _colorDefault;
        Color _colorFocus;

        Action OnClick;
        public void Click()
        {
            if (OnClick != null)
            {
                OnClick();
            }
        }

        bool _isFocused;
        bool _enabled = true;

        public bool HasFocus { set { _isFocused = value; } get { return _isFocused; } }
        public bool Enabled { get { return _enabled; } set { _enabled = value; } }

        // Constructor for text-only buttons
        public Button(Point Position, string TextKey, SpriteFont Font, Color Default,
            Color MouseOver, Action Callback)
        {
            string Text = Localization.Get().Text(TextKey);
            // Grab the extents of this text so we can align it properly
            Vector2 vTextSize = Font.MeasureString(Text);
            Vector2 vHoverTextSize = Font.MeasureString("< " + Text + " >");

            _bounds.Location = Position;
            _bounds.X -= (int)(vTextSize.X / 2.0f);
            _bounds.Y -= (int)(vTextSize.Y / 2.0f);
            _bounds.Width = (int)vTextSize.X;
            _bounds.Height = (int)vTextSize.Y;

            _boundsHover.Location = Position;
            _boundsHover.X -= (int)(vHoverTextSize.X / 2.0f);
            _boundsHover.Y -= (int)(vHoverTextSize.Y / 2.0f);
            _boundsHover.Width = (int)vHoverTextSize.X;
            _boundsHover.Height = (int)vHoverTextSize.Y;

            _text = Text;
            _font = Font;
            _colorDefault = Default;
            _colorFocus = MouseOver;

            OnClick += Callback;
        }

        // This is for image buttons
        public Button(Point Position, Texture2D DefaultTexture, Texture2D FocusTexture,
            Action Callback)
        {
            _texDefault = DefaultTexture;
            _texFocus = FocusTexture;

            _bounds.Location = Position;
            _bounds.Width = _texDefault.Width;
            _bounds.Height = _texDefault.Height;

            OnClick += Callback;
        }

        public void Draw(float fDeltaTime, SpriteBatch DrawBatch)
        {
            if (HasFocus)
            {
                if (_texFocus != null)
                {
                    DrawBatch.Draw(_texFocus, _bounds, Color.White);
                }

                if (_text != "")
                {
                    DrawBatch.DrawString(_font, "< " + _text + " >", new Vector2(_boundsHover.X, _boundsHover.Y), _colorFocus);
                }
            }
            else
            {
                if (_texDefault != null)
                {
                    DrawBatch.Draw(_texDefault, _bounds, Color.White);
                }

                if (_text != "")
                {
                    DrawBatch.DrawString(_font, _text, new Vector2(_bounds.X, _bounds.Y), _colorDefault);
                }
            }
#if DEBUG
            // Draw the bounds of the rect
            if (DebugDefines.drawButtonBounds)
            {
                Vector2 vStart = new Vector2(_bounds.X, _bounds.Y);
                Vector2 vEnd = vStart;
                vEnd.X += _bounds.Width;
                // Top
                Graphics.GraphicsManager.Get().DrawLine(DrawBatch, 1.0f, Color.White, vStart, vEnd);

                // Left
                vEnd = vStart;
                vEnd.Y += _bounds.Height;
                Graphics.GraphicsManager.Get().DrawLine(DrawBatch, 1.0f, Color.White, vStart, vEnd);

                // Bottom
                vStart.Y += _bounds.Height;
                vEnd = vStart;
                vEnd.X += _bounds.Width;
                Graphics.GraphicsManager.Get().DrawLine(DrawBatch, 1.0f, Color.White, vStart, vEnd);

                // Right
                vStart = new Vector2(_bounds.X, _bounds.Y);
                vStart.X += _bounds.Width;
                vEnd = vStart;
                vEnd.Y += _bounds.Height;
                Graphics.GraphicsManager.Get().DrawLine(DrawBatch, 1.0f, Color.White, vStart, vEnd);
            }
#endif
        }
    }
}

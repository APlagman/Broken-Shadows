using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace Broken_Shadows.UI
{
    public class UIScreen
    {
        protected LinkedList<Button> _buttons = new LinkedList<Button>();
        protected ContentManager _content;
        protected float _liveTime = 0.0f;
        // Determines whether or not you can press ESC to leave the screen
        protected bool _canExit = false;

        public UIScreen(ContentManager Content)
        {
            _content = Content;
        }

        public virtual void Update(float fDeltaTime)
        {
            _liveTime += fDeltaTime;
            foreach (Button b in _buttons)
            {
                // If the button is enabled, the mouse is pointing to it, and the UI is the top one
                if (b.Enabled && b._bounds.Contains(InputManager.Get().MousePosition) &&
                    GameState.Get().GetCurrentUI() == this)
                {
                    b.HasFocus = true;
                }
                else
                {
                    b.HasFocus = false;
                }
            }
        }

        public virtual void Draw(float fDeltaTime, SpriteBatch DrawBatch)
        {
            DrawButtons(fDeltaTime, DrawBatch);
        }

        public virtual bool MouseClick(Point Position)
        {
            bool clicked = false;
            foreach (Button b in _buttons)
            {
                if (b.Enabled && b._bounds.Contains(Position))
                {
                    b.Click();
                    clicked = true;
                    break;
                }
            }

            return clicked;
        }

        protected void DrawButtons(float fDeltaTime, SpriteBatch DrawBatch)
        {
            foreach (Button b in _buttons)
            {
                if (b.Enabled)
                {
                    b.Draw(fDeltaTime, DrawBatch);
                }
            }
        }

        public void DrawCenteredString(SpriteBatch DrawBatch, string sText,
            SpriteFont font, Color color, Vector2 vOffset)
        {
            Vector2 pos = new Vector2(Graphics.GraphicsManager.Get().Width / 2.0f, Graphics.GraphicsManager.Get().Height / 2.0f);
            pos -= font.MeasureString(sText) / 2.0f;
            pos += vOffset;
            DrawBatch.DrawString(font, sText, pos, color);
        }

        public virtual void KeyboardInput(SortedList<eBindings, BindInfo> binds)
        {
            if (binds.ContainsKey(eBindings.UI_Exit))
            {
                if (_canExit)
                {
                    GameState.Get().PopUI();
                }

                binds.Remove(eBindings.UI_Exit);
            }
        }

        public virtual void OnExit()
        {

        }
    }
}

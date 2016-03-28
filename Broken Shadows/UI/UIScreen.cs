using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace Broken_Shadows.UI
{
    public class UIScreen
    {
        protected LinkedList<Button> buttons = new LinkedList<Button>();
        protected ContentManager content;
        protected float liveTime = 0.0f;
        // Determines whether or not you can press ESC to leave the screen
        protected bool canExit = false;

        public UIScreen(ContentManager Content)
        {
            content = Content;
        }

        public virtual void Update(float deltaTime)
        {
            liveTime += deltaTime;
            foreach (Button b in buttons)
            {
                // If the button is enabled, the mouse is pointing to it, and the UI is the top one
                if (b.Enabled && b.Bounds.Contains(InputManager.Get().MousePosition) &&
                    StateHandler.Get().GetCurrentUI() == this)
                {
                    b.HasFocus = true;
                }
                else
                {
                    b.HasFocus = false;
                }
            }
        }

        public virtual void Draw(float deltaTime, SpriteBatch DrawBatch)
        {
            DrawButtons(deltaTime, DrawBatch);
        }

        public virtual bool MouseClick(Point Position)
        {
            bool clicked = false;
            foreach (Button b in buttons)
            {
                if (b.Enabled && b.Bounds.Contains(Position))
                {
                    b.Click();
                    clicked = true;
                    break;
                }
            }

            return clicked;
        }

        protected void DrawButtons(float deltaTime, SpriteBatch DrawBatch)
        {
            foreach (Button b in buttons)
            {
                if (b.Enabled)
                {
                    b.Draw(deltaTime, DrawBatch);
                }
            }
        }

        protected void DrawCenteredString(SpriteBatch DrawBatch, string sText,
            SpriteFont font, Color color, Vector2 vOffset)
        {
            Vector2 pos = new Vector2(Graphics.GraphicsManager.Get().Width / 2.0f, Graphics.GraphicsManager.Get().Height / 2.0f);
            pos -= font.MeasureString(sText) / 2.0f;
            pos += vOffset;
            DrawBatch.DrawString(font, sText, pos, color);
        }

        public virtual void KeyboardInput(SortedList<Binding, BindInfo> binds)
        {
            if (binds.ContainsKey(Binding.UI_Exit))
            {
                if (canExit)
                {
                    StateHandler.Get().PopUI();
                }

                binds.Remove(Binding.UI_Exit);
            }
        }

        public virtual void OnExit()
        {

        }
    }
}

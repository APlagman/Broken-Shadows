using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace Broken_Shadows.UI
{
    public class UIMapEditor : UIScreen
    {
        private SpriteFont titleFont;
        private SpriteFont buttonFont;

        public UIMapEditor(ContentManager Content) :
            base(Content)
        {
            titleFont = content.Load<SpriteFont>("Fonts/FixedTitle");
            buttonFont = content.Load<SpriteFont>("Fonts/FixedButton");
            
            // Create buttons
            Point vPos = new Point();
            vPos.X = Graphics.GraphicsManager.Get().Width - 100;
            vPos.Y = 50;

            buttons.AddLast(new Button(vPos, "ui_blankMap",
                buttonFont, Color.White,
                Color.Purple, New));

            vPos.Y += 50;
            buttons.AddLast(new Button(vPos, "ui_reset",
                buttonFont, Color.White,
                Color.Purple, Reset));

            vPos.Y += 50;
            buttons.AddLast(new Button(vPos, "ui_resize",
                buttonFont, Color.White,
                Color.Purple, Resize));

            vPos.Y += 50;
            buttons.AddLast(new Button(vPos, "ui_save",
                buttonFont, Color.White,
                Color.Purple, Save));

            vPos.Y += 50;
            buttons.AddLast(new Button(vPos, "ui_quit",
                buttonFont, Color.White,
                Color.Purple, Quit));

            vPos.Y += 100;
            buttons.AddLast(new Button(vPos, "ui_change_tile",
                buttonFont, Color.White,
                Color.Purple, ChangeTile));
        }

        public void New()
        {
            StateHandler.Get().NewMap();
        }

        public void Reset()
        {
            StateHandler.Get().ResetMap();
        }

        public void Resize()
        {
            StateHandler.Get().ResizeMap();
        }

        public void Save()
        {
            StateHandler.Get().SaveMap();
        }

        public void Quit()
        {
            StateHandler.Get().SetState(GameState.MainMenu);
        }

        public void ChangeTile()
        {
            StateHandler.Get().ChangeMapTile();
        }

        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);
        }

        public override void Draw(float deltaTime, SpriteBatch drawBatch)
        {
            var g = Graphics.GraphicsManager.Get();
            Graphics.GraphicsManager.Get().DrawFilled(drawBatch, new Rectangle(g.Width - 188, -4, 188, g.Height + 4), new Color(30, 30, 30), 4.0f, Color.Purple);

            base.Draw(deltaTime, drawBatch);
        }

        public override void KeyboardInput(SortedList<Binding, BindInfo> binds)
        {
            StateHandler g = StateHandler.Get();

            base.KeyboardInput(binds);
        }
    }
}

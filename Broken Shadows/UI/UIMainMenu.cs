using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace Broken_Shadows.UI
{
    public class UIMainMenu : UIScreen
    {
        private SpriteFont titleFont;
        private SpriteFont buttonFont;
        private string title;

        public UIMainMenu(ContentManager Content) :
            base(Content)
        {
            titleFont = content.Load<SpriteFont>("Fonts/QuartzTitle");
            buttonFont = content.Load<SpriteFont>("Fonts/QuartzButton");

            // Create buttons
            Point vPos = new Point();
            vPos.X = (int)(Graphics.GraphicsManager.Get().Width / 2.0f);
            vPos.Y = (int)(Graphics.GraphicsManager.Get().Height / 2.0f);

            title = Localization.Get().Text("ui_title");

            buttons.AddLast(new Button(vPos, "ui_new_game",
                buttonFont, Color.White,
                Color.Purple, NewGame));

            vPos.Y += 50;
            buttons.AddLast(new Button(vPos, "ui_map_creator",
                buttonFont, Color.White,
                Color.Purple, MapCreator));

            vPos.Y += 50;
            buttons.AddLast(new Button(vPos, "ui_help_title",
                buttonFont, Color.White,
                Color.Purple, Help));

            vPos.Y += 50;
            buttons.AddLast(new Button(vPos, "ui_shutdown",
                buttonFont, Color.White,
                Color.Purple, Exit));
        }

        public void NewGame()
        {
            //SoundManager.Get().PlaySoundCue("MenuClick");
            StateHandler.Get().SetState(GameState.OverWorld);
        }

        public void Options()
        {
        }

        public void MapCreator()
        {
            StateHandler.Get().SetState(GameState.Creator);
        }

        public void Help()
        {
            StateHandler.Get().ShowHelpMenu();
        }

        public void Exit()
        {
            //SoundManager.Get().PlaySoundCue("MenuClick");
            StateHandler.Get().Exit();
        }

        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);
        }

        public override void Draw(float deltaTime, SpriteBatch DrawBatch)
        {
            Vector2 vOffset = Vector2.Zero;
            vOffset.Y = -1.0f * Graphics.GraphicsManager.Get().Height / 4.0f;
            DrawCenteredString(DrawBatch, title, titleFont, Color.MediumPurple, vOffset);

            base.Draw(deltaTime, DrawBatch);
        }
    }
}

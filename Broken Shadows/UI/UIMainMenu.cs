using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace Broken_Shadows.UI
{
    public class UIMainMenu : UIScreen
    {
        SpriteFont _titleFont;
        SpriteFont _buttonFont;
        string _title;

        public UIMainMenu(ContentManager Content) :
            base(Content)
        {
            _titleFont = _content.Load<SpriteFont>("Fonts/QuartzTitle");
            _buttonFont = _content.Load<SpriteFont>("Fonts/QuartzButton");

            // Create buttons
            Point vPos = new Point();
            vPos.X = (int)(Graphics.GraphicsManager.Get().Width / 2.0f);
            vPos.Y = (int)(Graphics.GraphicsManager.Get().Height / 2.0f);

            _title = Localization.Get().Text("ui_title");

            _buttons.AddLast(new Button(vPos, "ui_new_game",
                _buttonFont, Color.White,
                Color.Purple, NewGame));

            vPos.Y += 100;
            _buttons.AddLast(new Button(vPos, "ui_help_title",
                _buttonFont, Color.White,
                Color.Purple, Help));

            vPos.Y += 50;
            _buttons.AddLast(new Button(vPos, "ui_shutdown",
                _buttonFont, Color.White,
                Color.Purple, Exit));
        }

        public void NewGame()
        {
            //SoundManager.Get().PlaySoundCue("MenuClick");
            GameState.Get().SetState(eGameState.OverWorld);
        }

        public void Options()
        {
        }

        public void Help()
        {
            GameState.Get().ShowHelpMenu();
        }

        public void Exit()
        {
            //SoundManager.Get().PlaySoundCue("MenuClick");
            GameState.Get().Exit();
        }

        public override void Update(float fDeltaTime)
        {
            base.Update(fDeltaTime);
        }

        public override void Draw(float fDeltaTime, SpriteBatch DrawBatch)
        {
            Vector2 vOffset = Vector2.Zero;
            vOffset.Y = -1.0f * Graphics.GraphicsManager.Get().Height / 4.0f;
            DrawCenteredString(DrawBatch, _title, _titleFont, Color.MediumPurple, vOffset);

            base.Draw(fDeltaTime, DrawBatch);
        }
    }
}

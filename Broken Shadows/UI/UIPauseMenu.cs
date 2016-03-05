using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace Broken_Shadows.UI
{
    public class UIPauseMenu : UIScreen
    {
        SpriteFont _titleFont;
        SpriteFont _buttonFont;
        string _pausedText;

        public UIPauseMenu(ContentManager Content) :
            base(Content)
        {
            _canExit = true;

            _titleFont = _content.Load<SpriteFont>("Fonts/FixedTitle");
            _buttonFont = _content.Load<SpriteFont>("Fonts/FixedButton");

            _pausedText = Localization.Get().Text("ui_paused");
            // Create buttons
            Point vPos = new Point();
            vPos.X = (int)(Graphics.GraphicsManager.Get().Width / 2.0f);
            vPos.Y = (int)(Graphics.GraphicsManager.Get().Height / 2.0f);

            _buttons.AddLast(new Button(vPos, "ui_resume",
                _buttonFont, Color.White,
                Color.Purple, Resume));

            vPos.Y += 50;
            _buttons.AddLast(new Button(vPos, "ui_quit",
                _buttonFont, Color.White,
                Color.Purple, Quit));

            //SoundManager.Get().PlaySoundCue("MenuClick");
        }

        public void Resume()
        {
            GameState.Get().PopUI();
            //SoundManager.Get().PlaySoundCue("MenuClick");
        }

        public void Quit()
        {
            GameState.Get().SetState(eGameState.MainMenu);
            //SoundManager.Get().PlaySoundCue("MenuClick");
        }

        public override void Update(float fDeltaTime)
        {
            base.Update(fDeltaTime);
        }

        public override void Draw(float fDeltaTime, SpriteBatch DrawBatch)
        {
            // Draw background
            var g = Graphics.GraphicsManager.Get();
            Rectangle rect = new Rectangle(g.Width / 2 - 200, g.Height / 2 - 125,
                400, 250);
            g.DrawFilled(DrawBatch, rect, Color.Black, 4.0f, Color.Purple);

            Vector2 vOffset = Vector2.Zero;
            vOffset.Y -= 75;
            DrawCenteredString(DrawBatch, _pausedText, _titleFont, Color.White, vOffset);

            base.Draw(fDeltaTime, DrawBatch);
        }

        public override void OnExit()
        {
            GameState.Get().IsPaused = false;
        }
    }
}

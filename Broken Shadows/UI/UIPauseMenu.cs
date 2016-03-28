using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace Broken_Shadows.UI
{
    public class UIPauseMenu : UIScreen
    {
        private SpriteFont titleFont;
        private SpriteFont buttonFont;
        private string pausedText;

        public UIPauseMenu(ContentManager Content) :
            base(Content)
        {
            canExit = true;

            titleFont = content.Load<SpriteFont>("Fonts/FixedTitle");
            buttonFont = content.Load<SpriteFont>("Fonts/FixedButton");

            pausedText = Localization.Get().Text("ui_paused");
            // Create buttons
            Point vPos = new Point();
            vPos.X = (int)(Graphics.GraphicsManager.Get().Width / 2.0f);
            vPos.Y = (int)(Graphics.GraphicsManager.Get().Height / 2.0f);

            buttons.AddLast(new Button(vPos, "ui_resume",
                buttonFont, Color.White,
                Color.Purple, Resume));

            vPos.Y += 50;
            buttons.AddLast(new Button(vPos, "ui_quit",
                buttonFont, Color.White,
                Color.Purple, Quit));

            //SoundManager.Get().PlaySoundCue("MenuClick");
        }

        public void Resume()
        {
            StateHandler.Get().PopUI();
            //SoundManager.Get().PlaySoundCue("MenuClick");
        }

        public void Quit()
        {
            StateHandler.Get().SetState(GameState.MainMenu);
            //SoundManager.Get().PlaySoundCue("MenuClick");
        }

        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);
        }

        public override void Draw(float deltaTime, SpriteBatch drawBatch)
        {
            // Draw background
            var g = Graphics.GraphicsManager.Get();
            Rectangle rect = new Rectangle(g.Width / 2 - 200, g.Height / 2 - 125,
                400, 250);
            g.DrawFilled(drawBatch, rect, new Color(30, 30, 30), 4.0f, Color.Purple);

            Vector2 vOffset = Vector2.Zero;
            vOffset.Y -= 75;
            DrawCenteredString(drawBatch, pausedText, titleFont, Color.White, vOffset);

            base.Draw(deltaTime, drawBatch);
        }

        public override void OnExit()
        {
            StateHandler.Get().IsPaused = false;
        }
    }
}

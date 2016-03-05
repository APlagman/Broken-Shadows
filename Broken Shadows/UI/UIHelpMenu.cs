using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace Broken_Shadows.UI
{
    public class UIHelpMenu : UIScreen
    {
        SpriteFont _titleFont;
        SpriteFont _buttonFont;
        string _title;

        Utils.CompiledMarkup _instructions;

        public UIHelpMenu(ContentManager Content) :
            base(Content)
        {
            _titleFont = _content.Load<SpriteFont>("Fonts/QuartzTitle");
            _buttonFont = _content.Load<SpriteFont>("Fonts/QuartzButton");
            _title = Localization.Get().Text("ui_help_title");

            _canExit = true;

            // Create buttons
            Point vPos = new Point();
            vPos.X = (int)(Graphics.GraphicsManager.Get().Width / 2.0f);
            vPos.Y = (int)(Graphics.GraphicsManager.Get().Height - 100);
            
            _buttons.AddLast(new Button(vPos, "ui_help_back",
                _buttonFont, Color.White,
                Color.Purple, Back));

            _instructions = Graphics.GraphicsManager.Get().MarkupEngine.Compile(
                Localization.Get().Text("ui_help_instructions"), Graphics.GraphicsManager.Get().Width / 1.5f);
        }

        public void Back()
        {
            GameState.Get().PopUI();
            //SoundManager.Get().PlaySoundCue("MenuClick");
        }

        public override void OnExit()
        {
            //SoundManager.Get().PlaySoundCue("MenuClick");
            base.OnExit();
        }

        public override void Update(float fDeltaTime)
        {
            base.Update(fDeltaTime);
        }

        public override void Draw(float fDeltaTime, SpriteBatch DrawBatch)
        {
            // Draw background
            var g = Graphics.GraphicsManager.Get();
            Rectangle rect = new Rectangle(0, 0, g.Width, g.Height);
            g.DrawFilled(DrawBatch, rect, Color.Black, 4.0f, Color.Black);

            // Title
            Vector2 vOffset = Vector2.Zero;
            vOffset.Y = -1.0f * Graphics.GraphicsManager.Get().Height / 2.5f;
            DrawCenteredString(DrawBatch, _title, _titleFont, Color.White, vOffset);

            vOffset.X = Graphics.GraphicsManager.Get().Width / 2.0f - _instructions.Size.X / 2.0f;
            vOffset.Y = Graphics.GraphicsManager.Get().Height / 2.0f - _instructions.Size.Y / 2.0f;
            _instructions.Draw(DrawBatch, vOffset);

            base.Draw(fDeltaTime, DrawBatch);
        }
    }
}

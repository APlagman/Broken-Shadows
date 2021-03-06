﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Broken_Shadows.UI
{
    internal class UIMapResize : UIScreen
    {
        private SpriteFont titleFont;
        private SpriteFont buttonFont;
        private string sizeText;
        private int width, height;
        private bool shiftLeft = false, shiftTop = false;

        public UIMapResize(ContentManager Content, int width, int height) :
            base(Content)
        {
            this.width = width;
            this.height = height;
            titleFont = content.Load<SpriteFont>("Fonts/FixedTitle");
            buttonFont = content.Load<SpriteFont>("Fonts/FixedButton");

            sizeText = Localization.Get().Text("ui_chooseSize");
            // Create buttons
            Point vPos = new Point();
            vPos.X = (int)(Graphics.GraphicsManager.Get().Width / 2.0f);
            vPos.Y = (int)(Graphics.GraphicsManager.Get().Height / 2.0f);

            vPos.Y -= 55;
            vPos.X -= 75;
            buttons.AddLast(new Button(vPos, "ui_plus",
                titleFont, Color.White,
                Color.Purple, AddWidth));
            vPos.X += 150;
            buttons.AddLast(new Button(vPos, "ui_minus",
                titleFont, Color.White,
                Color.Purple, RemoveWidth));
            vPos.Y += 25;
            vPos.X -= 150;
            buttons.AddLast(new Button(vPos, "ui_plus",
                titleFont, Color.White,
                Color.Purple, AddHeight));
            vPos.X += 150;
            buttons.AddLast(new Button(vPos, "ui_minus",
                titleFont, Color.White,
                Color.Purple, RemoveHeight));

            vPos.X -= 100;
            vPos.Y += 45;
            buttons.AddLast(new Button(vPos, "ui_toggle_horiz",
                buttonFont, Color.White,
                Color.Purple, ToggleHorizontal));

            vPos.Y += 30;
            buttons.AddLast(new Button(vPos, "ui_toggle_vert",
                buttonFont, Color.White,
                Color.Purple, ToggleVertical));

            vPos.X += 25;
            vPos.Y += 30;
            buttons.AddLast(new Button(vPos, "ui_resize",
                buttonFont, Color.White,
                Color.Purple, Resize));
        }

        #region Button Methods
        private void AddWidth()
        {
            if (width < 150)
                width++;
        }

        private void RemoveWidth()
        {
            if (width > 1)
                width--;
        }

        private void AddHeight()
        {
            if (height < 150)
                height++;
        }

        private void RemoveHeight()
        {
            if (height > 1)
                height--;
        }

        private void ToggleHorizontal()
        {
            shiftLeft = !shiftLeft;
        }

        private void ToggleVertical()
        {
            shiftTop = !shiftTop;
        }

        private void Resize()
        {
            StateHandler.Get().ResizeCurrentMap(width, height, shiftLeft, shiftTop);
            StateHandler.Get().PopUI();
        }

        private void Quit()
        {
            StateHandler.Get().SetState(GameState.MainMenu);
        }
        #endregion

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

            Vector2 vOffset = new Vector2(90, 45);
            DrawCenteredString(drawBatch, (shiftTop) ? "Top" : "Bottom", titleFont, Color.MonoGameOrange, vOffset);
            vOffset.Y -= 30;
            DrawCenteredString(drawBatch, (shiftLeft) ? "Left" : "Right", titleFont, Color.MonoGameOrange, vOffset);
            vOffset.X -= 90;
            vOffset.Y -= 44;
            DrawCenteredString(drawBatch, "Height: " + height, titleFont, Color.MonoGameOrange, vOffset);
            vOffset.Y -= 25;
            DrawCenteredString(drawBatch, "Width: " + width, titleFont, Color.MonoGameOrange, vOffset);
            vOffset.Y -= 40;
            DrawCenteredString(drawBatch, sizeText, titleFont, Color.White, vOffset);

            base.Draw(deltaTime, drawBatch);
        }
    }
}
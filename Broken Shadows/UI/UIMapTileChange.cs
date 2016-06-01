using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Broken_Shadows.Objects;

namespace Broken_Shadows.UI
{
    internal class UIMapTileChange : UIScreen
    {
        private SpriteFont titleFont;
        private SpriteFont buttonFont;
        private string tileChangeText;
        private Tile[] selectedTiles;
        private int data;

        public UIMapTileChange(ContentManager Content, Tile[] selected) :
            base(Content)
        {
            selectedTiles = selected;
            data = selectedTiles[0].ToData();
            titleFont = content.Load<SpriteFont>("Fonts/FixedTitle");
            buttonFont = content.Load<SpriteFont>("Fonts/FixedButton");

            tileChangeText = Localization.Get().Text("ui_choose_tile");
            // Create buttons
            Point vPos = new Point();
            vPos.X = (int)(Graphics.GraphicsManager.Get().Width / 2.0f);
            vPos.Y = (int)(Graphics.GraphicsManager.Get().Height / 2.0f);

            vPos.Y -= 55;
            vPos.X -= 40;
            buttons.AddLast(new Button(vPos, "ui_plus",
                titleFont, Color.White,
                Color.Purple, Add));
            vPos.X += 80;
            buttons.AddLast(new Button(vPos, "ui_minus",
                titleFont, Color.White,
                Color.Purple, Subtract));

            vPos.X -= 40;
            vPos.Y += 45;
            buttons.AddLast(new Button(vPos, "ui_change_tile",
                buttonFont, Color.White,
                Color.Purple, Change));

            vPos.Y += 30;
            buttons.AddLast(new Button(vPos, "ui_map_back",
                buttonFont, Color.White,
                Color.Purple, Back));
        }

        #region Button Methods
        public void Add()
        {
            data++;
        }

        public void Subtract()
        {
            if (data > 0)
                data--;
        }

        public void Change()
        {
            for (int t = 0; t < selectedTiles.Length; t++)
            {
                Vector2 tempOrigin = new Vector2(selectedTiles[t].OriginPosition.X, selectedTiles[t].OriginPosition.Y);
                Pose2D tempPose = new Pose2D(selectedTiles[t].Pose.Position);
                Tile newTile = Level.CreateTile(selectedTiles[t].Game, Vector2.Zero, (Level.TileType)data, true);

                if (newTile != null)
                {
                    Graphics.GraphicsManager.Get().RemoveGameObject(selectedTiles[t]);
                    selectedTiles[t] = newTile;
                    selectedTiles[t].IsSelected = true;
                    selectedTiles[t].Pose = tempPose;
                    selectedTiles[t].OriginPosition = tempOrigin;
                    StateHandler.Get().ChangeSelected(selectedTiles[t]);
                }
            }
            Back();
        }

        private void Back()
        {
            StateHandler.Get().PopUI();
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
            Rectangle rect = new Rectangle(g.Width / 2 - 100, g.Height / 2 - 125,
                200, 200);
            g.DrawFilled(drawBatch, rect, new Color(30, 30, 30), 4.0f, Color.Purple);

            Vector2 vOffset = new Vector2(0, -100);
            DrawCenteredString(drawBatch, tileChangeText, titleFont, Color.White, vOffset);
            vOffset.Y += 50;
            DrawCenteredString(drawBatch, data.ToString(), buttonFont, Color.White, vOffset);
            
            Tile preview = new Tile(selectedTiles[0].Game, new Pose2D(new Vector2(g.Width + 96, g.Height + 16)), (Level.TileType)data, Tile.ToTexture(data));
            preview.Draw(drawBatch, 0, preview.Pose.Position, 0.5f, SpriteEffects.None, 0);

            vOffset.Y += 100;
            DrawCenteredString(drawBatch, preview.TextureName, buttonFont, Color.White, vOffset);

            base.Draw(deltaTime, drawBatch);
        }

        public override void OnExit()
        {
            StateHandler.Get().IsPaused = false;
        }
    }
}
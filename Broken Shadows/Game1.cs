using System;
using System.Text;
using System.Xml;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Intermediate;
using Microsoft.Xna.Framework;

namespace Broken_Shadows
{
    public class Game1 : Game
    { 
        public Game1()
        {
            Graphics.GraphicsManager.Get().Start(this);
            IsMouseVisible = false;
            Content.RootDirectory = "Content";      
        }

        protected override void Initialize()
        {
            /*var testData = new LevelData();
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            using (XmlWriter writer = XmlWriter.Create("test.xml", settings))
            {
                IntermediateSerializer.Serialize(writer, testData, null);
            }*/

            GameState.Get().Start(this);
            GameState.Get().SetState(eGameState.OverWorld);
            InputManager.Get().Start();
            Localization.Get().Start(GlobalDefines.DEFAULT_LOC_FILE);
            base.Initialize();
        }

        protected override void LoadContent()
        {
            Graphics.GraphicsManager.Get().LoadContent();
        }

        protected override void UnloadContent()
        {
        }

        protected override void Update(GameTime gameTime)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (IsActive)
            {
                InputManager.Get().Update(deltaTime);
                GameState.Get().Update(deltaTime);
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            Graphics.GraphicsManager.Get().Draw((float)gameTime.ElapsedGameTime.TotalSeconds);

            base.Draw(gameTime);
        }
    }
}

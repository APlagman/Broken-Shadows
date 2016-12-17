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
            IsFixedTimeStep = GlobalDefines.UseFpsCap;
            if (IsFixedTimeStep)
                TargetElapsedTime = new TimeSpan(0, 0, 0, 0, 1000 / GlobalDefines.MaxGameFps);
            Content.RootDirectory = "Content";

            Window.Title = "Stay off my lawn!"; 
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

            StateHandler.Get().Start(this);
            StateHandler.Get().SetState(GameState.MainMenu);
            InputManager.Get().Start();
            Localization.Get().Start(GlobalDefines.DefaultLocFile);
            base.Initialize();
            System.Diagnostics.Debug.WriteLine("\nInitialization complete!\n");
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
                StateHandler.Get().Update(deltaTime);
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

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Broken_Shadows.Graphics
{
    public class GraphicsManager : Patterns.Singleton<GraphicsManager>
    {
        GraphicsDeviceManager _graphics;
        Game _game;
        SpriteBatch _spriteBatch;
        Texture2D _blank;
        Texture2D[] _mouseTextures = new Texture2D[(int)eMouseState.MAX_STATES];
        SpriteFont _fpsFont;

        LinkedList<Objects.GameObject> _objects = new LinkedList<Objects.GameObject>();
        LinkedList<Objects.Player> _playerObjects = new LinkedList<Objects.Player>();

        public bool IsFullScreen
        {
            get { return _graphics.IsFullScreen; }
            set { _graphics.IsFullScreen = value; }
        }
        public bool IsVSync
        {
            get { return _graphics.SynchronizeWithVerticalRetrace; }
            set { _graphics.SynchronizeWithVerticalRetrace = value; }
        }
        public int Width { get { return _graphics.PreferredBackBufferWidth; } }
        public int Height { get { return _graphics.PreferredBackBufferHeight; } }
        public GraphicsDevice GraphicsDevice { get { return _graphics.GraphicsDevice; } }

        public void Start(Game game)
        {
            _graphics = new GraphicsDeviceManager(game);
            _game = game;
            
            if (!GlobalDefines.IS_FULLSCREEN)
            {
                SetResolution(GlobalDefines.WINDOW_WIDTH, GlobalDefines.WINDOW_HEIGHT);
            }
            else
            {
                SetResolutionToCurrent();
                ToggleFullScreen();
            }
        }

        public void LoadContent()
        {
            _spriteBatch = new SpriteBatch(_graphics.GraphicsDevice);

            // Load mouse textures
            _mouseTextures[(int)eMouseState.Default] = _game.Content.Load<Texture2D>("UI/Mouse_Default");

            // Load FPS font
            _fpsFont = _game.Content.Load<SpriteFont>("Fonts/FixedText");

            // Debug stuff for line drawing
            _blank = new Texture2D(_graphics.GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
            _blank.SetData(new[] { Color.White });
        }

        public void SetResolutionToCurrent()
        {
            _graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            _graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;

            if (_graphics.GraphicsDevice != null)
            {
                _graphics.ApplyChanges();
            }
        }

        public void SetResolution(int Width, int Height)
        {
            _graphics.PreferredBackBufferWidth = Width;
            _graphics.PreferredBackBufferHeight = Height;

            if (_graphics.GraphicsDevice != null)
            {
                _graphics.ApplyChanges();
            }
        }

        public void ToggleFullScreen()
        {
            _graphics.ToggleFullScreen();
        }

        public void Draw(float fDeltaTime)
        {
            _graphics.GraphicsDevice.Clear(Color.Black);
            
            _spriteBatch.Begin();

            GameState.Get().DrawUI(fDeltaTime, _spriteBatch);

            foreach (Objects.GameObject o in _objects)
            {
                o.Draw(_spriteBatch);
            }
            foreach (Objects.Player p in _playerObjects)
            {
                p.Draw(_spriteBatch);
            }

            // Draw mouse cursor
            Point MousePos = InputManager.Get().MousePosition;
            Vector2 vMousePos = new Vector2(MousePos.X, MousePos.Y);
            _spriteBatch.Draw(GetMouseTexture(InputManager.Get().MouseState), vMousePos, Color.White);

            // Draw FPS counter
            Vector2 vFPSPos = Vector2.Zero;
            if (DebugDefines.showBuildString)
            {
                _spriteBatch.DrawString(_fpsFont, "Tyrais (Prototype)", vFPSPos, Color.White);
                vFPSPos.Y += 25.0f;
            }
            if (DebugDefines.showFPS)
            {
                string sFPS = String.Format("FPS: {0}", (int)(1 / fDeltaTime));
                _spriteBatch.DrawString(_fpsFont, sFPS, vFPSPos, Color.White);
            }

            _spriteBatch.End();
        }

        Texture2D GetMouseTexture(eMouseState e)
        {
            return _mouseTextures[(int)e];
        }

        public void DrawLine(SpriteBatch batch, float width, Color color,
            Vector2 point1, Vector2 point2)
        {
            float angle = (float)Math.Atan2(point2.Y - point1.Y, point2.X - point1.X);
            float length = Vector2.Distance(point1, point2);

            batch.Draw(_blank, point1, null, color,
                       angle, Vector2.Zero, new Vector2(length, width),
                       SpriteEffects.None, 0);
        }

        public void DrawFilled(SpriteBatch batch, Rectangle rect, Color color, float outWidth, Color outColor)
        {
            // Draw the background
            batch.Draw(_blank, rect, color);

            // Draw the outline
            DrawLine(batch, outWidth, outColor, new Vector2(rect.Left, rect.Top),
                new Vector2(rect.Right, rect.Top));
            DrawLine(batch, outWidth, outColor, new Vector2(rect.Left, rect.Top),
                new Vector2(rect.Left, rect.Bottom + (int)outWidth));
            DrawLine(batch, outWidth, outColor, new Vector2(rect.Left, rect.Bottom),
                new Vector2(rect.Right, rect.Bottom));
            DrawLine(batch, outWidth, outColor, new Vector2(rect.Right, rect.Top),
                new Vector2(rect.Right, rect.Bottom));
        }

        public void AddGameObject(Objects.GameObject o)
        {
            _objects.AddLast(o);
        }

        public void RemoveGameObject(Objects.GameObject o)
        {
            _objects.Remove(o);
        }

        public void AddPlayerObject(Objects.Player p)
        {
            _playerObjects.AddLast(p);
        }

        public void RemovePlayerObject(Objects.Player p)
        {
            _playerObjects.Remove(p);
        }

        public void ClearAllObjects()
        {
            _objects.Clear();
            _playerObjects.Clear();
        }
    }
}

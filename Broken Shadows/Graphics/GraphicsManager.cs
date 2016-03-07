using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Broken_Shadows.Graphics
{
    public class GraphicsManager : Patterns.Singleton<GraphicsManager>
    {
        static readonly float MOUSE_LIGHT_SCALE = 1f;
        static readonly Color MOUSE_LIGHT_COLOR = Color.White;

        Color LevelColor;

        Texture2D spotLightMask;
        Texture2D squareLightMask;
        Effect lightEffect;
        Effect combineEffect;
        Effect blurEffect;
        Effect lightingEffect;
        RenderTarget2D lightsTarget;
        RenderTarget2D mainTarget;

        bool _renderLights;
        public bool RenderLights { get { return _renderLights; } set { _renderLights = value; } }

        GraphicsDeviceManager _graphics;
        FrameCounter _frameCounter = new FrameCounter();
        Game _game;
        SpriteBatch _spriteBatch;
        Texture2D _blank;
        Texture2D[] _mouseTextures = new Texture2D[(int)eMouseState.MAX_STATES];
        SpriteFont _fpsFont;

        LinkedList<Objects.GameObject> _objects = new LinkedList<Objects.GameObject>();
        LinkedList<Objects.Player> _playerObjects = new LinkedList<Objects.Player>();
        LinkedList<Objects.Light> _lightObjects = new LinkedList<Objects.Light>();

        public string posString = ""; // For Debug

        private bool IsFullScreen { get { return _graphics.IsFullScreen; } set { _graphics.IsFullScreen = value; } }
        private bool IsVSync { get { return _graphics.SynchronizeWithVerticalRetrace; } set { _graphics.SynchronizeWithVerticalRetrace = value; } }
        public int Width { get { return _graphics.PreferredBackBufferWidth; } }
        public int Height { get { return _graphics.PreferredBackBufferHeight; } }
        public GraphicsDevice GraphicsDevice { get { return _graphics.GraphicsDevice; } }

        public Utils.MarkupTextEngine MarkupEngine
        {
            get; internal set;
        }

        public void Start(Game game)
        {
            _graphics = new GraphicsDeviceManager(game);
            _game = game;
            IsVSync = GlobalDefines.VSync;
            _game.TargetElapsedTime = new TimeSpan(0, 0, 0, 0, 1000 / GlobalDefines.FPS);
            _renderLights = true;
            
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

            // Load lighting
            spotLightMask = _game.Content.Load<Texture2D>("Shaders//spotlightmask");
            squareLightMask = _game.Content.Load<Texture2D>("Shaders//squarelightmask");
            lightingEffect = _game.Content.Load<Effect>("Shaders//lighteffect");
            var pp = _graphics.GraphicsDevice.PresentationParameters;
            lightsTarget = new RenderTarget2D(_graphics.GraphicsDevice, pp.BackBufferWidth, pp.BackBufferHeight);
            mainTarget = new RenderTarget2D(_graphics.GraphicsDevice, pp.BackBufferWidth, pp.BackBufferHeight);

            // Load mouse textures
            _mouseTextures[(int)eMouseState.Default] = _game.Content.Load<Texture2D>("UI/Mouse_Default");

            // Load FPS font
            _fpsFont = _game.Content.Load<SpriteFont>("Fonts/FixedText");

            // Debug stuff for line drawing
            _blank = new Texture2D(_graphics.GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
            _blank.SetData(new[] { Color.White });

            // Setup the font, image, video and condition resolvers for the engine.
            // The resolvers are simple lambdas that map a string to its corresponding data
            // e.g. an image resolver maps a string to a Texture2D
            var fonts = new Dictionary<string, SpriteFont>();
            Func<string, SpriteFont> fontResolver = f => fonts[f];
            fonts.Add("Instructions", _game.Content.Load<SpriteFont>("Fonts/FixedText"));

            var buttons = new Dictionary<string, Texture2D>();
            Func<string, Texture2D> imageResolver = b => buttons[b];

            var conditions = new Dictionary<string, bool>();
            Func<string, bool> conditionalResolver = c => conditions[c];

            MarkupEngine = new Utils.MarkupTextEngine(fontResolver, imageResolver, conditionalResolver);
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
            // Create a light mask to pass to the pixel shader
            _graphics.GraphicsDevice.SetRenderTarget(lightsTarget);
            _graphics.GraphicsDevice.Clear(Color.Black);
            _spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive);
            _spriteBatch.Draw(squareLightMask, Vector2.Zero, null, LevelColor, 0f, Vector2.Zero, GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width / squareLightMask.Width, SpriteEffects.None, 0f);
            foreach (Objects.Light l in _lightObjects)
            {
                if (_renderLights) l.Draw(_spriteBatch);
            }
            _spriteBatch.Draw(spotLightMask, new Vector2(Mouse.GetState().Position.X - spotLightMask.Width / 2, Mouse.GetState().Position.Y - spotLightMask.Height / 2), null, MOUSE_LIGHT_COLOR, 0f, Vector2.Zero, MOUSE_LIGHT_SCALE, SpriteEffects.None, 0f);
            _spriteBatch.End();

            // Draw the main scene to the render target
            _graphics.GraphicsDevice.SetRenderTarget(mainTarget);
            _graphics.GraphicsDevice.Clear(Color.Black);
            _spriteBatch.Begin();

            foreach (Objects.GameObject o in _objects)
            {
                o.Draw(_spriteBatch);
            }
            foreach (Objects.Player p in _playerObjects)
            {
                p.Draw(_spriteBatch);
            }
            _spriteBatch.End();

            // Draw the main scene with a pixel
            _graphics.GraphicsDevice.SetRenderTarget(null);
            _graphics.GraphicsDevice.Clear(Color.Black);
            _spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);

            lightingEffect.Parameters["lightMask"].SetValue(lightsTarget);
            lightingEffect.CurrentTechnique.Passes[0].Apply();
            _spriteBatch.Draw(mainTarget, Vector2.Zero, Color.White);
            _spriteBatch.End();

            DrawOverlay(fDeltaTime);
            if (!_renderLights) _renderLights = true;
        }

        private void DrawOverlay(float fDeltaTime)
        {
            _spriteBatch.Begin();

            GameState.Get().DrawUI(fDeltaTime, _spriteBatch);

            // Draw mouse cursor
            Point MousePos = InputManager.Get().MousePosition;
            Vector2 vMousePos = new Vector2(MousePos.X, MousePos.Y);
            _spriteBatch.Draw(GetMouseTexture(InputManager.Get().MouseState), vMousePos, Color.White);

            // Draw Build / FPS counter
            Vector2 vFPSPos = Vector2.Zero;
            if (DebugDefines.showBuildString)
            {
                _spriteBatch.DrawString(_fpsFont, "Broken Shadows (Prototype)", vFPSPos, Color.White);
                vFPSPos.Y += 25.0f;
            }
            if (DebugDefines.showFPS)
            {
                _frameCounter.Update(fDeltaTime);
                string sFPS = String.Format("FPS: {0:F2}", _frameCounter.AverageFramesPerSecond);
                _spriteBatch.DrawString(_fpsFont, sFPS, vFPSPos, Color.White);
                vFPSPos.Y += 25.0f;
            }

            // Draw Positions
            if (DebugDefines.showGridAndPlayerPositions)
            {
                _spriteBatch.DrawString(_fpsFont, posString, vFPSPos, Color.White);
                vFPSPos.Y += 25.0f;
            }

            _spriteBatch.End();
        }

        Texture2D GetMouseTexture(eMouseState e)
        {
            return _mouseTextures[(int)e];
        }

        public void DrawLine(SpriteBatch batch, float width, Color color, Vector2 point1, Vector2 point2)
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

        public void AddLightObject(Objects.Light l)
        {
            _lightObjects.AddLast(l);
        }

        public void RemoveLightObject(Objects.Light l)
        {
            _lightObjects.Remove(l);
        }

        public void ClearLightObjects()
        {
            _lightObjects.Clear();
        }

        public void ClearAllObjects()
        {
            _objects.Clear();
            _playerObjects.Clear();
        }

        public void ChangeColor(Color newColor)
        {
            LevelColor = newColor;
        }

        public void ChangeColor(byte addBrightness)
        {
            LevelColor.A += addBrightness;
        }
    }
}

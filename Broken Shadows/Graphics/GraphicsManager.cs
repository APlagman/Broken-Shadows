using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Broken_Shadows.Objects;

namespace Broken_Shadows.Graphics
{
    public class GraphicsManager : Patterns.Singleton<GraphicsManager>
    {
        public string gridPosString = ""; // For Debug
        public string playerPosString = "";
        public string mousePosString = "";

        public Effect LightEffect { get { return lightEffect; } }
        private Effect lightEffect, combineEffect, blurEffect;
        private RenderTarget2D colorMap, lightMap, blurMap;
        private Quad quad;

        private GraphicsDeviceManager graphics;
        private FrameCounter frameCounter = new FrameCounter();
        private Game game;
        private SpriteBatch spriteBatch;
        private Texture2D blank;
        private Texture2D[] mouseTextures = new Texture2D[(int)MouseState.NUM_STATES];
        private SpriteFont fpsFont;
        public Color BGColor;

        private List<GameObject> solids = new List<GameObject>();
        private List<GameObject> objects = new List<GameObject>();
        private List<Player> playerObjects = new List<Player>();
        private List<PointLight> lights = new List<PointLight>();
        public Level Level { private get; set; }

        public bool IsFullScreen { get { return graphics.IsFullScreen; } private set { graphics.IsFullScreen = value; } }
        private bool IsVSync { get { return graphics.SynchronizeWithVerticalRetrace; } set { graphics.SynchronizeWithVerticalRetrace = value; } }
        public int Width { get { return graphics.PreferredBackBufferWidth; } }
        public int Height { get { return graphics.PreferredBackBufferHeight; } }
        public GraphicsDevice GraphicsDevice { get { return graphics.GraphicsDevice; } }
        public MarkupTextEngine MarkupEngine { get; internal set; }
        public bool RenderLights { get; set; }

        #region Setup
        public void Start(Game game)
        {
            graphics = new GraphicsDeviceManager(game);
            this.game = game;
            IsVSync = GlobalDefines.VSync;
            
            if (!GlobalDefines.IsFullscreen)
            {
                SetResolution(GlobalDefines.WindowWidth, GlobalDefines.WindowHeight);
            }
            else
            {
                SetResolutionToCurrent();
                ToggleFullScreen();
            }
            BGColor = Color.Black;
        }

        public void LoadContent()
        {
            spriteBatch = new SpriteBatch(graphics.GraphicsDevice);

            var pp = graphics.GraphicsDevice.PresentationParameters;
            // Set up all render targets, the blur map doesn't need a depth buffer
            colorMap = new RenderTarget2D(GraphicsDevice, pp.BackBufferWidth, pp.BackBufferHeight, false, SurfaceFormat.Color, DepthFormat.Depth16);
            lightMap = new RenderTarget2D(GraphicsDevice, pp.BackBufferWidth, pp.BackBufferHeight, false, SurfaceFormat.Color, DepthFormat.Depth16);
            blurMap = new RenderTarget2D(GraphicsDevice, pp.BackBufferWidth, pp.BackBufferHeight, false, SurfaceFormat.Color, DepthFormat.None);

            combineEffect = game.Content.Load<Effect>("Shaders/Combine");
            lightEffect = game.Content.Load<Effect>("Shaders/Light");
            blurEffect = game.Content.Load<Effect>("Shaders/Blur");

            quad = new Quad();

            // Load mouse textures
            mouseTextures[(int)MouseState.Default] = game.Content.Load<Texture2D>("UI/Mouse_Default");

            // Load FPS font
            fpsFont = game.Content.Load<SpriteFont>("Fonts/FixedText");

            // Debug stuff for line drawing
            blank = new Texture2D(graphics.GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
            blank.SetData(new[] { Color.White });

            // Setup the font, image, video and condition resolvers for the engine.
            // The resolvers are simple lambdas that map a string to its corresponding data
            // e.g. an image resolver maps a string to a Texture2D
            var fonts = new Dictionary<string, SpriteFont>();
            Func<string, SpriteFont> fontResolver = f => fonts[f];
            fonts.Add("Instructions", game.Content.Load<SpriteFont>("Fonts/FixedText"));

            var buttons = new Dictionary<string, Texture2D>();
            Func<string, Texture2D> imageResolver = b => buttons[b];

            var conditions = new Dictionary<string, bool>();
            Func<string, bool> conditionalResolver = c => conditions[c];

            MarkupEngine = new MarkupTextEngine(fontResolver, imageResolver, conditionalResolver);

            //_lights.Add(new Light(lightEffect, new Vector2(300, 300), 50, Color.White, 1.0f));
        }

        public void SetResolutionToCurrent()
        {
            graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;

            if (graphics.GraphicsDevice != null)
            {
                graphics.ApplyChanges();
            }
        }

        public void SetResolution(int width, int height)
        {
            graphics.PreferredBackBufferWidth = width;
            graphics.PreferredBackBufferHeight = height;

            if (graphics.GraphicsDevice != null)
            {
                graphics.ApplyChanges();
            }
        }

        public void ToggleFullScreen()
        {
            graphics.ToggleFullScreen();
            if (!graphics.IsFullScreen)
            {
                SetResolution(GlobalDefines.WindowWidth, GlobalDefines.WindowHeight);
            }
            else
            {
                SetResolutionToCurrent();
            }
            if (graphics.GraphicsDevice != null) LoadContent();
            StateHandler.Get().RefreshUI();
        }
        #endregion

        public void Draw(float deltaTime)
        {
            // Draw the colors
            DrawColorMap();

            // Draw the lights
            if (Level != null) DrawLightMap((float)(Level.LevelColor.A / 255.0));

            // Blur the shadows
            BlurRenderTarget(lightMap, 2.5f);

            // Combine
            CombineAndDraw();

            if (RenderLights)
                DrawSegments();

            DrawOverlay(deltaTime);
            
            RenderLights = true;
        }

        #region Draw Helpers
        private void DrawColorMap()
        {
            GraphicsDevice.SetRenderTarget(colorMap);
            GraphicsDevice.Clear(BGColor);

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.AnisotropicClamp, DepthStencilState.Default, RasterizerState.CullCounterClockwise, null);
          
            foreach (GameObject o in objects)
            {
                Vector2 origin = new Vector2(o.Texture.Width / 2.0f, o.Texture.Height / 2.0f);
                o.Draw(spriteBatch, 0, origin, 1, SpriteEffects.None, 0);
            }
            foreach (Player p in playerObjects)
            {
                Vector2 origin = new Vector2(p.Texture.Width / 2.0f, p.Texture.Height / 2.0f);
                spriteBatch.Draw(p.Texture, p.Pose.Position, null, Color.White, p.Pose.Rotation, origin, p.Pose.Scale, SpriteEffects.None, 0);
            }

            spriteBatch.End();
        }
        
        private void DrawLightMap(float ambientLightStrength)
        {
            GraphicsDevice.SetRenderTarget(lightMap);
            GraphicsDevice.Clear(Color.White * ((RenderLights) ? ambientLightStrength : 0));

            // Draw normal objects that emit light
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.AnisotropicClamp, DepthStencilState.Default, RasterizerState.CullCounterClockwise);

            foreach (GameObject o in objects)
            {
                if (o.GlowTexture != null)
                {
                    Vector2 origin = new Vector2(o.GlowTexture.Width / 2.0f, o.GlowTexture.Height / 2.0f);
                    spriteBatch.Draw(o.GlowTexture, o.Pose.Position, null, Color.White, o.Pose.Rotation, origin, o.Pose.Scale, SpriteEffects.None, 0);
                }
            }
            foreach (Player p in playerObjects)
            {
                if (p.GlowTexture != null)
                {
                    Vector2 origin = new Vector2(p.Texture.Width / 2.0f, p.Texture.Height / 2.0f);
                    spriteBatch.Draw(p.GlowTexture, p.Pose.Position, null, Color.White, p.Pose.Rotation, origin, p.Pose.Scale, SpriteEffects.None, 0);
                }
            }

            spriteBatch.End();

            GraphicsDevice.BlendState = BlendState.Additive;
            // Samplers states are set by the shader itself            
            GraphicsDevice.DepthStencilState = DepthStencilState.None;
            GraphicsDevice.RasterizerState = RasterizerState.CullNone;

            if (RenderLights)// && solids.Count > 0)
            {
                foreach (PointLight l in lights)
                {
                    l.Render(GraphicsDevice, Level, solids);
                }
            }
        }

        private void BlurRenderTarget(RenderTarget2D target, float strength)
        {
            Vector2 renderTargetSize = new Vector2(target.Width, target.Height );

            blurEffect.Parameters["renderTargetSize"].SetValue(renderTargetSize);
            blurEffect.Parameters["blur"].SetValue(strength);

            // Pass one
            GraphicsDevice.SetRenderTarget(blurMap);
            GraphicsDevice.Clear(Color.Black);

            blurEffect.Parameters["InputTexture"].SetValue(target);

            blurEffect.CurrentTechnique = blurEffect.Techniques["BlurHorizontally"];
            blurEffect.CurrentTechnique.Passes[0].Apply();
            quad.Render(GraphicsDevice, Vector2.One * -1, Vector2.One);

            // Pass two
            GraphicsDevice.SetRenderTarget(blurMap);
            GraphicsDevice.Clear(Color.Black);

            blurEffect.Parameters["InputTexture"].SetValue(target);

            blurEffect.CurrentTechnique = blurEffect.Techniques["BlurVertically"];
            blurEffect.CurrentTechnique.Passes[0].Apply();
            quad.Render(GraphicsDevice, Vector2.One * -1, Vector2.One);
        }

        private void CombineAndDraw()
        {
            GraphicsDevice.SetRenderTarget(null);

            GraphicsDevice.Clear(Color.Black);

            GraphicsDevice.BlendState = BlendState.Opaque;
            // Samplers states are set by the shader itself            
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

            combineEffect.Parameters["colorMap"].SetValue(colorMap);
            combineEffect.Parameters["lightMap"].SetValue(lightMap);

            combineEffect.Techniques[0].Passes[0].Apply();
            quad.Render(GraphicsDevice, Vector2.One * -1.0f, Vector2.One);
        }

        private void DrawOverlay(float deltaTime)
        {
            spriteBatch.Begin();

            StateHandler.Get().DrawUI(deltaTime, spriteBatch);

            // Draw mouse cursor
            Vector2 mousePosition = InputManager.Get().MousePosition.ToVector2();
            spriteBatch.Draw(GetMouseTexture(InputManager.Get().MouseState), mousePosition, Color.White);

            // Draw Build / FPS counter
            Vector2 fpsPosition = Vector2.Zero;
            if (DebugDefines.ShowBuildString)
            {
                spriteBatch.DrawString(fpsFont, "Broken Shadows (Prototype)", fpsPosition, Color.MonoGameOrange);
                fpsPosition.Y += 25.0f;
            }
            if (DebugDefines.ShowFPS)
            {
                frameCounter.Update(deltaTime);
                string sFPS = String.Format("FPS: {0:F2}", frameCounter.AverageFramesPerSecond);
                spriteBatch.DrawString(fpsFont, sFPS, fpsPosition, Color.MonoGameOrange);
                fpsPosition.Y += 25.0f;
            }

            // Draw Positions
            if (DebugDefines.ShowGridAndPlayerPositions)
            {
                spriteBatch.DrawString(fpsFont, gridPosString, fpsPosition, Color.MonoGameOrange);
                fpsPosition.Y += 25.0f;
                spriteBatch.DrawString(fpsFont, playerPosString, fpsPosition, Color.MonoGameOrange);
                fpsPosition.Y += 25.0f;
            }
            if (DebugDefines.ShowMousePos)
            {
                spriteBatch.DrawString(fpsFont, mousePosString, fpsPosition, Color.MonoGameOrange);
                fpsPosition.Y += 25.0f;
            }
            spriteBatch.End();
        }

        private void DrawSegments()
        {
            spriteBatch.Begin();
            if (DebugDefines.DrawStaticSegments || DebugDefines.DrawStaticVis)
            {
                foreach (GameObject o in objects)
                {
                    if (o.GetType().Equals(typeof(Tile)))
                    {
                        Tile t = (Tile)o;
                        if (t.Light != null)
                        {
                            if (DebugDefines.DrawStaticSegments)
                            {
                                List<Visibility.Segment> segs = t.Light.Visibility.Segments;
                                foreach (Visibility.Segment s in segs)
                                {
                                    DrawLine(spriteBatch, 1f, Color.Yellow, s.P1.Position, s.P2.Position);
                                }
                            }
                            if (DebugDefines.DrawStaticVis)
                            {
                                for (int v = 0; v < t.Light.Encounters.Count; v++)
                                {
                                    if (v < t.Light.Encounters.Count - 1)
                                        DrawLine(spriteBatch, 1, Color.Orange, t.Light.Encounters[v], t.Light.Encounters[v + 1]);
                                    else
                                        DrawLine(spriteBatch, 1, Color.Orange, t.Light.Encounters[v], t.Light.Encounters[0]);
                                }
                            }
                        }
                    }
                }
            }
            if (DebugDefines.DrawPlayerSegments || DebugDefines.DrawPlayerVertices || DebugDefines.DrawPlayerVis)
            {
                foreach (Player p in playerObjects)
                {
                    List<Visibility.Segment> segs = p.Light.Visibility.Segments;
                    foreach (Visibility.Segment s in segs)
                    {
                        if (DebugDefines.DrawPlayerSegments)
                            DrawLine(spriteBatch, 1f, Color.Red, s.P1.Position, s.P2.Position);
                        if (DebugDefines.DrawPlayerVertices)
                        {
                            DrawCircle(spriteBatch, 10, Color.DarkRed, new Vector2(s.P1.Position.X - 5f, s.P1.Position.Y - 5f));
                            DrawCircle(spriteBatch, 10, Color.DarkRed, new Vector2(s.P2.Position.X - 5f, s.P2.Position.Y - 5f));
                        }
                    }

                    if (p.Light != null && p.Light.Encounters != null)
                    {
                        for (int v = 0; v < p.Light.Encounters.Count; v++)
                        {
                            if (DebugDefines.DrawPlayerVis)
                            {
                                if (v < p.Light.Encounters.Count - 1)
                                    DrawLine(spriteBatch, 1, Color.Green, p.Light.Encounters[v], p.Light.Encounters[v + 1]);
                                else
                                    DrawLine(spriteBatch, 1, Color.Green, p.Light.Encounters[v], p.Light.Encounters[0]);
                            }
                            if (DebugDefines.DrawPlayerVisVertices)
                            {
                                DrawCircle(spriteBatch, 10, Color.DarkGreen, new Vector2(p.Light.Encounters[v].X - 5f, p.Light.Encounters[v].Y - 5f));
                            }
                        }
                    }
                }
            }
            spriteBatch.End();
        }
        #endregion

        #region Misc Draw Helpers
        Texture2D GetMouseTexture(MouseState e)
        {
            return mouseTextures[(int)e];
        }

        private void DrawCircle(SpriteBatch batch, int radius, Color color, Vector2 center)
        {
            Texture2D circTex = new Texture2D(GraphicsDevice, radius, radius);
            Color[] colorData = new Color[radius * radius];

            float diam = radius / 2f;
            float diamsq = diam * diam;

            for (int x = 0; x < radius; x++)
            {
                for (int y = 0; y < radius; y++)
                {
                    int index = x * radius + y;
                    Vector2 pos = new Vector2(x - diam, y - diam);
                    if (pos.LengthSquared() <= diamsq)
                    {
                        colorData[index] = Color.White;
                    }
                    else
                    {
                        colorData[index] = Color.Transparent;
                    }
                }
            }

            circTex.SetData(colorData);
            batch.Draw(circTex, center, color);
        }

        public void DrawLine(SpriteBatch batch, float width, Color color, Vector2 point1, Vector2 point2)
        {
            float angle = (float)Math.Atan2(point2.Y - point1.Y, point2.X - point1.X);
            float length = Vector2.Distance(point1, point2);

            batch.Draw(blank, point1, null, color,
                       angle, Vector2.Zero, new Vector2(length, width),
                       SpriteEffects.None, 0);
        }

        public void DrawFilled(SpriteBatch batch, Rectangle rect, Color color, float outWidth, Color outColor)
        {
            // Draw the background
            batch.Draw(blank, rect, color);

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
        #endregion

        #region Objects
        public void AddGameObject(GameObject o)
        {
            objects.Add(o);
        }

        public void RemoveGameObject(GameObject o)
        {
            objects.Remove(o);
        }

        public void AddPlayerObject(Player p)
        {
            playerObjects.Add(p);
        }

        public void RemovePlayerObject(Player p)
        {
            playerObjects.Remove(p);
        }

        public void AddSolid(GameObject o)
        {
            solids.Add(o);
        }

        public void AddLight(PointLight l)
        {
            lights.Add(l);
        }

        public void ClearAllObjects()
        {
            lights.Clear();
            solids.Clear();
            objects.Clear();
            playerObjects.Clear();
        }
        #endregion
    }
}

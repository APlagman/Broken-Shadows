using System.Collections.Generic;
using System.Linq;
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Broken_Shadows
{
    public enum GameState
    {
        None = 0,
        TitleScreen,
        MainMenu,
        OverWorld,
        NUM_STATES
    }

    public class StateHandler : Patterns.Singleton<StateHandler>
    {
        private Game game;
        private Stack<UI.UIScreen> UIStack;
        private GameState state, nextState;
        private bool paused;
        private bool helpViewed;

        private LinkedList<Objects.GameObject> gameObjects = new LinkedList<Objects.GameObject>();
        private List<Objects.Entity> creatures = new List<Objects.Entity>();
        private List<Objects.Player> players = new List<Objects.Player>();

        // For Tile / Player overworld movement and selection.
        private Level level;
        private int levelID;
        private Objects.Tile currentTile, selectedTile;
        private Vector2 gridPos;
        private Vector2 inputDir, playerDir, prevDir, inputFramesLeft, playerFramesLeft;
        private bool gridIsMoving, centerPlayer;

        public bool IsPaused { get { return paused; } set { paused = value; } }

        #region Setup & State Changes
        public void Start(Game game)
        {
            this.game = game;
            state = GameState.None;
            UIStack = new Stack<UI.UIScreen>();
        }

        public void SetState(GameState NewState)
        {
            nextState = NewState;
        }

        void HandleStateChange()
        {
            if (nextState == state)
                return;

            switch (nextState)
            {
                case GameState.MainMenu:
                    Graphics.GraphicsManager.Get().posString = "";
                    UIStack.Clear();
                    UIStack.Push(new UI.UIMainMenu(game.Content));
                    ClearGameObjects();
                    break;
                case GameState.OverWorld:
                    SetupOverWorld();
                    if (!helpViewed) ShowHelpMenu();
                    break;
            }

            state = nextState;
        }

        void SetupOverWorld()
        {
            UIStack.Clear();
            UIStack.Push(new UI.UIOverWorld(game.Content));

            paused = false;
            level = new Level(game);
            levelID = 1;
            LoadLevel();

            gridPos = new Vector2(GlobalDefines.WindowWidth / 2 - GlobalDefines.TileSize / 2, GlobalDefines.WindowHeight / 2 - GlobalDefines.TileSize) - players.First().OriginPosition;
            gridIsMoving = true;
            centerPlayer = true;
        }
        #endregion

        public void Update(float deltaTime)
        {
            HandleStateChange();

            switch (state)
            {
                case GameState.MainMenu:
                    UpdateMainMenu(deltaTime);
                    break;
                case GameState.OverWorld:
                    UpdateOverWorld(deltaTime);
                    break;
            }

            foreach (UI.UIScreen u in UIStack)
            {
                u.Update(deltaTime);
            }
        }

        #region Update Helpers
        void UpdateMainMenu(float deltaTime)
        {

        }

        void UpdateOverWorld(float deltaTime)
        {
            if (!paused)
            {
                if (centerPlayer)
                {
                    gridPos = new Vector2(GlobalDefines.WindowWidth / 2 - GlobalDefines.TileSize / 2, GlobalDefines.WindowHeight / 2 - GlobalDefines.TileSize) - players.First().OriginPosition;
                    gridIsMoving = true;
                }
                UpdateFrames();
                UpdateTiles(deltaTime);
                UpdatePlayers(deltaTime);

                // Use mouse picking to select the appropriate tile.
                Point point = InputManager.Get().CalculateMousePoint();
                currentTile = level.Intersects(point);

                if (DebugDefines.ShowGridAndPlayerPositions)
                    Graphics.GraphicsManager.Get().posString = string.Format("Grid: {0}\nPlayer: {1}", gridPos, players.First().Pose.Position);
            }
        }

        /// <summary>
        /// Reduces input/player frames and sets the vector representing the player's movement.
        /// Input frames allow a slight delay in order to help with diagonal input before moving the player.
        /// Also refreshes the player's previous move direction.
        /// </summary>
        /// <returns>True if any Tiles have been </returns>
        void UpdateFrames()
        {
            bool checkTiles = false;
            // Input
            if (inputFramesLeft.X >= 0)
            {
                inputFramesLeft.X--;
                if (inputFramesLeft.X < 0)
                {
                    playerFramesLeft.X = GlobalDefines.MovementFrames;
                    checkTiles = true;
                }
            }
            if (inputFramesLeft.Y >= 0)
            {
                inputFramesLeft.Y--;
                if (inputFramesLeft.Y < 0)
                {
                    playerFramesLeft.Y = GlobalDefines.MovementFrames;
                    checkTiles = true;
                }
            }

            // Player movement
            if (playerFramesLeft.X >= 0)
            {
                playerFramesLeft.X--;
                if (playerFramesLeft.X < 0 && inputFramesLeft.X < 0)
                    inputDir.X = 0;
            }
            if (playerFramesLeft.Y >= 0)
            {
                playerFramesLeft.Y--;
                if (playerFramesLeft.Y < 0 && inputFramesLeft.Y < 0)
                    inputDir.Y = 0;
            }
            playerDir = new Vector2((playerFramesLeft.X >= 0) ? inputDir.X : 0, (playerFramesLeft.Y >= 0) ? inputDir.Y : 0);
            if (playerDir != Vector2.Zero)
                prevDir = playerDir;
            if (checkTiles && players.First().HasLegalNeighbor(playerDir))
                level.ShiftTiles(playerDir);
        }

        void UpdateTiles(float deltaTime)
        {
            foreach (Objects.GameObject o in gameObjects)
            {
                if (o.Enabled)
                {
                    if (o.GetType().Name.Equals("Tile"))
                    {
                        var t = (Objects.Tile)o;
                        if (playerDir == Vector2.Zero)
                            t.IsMoving = false;
                        if (t.IsMoving)
                            t.OriginPosition += playerDir * GlobalDefines.TileStepSize;
                        t.Pose.Position = t.OriginPosition + gridPos;
                    }
                    o.Update(deltaTime);
                }
            }
        }

        void UpdatePlayers(float deltaTime)
        {
            foreach (Objects.Player player in players)
            {
                if (gridIsMoving)
                {
                    player.Pose.Position = player.CurrentTile.Pose.Position;
                }
                if (player.HasLegalNeighbor(playerDir))
                {
                    player.OriginPosition += playerDir * GlobalDefines.TileStepSize;
                    player.Pose.Position = player.OriginPosition + gridPos;
                    if (centerPlayer) // Both OriginPosition and gridPos have moved, so the player is one step off-center.
                        player.Pose.Position -= playerDir * GlobalDefines.TileStepSize;
                    if (playerFramesLeft.X <= 0 && playerFramesLeft.Y <= 0)
                        player.CurrentTile = level.Intersects(player.Pose.Position.ToPoint());
                }
                if (player.CurrentTile.Equals(level.GoalTile))
                {
                    LoadLevel(true);
                    return;
                }
                player.Update(deltaTime);
            }
        }
        #endregion
        
        void LoadLevel(bool loadNext = false)
        {
            ClearGameObjects();
            inputDir = playerDir = prevDir = Vector2.Zero;
            inputFramesLeft = playerFramesLeft = new Vector2(-1);

            selectedTile = null;
            currentTile = null;

            level.LoadLevel("Levels/Level" + ((loadNext) ? ++levelID : levelID));
            Graphics.GraphicsManager.Get().Level = level;

            players.Add(new Objects.Player(game));
            foreach (Objects.Player player in players)
            {
                player.CurrentTile = level.SpawnTile;
                player.OriginPosition = player.Pose.Position = player.CurrentTile.OriginPosition;
                Graphics.GraphicsManager.Get().AddPlayerObject(player);
                Graphics.GraphicsManager.Get().AddLight(player.Light);
            }
            Graphics.GraphicsManager.Get().RenderLights = false;
        }

        #region Object Creation

        public void SpawnGameObject(Objects.GameObject o, bool isSolid = false)
        {
            o.Load();
            gameObjects.AddLast(o);
            Graphics.GraphicsManager.Get().AddGameObject(o);
            if (isSolid) Graphics.GraphicsManager.Get().AddSolid(o);
        }

        public void RemoveGameObject(Objects.GameObject o, bool removeFromList = true)
        {
            o.Enabled = false;
            o.Unload();
            if (removeFromList)
            {
                gameObjects.Remove(o);
            }
        }

        public void RemovePlayer(Objects.Player p, bool removeFromList = true)
        {
            p.Enabled = false;
            p.Unload();
            if (removeFromList)
            {
                players.Remove(p);
            }
        }

        protected void ClearGameObjects()
        {
            // Clear out any and all game objects
            foreach (Objects.GameObject o in gameObjects)
            {
                RemoveGameObject(o, false);
            }
            gameObjects.Clear();
            foreach (Objects.Player p in players)
            {
                RemovePlayer(p, false);
            }
            players.Clear();
            Graphics.GraphicsManager.Get().ClearAllObjects();
        }
        #endregion

        #region Input
        void SetSelected(Objects.Tile t)
        {
            if (selectedTile != null)
            {
                selectedTile.IsSelected = false;
            }

            selectedTile = t;

            if (selectedTile != null)
            {
                selectedTile.IsSelected = true;
            }
        }

        public void MouseClick(Point Position)
        {
            if (state == GameState.OverWorld && !paused)
            {
                if (currentTile != selectedTile)
                {
                    SetSelected(currentTile);
                }
            }
        }

        /// <summary>
        /// Handles logic regarding player keyboard input.
        /// </summary>
        /// <param name="binds"></param>
        public void KeyboardInput(SortedList<Binding, BindInfo> binds)
        {
            if (state == GameState.OverWorld && !paused)
            {
                gridIsMoving = false;
                if (binds.ContainsKey(Binding.Pan_Left))
                {
                    gridPos.X -= GlobalDefines.GridStepSize;
                    gridIsMoving = true;
                }
                if (binds.ContainsKey(Binding.Pan_Right))
                {
                    gridPos.X += GlobalDefines.GridStepSize;
                    gridIsMoving = true;
                }
                if (binds.ContainsKey(Binding.Pan_Up))
                {
                    gridPos.Y -= GlobalDefines.GridStepSize;
                    gridIsMoving = true;
                }
                if (binds.ContainsKey(Binding.Pan_Down))
                {
                    gridPos.Y += GlobalDefines.GridStepSize;
                    gridIsMoving = true;
                }
                if (binds.ContainsKey(Binding.Reset_Pan))
                {
                    gridPos = Vector2.Zero;
                    gridIsMoving = true;
                }
                if (binds.ContainsKey(Binding.Toggle_Center_Player))
                {
                    centerPlayer = !centerPlayer;
                }
                if (binds.ContainsKey(Binding.Reset_Level))
                {
                    LoadLevel();
                }
                if (binds.ContainsKey(Binding.Move_Up))
                {
                    if (playerFramesLeft.X < 0 && playerFramesLeft.Y < 0 && inputFramesLeft.Y < 0)
                    {
                        inputDir.Y = -1;
                        if (prevDir.Y != -1)
                        {
                            inputFramesLeft.Y = GlobalDefines.InputDelayFrames;
                            if (inputFramesLeft.X >= 0)
                                inputFramesLeft.X = GlobalDefines.InputDelayFrames;
                        }
                        else
                        {
                            inputFramesLeft.Y = 0;
                            if (inputFramesLeft.X >= 0)
                                inputFramesLeft.X = 0;
                        }
                    }
                }
                if (binds.ContainsKey(Binding.Move_Down))
                {
                    if (playerFramesLeft.X < 0 && playerFramesLeft.Y < 0 && inputFramesLeft.Y < 0)
                    {
                        inputDir.Y = 1;
                        if (prevDir.Y != 1)
                        {
                            inputFramesLeft.Y = GlobalDefines.InputDelayFrames;
                            if (inputFramesLeft.X >= 0)
                                inputFramesLeft.X = GlobalDefines.InputDelayFrames;
                        }
                        else
                        {
                            inputFramesLeft.Y = 0;
                            if (inputFramesLeft.X >= 0)
                                inputFramesLeft.X = 0;
                        }
                    }
                }
                if (binds.ContainsKey(Binding.Move_Left))
                {
                    if (playerFramesLeft.X < 0 && playerFramesLeft.Y < 0 && inputFramesLeft.X < 0)
                    {
                        inputDir.X = -1;
                        if (prevDir.X != -1)
                        {
                            inputFramesLeft.X = GlobalDefines.InputDelayFrames;
                            if (inputFramesLeft.Y >= 0)
                                inputFramesLeft.Y = GlobalDefines.InputDelayFrames;
                        }
                        else
                        {
                            inputFramesLeft.X = 0;
                            if (inputFramesLeft.Y >= 0)
                                inputFramesLeft.Y = 0;
                        }
                    }
                }
                if (binds.ContainsKey(Binding.Move_Right))
                {
                    if (playerFramesLeft.X < 0 && playerFramesLeft.Y < 0 && inputFramesLeft.X < 0)
                    {
                        inputDir.X = 1;
                        if (prevDir.X != 1)
                        {
                            inputFramesLeft.X = GlobalDefines.InputDelayFrames;
                            if (inputFramesLeft.Y >= 0)
                                inputFramesLeft.Y = GlobalDefines.InputDelayFrames;
                        }
                        else
                        {
                            inputFramesLeft.X = 0;
                            if (inputFramesLeft.Y >= 0)
                                inputFramesLeft.Y = 0;
                        }
                    }
                }
            }
        }
        #endregion

        #region UI
        public UI.UIScreen GetCurrentUI()
        {
            return UIStack.Peek();
        }

        public int UICount
        {
            get { return UIStack.Count; }
        }

        // Has to be here because only this can access stack!
        public void DrawUI(float deltaTime, SpriteBatch batch)
        {
            // We draw in reverse so the items at the TOP of the stack are drawn after those on the bottom
            foreach (UI.UIScreen u in UIStack.Reverse())
            {
                u.Draw(deltaTime, batch);
            }
        }
        
        public void PopUI()
        {
            UIStack.Peek().OnExit();
            UIStack.Pop();

            if (paused == true && state == GameState.OverWorld)
                paused = false;
        }

        public void ShowPauseMenu()
        {
            paused = true;
            UIStack.Push(new UI.UIPauseMenu(game.Content));
        }

        public void ShowHelpMenu()
        {
            helpViewed = true;
            paused = true;
            UIStack.Push(new UI.UIHelpMenu(game.Content));
        }

        public void Exit()
        {
            game.Exit();
        }
        #endregion UI
    }
}

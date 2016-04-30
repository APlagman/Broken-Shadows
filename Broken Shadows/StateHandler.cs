using System.Collections.Generic;
using System.Linq;
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Broken_Shadows.Objects;

namespace Broken_Shadows
{
    public enum GameState
    {
        None = 0,
        TitleScreen,
        MainMenu,
        OverWorld,
        Creator,
        NUM_STATES
    }

    public class StateHandler : Patterns.Singleton<StateHandler>
    {
        private Game game;
        private Stack<UI.UIScreen> UIStack;
        private GameState state, nextState;
        private bool paused;
        private bool helpViewed;

        private LinkedList<GameObject> gameObjects = new LinkedList<GameObject>();
        private List<Entity> creatures = new List<Entity>();
        private List<Player> players = new List<Player>();

        // For Tile / Player overworld movement and selection.
        private Level level;
        private int levelID;
        private Tile[] currentTiles, selectedTiles;
        private Vector2 gridPos;
        public Vector2 GridPos { get { return gridPos; } }
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

        private void HandleStateChange()
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
                case GameState.Creator:
                    SetupCreator();
                    break;
            }

            state = nextState;
        }

        private void SetupOverWorld()
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

        private void SetupCreator()
        {
            UIStack.Clear();
            UIStack.Push(new UI.UIMapEditor(game.Content));
            
            level = new Level(game);
            UIStack.Push(new UI.UIMapCreate(game.Content));
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
                case GameState.Creator:
                    UpdateCreator(deltaTime);
                    break;
            }

            foreach (UI.UIScreen u in UIStack)
            {
                u.Update(deltaTime);
            }
        }

        #region Update Helpers
        private void UpdateMainMenu(float deltaTime)
        {

        }

        private void UpdateOverWorld(float deltaTime)
        {
            if (!paused)
            {
                if (centerPlayer)
                {
                    gridPos = new Vector2(Graphics.GraphicsManager.Get().Width / 2 - GlobalDefines.TileSize / 2, Graphics.GraphicsManager.Get().Height / 2 - GlobalDefines.TileSize) - players.First().OriginPosition;
                    gridIsMoving = true;
                }
                UpdateFrames();
                UpdateTiles(deltaTime);
                UpdatePlayers(deltaTime);

                if (DebugDefines.ShowGridAndPlayerPositions)
                    Graphics.GraphicsManager.Get().posString = string.Format("Grid: {0}\nPlayer: {1}", gridPos, players.First().Pose.Position);
            }
        }

        /// <summary>
        /// Update helper. Updates tile selection on the map creation interface.
        /// </summary>
        /// <param name="deltaTime"></param>
        private void UpdateCreator(float deltaTime)
        {
            UpdateTiles(deltaTime);

            // Use mouse picking to select the appropriate tile.
            Rectangle selectionBounds = InputManager.Get().CalculateSelectionBounds();
            currentTiles = level.Intersects(selectionBounds);
        }

        /// <summary>
        /// Update helper.
        /// Reduces input/player frames and sets the vector representing the player's movement.
        /// Input frames allow a slight delay in order to help with diagonal input before moving the player.
        /// Also refreshes the player's previous move direction.
        /// </summary>
        private void UpdateFrames()
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
            {
                prevDir = playerDir;
                if (checkTiles && players.First().HasLegalNeighbor(playerDir))
                {
                    System.Diagnostics.Debug.WriteLine("Player Starting Position: " + players.First().OriginPosition);
                    level.ShiftTiles(playerDir);
                }
            }
        }

        /// <summary>
        /// Update helper.
        /// Moves any non-rigid tiles with valid adjacent spots in the same direction as the player.
        /// </summary>
        /// <param name="deltaTime"></param>
        private void UpdateTiles(float deltaTime)
        {
            foreach (GameObject o in gameObjects)
            {
                if (o.Enabled)
                {
                    if (o.GetType().Name.Equals("Tile"))
                    {
                        var t = (Tile)o;
                        if (playerDir == Vector2.Zero)
                            t.IsMoving = false;
                        if (t.IsMoving)
                        {
                            t.OriginPosition += playerDir * GlobalDefines.TileStepSize;
                        }
                        t.Pose.Position = t.OriginPosition + gridPos;
                        if (t.IsMoving && centerPlayer) // Both OriginPosition and gridPos have moved, so the tile is one step off-center.
                            t.Pose.Position -= playerDir * GlobalDefines.TileStepSize;
                    }
                    o.Update(deltaTime);
                }
            }
        }

        /// <summary>
        /// Update helper.
        /// Moves the player's absolute and screen positions appropriately and updates the player's current tile. 
        /// Also checks if the player has reached the goal.
        /// </summary>
        /// <param name="deltaTime"></param>
        private void UpdatePlayers(float deltaTime)
        {
            foreach (Player player in players)
            {
                if (gridIsMoving)
                {
                    player.Pose.Position = player.CurrentTile.Pose.Position;
                }
                if (playerDir != Vector2.Zero && player.HasLegalNeighbor(playerDir) || player.CurrentTile.IsMoving)
                {
                    player.OriginPosition += playerDir * GlobalDefines.TileStepSize;
                    player.Pose.Position = player.OriginPosition + gridPos;
                    if (centerPlayer) // Both OriginPosition and gridPos have moved, so the player is one step off-center.
                        player.Pose.Position -= playerDir * GlobalDefines.TileStepSize;
                    if (playerFramesLeft.X <= 0 && playerFramesLeft.Y <= 0)
                    {
                        System.Diagnostics.Debug.WriteLine("Player Ending Position: " + players.First().OriginPosition);
                        System.Diagnostics.Debug.WriteLine("");
                        player.CurrentTile = level.Intersects(player.Pose.Position.ToPoint());
                    }
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

        private void LoadLevel(bool loadNext = false)
        {
            ClearGameObjects();
            inputDir = playerDir = prevDir = Vector2.Zero;
            inputFramesLeft = playerFramesLeft = new Vector2(-1);

            selectedTiles = null;
            currentTiles = null;

            level.LoadLevel("Levels/Level" + ((loadNext) ? ++levelID : levelID));

            Graphics.GraphicsManager.Get().Level = level;

            players.Add(new Player(game));
            foreach (Player player in players)
            {
                player.CurrentTile = level.SpawnTile;
                player.OriginPosition = player.Pose.Position = player.CurrentTile.OriginPosition;
                player.Pose.Position = player.OriginPosition + gridPos;
                Graphics.GraphicsManager.Get().AddPlayerObject(player);
                Graphics.GraphicsManager.Get().AddLight(player.Light);
            }
            Graphics.GraphicsManager.Get().RenderLights = false;
        }

        #region Map Editor
        public void LoadMap(int width, int height)
        {
            ClearGameObjects();
            selectedTiles = null;
            currentTiles = null;
            level.LoadLevel(width, height);
            Graphics.GraphicsManager.Get().Level = level;
            gridPos = new Vector2(GlobalDefines.TileSize / 2);
        }

        public void ResizeCurrentMap(int width, int height, bool shiftLeft, bool shiftTop)
        {
            ClearGameObjects();
            selectedTiles = null;
            currentTiles = null;
            level.ResizeLevel(width, height, shiftLeft, shiftTop);
            Graphics.GraphicsManager.Get().Level = level;
            gridPos = new Vector2(GlobalDefines.TileSize / 2);
        }

        public void NewMap()
        {
            UIStack.Push(new UI.UIMapCreate(game.Content));
        }

        public void ResetMap()
        {
            ClearGameObjects();
            selectedTiles = null;
            currentTiles = null;
            level.LoadLevel();
            gridPos = new Vector2(GlobalDefines.TileSize / 2);
        }

        public void ResizeMap()
        {
            UIStack.Push(new UI.UIMapResize(game.Content, level.Width, level.Height));
        }

        public void ChangeMapTile()
        {
            if (selectedTiles != null && selectedTiles.Length > 0)
            {
                UIStack.Push(new UI.UIMapTileChange(game.Content, selectedTiles));
                paused = true;
            }
        }

        public void ChangeSelected(Tile selectedTile)
        {
            level.ChangeSelected(selectedTile);
        }

        public void SaveMap()
        {
            level.WriteToXml();
        }
        #endregion

        #region Object Creation
        public void SpawnGameObject(Objects.GameObject o, bool isSolid = false)
        {
            o.Load();
            gameObjects.AddLast(o);
            Graphics.GraphicsManager.Get().AddGameObject(o);
            if (isSolid && !o.GetType().Equals(typeof(Objects.Tile))) Graphics.GraphicsManager.Get().AddSolid(o);
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

        private void ClearGameObjects()
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
        private void SetSelected(Tile[] newTiles)
        {
            if (selectedTiles != null)
            {
                foreach (Tile ti in selectedTiles)
                {
                    if (ti != null)
                        ti.IsSelected = false;
                }
            }

            selectedTiles = newTiles;

            if (selectedTiles != null)
            {
                foreach (Tile ti in selectedTiles)
                    ti.IsSelected = true;
            }
        }

        public void MouseClick(Point Position)
        {
            if (state == GameState.Creator && !paused)
            {
                if ((currentTiles == null && selectedTiles != null) || 
                    (currentTiles != null && selectedTiles == null) || 
                    (currentTiles != null && selectedTiles != null && !currentTiles.Equals(selectedTiles)))
                {
                    SetSelected(currentTiles);
                }
                else
                {
                    ChangeMapTile();
                }
            }
        }

        /// <summary>
        /// Handles logic regarding player keyboard input.
        /// </summary>
        /// <param name="binds"></param>
        public void KeyboardInput(SortedList<Binding, BindInfo> binds)
        {
            if (binds.ContainsKey(Binding.Toggle_FullScreen))
            {
                Graphics.GraphicsManager.Get().ToggleFullScreen();
            }

            #region Map Editor Keybinds
            if (state == GameState.Creator)
            {
                if (binds.ContainsKey(Binding.Enter))
                {
                    if (!paused)
                        ChangeMapTile();
                    else
                    {
                        ((UI.UIMapTileChange)UIStack.Peek()).Change();
                    }
                }
                if ((binds.ContainsKey(Binding.Up) || binds.ContainsKey(Binding.Left)) && paused)
                {
                    ((UI.UIMapTileChange)UIStack.Peek()).Add();
                }
                if ((binds.ContainsKey(Binding.Down) || binds.ContainsKey(Binding.Right)) && paused)
                {
                    ((UI.UIMapTileChange)UIStack.Peek()).Subtract();
                }
            }
            #endregion

            #region Grid Pan Keybinds
            if (state == GameState.OverWorld || state == GameState.Creator && !paused)
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
                    gridPos = new Vector2(GlobalDefines.TileSize / 2);
                    gridIsMoving = true;
                }
            }
            #endregion

            #region OverWorld Keybinds
            if (state == GameState.OverWorld && !paused)
            {
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
            #endregion
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

        public void RefreshUI()
        {
            if (UIStack != null)
            {
                var tempStack = new Stack<UI.UIScreen>(UIStack.Reverse());
                UIStack.Clear();
                for (int u = tempStack.Count - 1; u > -1; u--)
                {
                    UIStack.Push((UI.UIScreen)Activator.CreateInstance(tempStack.ElementAt(u).GetType(), game.Content));
                }
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
            if (Graphics.GraphicsManager.Get().IsFullScreen) Graphics.GraphicsManager.Get().ToggleFullScreen();
            game.Exit();
        }
        #endregion UI
    }
}

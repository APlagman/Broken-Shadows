using System.Collections.Generic;
using System.Linq;
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Broken_Shadows
{
    public enum eGameState
    {
        None = 0,
        TitleScreen,
        MainMenu,
        OverWorld,
        NUM_STATES
    }

    public class GameState : Patterns.Singleton<GameState>
    {
        static int MOVE_FRAMES = GlobalDefines.MOVEMENT_FRAMES;
        static int DELAY_FRAMES = GlobalDefines.INPUT_DELAY_FRAMES;

        Game _game;
        Stack<UI.UIScreen> _UIStack;
        eGameState _state, _nextState;
        bool _paused;
        bool _helpViewed;

        LinkedList<Objects.GameObject> _gameObjects = new LinkedList<Objects.GameObject>();
        List<Objects.Entity> _creatures = new List<Objects.Entity>();
        List<Objects.Player> _players = new List<Objects.Player>();

        // For Tile / Player overworld movement and selection.
        Level _level;
        int _levelID;
        Objects.Tile _currentTile, _selectedTile;
        Vector2 _gridPos;
        Vector2 _inputDir, _playerDir, _prevDir, _inputFramesLeft, _playerFramesLeft;
        bool _gridIsMoving, _centerPlayer;

        public bool IsPaused { get { return _paused; } set { _paused = value; } }

        #region Setup & State Changes
        public void Start(Game game)
        {
            _game = game;
            _state = eGameState.None;
            _UIStack = new Stack<UI.UIScreen>();
        }

        public void SetState(eGameState NewState)
        {
            _nextState = NewState;
        }

        void HandleStateChange()
        {
            if (_nextState == _state)
                return;

            switch (_nextState)
            {
                case eGameState.MainMenu:
                    Graphics.GraphicsManager.Get().posString = "";
                    _UIStack.Clear();
                    _UIStack.Push(new UI.UIMainMenu(_game.Content));
                    ClearGameObjects();
                    break;
                case eGameState.OverWorld:
                    SetupOverWorld();
                    if (!_helpViewed) ShowHelpMenu();
                    break;
            }

            _state = _nextState;
        }

        void SetupOverWorld()
        {
            _UIStack.Clear();
            _UIStack.Push(new UI.UIOverWorld(_game.Content));

            _paused = false;
            _level = new Level(_game);
            _levelID = 1;
            LoadLevel();

            _gridPos = new Vector2(GlobalDefines.WINDOW_WIDTH / 2 - GlobalDefines.TILE_SIZE / 2, GlobalDefines.WINDOW_HEIGHT / 2 - GlobalDefines.TILE_SIZE) - _players.First().OriginPosition;
            _gridIsMoving = true;
            _centerPlayer = true;
        }
        #endregion

        public void Update(float fDeltaTime)
        {
            HandleStateChange();

            switch (_state)
            {
                case eGameState.MainMenu:
                    UpdateMainMenu(fDeltaTime);
                    break;
                case eGameState.OverWorld:
                    UpdateOverWorld(fDeltaTime);
                    break;
            }

            foreach (UI.UIScreen u in _UIStack)
            {
                u.Update(fDeltaTime);
            }
        }

        #region Update Helpers
        void UpdateMainMenu(float fDeltaTime)
        {

        }

        void UpdateOverWorld(float fDeltaTime)
        {
            if (!_paused)
            {
                if (_centerPlayer)
                {
                    _gridPos = new Vector2(GlobalDefines.WINDOW_WIDTH / 2 - GlobalDefines.TILE_SIZE / 2, GlobalDefines.WINDOW_HEIGHT / 2 - GlobalDefines.TILE_SIZE) - _players.First().OriginPosition;
                    _gridIsMoving = true;
                }
                UpdateFrames();
                UpdateTiles(fDeltaTime);
                UpdatePlayers(fDeltaTime);

                // Use mouse picking to select the appropriate tile.
                Point point = InputManager.Get().CalculateMousePoint();
                _currentTile = _level.Intersects(point);

                if (DebugDefines.showGridAndPlayerPositions)
                    Graphics.GraphicsManager.Get().posString = string.Format("Grid: {0}\nPlayer: {1}", _gridPos, _players.First().Position);
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
            if (_inputFramesLeft.X >= 0)
            {
                _inputFramesLeft.X--;
                if (_inputFramesLeft.X < 0)
                {
                    _playerFramesLeft.X = MOVE_FRAMES;
                    checkTiles = true;
                }
            }
            if (_inputFramesLeft.Y >= 0)
            {
                _inputFramesLeft.Y--;
                if (_inputFramesLeft.Y < 0)
                {
                    _playerFramesLeft.Y = MOVE_FRAMES;
                    checkTiles = true;
                }
            }

            // Player movement
            if (_playerFramesLeft.X >= 0)
            {
                _playerFramesLeft.X--;
                if (_playerFramesLeft.X < 0 && _inputFramesLeft.X < 0)
                    _inputDir.X = 0;
            }
            if (_playerFramesLeft.Y >= 0)
            {
                _playerFramesLeft.Y--;
                if (_playerFramesLeft.Y < 0 && _inputFramesLeft.Y < 0)
                    _inputDir.Y = 0;
            }
            _playerDir = new Vector2((_playerFramesLeft.X >= 0) ? _inputDir.X : 0, (_playerFramesLeft.Y >= 0) ? _inputDir.Y : 0);
            if (_playerDir != Vector2.Zero)
                _prevDir = _playerDir;
            if (checkTiles && _players.First().HasLegalNeighbor(_playerDir))
                _level.ShiftTiles(_playerDir);
        }

        void UpdateTiles(float fDeltaTime)
        {
            foreach (Objects.GameObject o in _gameObjects)
            {
                if (o.Enabled)
                {
                    if (o.GetType().Name.Equals("Tile"))
                    {
                        var t = (Objects.Tile)o;
                        if (_playerDir == Vector2.Zero)
                            t.IsMoving = false;
                        if (t.IsMoving)
                            t.OriginPosition += _playerDir * GlobalDefines.TILE_STEP_SIZE;
                        t.Position = t.OriginPosition + _gridPos;
                    }
                    o.Update(fDeltaTime);
                }
            }
        }

        void UpdatePlayers(float fDeltaTime)
        {
            foreach (Objects.Player player in _players)
            {
                if (_gridIsMoving)
                {
                    player.Position = new Vector2(player.CurrentTile.Position.X + GlobalDefines.PLAYER_OFFSET, player.CurrentTile.Position.Y + GlobalDefines.PLAYER_OFFSET);
                }
                if (_playerDir != Vector2.Zero)
                {
                    player.Light.Rotation = CalcAngle(_playerDir);
                }
                if (player.HasLegalNeighbor(_playerDir))
                {
                    player.OriginPosition += _playerDir * GlobalDefines.TILE_STEP_SIZE;
                    player.Position = player.OriginPosition + _gridPos;
                    if (_centerPlayer) // Both OriginPosition and _gridPos have moved, so the player is one step off-center.
                        player.Position -= _playerDir * GlobalDefines.TILE_STEP_SIZE;
                    if (_playerFramesLeft.X <= 0 && _playerFramesLeft.Y <= 0)
                        player.CurrentTile = _level.Intersects(player.OriginPosition.ToPoint());
                }
                if (player.CurrentTile.Equals(_level.GoalTile))
                {
                    LoadLevel(true);
                    return;
                }
                player.Update(fDeltaTime);
            }
        }
        #endregion
        
        void LoadLevel(bool loadNext = false)
        {
            ClearGameObjects();
            _inputDir = _playerDir = _prevDir = Vector2.Zero;
            _inputFramesLeft = _playerFramesLeft = new Vector2(-1);

            _selectedTile = null;
            _currentTile = null;

            _level.LoadLevel("Levels/Level" + ((loadNext) ? ++_levelID : _levelID));
            Graphics.GraphicsManager.Get().ChangeColor(_level.LevelColor);

            _players.Add(new Objects.Player(_game));
            foreach (Objects.Player player in _players)
            {
                player.CurrentTile = _level.SpawnTile;
                player.OriginPosition = player.Position = new Vector2(player.CurrentTile.OriginPosition.X + GlobalDefines.PLAYER_OFFSET, player.CurrentTile.OriginPosition.Y + GlobalDefines.PLAYER_OFFSET);
                player.Position = new Vector2(player.CurrentTile.OriginPosition.X + _gridPos.X + GlobalDefines.PLAYER_OFFSET, player.CurrentTile.OriginPosition.Y + _gridPos.Y + GlobalDefines.PLAYER_OFFSET);
                Graphics.GraphicsManager.Get().AddPlayerObject(player);
                Graphics.GraphicsManager.Get().AddLightObject(player.Light);
            }
            Graphics.GraphicsManager.Get().RenderLights = false;
        }

        #region Object Creation

        public void SpawnGameObject(Objects.GameObject o)
        {
            o.Load();
            _gameObjects.AddLast(o);
            Graphics.GraphicsManager.Get().AddGameObject(o);
        }

        public void RemoveGameObject(Objects.GameObject o, bool removeFromList = true)
        {
            o.Enabled = false;
            o.Unload();
            Graphics.GraphicsManager.Get().RemoveGameObject(o);
            if (removeFromList)
            {
                _gameObjects.Remove(o);
            }
        }

        public void RemovePlayer(Objects.Player p, bool removeFromList = true)
        {
            p.Enabled = false;
            p.Unload();
            Graphics.GraphicsManager.Get().RemovePlayerObject(p);
            if (removeFromList)
            {
                _players.Remove(p);
            }
        }

        protected void ClearGameObjects()
        {
            Graphics.GraphicsManager.Get().ClearLightObjects();
            // Clear out any and all game objects
            foreach (Objects.GameObject o in _gameObjects)
            {
                RemoveGameObject(o, false);
            }
            _gameObjects.Clear();
            foreach (Objects.Player p in _players)
            {
                RemovePlayer(p, false);
            }
            _players.Clear();
        }
        #endregion

        #region Input
        void SetSelected(Objects.Tile t)
        {
            if (_selectedTile != null)
            {
                _selectedTile.IsSelected = false;
            }

            _selectedTile = t;

            if (_selectedTile != null)
            {
                _selectedTile.IsSelected = true;
            }
        }

        public void MouseClick(Point Position)
        {
            if (_state == eGameState.OverWorld && !_paused)
            {
                if (_currentTile != _selectedTile)
                {
                    SetSelected(_currentTile);
                }
            }
        }

        /// <summary>
        /// Handles logic regarding player keyboard input.
        /// </summary>
        /// <param name="binds"></param>
        public void KeyboardInput(SortedList<eBindings, BindInfo> binds)
        {
            if (_state == eGameState.OverWorld && !_paused)
            {
                _gridIsMoving = false;
                if (binds.ContainsKey(eBindings.Pan_Left))
                {
                    _gridPos.X -= GlobalDefines.GRID_STEP_SIZE;
                    _gridIsMoving = true;
                }
                if (binds.ContainsKey(eBindings.Pan_Right))
                {
                    _gridPos.X += GlobalDefines.GRID_STEP_SIZE;
                    _gridIsMoving = true;
                }
                if (binds.ContainsKey(eBindings.Pan_Up))
                {
                    _gridPos.Y -= GlobalDefines.GRID_STEP_SIZE;
                    _gridIsMoving = true;
                }
                if (binds.ContainsKey(eBindings.Pan_Down))
                {
                    _gridPos.Y += GlobalDefines.GRID_STEP_SIZE;
                    _gridIsMoving = true;
                }
                if (binds.ContainsKey(eBindings.Reset_Pan))
                {
                    _gridPos = Vector2.Zero;
                    _gridIsMoving = true;
                }
                if (binds.ContainsKey(eBindings.Toggle_Center_Player))
                {
                    _centerPlayer = !_centerPlayer;
                }
                if (binds.ContainsKey(eBindings.Reset_Level))
                {
                    LoadLevel();
                }
                if (binds.ContainsKey(eBindings.Move_Up))
                {
                    if (_playerFramesLeft.X < 0 && _playerFramesLeft.Y < 0 && _inputFramesLeft.Y < 0)
                    {
                        _inputDir.Y = -1;
                        if (_prevDir.Y != -1)
                        {
                            _inputFramesLeft.Y = DELAY_FRAMES;
                            if (_inputFramesLeft.X >= 0)
                                _inputFramesLeft.X = DELAY_FRAMES;
                        }
                        else
                        {
                            _inputFramesLeft.Y = 0;
                            if (_inputFramesLeft.X >= 0)
                                _inputFramesLeft.X = 0;
                        }
                    }
                }
                if (binds.ContainsKey(eBindings.Move_Down))
                {
                    if (_playerFramesLeft.X < 0 && _playerFramesLeft.Y < 0 && _inputFramesLeft.Y < 0)
                    {
                        _inputDir.Y = 1;
                        if (_prevDir.Y != 1)
                        {
                            _inputFramesLeft.Y = DELAY_FRAMES;
                            if (_inputFramesLeft.X >= 0)
                                _inputFramesLeft.X = DELAY_FRAMES;
                        }
                        else
                        {
                            _inputFramesLeft.Y = 0;
                            if (_inputFramesLeft.X >= 0)
                                _inputFramesLeft.X = 0;
                        }
                    }
                }
                if (binds.ContainsKey(eBindings.Move_Left))
                {
                    if (_playerFramesLeft.X < 0 && _playerFramesLeft.Y < 0 && _inputFramesLeft.X < 0)
                    {
                        _inputDir.X = -1;
                        if (_prevDir.X != -1)
                        {
                            _inputFramesLeft.X = DELAY_FRAMES;
                            if (_inputFramesLeft.Y >= 0)
                                _inputFramesLeft.Y = DELAY_FRAMES;
                        }
                        else
                        {
                            _inputFramesLeft.X = 0;
                            if (_inputFramesLeft.Y >= 0)
                                _inputFramesLeft.Y = 0;
                        }
                    }
                }
                if (binds.ContainsKey(eBindings.Move_Right))
                {
                    if (_playerFramesLeft.X < 0 && _playerFramesLeft.Y < 0 && _inputFramesLeft.X < 0)
                    {
                        _inputDir.X = 1;
                        if (_prevDir.X != 1)
                        {
                            _inputFramesLeft.X = DELAY_FRAMES;
                            if (_inputFramesLeft.Y >= 0)
                                _inputFramesLeft.Y = DELAY_FRAMES;
                        }
                        else
                        {
                            _inputFramesLeft.X = 0;
                            if (_inputFramesLeft.Y >= 0)
                                _inputFramesLeft.Y = 0;
                        }
                    }
                }
            }
        }
        #endregion

        #region UI
        public UI.UIScreen GetCurrentUI()
        {
            return _UIStack.Peek();
        }

        public int UICount
        {
            get { return _UIStack.Count; }
        }

        // Has to be here because only this can access stack!
        public void DrawUI(float fDeltaTime, SpriteBatch batch)
        {
            // We draw in reverse so the items at the TOP of the stack are drawn after those on the bottom
            foreach (UI.UIScreen u in _UIStack.Reverse())
            {
                u.Draw(fDeltaTime, batch);
            }
        }
        
        public void PopUI()
        {
            _UIStack.Peek().OnExit();
            _UIStack.Pop();

            if (_paused == true && _state == eGameState.OverWorld)
                _paused = false;
        }

        public void ShowPauseMenu()
        {
            _paused = true;
            _UIStack.Push(new UI.UIPauseMenu(_game.Content));
        }

        public void ShowHelpMenu()
        {
            _helpViewed = true;
            _paused = true;
            _UIStack.Push(new UI.UIHelpMenu(_game.Content));
        }

        public void Exit()
        {
            _game.Exit();
        }
        #endregion UI

        private float CalcAngle(Vector2 dir)
        {
            if (dir.X == 1 && dir.Y == 1)
                return 0f;
            if (dir.X == 1 && dir.Y == 0)
                return -(float)Math.PI / 4;
            if (dir.X == 1 && dir.Y == -1)
                return -(float)Math.PI / 2;
            if (dir.X == 0 && dir.Y == 1)
                return (float)Math.PI / 4;
            if (dir.X == 0 && dir.Y == -1)
                return -3 * (float)Math.PI / 4;
            if (dir.X == -1 && dir.Y == 1)
                return (float)Math.PI / 2;
            if (dir.X == -1 && dir.Y == 0)
                return 3 * (float)Math.PI / 4;

            return (float)Math.PI;
        }
    }
}

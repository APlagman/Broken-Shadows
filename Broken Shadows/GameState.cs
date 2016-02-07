using System;
using System.Collections.Generic;
using System.Linq;
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
        private static int MOVE_FRAMES = GlobalDefines.MOVEMENT_FRAMES;
        private static int DELAY_FRAMES = GlobalDefines.INPUT_DELAY_FRAMES;

        Game _game;
        Stack<UI.UIScreen> _UIStack;
        UI.UIOverWorld _UIOverWorld;
        eGameState _state, _nextState;
        bool _paused = false;

        LinkedList<Objects.GameObject> _gameObjects = new LinkedList<Objects.GameObject>();
        List<Objects.Creature> _creatures = new List<Objects.Creature>();
        List<Objects.Player> _players = new List<Objects.Player>();

        // For Tile / Player overworld movement and selection.
        Level _level;
        Objects.Tile _currentTile, _selectedTile;
        Vector2 _gridPos;
        Vector2 _inputDir, _playerDir, _prevDir, _inputFramesLeft, _playerFramesLeft;
        bool _gridIsMoving = false;

        public eGameState State { get { return _state; } }
        public bool IsPaused { get { return _paused; } set { _paused = value; } }
        public Level Level { get { return _level; }  }
        public Objects.Tile SelectedTile { get { return _selectedTile; } }

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

        private void HandleStateChange()
        {
            if (_nextState == _state)
                return;

            switch (_nextState)
            {
                case eGameState.MainMenu:
                    _UIStack.Clear();
                    _UIOverWorld = null;
                    _UIStack.Push(new UI.UIMainMenu(_game.Content));
                    ClearGameObjects();
                    break;
                case eGameState.OverWorld:
                    SetupOverWorld();
                    break;
            }

            _state = _nextState;
        }

        public void SetupOverWorld()
        {
            ClearGameObjects();
            _UIStack.Clear();
            _UIOverWorld = new UI.UIOverWorld(_game.Content);
            _UIStack.Push(_UIOverWorld);

            _paused = false;
            _gridPos = Vector2.Zero;
            _gridIsMoving = false;
            _inputDir = _playerDir = _prevDir = Vector2.Zero;
            _inputFramesLeft = _playerFramesLeft = new Vector2(-1);

            _selectedTile = null;
            _currentTile = null;

            SpawnLevel();
            foreach (Objects.Player player in _players)
            {
                player.CurrentTile = _level.SpawnTile;
                player.OriginPosition = player.Position = new Vector2(player.CurrentTile.OriginPosition.X + (float)Math.Sqrt(GlobalDefines.TILE_SIZE), player.CurrentTile.OriginPosition.Y + (float)Math.Sqrt(GlobalDefines.TILE_SIZE));
                Graphics.GraphicsManager.Get().AddPlayerObject(player);
            }
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
            if (!IsPaused)
            {
                UpdateFrames();
                System.Diagnostics.Debug.WriteLine(_gridPos.ToString());
                foreach (Objects.GameObject o in _gameObjects)
                {
                    if (o.Enabled)
                    {
                        if (o.GetType().Name.Equals("Tile"))
                        {
                            o.Position = o.OriginPosition + _gridPos;
                        }
                        o.Update(fDeltaTime);
                    }
                } 
                foreach (Objects.Player player in _players)
                {
                    if (_gridIsMoving)
                    {
                        player.Position = new Vector2(player.CurrentTile.Position.X + (float)Math.Sqrt(GlobalDefines.TILE_SIZE), player.CurrentTile.Position.Y + (float)Math.Sqrt(GlobalDefines.TILE_SIZE));
                    }
                    if (player.HasLegalNeighbor(_playerDir))
                    {                       
                        player.OriginPosition += _playerDir * GlobalDefines.TILE_STEP_SIZE;
                        player.Position = player.OriginPosition + _gridPos;
                        if (_playerFramesLeft.X <= 0 && _playerFramesLeft.Y <= 0)
                            player.CurrentTile = _level.Intersects(player.Position.ToPoint());
                    }
                }              

                // Use mouse picking to select the appropriate tile.
                Point point = InputManager.Get().CalculateMousePoint();
                _currentTile = _level.Intersects(point);
            }
        }

        /// <summary>
        /// Reduces input/player frames and sets the vector representing the player's movement.
        /// Input frames allow a slight delay in order to help with diagonal input before moving the player.
        /// Also refreshes the player's previous move direction.
        /// </summary>
        private void UpdateFrames()
        {
            // Input
            if (_inputFramesLeft.X >= 0)
            {
                _inputFramesLeft.X--;
                if (_inputFramesLeft.X < 0)
                    _playerFramesLeft.X = MOVE_FRAMES;
            }
            if (_inputFramesLeft.Y >= 0)
            {
                _inputFramesLeft.Y--;
                if (_inputFramesLeft.Y < 0)
                    _playerFramesLeft.Y = MOVE_FRAMES;
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
        }
        #endregion

        #region Object Creation
        void SpawnLevel()
        {
            _level = new Level(_game);
            _level.LoadLevel("");
            _players.Add(new Objects.Player(_game));
        }

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

        protected void ClearGameObjects()
        {
            // Clear out any and all game objects
            foreach (Objects.GameObject o in _gameObjects)
            {
                RemoveGameObject(o, false);
            }
            _gameObjects.Clear();
        }
        #endregion

        public void SetSelected(Objects.Tile t)
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
            if (_state == eGameState.OverWorld && !IsPaused)
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
            if (_state == eGameState.OverWorld && !IsPaused)
            {
                _gridIsMoving = false;
                if (binds.ContainsKey(eBindings.Pan_Left))
                {
                    _gridPos.X -= GlobalDefines.TILE_STEP_SIZE;
                    _gridIsMoving = true;
                }
                if (binds.ContainsKey(eBindings.Pan_Right))
                {
                    _gridPos.X += GlobalDefines.TILE_STEP_SIZE;
                    _gridIsMoving = true;
                }
                if (binds.ContainsKey(eBindings.Pan_Up))
                {
                    _gridPos.Y -= GlobalDefines.TILE_STEP_SIZE;
                    _gridIsMoving = true;
                }
                if (binds.ContainsKey(eBindings.Pan_Down))
                {
                    _gridPos.Y += GlobalDefines.TILE_STEP_SIZE;
                    _gridIsMoving = true;
                }
                if (binds.ContainsKey(eBindings.Reset_Pan))
                {
                    _gridPos = Vector2.Zero;
                    _gridIsMoving = true;
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
        }

        public void ShowPauseMenu()
        {
            IsPaused = true;
            _UIStack.Push(new UI.UIPauseMenu(_game.Content));
        }

        public void Exit()
        {
            _game.Exit();
        }
        #endregion UI
    }
}

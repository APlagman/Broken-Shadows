using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Broken_Shadows
{
    public enum eBindType
    {
        JustPressed, // Was just pressed
        JustReleased, // Was just released
        Held // Was just pressed OR being held
    }

    public enum eBindings
    {
        UI_Exit = 0,
        Pan_Down,
        Pan_Up,
        Pan_Left,
        Pan_Right,
        Reset_Pan,
        Toggle_Center_Player,
        Reset_Level,
        Move_Down,
        Move_Up,
        Move_Left,
        Move_Right,
        NUM_BINDINGS
    }

    public class BindInfo
    {
        public BindInfo(Keys Key, eBindType Type)
        {
            _key = Key;
            _type = Type;
        }

        public Keys _key;
        public eBindType _type;

        public override string ToString()
        {
            return _key.ToString() + ", " + _type.ToString(); // For debug purposes
        }
    }

    public enum eMouseState
    {
        Default = 0,
        ScrollUp,
        ScrollRight,
        ScrollDown,
        ScrollLeft,
        MAX_STATES
    }

    public class InputManager : Patterns.Singleton<InputManager>
    {
        // Keyboard binding map
        private SortedList<eBindings, BindInfo> _bindings;
        private void InitializeBindings()
        {
            _bindings = new SortedList<eBindings, BindInfo>();
            // UI Bindings
            _bindings.Add(eBindings.UI_Exit, new BindInfo(Keys.Escape, eBindType.JustPressed));

            // Movement Bindings
            _bindings.Add(eBindings.Pan_Up, new BindInfo(Keys.Up, eBindType.Held));
            _bindings.Add(eBindings.Pan_Left, new BindInfo(Keys.Left, eBindType.Held));
            _bindings.Add(eBindings.Pan_Down, new BindInfo(Keys.Down, eBindType.Held));
            _bindings.Add(eBindings.Pan_Right, new BindInfo(Keys.Right, eBindType.Held));
            _bindings.Add(eBindings.Reset_Pan, new BindInfo(Keys.Tab, eBindType.JustPressed));
            _bindings.Add(eBindings.Toggle_Center_Player, new BindInfo(Keys.LeftShift, eBindType.JustPressed));
            _bindings.Add(eBindings.Reset_Level, new BindInfo(Keys.R, eBindType.JustPressed));
            _bindings.Add(eBindings.Move_Up, new BindInfo(Keys.W, eBindType.Held));
            _bindings.Add(eBindings.Move_Left, new BindInfo(Keys.A, eBindType.Held));
            _bindings.Add(eBindings.Move_Down, new BindInfo(Keys.S, eBindType.Held));
            _bindings.Add(eBindings.Move_Right, new BindInfo(Keys.D, eBindType.Held));
        }

        private SortedList<eBindings, BindInfo> _activeBinds = new SortedList<eBindings, BindInfo>();

        KeyboardState _prevKey, _curKey;
        MouseState _prevMouse, _curMouse;
        eMouseState _mouseState = eMouseState.Default;
        Point _mousePos = Point.Zero;
        public eMouseState MouseState { get { return _mouseState; } set { _mouseState = value; } }
        public Point MousePosition { get { return _mousePos; } }

        public void Start()
        {
            InitializeBindings();

            _prevMouse = Mouse.GetState();
            _curMouse = Mouse.GetState();

            _mousePos = _curMouse.Position;

            _prevKey = Keyboard.GetState();
            _curKey = Keyboard.GetState();
        }

        public void UpdateMouse(float fDeltaTime)
        {
            _prevMouse = _curMouse;
            _curMouse = Mouse.GetState();

            _mousePos = _curMouse.Position;

            // Check for click
            if (JustPressed(_prevMouse.LeftButton, _curMouse.LeftButton))
            {
                // If the UI doesn't handle it, send it to GameState
                if (GameState.Get().UICount == 0 ||
                    !GameState.Get().GetCurrentUI().MouseClick(_mousePos))
                {
                    GameState.Get().MouseClick(_mousePos);
                }
            }
        }

        public void UpdateKeyboard(float fDeltaTime)
        {
            _prevKey = _curKey;
            _curKey = Keyboard.GetState();
            _activeBinds.Clear();

            // Build the list of bindings which were triggered this frame
            foreach (KeyValuePair<eBindings, BindInfo> k in _bindings)
            {
                Keys Key = k.Value._key;
                eBindType Type = k.Value._type;
                switch (Type)
                {
                    case (eBindType.Held):
                        if ( _curKey.IsKeyDown(Key))
                        {
                            _activeBinds.Add(k.Key, k.Value);
                        }
                        break;
                    case (eBindType.JustPressed):
                        if (!_prevKey.IsKeyDown(Key) &&
                            _curKey.IsKeyDown(Key))
                        {
                            _activeBinds.Add(k.Key, k.Value);
                        }
                        break;
                    case (eBindType.JustReleased):
                        if (_prevKey.IsKeyDown(Key) &&
                            !_curKey.IsKeyDown(Key))
                        {
                            _activeBinds.Add(k.Key, k.Value);
                        }
                        break;
                }
            }

            if (_activeBinds.Count > 0)
            {
                // Send the list to the UI first, then any remnants to the game
                if (GameState.Get().UICount != 0)
                {
                    GameState.Get().GetCurrentUI().KeyboardInput(_activeBinds);
                }

                GameState.Get().KeyboardInput(_activeBinds);
            }
        }

        public void Update(float fDeltaTime)
        {
            UpdateMouse(fDeltaTime);
            UpdateKeyboard(fDeltaTime);
            /*if (_activeBinds.Count > 0)
            {
                foreach (KeyValuePair<eBindings, BindInfo> p in _activeBinds)
                    System.Diagnostics.Debug.WriteLine(p);
            }*/
        }

        public Point CalculateMousePoint()
        {
            return _mousePos;
        }

        protected bool JustPressed(ButtonState Previous, ButtonState Current)
        {
            if (Previous == ButtonState.Released &&
                Current == ButtonState.Pressed)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        // Convert key binding to string representing the name
        // TODO: THIS IS NOT LOCALIZED
        public string GetBinding(eBindings binding)
        {
            Keys k = _bindings[binding]._key;
            string name = Enum.GetName(typeof(Keys), k);
            if (k == Keys.OemPlus)
            {
                name = "+";
            }
            else if (k == Keys.OemMinus)
            {
                name = "-";
            }

            return name;
        }
    }
}

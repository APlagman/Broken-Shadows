using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Broken_Shadows
{
    public enum BindType
    {
        JustPressed, // Was just pressed
        JustReleased, // Was just released
        Held // Was just pressed OR being held
    }

    public enum Binding
    {
        UI_Exit = 0,
        Toggle_FullScreen,
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
        Enter,
        Up,
        Down,
        Left,
        Right,
        NUM_BINDINGS
    }

    public class BindInfo
    {
        public BindInfo(Keys Key, BindType Type)
        {
            key = Key;
            type = Type;
        }

        public Keys key;
        public BindType type;

        public override string ToString()
        {
            return key.ToString() + ", " + type.ToString(); // For debug purposes
        }
    }

    public enum MouseState
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
        private SortedList<Binding, BindInfo> bindings;
        private void InitializeBindings()
        {
            bindings = new SortedList<Binding, BindInfo>();
            // UI Bindings
            bindings.Add(Binding.UI_Exit, new BindInfo(Keys.Escape, BindType.JustPressed));
            bindings.Add(Binding.Toggle_FullScreen, new BindInfo(Keys.F12, BindType.JustPressed));

            // Movement Bindings
            bindings.Add(Binding.Pan_Up, new BindInfo(Keys.Up, BindType.Held));
            bindings.Add(Binding.Pan_Left, new BindInfo(Keys.Left, BindType.Held));
            bindings.Add(Binding.Pan_Down, new BindInfo(Keys.Down, BindType.Held));
            bindings.Add(Binding.Pan_Right, new BindInfo(Keys.Right, BindType.Held));
            bindings.Add(Binding.Reset_Pan, new BindInfo(Keys.Tab, BindType.JustPressed));
            bindings.Add(Binding.Toggle_Center_Player, new BindInfo(Keys.LeftShift, BindType.JustPressed));
            bindings.Add(Binding.Reset_Level, new BindInfo(Keys.R, BindType.JustPressed));
            bindings.Add(Binding.Move_Up, new BindInfo(Keys.W, BindType.Held));
            bindings.Add(Binding.Move_Left, new BindInfo(Keys.A, BindType.Held));
            bindings.Add(Binding.Move_Down, new BindInfo(Keys.S, BindType.Held));
            bindings.Add(Binding.Move_Right, new BindInfo(Keys.D, BindType.Held));
            bindings.Add(Binding.Enter, new BindInfo(Keys.Enter, BindType.JustPressed));
            bindings.Add(Binding.Up, new BindInfo(Keys.Up, BindType.JustPressed));
            bindings.Add(Binding.Down, new BindInfo(Keys.Down, BindType.JustPressed));
            bindings.Add(Binding.Left, new BindInfo(Keys.Left, BindType.JustPressed));
            bindings.Add(Binding.Right, new BindInfo(Keys.Right, BindType.JustPressed));
        }

        private SortedList<Binding, BindInfo> activeBinds = new SortedList<Binding, BindInfo>();

        private KeyboardState prevKey, curKey;
        private Microsoft.Xna.Framework.Input.MouseState prevMouse, curMouse;
        private MouseState mouseState = MouseState.Default;
        private Point mousePos = Point.Zero, mouseDownPos = Point.Zero, mouseUpPos = Point.Zero;
        public MouseState MouseState { get { return mouseState; } set { mouseState = value; } }
        public Point MousePosition { get { return mousePos; } }

        public void Start()
        {
            InitializeBindings();

            prevMouse = Mouse.GetState();
            curMouse = Mouse.GetState();

            mousePos = curMouse.Position;

            prevKey = Keyboard.GetState();
            curKey = Keyboard.GetState();
        }

        public void UpdateMouse(float deltaTime)
        {
            prevMouse = curMouse;
            curMouse = Mouse.GetState();

            mousePos = curMouse.Position;

            // Check for click
            if (JustPressed(prevMouse.LeftButton, curMouse.LeftButton))
            {
                mouseDownPos = curMouse.Position;
            }
            // Check for release
            if (prevMouse.LeftButton == ButtonState.Pressed && curMouse.LeftButton == ButtonState.Released)
            {
                mouseUpPos = curMouse.Position;

                // If the UI doesn't handle it, send it to GameState
                if (StateHandler.Get().UICount == 0 ||
                    !StateHandler.Get().GetCurrentUI().MouseClick(mousePos))
                {
                    StateHandler.Get().MouseClick(mousePos);
                }
            }
        }

        public void UpdateKeyboard(float deltaTime)
        {
            prevKey = curKey;
            curKey = Keyboard.GetState();
            activeBinds.Clear();

            // Build the list of bindings which were triggered this frame
            foreach (KeyValuePair<Binding, BindInfo> k in bindings)
            {
                Keys Key = k.Value.key;
                BindType Type = k.Value.type;
                switch (Type)
                {
                    case (BindType.Held):
                        if ( curKey.IsKeyDown(Key))
                        {
                            activeBinds.Add(k.Key, k.Value);
                        }
                        break;
                    case (BindType.JustPressed):
                        if (!prevKey.IsKeyDown(Key) &&
                            curKey.IsKeyDown(Key))
                        {
                            activeBinds.Add(k.Key, k.Value);
                        }
                        break;
                    case (BindType.JustReleased):
                        if (prevKey.IsKeyDown(Key) &&
                            !curKey.IsKeyDown(Key))
                        {
                            activeBinds.Add(k.Key, k.Value);
                        }
                        break;
                }
            }

            if (activeBinds.Count > 0)
            {
                // Send the list to the UI first, then any remnants to the game
                if (StateHandler.Get().UICount != 0)
                {
                    StateHandler.Get().GetCurrentUI().KeyboardInput(activeBinds);
                }

                StateHandler.Get().KeyboardInput(activeBinds);
            }
        }

        public void Update(float deltaTime)
        {
            UpdateMouse(deltaTime);
            UpdateKeyboard(deltaTime);
            /*if (activeBinds.Count > 0)
            {
                foreach (KeyValuePair<eBindings, BindInfo> p in activeBinds)
                    System.Diagnostics.Debug.WriteLine(p);
            }*/
        }

        public Point CalculateMousePoint()
        {
            return mousePos;
        }

        public Rectangle CalculateSelectionBounds()
        {
            int deltaX = mouseUpPos.X - mouseDownPos.X;
            int deltaY = mouseUpPos.Y - mouseDownPos.Y;
            int width = Math.Abs(deltaX);
            int height = Math.Abs(deltaY);

            if (deltaX >= 0 && deltaY >= 0)
                return new Rectangle(mouseDownPos.X, mouseDownPos.Y, width, height);
            else if (deltaX >= 0)
                return new Rectangle(mouseDownPos.X, mouseDownPos.Y - height, width, height);
            else if (deltaY >= 0)
                return new Rectangle(mouseDownPos.X - width, mouseDownPos.Y, width, height);
            else
                return new Rectangle(mouseDownPos.X - width, mouseDownPos.Y - height, width, height);
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
        public string GetBinding(Binding binding)
        {
            Keys k = bindings[binding].key;
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

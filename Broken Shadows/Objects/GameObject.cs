using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Broken_Shadows.Objects
{
    public class GameObject
    {
        private bool _enabled = true;
        protected Game _game;
        protected Texture2D _texture;
        protected string _textureName;
        Vector2 _position, _origin = Vector2.Zero;

        public Vector2 Position { get { return _position; } set { _position = value; } }   
        public Vector2 OriginPosition { get { return _origin; } set { _origin = value; } }
        public bool Enabled { get { return _enabled; } set { _enabled = value; } }
        public string Texture { set { _textureName = value; } }

        public GameObject(Game game, int xPos = 0, int yPos = 0)
        {
            _game = game;
            OriginPosition = Position = new Vector2(xPos, yPos);
        }

        public virtual void Load()
        {
            _texture = _game.Content.Load<Texture2D>(_textureName);
        }

        public virtual void Unload()
        {
        }

        public virtual void Update(float fDeltaTime)
        {
        }

        public virtual void Draw(SpriteBatch batch)
        {
            batch.Draw(_texture, Position, Color.White);
        }
    }
}

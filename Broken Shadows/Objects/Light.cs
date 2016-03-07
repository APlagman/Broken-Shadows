using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Broken_Shadows.Objects
{
    public enum LightType
    {
        SPOTLIGHT = 0,
        CONE,
        NUM_TYPES
    }
    public class Light : GameObject
    {
        Effect lightEffect;

        Texture2D _parentTexture;
        LightType _type;
        Color _color;
        Vector2 _rotationOrigin;
        float _scale;
        float _rotation;

        public int Width { get { return _texture.Width; } }
        public int Height { get { return _texture.Height; } }
        public float Rotation { set { _rotation = value; } }

        public Light(Game game, Color color, LightType type, Texture2D pTex, float scale = 1f)
            : base(game)
        {
            _type = type;
            switch (type)
            {
                case LightType.SPOTLIGHT:
                    Texture = "Shaders/spotlightmask";
                    _rotationOrigin = Vector2.Zero;
                    break;
                case LightType.CONE:
                    Texture = "Shaders/conelightmask";
                    break;
            }
            _parentTexture = pTex;
            _scale = scale;
            _color = color;
            Load();
        }

        public override void Draw(SpriteBatch batch)
        {
            batch.Draw(_texture, _position, null, _color, _rotation, _rotationOrigin, _scale, SpriteEffects.None, 0f);
        }

        public void UpdatePosition(Vector2 pos)
        {
            switch (_type)
            {
                case LightType.SPOTLIGHT:
                    Position = new Vector2(pos.X - Width * _scale / 2 + _parentTexture.Width / 2, pos.Y - Height * _scale / 2 + _parentTexture.Height / 2);
                    break;
                case LightType.CONE:
                    Position = new Vector2(pos.X + _parentTexture.Width / 2, pos.Y + _parentTexture.Height / 2);
                    break;
            }
        }
    }
}

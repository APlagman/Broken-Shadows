using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Broken_Shadows.Objects
{
    public class GameObject
    {
        protected Game game;

        public Pose2D Pose { get; set; }
        // OriginPosition is relative to (0,0) in case the grid is reset to match how it would look in a file. 
        public Vector2 OriginPosition { get; set; }
        public bool Enabled { get; set; }

        public string TextureName { get; set; }
        public string GlowTextureName { get; set; }
        public Texture2D GlowTexture { get; protected set; }
        public Texture2D Texture { get; protected set; }

        public GameObject(Game game, string texture, Pose2D pose)
            :this(game, texture, pose, null) { }

        public GameObject(Game game, string texture, Vector2 position, float rotation)
            : this(game, texture, new Pose2D(position, rotation)) { }

        public GameObject(Game game, string texture, Vector2 position, float rotation, string glowTexture)
            : this(game, texture, new Pose2D(position, rotation), glowTexture) { }

        public GameObject(Game game, string texture, Pose2D pose, string glowTexture)
        {
            Enabled = true;
            this.game = game;
            TextureName = texture;
            GlowTextureName = glowTexture;
            Pose = pose;          
            OriginPosition = pose.Position;
        }

        public virtual void Load()
        {
            Texture = game.Content.Load<Texture2D>(TextureName);
            if (GlowTextureName != null) GlowTexture = game.Content.Load<Texture2D>(GlowTextureName);
        }

        public virtual void Unload()
        {
        }

        public virtual void Update(float deltaTime)
        {
        }

        public virtual void Draw(SpriteBatch batch, float rotation, Vector2 origin, float scale, SpriteEffects effects, float depth)
        {
            batch.Draw(Texture, Pose.Position, null, Color.White, rotation, origin, scale, effects, depth);
        }
    }
}

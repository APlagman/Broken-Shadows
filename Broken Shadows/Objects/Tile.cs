using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Broken_Shadows.Objects
{
    public class Tile : GameObject
    {
        private string selectName = "Tiles/Highlight";
        private static Texture2D selectTexture;
        private Level.TileType type;

        public bool AllowsMovement { get; }
        public bool IsInteractable { get; }
        public bool IsSpawn { get; }
        public bool IsGoal { get; }
        public bool IsRigid { get; }
        public bool IsMoving { get; set; } = false;
        public bool IsSelected { get; set; }
        public Graphics.PointLight Light { get; set; }
        public List<NeighborTile> Neighbors { get; } = new List<NeighborTile>();

        public Tile(Game game, Pose2D pose, Level.TileType type, string textureName = "Tiles/Default", bool isSpawn = false, bool movementAllowed = false, bool isGoal = false, bool isRigid = true, bool canInteract = false, bool selected = false)
            : base(game, textureName, pose)
        {
            IsInteractable = canInteract;
            AllowsMovement = movementAllowed;
            IsSpawn = isSpawn;        
            IsGoal = isGoal;
            IsRigid = isRigid;
            IsSelected = selected;

            this.type = type;
            TextureName = textureName;

            Load();
            if (isSpawn)
                Light = new Graphics.PointLight(Graphics.GraphicsManager.Get().LightEffect, Pose.Position, 75f, Color.Green, 0.5f);
            else if (isGoal)
                Light = new Graphics.PointLight(Graphics.GraphicsManager.Get().LightEffect, Pose.Position, 75f, Color.Red, 0.5f);
        }

        public override void Update(float deltaTime)
        {
            if (Light != null)
            {
                Light.Position = Pose.Position;
                Light.LightMoved = true;
            }

            base.Update(deltaTime);
        }

        public override void Draw(SpriteBatch batch, float rotation, Vector2 origin, float scale, SpriteEffects effects, float depth)
        {
            base.Draw(batch, rotation, origin, scale, effects, depth);
            if (IsSelected)
            {
                batch.Draw(selectTexture, Pose.Position, null, Color.White, rotation, origin, scale, effects, depth);
            }
        }

        public override void Load()
        {
            base.Load();
            selectTexture = game.Content.Load<Texture2D>(selectName);
        }

        public void AddNeighbor(Tile t, string direction)
        {
            Neighbors.Add(new NeighborTile(t, (Direction)Enum.Parse(typeof(Direction), direction)));
        }

        public int ToData()
        {
            return (int)type;
        }
    }
}

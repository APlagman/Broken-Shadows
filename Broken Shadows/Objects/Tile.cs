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
                Light = new Graphics.PointLight(Graphics.GraphicsManager.Get().LightEffect, Pose.Position, 150f, Color.Green, 0.5f);
            else if (isGoal)
                Light = new Graphics.PointLight(Graphics.GraphicsManager.Get().LightEffect, Pose.Position, 100f, Color.Red, 0.5f);
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
            selectTexture = Game.Content.Load<Texture2D>(selectName);
        }

        public void AddNeighbor(Tile t, Direction direction)
        {
            Neighbors.Add(new NeighborTile(t, direction));
        }

        public int ToData()
        {
            return (int)type;
        }

        public static string ToTexture(int data)
        {
            switch (data)
            {
                default: return "Tiles/Empty";
                case 1: return "Tiles/Path";
                case 2: return "Tiles/Wall";
                case 3: return "Tiles/Spawn";
                case 4: return "Tiles/Goal";
                case 5: return "Tiles/Moveable";
            }
        }

        public bool CanMove(Vector2 dir)
        {
            if (IsRigid) return false;

            return Neighbors.FindAll(n => dir.ToAdjacentDirections().Contains(n.Direction)).Count 
                == Neighbors.FindAll(n => dir.ToAdjacentDirections().Contains(n.Direction) && n.GetTile.AllowsMovement).Count;
        }
    }
}

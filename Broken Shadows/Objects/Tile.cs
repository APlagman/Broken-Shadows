using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Broken_Shadows.Objects
{
    public enum Direction
    {
        North,
        NorthEast,
        East,
        SouthEast,
        South,
        SouthWest,
        West,
        NorthWest,
        None
    }

    public enum TileType
    {
        Empty,
        Path,
        Wall,
        Spawn,
        Goal,
        MoveablePath,
        MoveablePath2,
        NUM_TILE_TYPES
    }

    public class Tile : GameObject
    {
        private string selectName = "Tiles/Highlight";
        private static Texture2D selectTexture;
        private TileType type;

        private Level level;

        // Properties for tiles which generally shouldn't change.\
        // Can the player enter the tile?
        public bool AllowsMovement { get; private set; }
        // Can the player interact with the tile?
        public bool IsInteractable { get; private set; }
        // Will the player spawn on this tile?
        public bool IsSpawn { get; private set; }
        // Is this tile the end of a map?
        public bool IsGoal { get; private set; }
        // Is this tile moveable by the player?
        public bool IsRigid { get; private set; }

        // Properties for tiles which may change.
        public bool IsMoving { get; set; } = false;
        public bool IsSelected { get; set; }

        // Used if the tile contains a light.
        public Graphics.PointLight Light { get; set; }
        public bool RecalculateLights { get; set; }

        // Every tile has neighbors and coordinates.
        public Point GridCoordinates { get; set; }

        public Tile(Game game, Pose2D pose, TileType type, Level level, string textureName = "Tiles/Default", bool isSpawn = false, bool movementAllowed = false, bool isGoal = false, bool isRigid = true, bool canInteract = false, bool selected = false)
            : base(game, textureName, pose)
        {
            IsInteractable = canInteract;
            AllowsMovement = movementAllowed;
            IsSpawn = isSpawn;        
            IsGoal = isGoal;
            IsRigid = isRigid;
            IsSelected = selected;

            this.level = level;
            this.type = type;
            TextureName = textureName;

            Load();
            if (isSpawn) // TODO: Remove hardcoded spawn/goal light colors.
                Light = new Graphics.PointLight(Graphics.GraphicsManager.Get().LightEffect, Pose.Position, 150f, Color.Green, 0.5f);
            else if (isGoal)
                Light = new Graphics.PointLight(Graphics.GraphicsManager.Get().LightEffect, Pose.Position, 100f, Color.Red, 0.5f);
        }

        public override void Update(float deltaTime)
        {
            if (Light != null)
            {
                Light.Position = Pose.Position;
                if (RecalculateLights)
                {
                    Light.Recalculate = true;
                }
            }
            RecalculateLights = false;

            base.Update(deltaTime);
        }

        public void ShiftLights(Vector2 toShift)
        {
            if (Light != null && toShift != Vector2.Zero)
            {
                Light.ShiftEncounters(toShift);
                Light.ShiftSegments(toShift);
            }
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

        public Tile GetNeighbor(Direction direction)
        {
            return level.GetTileAt(GridCoordinates.Add(direction.ToVector2()));
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

        /// <summary>
        /// Determines whether the tile can move in the given direction.
        /// </summary>
        /// <param name="vDir">The vector representation of the direction to check.</param>
        /// <param name="endImmediately">Whether to end the check immediately or check non-rigid neighbors recursively.</param>
        /// <returns></returns>
        public bool CanMove(Vector2 vDir, bool endImmediately = false)
        {
            if (IsRigid) return false;
            if (vDir == Vector2.Zero) return false;

            foreach (Direction d in vDir.ToAdjacentDirections())
            {
                Tile neighbor = GetNeighbor(d); 
                // Tiles can only move into non-occupied space and cannot clip through tiles diagonally.
                if (neighbor != null && (neighbor.IsRigid == true || endImmediately == true || neighbor.CanMove(d.ToVector2()) == false))
                    return false;
            }
            return true;
        }
    }
}

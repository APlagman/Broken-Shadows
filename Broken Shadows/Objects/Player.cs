using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace Broken_Shadows.Objects
{
    public class Player : GameObject
    {
        public Tile CurrentTile { get; set; }
        public Graphics.PointLight Light { get; private set; }
        public bool RecalculateLights { get; set; }
        public bool IsMoving { get; set; }

        public Player(Game game)
            : base(game, "Entities/Player", new Pose2D(), null)
        {
            base.Load();

            Load();
            Light = new Graphics.PointLight(Graphics.GraphicsManager.Get().LightEffect, Pose.Position, 250f, Color.White);
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
                Light.ShiftEncounters(toShift);
        }

        /// <summary>
        /// Returns whether the player's current tile has a neighbor in the corresponding direction that allows movement.
        /// </summary>
        /// <param name="dir">The vector direction to check.</param>
        /// <returns>True if the player can move to its neighbor.</returns>
        public bool HasLegalNeighbor(Vector2 dir)
        {
            if (CurrentTile.CanMove(dir))
                return true;

            foreach (Direction d in dir.ToAdjacentDirections())
            {
                Tile neighbor = CurrentTile.GetNeighbor(d);
                if (neighbor == null || neighbor.CanMove(dir) == true || neighbor.AllowsMovement == false)
                    return false;
            }

            return true;
        }
    }
}

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
                    //System.Diagnostics.Debug.WriteLine("Refreshed occluders for player light at " + Light.Position);
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
        /// Returns true if the player's current tile has a neighbor in the corresponding direction that allows movement.
        /// </summary>
        /// <param name="dir">The vector direction to check.</param>
        /// <returns></returns>
        public bool HasLegalNeighbor(Vector2 dir)
        {
            Direction checkDir = dir.ToDirection();

            if (CurrentTile.IsRigid)
            {
                if (checkDir.IsDiagonal())
                {
                    List<Direction> adjDirections = dir.ToAdjacentDirections();
                    List<NeighborTile> neighbors = CurrentTile.Neighbors.FindAll(n => adjDirections.Contains(n.Direction));

                    return neighbors.Exists(n => n.Direction == checkDir)
                        && !neighbors.SingleOrDefault(n => n.Direction == checkDir).GetTile.CanMove(dir)
                        && neighbors.FindAll(n => n.GetTile.AllowsMovement).Count == neighbors.Count;
                }
                else
                {
                    NeighborTile neighbor = CurrentTile.Neighbors.SingleOrDefault(n => n.Direction == checkDir);
                    if (neighbor != null)
                        return neighbor.GetTile.AllowsMovement;
                    return false;
                }
            }
            else
            {
                return CurrentTile.CanMove(dir);
            }
        }
    }
}

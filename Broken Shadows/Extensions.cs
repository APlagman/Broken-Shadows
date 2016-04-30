using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Broken_Shadows.Objects;

namespace Broken_Shadows
{
    static class Extensions
    {
        public static bool IsDiagonal(this Direction dir)
        {
            return dir == Direction.SouthWest || dir == Direction.NorthWest || dir == Direction.SouthEast || dir == Direction.NorthEast;
        }

        public static Direction ToDirection(this Vector2 dir)
        {
            if (dir.X == 0 && dir.Y == 1)
            {
                return Direction.South;
            }
            if (dir.X == 0 && dir.Y == -1)
            {
                return Direction.North;
            }
            if (dir.X == 1 && dir.Y == 1)
            {
                return Direction.SouthEast;
            }
            if (dir.X == 1 && dir.Y == 0)
            {
                return Direction.East;
            }
            if (dir.X == 1 && dir.Y == -1)
            {
                return Direction.NorthEast;
            }
            if (dir.X == -1 && dir.Y == 1)
            {
                return Direction.SouthWest;
            }
            if (dir.X == -1 && dir.Y == 0)
            {
                return Direction.West;
            }
            if (dir.X == -1 && dir.Y == -1)
            {
                return Direction.NorthWest;
            }
            return Direction.None;
        }

        public static List<Direction> ToAdjacentDirections(this Vector2 dir)
        {
            if (dir.X == 0 && dir.Y == 1)
            {
                return new List<Direction> { Direction.South };
            }
            if (dir.X == 0 && dir.Y == -1)
            {
                return new List<Direction> { Direction.North };
            }
            if (dir.X == 1 && dir.Y == 1)
            {
                return new List<Direction> { Direction.South, Direction.East, Direction.SouthEast };
            }
            if (dir.X == 1 && dir.Y == 0)
            {
                return new List<Direction> { Direction.East };
            }
            if (dir.X == 1 && dir.Y == -1)
            {
                return new List<Direction> { Direction.North, Direction.East, Direction.NorthEast };
            }
            if (dir.X == -1 && dir.Y == 1)
            {
                return new List<Direction> { Direction.South, Direction.West, Direction.SouthWest };
            }
            if (dir.X == -1 && dir.Y == 0)
            {
                return new List<Direction> { Direction.West };
            }
            if (dir.X == -1 && dir.Y == -1)
            {
                return new List<Direction> { Direction.North, Direction.West, Direction.NorthWest };
            }
            return new List<Direction> { Direction.None }; 
        }
    }
}

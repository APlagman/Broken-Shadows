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

        public static Vector2 ToVector2(this Direction direction)
        {
            if (direction == Direction.None) return Vector2.Zero;

            int x = 0, y = 0, dir = (int)direction;

            switch (dir)
            {
                case ((int)Direction.North):
                    y = -1;
                    break;
                case ((int)Direction.NorthEast):
                    x = 1;
                    y = -1;
                    break;
                case ((int)Direction.East):
                    x = 1;
                    break;
                case ((int)Direction.SouthEast):
                    x = 1;
                    y = 1;
                    break;
                case ((int)Direction.South):
                    y = 1;
                    break;
                case ((int)Direction.SouthWest):
                    x = -1;
                    y = 1;
                    break;
                case ((int)Direction.West):
                    x = -1;
                    break;
                case ((int)Direction.NorthWest):
                    x = -1;
                    y = -1;
                    break;
            }

            return new Vector2(x, y);
        }

        /// <summary>
        /// Returns whether the given int is between two values, inclusive.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="l"></param>
        /// <param name="r"></param>
        /// <returns></returns>
        public static bool IsBetween(this int x, int l, int r)
        {
            return (x >= l && x <= r);
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

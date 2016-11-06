/**
* Used for checking legal player movement - contains a tile object and a direction enum.
*/
using Microsoft.Xna.Framework;

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

    public class NeighborTile
    {
        public Tile GetTile { get; }
        public Direction Direction { get; }
        public NeighborTile(Tile tile, Direction direction)
        {
            GetTile = tile;
            Direction = direction;
        }
    }
}

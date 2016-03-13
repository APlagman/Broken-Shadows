/**
* Used for checking legal player movement - contains a tile object and a direction enum.
*/
namespace Broken_Shadows.Objects
{
    public enum Direction
    {
        North = 0,
        South,
        West,
        East,
        NorthWest,
        NorthEast,
        SouthWest,
        SouthEast
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

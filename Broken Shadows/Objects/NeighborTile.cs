/**
* Used for checking legal player movement - contains a tile object and a direction enum.
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Broken_Shadows.Objects
{
    public enum eDirection
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
        public eDirection Direction { get; }
        public NeighborTile(Tile tile, eDirection direction)
        {
            GetTile = tile;
            Direction = direction;
        }
    }
}

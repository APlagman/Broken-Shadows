using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Broken_Shadows
{
    public enum eTileType
    {
        Default = 0,
        Test1,
        Test2
    }

    public class Level
    {
        Game _game;
        LinkedList<Objects.Tile> _tiles = new LinkedList<Objects.Tile>();
        Objects.Tile _spawnTile;
        public Objects.Tile SpawnTile { get { return _spawnTile; } }

        public Level(Game game)
        {
            _game = game;
        }

        /// <summary>
        /// Creates an array of tiles from level data and assigns neighbors.
        /// </summary>
        /// <param name="levelName">String representing the name of the level file to load.</param>
        public virtual void LoadLevel(string levelName)
        {
            _tiles.Clear();

            int[][][] TileData = new int[][][]
            {
                // Tile Mappings - WYSIWYG. #-1 represents the absence of a tile.
                new int[][] {
                    new int[] { -1,  2,  2,  2,  1,  2,  2,  2,  2,  2,  2,  2,  2 },
                    new int[] { -1,  2,  2,  1,  1,  2,  2,  2,  2,  2,  2,  2,  2 },
                    new int[] { -1,  2,  2,  1,  2,  2,  2,  2,  2,  2,  2,  2 },
                    new int[] { -1,  2,  2,  2,  1,  2,  2,  2,  2,  2,  2,  2 },
                    new int[] { -1,  2,  2,  2,  2,  1,  1,  1,  2,  2,  2,  2 },
                    new int[] { -1,  2,  2,  2,  2,  1, -1,  1,  2,  2,  2,  2 },
                    new int[] { -1,  2,  2,  2,  2,  1,  1,  1,  2,  2,  2,  2 },
                    new int[] { -1,  2,  2,  2,  2,  2,  2,  2,  2,  2,  2,  2 },
                    new int[] { -1,  2,  2,  2,  2,  2,  2,  2,  2,  2,  2,  2 },
                    new int[] {  2,  2,  2,  2,  2,  2,  2,  2,  2,  2,  2,  2 },
                    new int[] {  2,  2,  2,  2,  2,  2,  2,  2,  2,  2,  2,  2 }
                }, // Metadata - Must contain 1+ spawnable tiles. #1 represents a spawn.
                new int[][] {
                    new int[] { 0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0 },
                    new int[] { 0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0 },
                    new int[] { 0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0 },
                    new int[] { 0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0 },
                    new int[] { 0,  0,  0,  0,  0,  1,  0,  0,  0,  0,  0,  0 },
                    new int[] { 0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0 },
                    new int[] { 0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0 },
                    new int[] { 0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0 },
                    new int[] { 0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0 },
                    new int[] { 0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0 },
                    new int[] { 0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0 }
                },
            };

            // Generate tile array based on height
            int height = TileData[0].Length;
            Objects.Tile[][] Tiles = new Objects.Tile[height][];

            int rowCount = 0;
            while (rowCount < height)
            {
                int width = TileData[0][rowCount].Length;
                Tiles[rowCount] = new Objects.Tile[width];
                int colCount = 0;
                while (colCount < width)
                {
                    if (TileData[0][rowCount][colCount] != -1)
                    {
                        bool isSpawn = (TileData[1][rowCount][colCount] == 1);
                        eTileType type = (eTileType)TileData[0][rowCount][colCount];
                        float tileHeight = GlobalDefines.TILE_SIZE;
                        Objects.Tile t = CreateTile(new Vector2(colCount * tileHeight, rowCount * tileHeight), type, isSpawn);
                        if (t.IsSpawn)
                            _spawnTile = t;

                        Tiles[rowCount][colCount] = t;
                    }
                    colCount++;
                }
                rowCount++;
            }

            // Now loop through the array of tiles to assign neighbors.
            for (int r = 0; r < height; r++)
            {
                int width = Tiles[r].Length;
                for (int c = 0; c < width; c++)
                {
                    if (Tiles[r][c] != null)
                    {
                        if (c + 1 < width && Tiles[r][c + 1] != null)
                        {
                            Tiles[r][c].AddNeighbor(Tiles[r][c + 1], "East");
                        }
                        if (c - 1 >= 0 && Tiles[r][c - 1] != null)
                        {
                            Tiles[r][c].AddNeighbor(Tiles[r][c - 1], "West");
                        }
                        if (r - 1 >= 0 && Tiles[r - 1][c] != null)
                        {
                            Tiles[r][c].AddNeighbor(Tiles[r - 1][c], "North");
                        }
                        if (r + 1 < height && c < Tiles[r + 1].Length && Tiles[r + 1][c] != null)
                        {
                            Tiles[r][c].AddNeighbor(Tiles[r + 1][c], "South");
                        }
                        if ((r - 1 >= 0) && (c + 1 < Tiles[r - 1].Length) && Tiles[r - 1][c + 1] != null)
                        {
                            Tiles[r][c].AddNeighbor(Tiles[r - 1][c + 1], "NorthEast");
                        }
                        if ((r + 1 < height) && (c + 1 < Tiles[r + 1].Length) && Tiles[r + 1][c + 1] != null)
                        {
                            Tiles[r][c].AddNeighbor(Tiles[r + 1][c + 1], "SouthEast");
                        }
                        if ((r + 1 < height) && (c - 1 >= 0) && Tiles[r + 1][c - 1] != null)
                        {
                            Tiles[r][c].AddNeighbor(Tiles[r + 1][c - 1], "SouthWest");
                        }
                        if ((r - 1 >= 0) && (c - 1 >= 0) && Tiles[r - 1][c - 1] != null)
                        {
                            Tiles[r][c].AddNeighbor(Tiles[r - 1][c - 1], "NorthWest");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Returns the correct type of tile based on level data and assigns proper textures/fields.
        /// </summary>
        /// <param name="vPos">The origin position of the tile.</param>
        /// <param name="type">The type of the tile.</param>
        /// <param name="isSpawn">Whether the tile is a spawn.</param>
        /// <returns></returns>
        Objects.Tile CreateTile(Vector2 vPos, eTileType type = eTileType.Default, bool isSpawn = false)
        {
            Objects.Tile Tile = null;
            switch (type)
            {
                case (eTileType.Default):
                    Tile = new Objects.Tile(_game, isSpawn);
                    break;
                case (eTileType.Test1):
                    Tile = new Objects.Tile(_game, isSpawn, "Tiles/TestTile1", true);
                    break;
                case (eTileType.Test2):
                    Tile = new Objects.Tile(_game, isSpawn, "Tiles/TestTile2");
                    break;
            }

            if (Tile != null)
            {
                Tile.OriginPosition = vPos;
                GameState.Get().SpawnGameObject(Tile);
                _tiles.AddLast(Tile);
            }

            return Tile;
        }
        
        public Objects.Tile Intersects(Point point)
        {
            Objects.Tile selected = null;
            Rectangle pointRect = new Rectangle(point, new Point(1));
            foreach (Objects.Tile t in _tiles)
            {
                Rectangle tileRect = new Rectangle(new Point((int)t.Position.X, (int)t.Position.Y), new Point((int)GlobalDefines.TILE_SIZE));
                if (pointRect.Intersects(tileRect))
                    selected = t;
            }

            return selected;
        }
    }
}

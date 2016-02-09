using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;

namespace Broken_Shadows
{
    public enum eTileType
    {
        Default = 0,
        Path,
        Wall,
        Spawn,
        Goal,
        MoveablePath,
        NUM_TILE_TYPES
    }

    public class Level
    {
        Game _game;
        Objects.Tile[,] _Tiles;
        Objects.Tile _spawnTile, _goalTile;
        public Objects.Tile SpawnTile { get { return _spawnTile; } }
        public Objects.Tile GoalTile { get { return _goalTile; } }

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
            _Tiles = null;
            LevelData lData = _game.Content.Load<LevelData>(levelName);
            int height = lData.Height;
            int width = lData.Width;
            int[,] TileData = TranslateTo2D(lData.Layout, height, width);
            
            _Tiles = new Objects.Tile[height,width];

            FillTiles(TileData, _Tiles, height, width);
            AssignNeighbors(_Tiles, height, width);
        }

        /// <summary>
        /// Returns a 2D rectangular array based on a 1D array, height, and width.
        /// </summary>
        /// <param name="array">The 1D array to translate.</param>
        /// <param name="height">The number of rows of elements the 2D array should have.</param>
        /// <param name="width">The number of columns of elements the 2D array should have.</param>
        /// <returns></returns>
        private int[,] TranslateTo2D(int[] array, int height, int width)
        {
            int[,] twoD = new int[height,width];
            for (int i = 0; i < array.Length; i++)
            {
                int r = i / width;
                int c = i % width;
                twoD[r,c] = array[i];
            }
            return twoD;
        }

        /// <summary>
        /// Creates the contents of the Tile array based on the data read from the level file.
        /// </summary>
        /// <param name="TileData">The int array of tile data.</param>
        /// <param name="Tiles">The Tile array to fill.</param>
        /// <param name="height">The number of rows in the Tile array.</param>
        private void FillTiles(int[,] TileData, Objects.Tile[,] Tiles, int height, int width)
        {
            int rowCount = 0;
            while (rowCount < height)
            {
                int colCount = 0;
                while (colCount < width)
                {
                    if (TileData[rowCount,colCount] != -1)
                    {
                        eTileType type = (eTileType)TileData[rowCount,colCount];
                        float tileHeight = GlobalDefines.TILE_SIZE;
                        Objects.Tile t = CreateTile(new Vector2(colCount * tileHeight, rowCount * tileHeight), type);

                        if (t.IsSpawn)
                            _spawnTile = t;
                        if (t.IsGoal)
                            _goalTile = t;

                        Tiles[rowCount,colCount] = t;
                    }
                    colCount++;
                }
                rowCount++;
            }
        }

        /// <summary>
        /// Returns a Tile object based on the tile data and assigns proper textures/fields.
        /// </summary>
        /// <param name="vPos">The origin position of the tile.</param>
        /// <param name="type">The type of the tile.</param>
        /// <param name="isSpawn">Whether the tile is a spawn.</param>
        /// <returns></returns>
        private Objects.Tile CreateTile(Vector2 vPos, eTileType type = eTileType.Default)
        {
            Objects.Tile Tile = null;
            switch (type)
            {
                case (eTileType.Default):
                    Tile = new Objects.Tile(_game);
                    break;
                case (eTileType.Path):
                    Tile = new Objects.Tile(_game, "Tiles/Path", false, true);
                    break;
                case (eTileType.Wall):
                    Tile = new Objects.Tile(_game, "Tiles/Wall");
                    break;
                case (eTileType.Spawn):
                    Tile = new Objects.Tile(_game, "Tiles/Spawn", true, true);
                    break;
                case (eTileType.Goal):
                    Tile = new Objects.Tile(_game, "Tiles/Goal", false, true, true);
                    break;
                case (eTileType.MoveablePath):
                    Tile = new Objects.Tile(_game, "Tiles/Moveable", false, true, false, false);
                    break;
            }

            if (Tile != null)
            {
                Tile.OriginPosition = vPos;
                GameState.Get().SpawnGameObject(Tile);
            }

            return Tile;
        }
    
        /// <summary>
        /// Loops through the Tile array and assigns neighbors depending on position.
        /// </summary>
        /// <param name="tiles">The array of Tile objects.</param>
        /// <param name="height">The number of rows in the array.</param>
        private void AssignNeighbors(Objects.Tile[,] tiles, int height, int width)
        {
            for (int r = 0; r < height; r++)
            {
                for (int c = 0; c < width; c++)
                {
                    if (tiles[r,c] != null)
                    {
                        if (c + 1 < width && tiles[r,c + 1] != null)
                        {
                            tiles[r,c].AddNeighbor(tiles[r,c + 1], "East");
                        }
                        if (c - 1 >= 0 && tiles[r,c - 1] != null)
                        {
                            tiles[r,c].AddNeighbor(tiles[r,c - 1], "West");
                        }
                        if (r - 1 >= 0 && tiles[r - 1,c] != null)
                        {
                            tiles[r,c].AddNeighbor(tiles[r - 1,c], "North");
                        }
                        if (r + 1 < height && (c < width) && tiles[r + 1,c] != null)
                        {
                            tiles[r,c].AddNeighbor(tiles[r + 1,c], "South");
                        }
                        if ((r - 1 >= 0) && (c + 1 < width) && tiles[r - 1,c + 1] != null)
                        {
                            tiles[r,c].AddNeighbor(tiles[r - 1,c + 1], "NorthEast");
                        }
                        if ((r + 1 < height) && (c + 1 < width) && tiles[r + 1,c + 1] != null)
                        {
                            tiles[r,c].AddNeighbor(tiles[r + 1,c + 1], "SouthEast");
                        }
                        if ((r + 1 < height) && (c - 1 >= 0) && tiles[r + 1,c - 1] != null)
                        {
                            tiles[r,c].AddNeighbor(tiles[r + 1,c - 1], "SouthWest");
                        }
                        if ((r - 1 >= 0) && (c - 1 >= 0) && tiles[r - 1,c - 1] != null)
                        {
                            tiles[r,c].AddNeighbor(tiles[r - 1,c - 1], "NorthWest");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Clears each Tile's neighbors and calls AssignNeighbors again.
        /// </summary>
        public void ReassignNeighbors()
        {
            foreach (Objects.Tile t in _Tiles)
            {
                t.Neighbors.Clear();
            }
            AssignNeighbors(_Tiles, _Tiles.Length, _Tiles.GetLength(0));
        }
       
        public Objects.Tile Intersects(Point point)
        {
            Objects.Tile selected = null;
            Rectangle pointRect = new Rectangle(point, new Point(1));
            foreach (Objects.Tile t in _Tiles)
            {
                if (t != null)
                {
                    Rectangle tileRect = new Rectangle(new Point((int)t.Position.X, (int)t.Position.Y), new Point((int)GlobalDefines.TILE_SIZE));
                    if (pointRect.Intersects(tileRect))
                        selected = t;
                }
            }

            return selected;
        }
    }
}

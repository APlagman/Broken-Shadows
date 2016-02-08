using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;

namespace Broken_Shadows
{
    public enum eTileType
    {
        Default = 0,
        TestWalk,
        TestWall,
        TestSpawn,
        TestGoal,
    }

    public class Level
    {
        Game _game;
        LinkedList<Objects.Tile> _tiles = new LinkedList<Objects.Tile>();
        Objects.Tile _spawnTile, _goalTile;
        public Objects.Tile SpawnTile { get { return _spawnTile; } }
        public Objects.Tile GoalTile { get { return _goalTile; } }
        LevelData lData;

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
            ReadXML(levelName);
            int[][][] TileData = new int[2][][] { lData.layout, lData.metadata };

            int height = TileData[0].Length;
            Objects.Tile[][] Tiles = new Objects.Tile[height][];

            FillTiles(TileData, Tiles, height);
            AssignNeighbors(Tiles, height);
        }

        private void ReadXML(string levelName)
        {
            using (FileStream fs = File.OpenRead("Content/Levels/" + levelName))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(LevelData));
                lData = (LevelData)serializer.Deserialize(fs);
            }
        }

        /// <summary>
        /// Creates the contents of the Tile array based on the data read from the level file.
        /// </summary>
        /// <param name="TileData">The int array of tile data.</param>
        /// <param name="Tiles">The Tile array to fill.</param>
        /// <param name="height">The number of rows in the Tile array.</param>
        private void FillTiles(int[][][] TileData, Objects.Tile[][] Tiles, int height)
        {
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
                        bool isGoal = (TileData[1][rowCount][colCount] == 2);

                        eTileType type = (eTileType)TileData[0][rowCount][colCount];
                        float tileHeight = GlobalDefines.TILE_SIZE;
                        Objects.Tile t = CreateTile(new Vector2(colCount * tileHeight, rowCount * tileHeight), type, isSpawn, isGoal);

                        if (t.IsSpawn)
                            _spawnTile = t;
                        if (t.IsGoal)
                            _goalTile = t;

                        Tiles[rowCount][colCount] = t;
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
        private Objects.Tile CreateTile(Vector2 vPos, eTileType type = eTileType.Default, bool isSpawn = false, bool isGoal = false)
        {
            Objects.Tile Tile = null;
            switch (type)
            {
                case (eTileType.Default):
                    Tile = new Objects.Tile(_game, isSpawn: isSpawn);
                    break;
                case (eTileType.TestWalk):
                    Tile = new Objects.Tile(_game, "Tiles/TestWalk", isSpawn, true);
                    break;
                case (eTileType.TestWall):
                    Tile = new Objects.Tile(_game, "Tiles/TestWall", isSpawn);
                    break;
                case (eTileType.TestSpawn):
                    Tile = new Objects.Tile(_game, "Tiles/TestSpawn", isSpawn, true);
                    break;
                case (eTileType.TestGoal):
                    Tile = new Objects.Tile(_game, "Tiles/TestGoal", isSpawn, true, true);
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
    
        /// <summary>
        /// Loops through the Tile array and assigns neighbors depending on position.
        /// </summary>
        /// <param name="tiles">The array of Tile objects.</param>
        /// <param name="height">The number of rows in the array.</param>
        private void AssignNeighbors(Objects.Tile[][] tiles, int height)
        {
            for (int r = 0; r < height; r++)
            {
                int width = tiles[r].Length;
                for (int c = 0; c < width; c++)
                {
                    if (tiles[r][c] != null)
                    {
                        if (c + 1 < width && tiles[r][c + 1] != null)
                        {
                            tiles[r][c].AddNeighbor(tiles[r][c + 1], "East");
                        }
                        if (c - 1 >= 0 && tiles[r][c - 1] != null)
                        {
                            tiles[r][c].AddNeighbor(tiles[r][c - 1], "West");
                        }
                        if (r - 1 >= 0 && tiles[r - 1][c] != null)
                        {
                            tiles[r][c].AddNeighbor(tiles[r - 1][c], "North");
                        }
                        if (r + 1 < height && c < tiles[r + 1].Length && tiles[r + 1][c] != null)
                        {
                            tiles[r][c].AddNeighbor(tiles[r + 1][c], "South");
                        }
                        if ((r - 1 >= 0) && (c + 1 < tiles[r - 1].Length) && tiles[r - 1][c + 1] != null)
                        {
                            tiles[r][c].AddNeighbor(tiles[r - 1][c + 1], "NorthEast");
                        }
                        if ((r + 1 < height) && (c + 1 < tiles[r + 1].Length) && tiles[r + 1][c + 1] != null)
                        {
                            tiles[r][c].AddNeighbor(tiles[r + 1][c + 1], "SouthEast");
                        }
                        if ((r + 1 < height) && (c - 1 >= 0) && tiles[r + 1][c - 1] != null)
                        {
                            tiles[r][c].AddNeighbor(tiles[r + 1][c - 1], "SouthWest");
                        }
                        if ((r - 1 >= 0) && (c - 1 >= 0) && tiles[r - 1][c - 1] != null)
                        {
                            tiles[r][c].AddNeighbor(tiles[r - 1][c - 1], "NorthWest");
                        }
                    }
                }
            }
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

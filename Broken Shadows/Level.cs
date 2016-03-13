using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Broken_Shadows.Objects;

namespace Broken_Shadows
{
    public class Level
    {
        private enum TileType
        {
            Default = 0,
            Path,
            Wall,
            Spawn,
            Goal,
            MoveablePath,
            MoveablePath2,
            NUM_TILE_TYPES
        }

        private Game game;
        private Tile[,] Tiles;
        private Tile spawnTile, goalTile;
        public Tile SpawnTile { get { return spawnTile; } }
        public Tile GoalTile { get { return goalTile; } }
        public Color LevelColor;

        public Level(Game game)
        {
            this.game = game;
        }

        public Tile Intersects(Point point)
        {
            Tile selected = null;
            Rectangle pointRect = new Rectangle(point, new Point(1));
            foreach (Tile t in Tiles)
            {
                if (t != null)
                {
                    Rectangle tileRect = new Rectangle(new Point((int)t.Pose.Position.X - t.Texture.Width / 2, (int)t.Pose.Position.Y - t.Texture.Height / 2), new Point((int)GlobalDefines.TileSize));
                    if (pointRect.Intersects(tileRect))
                        selected = t;
                }
            }

            return selected;
        }

        /// <summary>
        /// Creates an array of tiles from level data and assigns neighbors.
        /// </summary>
        /// <param name="levelName">String representing the name of the level file to load.</param>
        public virtual void LoadLevel(string levelName)
        {
            Tiles = null;
            LevelData lData = game.Content.Load<LevelData>(levelName);
            LevelColor = new Color(lData.RGBA[0], lData.RGBA[1], lData.RGBA[2], lData.RGBA[3]);
            int height = lData.Height;
            int width = lData.Width;
            int[,] TileData = TranslateTo2D(lData.Layout, height, width);
            
            Tiles = new Tile[height,width];

            FillTiles(TileData, Tiles, height, width);
            AssignNeighbors(Tiles, height, width);
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
        private void FillTiles(int[,] TileData, Tile[,] Tiles, int height, int width)
        {
            int rowCount = 0;
            while (rowCount < height)
            {
                int colCount = 0;
                while (colCount < width)
                {
                    if (TileData[rowCount,colCount] != -1)
                    {
                        TileType type = (TileType)TileData[rowCount,colCount];
                        float tileHeight = GlobalDefines.TileSize;
                        Tile t = CreateTile(new Vector2(colCount * tileHeight, rowCount * tileHeight), type);

                        if (t.IsSpawn)
                            spawnTile = t;
                        if (t.IsGoal)
                            goalTile = t;

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
        private Tile CreateTile(Vector2 vPos, TileType type = TileType.Default)
        {
            Tile Tile = null;
            switch (type)
            {
                case (TileType.Default):
                    Tile = new Tile(game, new Pose2D(vPos, 0));
                    break;
                case (TileType.Path):
                    Tile = new Tile(game, new Pose2D(vPos, 0), "Tiles/Path", false, true);
                    break;
                case (TileType.Wall):
                    Tile = new Tile(game, new Pose2D(vPos, 0), "Tiles/Wall");
                    break;
                case (TileType.Spawn):
                    Tile = new Tile(game, new Pose2D(vPos, 0), "Tiles/Spawn", true, true);
                    break;
                case (TileType.Goal):
                    Tile = new Tile(game, new Pose2D(vPos, 0), "Tiles/Goal", false, true, true);
                    break;
                case (TileType.MoveablePath):
                    Tile = new Tile(game, new Pose2D(vPos, 0), "Tiles/Moveable", false, true, false, false);
                    break;
                case (TileType.MoveablePath2):
                    Tile = new Tile(game, new Pose2D(vPos, 0), "Tiles/Moveable2", false, true, false, false);
                    break;
            }

            if (Tile != null)
            {
                StateHandler.Get().SpawnGameObject(Tile, !Tile.AllowsMovement);
                if (Tile.Light != null)
                    Graphics.GraphicsManager.Get().AddLight(Tile.Light);
            }

            return Tile;
        }
    
        /// <summary>
        /// Loops through the Tile array and assigns neighbors depending on position.
        /// </summary>
        /// <param name="tiles">The array of Tile </param>
        /// <param name="height">The number of rows in the array.</param>
        private void AssignNeighbors(Tile[,] tiles, int height, int width)
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
        /// Based on the player's movement direction, checks moveable Tiles and moves them if possible.
        /// </summary>
        /// <param name="vDir">The direction vector that the player moved in.</param>
        /// <returns>True if any tiles were shifted.</returns>
        public bool ShiftTiles(Vector2 vDir)
        {
            if (vDir == Vector2.Zero)
                return false;
            bool shifted = false;
            bool keepChecking = false;
            do
            {         
                Direction direction = GetDirection(vDir);
                System.Diagnostics.Debug.WriteLine("Checking tiles to the " + direction);
                List<Tile> movingTiles = FindMoving(direction);
                if (movingTiles.Count > 0)
                {
                    shifted = true;
                    keepChecking = MoreTilesToMove(direction);
                    SwapTiles(movingTiles, vDir);
                    ReassignNeighbors();
                }
            } while (keepChecking);
            System.Diagnostics.Debug.WriteLine("");
            return shifted;
        }

        /// <summary>
        /// Returns a Direction enum value based on a 2D vector.
        /// </summary>
        /// <param name="vDir"></param>
        /// <returns></returns>
        private Direction GetDirection(Vector2 vDir)
        {
            if (vDir.X == 1 && vDir.Y == 0)
                return Direction.East;
            else if (vDir.X == -1 && vDir.Y == 0)
                return Direction.West;
            else if (vDir.X == 0 && vDir.Y == 1)
                return Direction.South;
            else if (vDir.X == 0 && vDir.Y == -1)
                return Direction.North;
            else if (vDir.X == 1 && vDir.Y == -1)
                return Direction.NorthEast;
            else if (vDir.X == -1 && vDir.Y == -1)
                return Direction.NorthWest;
            else if (vDir.X == 1 && vDir.Y == 1)
                return Direction.SouthEast;
            else
                return Direction.SouthWest;
        }

        /// <summary>
        /// Returns a list of Tiles that are moveable and do not contain a neighbor in the corresponding direction.
        /// </summary>
        /// <param name="direction"></param>
        /// <returns></returns>
        private List<Tile> FindMoving(Direction direction)
        {
            List<Tile> moving = new List<Tile>();
            foreach (Tile t in Tiles)
            {
                if (t != null && !t.IsRigid)
                {
                    bool canMove = true;
                    foreach (NeighborTile n in t.Neighbors)
                    {
                        if (n.Direction == direction) canMove = false;
                    }
                    if (canMove && !t.IsMoving)
                    {
                        t.IsMoving = true;
                        moving.Add(t);
                        System.Diagnostics.Debug.WriteLine(t.OriginPosition);
                    }
                }
            }

            return moving;
        }

        /// <summary>
        /// Returns true if any currently non-moving Tiles are moveable and have moving neighbors.
        /// This allows ShiftTiles() to repeat the movement process if needed.
        /// </summary>
        /// <param name="direction"></param>
        /// <returns></returns>
        private bool MoreTilesToMove(Direction direction)
        {
            foreach (Tile t in Tiles)
            {
                if (t != null && !t.IsRigid)
                {
                    foreach (NeighborTile n in t.Neighbors)
                    {
                        if (n.Direction == direction && !n.GetTile.IsRigid)
                        {
                            System.Diagnostics.Debug.WriteLine(t.OriginPosition + " " + direction + " " + n.GetTile.OriginPosition);
                            bool canMove = true;
                            foreach (NeighborTile nt in n.GetTile.Neighbors)
                            {
                                if (nt.Direction == direction)
                                    canMove = false;
                            }
                            if (canMove)
                                return true;
                        }
                    }
                }            
            }

            return false;
        }

        /// <summary>
        /// Swaps any moving tiles with their respective neighbors, including original position.
        /// </summary>
        /// <param name="movingTiles"></param>
        /// <param name="vDir"></param>
        private void SwapTiles(List<Tile> movingTiles, Vector2 vDir)
        {
            for (int r = 0; r < Tiles.Length; r++)
            {
                for (int c = 0; c < Tiles.GetLength(1); c++)
                {
                    if (movingTiles.Count > 0)
                    {
                        if (movingTiles.Contains(Tiles[r, c]))
                        {
                            Tile temp = Tiles[r, c];
                            Tiles[r, c] = Tiles[r + (int)vDir.Y, c + (int)vDir.X];
                            Tiles[r + (int)vDir.Y, c + (int)vDir.X] = temp;
                            movingTiles.Remove(temp);
                            System.Diagnostics.Debug.WriteLine("Swapping tiles... " + r + "," + c + " and " + (r + (int)vDir.Y) + "," + (c + (int)vDir.X));
                        }
                    }
                    else
                    {
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// Clears each Tile's neighbors and calls AssignNeighbors again.
        /// </summary>
        private void ReassignNeighbors()
        {
            foreach (Tile t in Tiles)
            {
                if (t != null)
                {
                    t.Neighbors.Clear();
                }
            }
            AssignNeighbors(Tiles, Tiles.GetLength(0), Tiles.GetLength(1));
        }      
    }
}

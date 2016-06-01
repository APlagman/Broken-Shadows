using System.Collections.Generic;
using System;
using System.Xml;
using Microsoft.Xna.Framework;
using Broken_Shadows.Objects;

namespace Broken_Shadows
{
    public class Level
    {
        public enum TileType
        {
            Empty = 0,
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
        public int Width { get { return Tiles.GetLength(1); } }
        public int Height { get { return Tiles.GetLength(0); } }
        public Tile SpawnTile { get; private set; }
        public Tile GoalTile { get; private set; }
        public Color LevelColor;

        public Level(Game game)
        {
            this.game = game;
        }

        public Tile Intersects(Point point)
        {
            if (Tiles == null)
                return null;
            Tile selected = null;
            Rectangle selectionRectangle = new Rectangle(point, new Point(1));
            foreach (Tile t in Tiles)
            {
                if (t != null)
                {
                    Rectangle tileRectangle = new Rectangle(new Point((int)t.Pose.Position.X - t.Texture.Width / 2, (int)t.Pose.Position.Y - t.Texture.Height / 2), new Point((int)GlobalDefines.TileSize));
                    if (selectionRectangle.Intersects(tileRectangle))
                        selected = t;
                }
            }

            return selected;
        }

        public Tile[] Intersects(Rectangle selectionRectangle)
        {
            if (Tiles == null)
                return null;
            List<Tile> selected = new List<Tile>();
            foreach (Tile t in Tiles)
            {
                if (t != null)
                {
                    Rectangle tileRectangle = new Rectangle(new Point((int)t.Pose.Position.X - t.Texture.Width / 2, (int)t.Pose.Position.Y - t.Texture.Height / 2), new Point((int)GlobalDefines.TileSize));
                    if (selectionRectangle.Intersects(tileRectangle))
                        selected.Add(t);
                }
            }

            return selected.ToArray();
        }

        #region Level Loading
        /// <summary>
        /// Creates an array of tiles from level data and assigns neighbors.
        /// </summary>
        /// <param name="levelName">String representing the name of the level file to load.</param>
        public virtual void LoadLevel(string levelName)
        {
            Tiles = null;
            LevelData lData;
            try
            {
                lData = game.Content.Load<LevelData>(levelName);
            }
            catch (Microsoft.Xna.Framework.Content.ContentLoadException)
            {
                System.Diagnostics.Debug.WriteLine("*****Level loading failed, returning to main menu.");
                StateHandler.Get().SetState(GameState.MainMenu);
                return;
            }
            LevelColor = new Color(lData.RGBA[0], lData.RGBA[1], lData.RGBA[2], lData.RGBA[3]);
            int height = lData.Height;
            int width = lData.Width;
            int[,] TileData = TranslateTo2D(lData.Layout, height, width);
            
            Tiles = new Tile[height,width];

            FillTiles(TileData, Tiles, height, width);
            AssignNeighbors(Tiles, height, width);
        }

        public virtual void LoadLevel()
        {
            LevelColor = Color.White;
            int[,] TileData = new int[Tiles.GetLength(0), Tiles.GetLength(1)];

            Tiles = new Tile[TileData.GetLength(0), TileData.GetLength(1)];
            FillTiles(TileData, Tiles, Tiles.GetLength(0), Tiles.GetLength(1));
        }

        public virtual void LoadLevel(int width, int height)
        {
            Tiles = null;
            LevelColor = Color.White;
            int[,] TileData = new int[height, width];

            TileData[0, 0] = 1;
            TileData[0, width - 1] = 1;
            TileData[height - 1, 0] = 1;
            TileData[height - 1, width - 1] = 1;

            Tiles = new Tile[height, width];
            FillTiles(TileData, Tiles, height, width);
        }

        public virtual void ResizeLevel(int width, int height, bool shiftLeft, bool shiftTop)
        {
            // Horiz true = left, vert true = top
            int horizMod = width - Tiles.GetLength(1);
            int vertMod = height - Tiles.GetLength(0);
            int[,] TileData = new int[height, width];
            for (int r = ((shiftTop) ? ((vertMod < 0) ? Math.Abs(vertMod) : 0) : 0); r < Tiles.GetLength(0); r++)
            {
                for (int c = ((shiftLeft) ? ((horizMod < 0) ? Math.Abs(horizMod) : 0) : 0); c < Tiles.GetLength(1); c++)
                {
                    System.Diagnostics.Debug.Write("TileData: " + ((shiftTop) ? ((vertMod < 0) ? r : r + vertMod) : r) 
                        + ", " + ((shiftLeft) ? ((horizMod < 0) ? c : c + horizMod) : c)
                        + " " + "Tile: " + r + ", " + c + " ");
                    bool copyTile = true;
                    if ((shiftTop && vertMod > 0 && r + vertMod >= TileData.GetLength(0)) ||
                       (shiftLeft && horizMod > 0 && c + horizMod >= TileData.GetLength(1)) ||
                       (!shiftLeft && c >= TileData.GetLength(1)) ||
                       (!shiftTop && r >= TileData.GetLength(0)))
                            copyTile = false;
                    System.Diagnostics.Debug.WriteLine(copyTile);
                    if (copyTile)
                    {
                        TileData[(shiftTop) ? r + vertMod : r, 
                                 (shiftLeft) ? c + horizMod : c] 
                            = Tiles[r, c].ToData();
                    }
                }
            }

            Tiles = new Tile[height, width];
            FillTiles(TileData, Tiles, height, width);
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
                            SpawnTile = t;
                        if (t.IsGoal)
                            GoalTile = t;

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
        public Tile CreateTile(Vector2 vPos, TileType type = TileType.Empty, bool isMap = false)
        {
            return CreateTile(game, vPos, type, isMap);
        }

        public static Tile CreateTile(Game game, Vector2 vPos, TileType type = TileType.Empty, bool isMap = false)
        {
            Tile Tile = null;
            switch (type)
            {
                case (TileType.Empty):
                    Tile = new Tile(game, new Pose2D(vPos, 0), type, "Tiles/Empty");
                    break;
                case (TileType.Path):
                    Tile = new Tile(game, new Pose2D(vPos, 0), type, "Tiles/Path", false, true);
                    break;
                case (TileType.Wall):
                    Tile = new Tile(game, new Pose2D(vPos, 0), type, "Tiles/Wall");
                    break;
                case (TileType.Spawn):
                    Tile = new Tile(game, new Pose2D(vPos, 0), type, "Tiles/Spawn", true, true);
                    break;
                case (TileType.Goal):
                    Tile = new Tile(game, new Pose2D(vPos, 0), type, "Tiles/Goal", false, true, true);
                    break;
                case (TileType.MoveablePath):
                    Tile = new Tile(game, new Pose2D(vPos, 0), type, "Tiles/Moveable", false, true, false, false);
                    break;
                case (TileType.MoveablePath2):
                    Tile = new Tile(game, new Pose2D(vPos, 0), type, "Tiles/Moveable2", false, true, false, false);
                    break;
            }

            if (Tile != null)
            {
                StateHandler.Get().SpawnGameObject(Tile, !isMap && !Tile.AllowsMovement);
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
        #endregion

        #region Tile Movement
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
            bool keepChecking = true;
            do
            {         
                Direction direction = vDir.ToDirection();
                System.Diagnostics.Debug.WriteLine("Checking tiles to the " + direction);
                List<Tile> movingTiles = FindMoving(direction);
                if (movingTiles.Count > 0)
                {
                    shifted = true;
                    SwapTiles(movingTiles, vDir);
                    ReassignNeighbors();
                }
                else
                    keepChecking = false;
            } while (keepChecking);
            return shifted;
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
                        System.Diagnostics.Debug.WriteLine(t.OriginPosition + " is moving.");
                    }
                }
            }

            return moving;
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
        
        #endregion

        public bool IsWall(int x, int y)
        {
            if (x >= 0 && x < Width && y >= 0 && y < Height)
            {
                if (Tiles[y, x] == null)
                    return false;
                return !Tiles[y, x].AllowsMovement && Tiles[y, x].Neighbors.Exists(n => n.GetTile.AllowsMovement);
            }
            else
                return true;
        }

        public void ChangeSelected(Tile selectedTile)
        {
            for (int r = 0; r < Tiles.GetLength(0); r++)
            {
                for (int c = 0; c < Tiles.GetLength(1); c++)
                {
                    if (Tiles[r, c].IsSelected && Tiles[r, c].OriginPosition.Equals(selectedTile.OriginPosition))
                    {
                        Tiles[r, c] = selectedTile;
                    }
                }
            }
        }

        #region XML
        public void WriteToXml()
        {
            XmlWriter writer = XmlWriter.Create(@"Custom.xml");
            writer.WriteStartDocument();

            writer.WriteStartElement("XnaContent");
            writer.WriteStartElement("Asset");
            writer.WriteAttributeString("Type", "Broken_Shadows.LevelData");
            writer.WriteElementString("Width", Width.ToString());
            writer.WriteElementString("Height", Height.ToString());
            writer.WriteElementString("Layout", LayoutString());
            writer.WriteElementString("RGBA", ColorString());
            writer.WriteEndElement();
            writer.WriteEndElement();

            writer.WriteEndDocument();
            writer.Close();
        }

        private string LayoutString()
        {
            string str = "\n";

            for (int r = 0; r < Tiles.GetLength(0); r++)
            {
                str += "\t";
                for (int c = 0; c < Tiles.GetLength(1); c++)
                {
                    str += ((Tiles[r, c].ToData() == 0) ? -1 : Tiles[r, c].ToData()) + " ";
                }
                str += "\n";
            }

            return str;
        }

        private string ColorString()
        {
            return LevelColor.R + " " + LevelColor.G + " " + LevelColor.B + " " + LevelColor.A;
        }
        #endregion
    }
}

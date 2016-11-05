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
        private Tile[,] tileGrid;
        private List<Tile> moveableTiles;
        public int Width { get { return tileGrid.GetLength(1); } }
        public int Height { get { return tileGrid.GetLength(0); } }
        public Tile SpawnTile { get; private set; }
        public Tile GoalTile { get; private set; }
        public Color LevelColor;

        public Level(Game game)
        {
            this.game = game;
        }

        public Tile Intersects(Point point)
        {
            if (tileGrid == null)
                return null;
            Tile selected = null;
            Rectangle selectionRectangle = new Rectangle(point, new Point(1));
            foreach (Tile t in tileGrid)
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
            if (tileGrid == null)
                return null;
            List<Tile> selected = new List<Tile>();
            foreach (Tile t in tileGrid)
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
        /// <returns>True upon successfully load, false otherwise.</returns>
        public virtual bool LoadLevel(string levelName)
        {
            tileGrid = null;
            LevelData lData;
            try
            {
                lData = game.Content.Load<LevelData>(levelName);
            }
            catch (Microsoft.Xna.Framework.Content.ContentLoadException)
            {
                System.Diagnostics.Debug.WriteLine("*****Level loading failed, returning to main menu.");
                StateHandler.Get().SetState(GameState.MainMenu);
                return false;
            }
            LevelColor = new Color(lData.RGBA[0], lData.RGBA[1], lData.RGBA[2], lData.RGBA[3]);
            int height = lData.Height;
            int width = lData.Width;
            int[,] TileData = TranslateTo2D(lData.Layout, height, width);
            
            tileGrid = new Tile[height,width];

            FillTiles(TileData, tileGrid, height, width);
            AssignNeighborsToAllTiles(tileGrid, height, width);
            return true;
        }

        public virtual void LoadLevel()
        {
            LevelColor = Color.White;
            int[,] TileData = new int[tileGrid.GetLength(0), tileGrid.GetLength(1)];

            tileGrid = new Tile[TileData.GetLength(0), TileData.GetLength(1)];
            FillTiles(TileData, tileGrid, tileGrid.GetLength(0), tileGrid.GetLength(1));
        }

        public virtual void LoadLevel(int width, int height)
        {
            tileGrid = null;
            LevelColor = Color.White;
            int[,] TileData = new int[height, width];

            TileData[0, 0] = 1;
            TileData[0, width - 1] = 1;
            TileData[height - 1, 0] = 1;
            TileData[height - 1, width - 1] = 1;

            tileGrid = new Tile[height, width];
            FillTiles(TileData, tileGrid, height, width);
        }

        public virtual void ResizeLevel(int width, int height, bool shiftLeft, bool shiftTop)
        {
            // Horiz true = left, vert true = top
            int horizMod = width - tileGrid.GetLength(1);
            int vertMod = height - tileGrid.GetLength(0);
            int[,] TileData = new int[height, width];
            for (int r = ((shiftTop) ? ((vertMod < 0) ? Math.Abs(vertMod) : 0) : 0); r < tileGrid.GetLength(0); r++)
            {
                for (int c = ((shiftLeft) ? ((horizMod < 0) ? Math.Abs(horizMod) : 0) : 0); c < tileGrid.GetLength(1); c++)
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
                            = tileGrid[r, c].ToData();
                    }
                }
            }

            tileGrid = new Tile[height, width];
            FillTiles(TileData, tileGrid, height, width);
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
                        t.GridCoordinates = new Point(colCount, rowCount);
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
                {
                    Graphics.GraphicsManager.Get().AddLight(Tile.Light);
                }
            }

            return Tile;
        }

        /// <summary>
        /// Loops through the Tile array and assigns neighbors to non-rigid tiles depending on position.
        /// </summary>
        /// <param name="tiles">The array of Tile </param>
        /// <param name="height">The number of rows in the array.</param>
        private void AssignNeighborsToAllTiles(Tile[,] tiles, int height, int width)
        {
            for (int dir = 0; dir < 8; dir++)
            {
                var dirVector = ((Direction)dir).ToVector2();
                int dx = (int)dirVector.X;
                int dy = (int)dirVector.Y;

                for (int r = 0; r < height; r++) // Row
                {
                    for (int c = 0; c < width; c++) // Column
                    {
                        int newRow = r + dy;
                        int newCol = c + dx;
                        if (tiles[r, c] != null && !tiles[r, c].IsRigid && IsWithinLevelBounds(newCol, newRow) && tiles[newRow, newCol] != null)
                        {
                            tiles[r, c].AddNeighbor(tiles[newRow, newCol], (Direction)dir);
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
            if (vDir == Vector2.Zero) // The player hasn't moved, so the tiles shouldn't move.
                return false;
            bool shifted = false; // Have any tiles shifted?
            bool keepChecking = true;
            
            // Moveable tiles don't all move at the same time; rather, they move in a "chain": e.g. 123x -> 12x3 -> 1x23 -> x123.
            // The loop tracks (via keepChecking) whether or not the previous iteration caused a change - have all chains completed moving?
            do
            {         
                Direction direction = vDir.ToDirection();
                List<Tile> movingTiles = FindMovingTiles(direction); // Which tiles can move?
                if (movingTiles.Count > 0) // If any can move, update them.
                {
                    shifted = true;
                    SwapTiles(movingTiles, vDir);
                    ReassignNeighborTiles(movingTiles); // Neighbors are relative to the tile's current position and must be updated accordingly.
                }
                else
                    keepChecking = false;
            } while (keepChecking);

            return shifted;
        }

        /// <summary>
        /// Returns a list of Tiles that are moveable and do not contain a neighbor in the corresponding direction.
        /// </summary>
        /// <param name="direction">The cardinal direction (up/down/left/right/diagonal) to check if a tile can move in.</param>
        /// <returns>A list of Tiles which can legally move.</returns>
        private List<Tile> FindMovingTiles(Direction direction)
        {
            List<Tile> moving = new List<Tile>();
            foreach (Tile t in moveableTiles) // The current level stores a list of moveable tiles to prevent iterating through the entire grid.
            {
                bool canMove = t.Neighbors.Exists(n => (n.Direction == direction)); // Tiles can only move into non-occupied space.
                if (canMove && !t.IsMoving)
                {
                    t.IsMoving = true; // IsMoving is used by the game update logic to visually "slide" tiles from one spot to another.
                    t.RecalculateLights = true; // Only dynamically update the lighting on tiles that move.
                    moving.Add(t);
                }
            }

            return moving;
        }

        /// <summary>
        /// Swaps any moving tiles with their respective neighbor in the grid. Has no effect on visuals.
        /// </summary>
        /// <param name="movingTiles">The list of Tiles to swap.</param>
        /// <param name="vDir">The direction in which to swap each tile.</param>
        private void SwapTiles(List<Tile> movingTiles, Vector2 vDir)
        {
            int dx = (int)vDir.X;
            int dy = (int)vDir.Y;

            foreach (Tile t in movingTiles)
            {
                int row = t.GridCoordinates.Y;
                int col = t.GridCoordinates.X;
                int newRow = row + dy;
                int newCol = col + dx;

                Tile temp = tileGrid[row, col];
                tileGrid[row, col] = tileGrid[newRow, newCol];
                tileGrid[row, col].GridCoordinates = new Point(col, row); // Update each tile's knowledge of its position in the grid to assist further shifting.
                tileGrid[newRow, newCol] = temp;
                tileGrid[newRow, newCol].GridCoordinates = new Point(newCol, newRow);
            }
        }

        /// <summary>
        /// Reassigns neighbors to the specified tiles in each of the 8 cardinal directions (includes diagonals).
        /// </summary>
        /// <param name="needReassigned">List of tiles that need their neighbors reassigned. Should only ever be moving tiles.</param>
        private void ReassignNeighborTiles(List<Tile> needReassigned)
        {
            for (int dir = 0; dir < 8; dir++)
            {
                var dirVector = ((Direction)dir).ToVector2();

                foreach (Tile t in needReassigned)
                {
                    t.Neighbors.Clear();
                    int newRow = t.GridCoordinates.Y + (int)dirVector.Y; // Row + dy
                    int newCol = t.GridCoordinates.X + (int)dirVector.X; // Column + dx
                    
                    if (IsWithinLevelBounds(newCol, newRow) && tileGrid[newRow, newCol] != null) // Make sure only valid neighbors are added.
                    {
                        t.AddNeighbor(tileGrid[newRow, newCol], (Direction)dir);
                    }
                }
            }
        }
        #endregion

        public bool IsWall(int x, int y)
        {
            if (x >= 0 && x < Width && y >= 0 && y < Height)
            {
                if (tileGrid[y, x] == null)
                    return false;
                return !tileGrid[y, x].AllowsMovement && tileGrid[y, x].Neighbors.Exists(n => n.GetTile.AllowsMovement);
            }
            else
                return false;
        }

        public bool IsWithinLevelBounds(int x, int y)
        {
            if (x.IsBetween(0, Width - 1) && y.IsBetween(0, Height - 1))
                return true;
            return false;
        }

        public void ChangeSelected(Tile selectedTile)
        {
            for (int r = 0; r < tileGrid.GetLength(0); r++)
            {
                for (int c = 0; c < tileGrid.GetLength(1); c++)
                {
                    if (tileGrid[r, c].IsSelected && tileGrid[r, c].OriginPosition.Equals(selectedTile.OriginPosition))
                    {
                        tileGrid[r, c] = selectedTile;
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

            for (int r = 0; r < tileGrid.GetLength(0); r++)
            {
                str += "\t";
                for (int c = 0; c < tileGrid.GetLength(1); c++)
                {
                    str += ((tileGrid[r, c].ToData() == 0) ? -1 : tileGrid[r, c].ToData()) + " ";
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

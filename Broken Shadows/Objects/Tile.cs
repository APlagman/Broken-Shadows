using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Broken_Shadows.Objects
{
    public class Tile : GameObject
    {
        string _selectName;
        static Texture2D _selectTexture;
        bool _interactable;
        bool _isSpawn;
        bool _isGoal;
        bool _allowsMovement;
        bool _isRigid;
        bool _isMoving;
        bool _selected;

        public bool AllowsMovement { get { return _allowsMovement; } }
        public bool IsInteractable { get { return _interactable; } }
        public bool IsSpawn { get { return _isSpawn; } }
        public bool IsGoal { get { return _isGoal; } }
        public bool IsRigid { get { return _isRigid; } }
        public bool IsMoving { get { return _isMoving; } set { _isMoving = value; } }
        public bool IsSelected { get { return _selected; } set { _selected = value; } }
        public LinkedList<NeighborTile> Neighbors = new LinkedList<NeighborTile>();

        public Tile(Game game, string textureName = "Tiles/BlankTile", bool isSpawn = false, bool movementAllowed = false, bool isGoal = false, bool isRigid = true, bool canInteract = false, bool selected = false)
            : base(game)
        {
            _interactable = canInteract;
            _allowsMovement = movementAllowed;
            _isSpawn = isSpawn;        
            _isGoal = isGoal;
            _isRigid = isRigid;
            _isMoving = false;
            _selected = selected;

            _textureName = textureName;
            _selectName = "Tiles/Highlight";
        }

        public override void Draw(SpriteBatch batch)
        {
            base.Draw(batch);
            if (IsSelected)
                batch.Draw(_selectTexture, Position, Color.White);
        }

        public override void Load()
        {
            base.Load();
            _selectTexture = _game.Content.Load<Texture2D>(_selectName);
        }

        public void AddNeighbor(Tile t, string direction)
        {
            Neighbors.AddLast(new NeighborTile(t, (eDirection)Enum.Parse(typeof(eDirection), direction)));
        }
    }
}

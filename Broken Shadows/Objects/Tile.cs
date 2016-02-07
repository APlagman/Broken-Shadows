using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Broken_Shadows.Objects
{
    public class Tile : GameObject
    {
        protected string _selectName;
        protected static Texture2D _selectTexture;
        protected bool _interactable;
        protected bool _spawnable;
        protected bool _walkable;
        protected bool _selected;

        public bool CanEnter { get { return _walkable; } }
        public bool IsInteractable { get { return _interactable; } }
        public bool IsSpawn { get { return _spawnable; } }
        public bool IsSelected { get { return _selected; } set { _selected = value; } }
        public LinkedList<NeighborTile> Neighbors = new LinkedList<NeighborTile>();

        public Tile(Game game, bool canSpawn = false, string textureName = "Tiles/BlankTile", bool canWalk = false, bool canInteract = false, bool selected = false)
            : base(game)
        {
            _interactable = canInteract;
            _spawnable = canSpawn;
            _walkable = canWalk;
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

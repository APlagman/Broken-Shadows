using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace Broken_Shadows.Objects
{
    public enum eSkillType
    {
        Attack = 0,
        Strength,
        Defence,
        Constitution,
        Ranged,
        Magic,
        Prayer,
        Woodcutting,
        Firemaking,
        Fletching,
        Mining,
        Smithing,
        Crafting,
        Thieving,
        Alchemy,
        Fishing,
        Cooking,
        Farming,
        Summoning
    }

    public struct Skill
    {
        public eSkillType Name { get; private set; }
        public int SkillLevel { get; set; }

        public Skill(eSkillType name, int level)
        {
            Name = name;
            SkillLevel = level;
        }
    }

    public class Player : Creature
    {
        private List<Skill> _levels;
        public List<Skill> SkillLevels { get { return _levels; } protected set { _levels = value; } }
        public Tile CurrentTile { get; set; }

        public Player(Game game)
            : base(game)
        {
            Texture = "Player";
            base.Load();
            _levels = CreateSkills();
            UpdateHealth();
            Defence = _levels.Find(s => s.Name == eSkillType.Defence).SkillLevel;
        }

        public override void Update(float fDeltaTime)
        {
            UpdateCombatLevel();
            
            base.Update(fDeltaTime);
        }

        /// <summary>
        /// Returns a new list of player skills starting at the base levels.
        /// </summary>
        /// <returns></returns>
        private List<Skill> CreateSkills()
        {
            var list = new List<Skill>();
            foreach (eSkillType skill in Enum.GetValues(typeof(eSkillType)))
            {
                list.Add(new Skill(skill, (skill == eSkillType.Constitution) ? 10 : 1)); // Constitution starts at level 10.
            }

            return list;
        }

        /// <summary>
        /// Calculates the player's total combat level based on their combat skills.
        /// </summary>
        private void UpdateCombatLevel()
        {
            List<Skill> fightSkills = _levels.FindAll(n => (n.Name == eSkillType.Attack || n.Name == eSkillType.Strength || n.Name == eSkillType.Ranged || n.Name == eSkillType.Magic)).ToList();
            int[] fightLevels = fightSkills.Select(l => l.SkillLevel).ToArray();
            int highestCombat = new int[] { fightLevels[0] + fightLevels[1], 2 * fightLevels[2], 2 * fightLevels[3] }.Max();
            int[] cbLevels = _levels.FindAll(l => l.Name == eSkillType.Defence || l.Name == eSkillType.Constitution || l.Name == eSkillType.Prayer || l.Name == eSkillType.Summoning).Select(l => l.SkillLevel).ToArray();
            cbLevels[2] = (int)(cbLevels[2] * 0.5);
            cbLevels[3] = (int)(cbLevels[3] * 0.5);

            _combatLevel = (int)(0.25 * (1.3 * highestCombat + cbLevels.Sum()));
        }

        /// <summary>
        /// Sets the player's max health depending on their Constitution level.
        /// </summary>
        public override void UpdateHealth()
        {
            _maxHealth = _levels.Find(s => s.Name == eSkillType.Constitution).SkillLevel * 10;
            base.UpdateHealth();
        }

        /// <summary>
        /// Returns true if the player's current tile has a neighbor in the corresponding direction that allows movement.
        /// </summary>
        /// <param name="dir">The vector direction to check.</param>
        /// <returns></returns>
        public bool HasLegalNeighbor(Vector2 dir)
        {
            bool canMove = false;
            // W
            if (dir.X == -1 && dir.Y == 0)
            {
                foreach (NeighborTile neighbor in CurrentTile.Neighbors)
                {
                    if (neighbor.Direction == eDirection.West && neighbor.GetTile.CanEnter)
                        canMove = true;
                }
            }
            // E
            if (dir.X == 1 && dir.Y == 0)
            {
                foreach (NeighborTile neighbor in CurrentTile.Neighbors)
                {
                    if (neighbor.Direction == eDirection.East && neighbor.GetTile.CanEnter)
                        canMove = true;
                }
            }
            // N
            if (dir.X == 0 && dir.Y == -1)
            {
                foreach (NeighborTile neighbor in CurrentTile.Neighbors)
                {
                    if (neighbor.Direction == eDirection.North && neighbor.GetTile.CanEnter)
                        canMove = true;
                }
            }
            // S
            if (dir.X == 0 && dir.Y == 1)
            {
                foreach (NeighborTile neighbor in CurrentTile.Neighbors)
                {
                    if (neighbor.Direction == eDirection.South && neighbor.GetTile.CanEnter)
                        canMove = true;
                }
            }
            // NW
            if (dir.X == -1 && dir.Y == -1)
            {
                foreach (NeighborTile neighbor in CurrentTile.Neighbors)
                {
                    if (neighbor.Direction == eDirection.NorthWest && neighbor.GetTile.CanEnter)
                        canMove = true;
                }
            }
            // NE
            if (dir.X == 1 && dir.Y == -1)
            {
                foreach (NeighborTile neighbor in CurrentTile.Neighbors)
                {
                    if (neighbor.Direction == eDirection.NorthEast && neighbor.GetTile.CanEnter)
                        canMove = true;
                }
            }
            // SW
            if (dir.X == -1 && dir.Y == 1)
            {
                foreach (NeighborTile neighbor in CurrentTile.Neighbors)
                {
                    if (neighbor.Direction == eDirection.SouthWest && neighbor.GetTile.CanEnter)
                        canMove = true;
                }
            }
            // SE
            if (dir.X == 1 && dir.Y == 1)
            {
                foreach (NeighborTile neighbor in CurrentTile.Neighbors)
                {
                    if (neighbor.Direction == eDirection.SouthEast && neighbor.GetTile.CanEnter)
                        canMove = true;
                }
            }
            return canMove;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace Broken_Shadows.Objects
{
    public enum SkillType
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
        public SkillType Name { get; private set; }
        public int SkillLevel { get; set; }

        public Skill(SkillType name, int level)
        {
            Name = name;
            SkillLevel = level;
        }
    }

    public class Player : Entity
    {
        //private List<Skill> levels;
        //public List<Skill> SkillLevels { get { return levels; } protected set { levels = value; } }
        public Tile CurrentTile { get; set; }
        public Graphics.PointLight Light { get; set; }

        public Player(Game game)
            : base(game, "Entities/Player", new Pose2D(), null)
        {
            base.Load();
            //levels = CreateSkills();
            UpdateHealth();
            //Defence = levels.Find(s => s.Name == eSkillType.Defence).SkillLevel;

            Load();
            Light = new Graphics.PointLight(Graphics.GraphicsManager.Get().LightEffect, Pose.Position, 250f, Color.White);
        }

        public override void Update(float deltaTime)
        {
            //UpdateCombatLevel();
            if (Light != null)
            {
                Light.Position = Pose.Position;
                Light.LightMoved = true;
            }

            base.Update(deltaTime);
        }

        /// <summary>
        /// Returns a new list of player skills starting at the base levels.
        /// </summary>
        /// <returns></returns>
        /*private List<Skill> CreateSkills()
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
            List<Skill> fightSkills = levels.FindAll(n => (n.Name == eSkillType.Attack || n.Name == eSkillType.Strength || n.Name == eSkillType.Ranged || n.Name == eSkillType.Magic)).ToList();
            int[] fightLevels = fightSkills.Select(l => l.SkillLevel).ToArray();
            int highestCombat = new int[] { fightLevels[0] + fightLevels[1], 2 * fightLevels[2], 2 * fightLevels[3] }.Max();
            int[] cbLevels = levels.FindAll(l => l.Name == eSkillType.Defence || l.Name == eSkillType.Constitution || l.Name == eSkillType.Prayer || l.Name == eSkillType.Summoning).Select(l => l.SkillLevel).ToArray();
            cbLevels[2] = (int)(cbLevels[2] * 0.5);
            cbLevels[3] = (int)(cbLevels[3] * 0.5);

            _combatLevel = (int)(0.25 * (1.3 * highestCombat + cbLevels.Sum()));
        }*/

        /// <summary>
        /// Sets the player's max health depending on their Constitution level.
        /// </summary>
        public override void UpdateHealth()
        {
            //_maxHealth = levels.Find(s => s.Name == eSkillType.Constitution).SkillLevel * 10;
            base.UpdateHealth();
        }

        /// <summary>
        /// Returns true if the player's current tile has a neighbor in the corresponding direction that allows movement.
        /// </summary>
        /// <param name="dir">The vector direction to check.</param>
        /// <returns></returns>
        public bool HasLegalNeighbor(Vector2 dir)
        {
            Direction checkDir = dir.ToDirection();

            if (CurrentTile.IsRigid)
            {
                if (checkDir.IsDiagonal())
                {
                    List<Direction> adjDirections = dir.ToAdjacentDirections();
                    List<NeighborTile> neighbors = CurrentTile.Neighbors.FindAll(n => adjDirections.Contains(n.Direction));

                    return neighbors.Exists(n => n.Direction == checkDir)
                        && !neighbors.SingleOrDefault(n => n.Direction == checkDir).GetTile.CanMove(dir)
                        && neighbors.FindAll(n => n.GetTile.AllowsMovement).Count == neighbors.Count;
                }
                else
                {
                    NeighborTile neighbor = CurrentTile.Neighbors.SingleOrDefault(n => n.Direction == checkDir);
                    if (neighbor != null)
                        return neighbor.GetTile.AllowsMovement;
                    return false;
                }
            }
            else
            {
                return CurrentTile.CanMove(dir);
            }
        }
    }
}

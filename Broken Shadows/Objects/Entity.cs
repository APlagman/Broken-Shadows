using Microsoft.Xna.Framework;

namespace Broken_Shadows.Objects
{
    public enum CombatStyle
    {
        All = 0,
        Melee,
        Ranged,
        Magic
    }

    public class Entity : GameObject
    {
        private int maxHealth;

        public int CombatLevel { get; protected set; }
        public int Health { get; set; }
        public int Damage { get; protected set; }
        public int Defence { get; protected set; }
        public CombatStyle Style { get; protected set; }

        public Entity(Game game, string textureName, Pose2D pose, string glowTextureName, int level = 1, int hp = 10, int dmg = 1, int def = 0, CombatStyle style = CombatStyle.All)
            : base(game, textureName, pose, glowTextureName)
        {
            CombatLevel = level;
            Health = maxHealth = hp;
            Damage = dmg;
            Defence = def;
            Style = style;
        }

        public virtual void UpdateHealth()
        {
            Health = maxHealth;
        }
    }
}

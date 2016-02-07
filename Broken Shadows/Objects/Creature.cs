using Microsoft.Xna.Framework;

namespace Broken_Shadows.Objects
{
    public class Creature : GameObject
    {
        protected int _combatLevel;
        protected int _maxHealth;
        protected int _currentHealth;
        protected int _maxDamage;
        protected int _defence;
        protected CombatStyle _cbStyle;

        public int CombatLevel { get { return _combatLevel; } protected set { _combatLevel = value; } }
        public int Health { get { return _currentHealth; } set { _currentHealth = value; } }
        public int Damage { get { return _maxDamage; } protected set { _maxDamage = value; } }
        public int Defence { get { return _defence; } protected set { _defence = value; } }
        public CombatStyle Style { get { return _cbStyle; } protected set { _cbStyle = value; } }

        public Creature(Game game, int level = 1, int hp = 10, int dmg = 1, int def = 0, CombatStyle style = CombatStyle.All)
            : base(game)
        {
            CombatLevel = level;
            Health = _maxHealth = hp;
            Damage = dmg;
            Defence = def;
            Style = style;
        }

        public virtual void UpdateHealth()
        {
            Health = _maxHealth;
        }
    }
}

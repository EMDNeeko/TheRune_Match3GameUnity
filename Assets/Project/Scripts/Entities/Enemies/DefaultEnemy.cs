using UnityEngine;

namespace Match3Game.Entities.Enemies
{
    public class DefaultEnemy : BaseEnemy
    {
        public DefaultEnemy() : base(new Data.HeroStats(), 100f)
        {
            HeroName = "Dummy";
            Stats.MaxHP = 5000;
            Stats.CurrentHP = 5000;
            Stats.Defense = 10;
            Stats.ManaRegen = 20;
            Stats.MaxMana = 100;
        }

        protected override void CastSkill(Heroes.BaseHero targetHero)
        {
            Debug.Log($"[{HeroName}] Use Skill");
            targetHero.TakeDamage(AttackPower * 1.5f, DamageType.Physical);
        }
    }
}

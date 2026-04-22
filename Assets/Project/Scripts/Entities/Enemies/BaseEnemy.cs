using UnityEngine;
using Match3Game.Data;
namespace Match3Game.Entities.Enemies
{
    public abstract class BaseEnemy : Heroes.BaseHero
    {
        public float AttackPower;
        public BaseEnemy(HeroStats initStats, float attackPower) : base(initStats)
        {
            AttackPower = attackPower;
        }

        public virtual void ExecuteTurn(Heroes.BaseHero targetHero)
        {
            if (Stats.CurrentMana >= Stats.MaxMana)
            {
                Stats.CurrentMana = 0;
                CastSkill(targetHero);
            }
            else
            {
                BasicAttack(targetHero);
            }

            Stats.CurrentMana = Mathf.Min(Stats.CurrentMana + Stats.ManaRegen, Stats.MaxMana);
        }

        protected virtual void BasicAttack(Heroes.BaseHero targetHero)
        {
            float finalDmg = AttackPower;

            finalDmg += finalDmg * (Stats.DamageAmplification / 100f);

            bool isCrit = Random.value < (Stats.CritRate / 100f);
            if (isCrit)
            {
                finalDmg *= (Stats.CritDamage / 100f);
                Debug.Log($"Boss tan cong CRIT");
            }

            targetHero.TakeDamageWithPenetration(finalDmg, DamageType.Physical, Stats.ArmorPenetration);
        }

        protected abstract void CastSkill(Heroes.BaseHero targetHero);
    }
}

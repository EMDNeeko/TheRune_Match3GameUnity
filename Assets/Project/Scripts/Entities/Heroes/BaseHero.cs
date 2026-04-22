using UnityEngine;
using Match3Game.Data;

namespace Match3Game.Entities.Heroes
{
    public abstract class BaseHero
    {
        public string HeroName;
        public HeroStats Stats;
        public Herostate CurrentState;

        public BaseHero(HeroStats initStats)
        {
            Stats = initStats;
            Stats.CurrentHP = Stats.MaxHP;
            CurrentState = Herostate.Combat;
            Stats.CurrentMana = 0;
        }

        public virtual void TakeDamage(float incomingDmg, DamageType type)
        {
            float finalDmg = incomingDmg;

            //reduce dmg
            finalDmg -= finalDmg * (Stats.DamageReduction / 100f);

            //True Dmg
            if (type != DamageType.TrueDamage)
            {
                //Def calc
                float defReduceRatio = Stats.Defense / (50f + Stats.Defense);
                finalDmg -= finalDmg * defReduceRatio;
            }

            //Shield
            if (Stats.Shield > 0)
            {
                if (Stats.Shield >= finalDmg)
                {
                    Stats.Shield -= finalDmg;
                    return;
                }
                else
                {
                    finalDmg -= Stats.Shield;
                    Stats.Shield = 0;
                }
            }

            //HP
            Stats.CurrentHP -= finalDmg;

            //check if ded
            if (Stats.CurrentHP <= 0)
            {
                Stats.CurrentHP = 0;
                Die();
            }
        }

        public virtual void TakeDamageWithPenetration(float incomingDmg, DamageType type, float penetrationRate)
        {
            float finalDmg = incomingDmg;

            //reduce dmg
            finalDmg -= finalDmg * (Stats.DamageReduction / 100f);

            //True Dmg
            if (type != DamageType.TrueDamage)
            {
                //Def calc
                float eDef = Stats.Defense * (1f - penetrationRate);
                float defReduceRatio = eDef / (50f + eDef);
                finalDmg -= finalDmg * defReduceRatio;
            }

            //Shield
            if (Stats.Shield > 0)
            {
                if (Stats.Shield >= finalDmg)
                {
                    Stats.Shield -= finalDmg;
                    return;
                }
                else
                {
                    finalDmg -= Stats.Shield;
                    Stats.Shield = 0;
                }
            }

            //HP
            Stats.CurrentHP -= finalDmg;

            //check if ded
            if (Stats.CurrentHP <= 0)
            {
                Stats.CurrentHP = 0;
                Die();
            }
        }

        //Poisoned and Burned Rune
        public virtual void ApplyStatus(StatusType status, int durationRounds)
        {
            Debug.Log($"{HeroName} was {status} in {durationRounds} rounds");
        }

        protected virtual void Die()
        {
            Debug.Log("Hero da tu tran");
            CurrentState = Herostate.Fallen;
        }

    }
}

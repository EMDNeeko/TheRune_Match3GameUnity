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
                Die();
            }
        }

        protected virtual void Die()
        {
            CurrentState = Herostate.Fallen;
        }

    }
}

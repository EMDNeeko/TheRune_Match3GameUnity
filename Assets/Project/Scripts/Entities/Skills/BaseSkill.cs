using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Match3Game.Entities.Heroes;

namespace Match3Game.Entities.Skills
{
    public abstract class BaseSkill
    {
        public string SkillName;
        public SkillType type;
        public int ManaCost;
        public int CooldownTurns;
        protected int currentCooldown;

        //cast
        public abstract void Execute(BaseHero caster, Object targetData = null);

        //update cd
        public virtual void UpdateCooldown()
        {
            if (currentCooldown > 0)
            {
                currentCooldown -= 1;
            }
        }
    }
}
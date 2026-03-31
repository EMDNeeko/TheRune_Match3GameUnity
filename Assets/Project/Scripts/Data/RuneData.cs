using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Match3Game.Data
{
    public class RuneData
    {
        public RuneType BaseType;
        public RuneType OriginalColor;
        public SpecialRuneType SpecialType;
        public RuneEffect CurrentEffect;
        public int effectStacks; // stack effect 
        public float empowerMultiplier = 0.2f; //Cường hoá

        public RuneData(RuneType type)
        {
            BaseType = type;
            OriginalColor = type;
            SpecialType = SpecialRuneType.None;
            CurrentEffect = RuneEffect.None;
            effectStacks = 0;
        }

        //check can swap
        public bool isMovable()
        {
            if (CurrentEffect == RuneEffect.Frozen && effectStacks > 0)
            {
                return false;
            }
            return true;
        }

        //Tinh toan khi thu thap
        public float GetEfficiency()
        {
            float effi = 1.0f;
            if (CurrentEffect == RuneEffect.Empowered)
            {
                effi += empowerMultiplier;
            }
            else if (CurrentEffect == RuneEffect.Frozen && effectStacks > 0)
            {
                effi -= 0.3f;
            }
            else if (CurrentEffect == RuneEffect.Nullified)
            {
                effi = 0f;
            }

            return effi < 0f ? 0f : effi;
        }
    }
}
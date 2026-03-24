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
    }
}
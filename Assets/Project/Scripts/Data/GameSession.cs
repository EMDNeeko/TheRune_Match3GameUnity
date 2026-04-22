using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Match3Game.Assets.Project.Scripts.Data
{
    public static class GameSession
    {
        public static string selectedHero = "Ramses";
        public static string selectedEnemy = "Dantalian";
        public static PriorityStat selectedPriorityStat = PriorityStat.PhysicalAttack;
    }
}
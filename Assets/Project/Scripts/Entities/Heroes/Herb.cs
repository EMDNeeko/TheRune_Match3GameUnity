using System.Collections.Generic;
using System.Reflection;
using Match3Game.Data;
using UnityEngine;

namespace Match3Game.Entities.Heroes
{
    public class Herb : BaseHero
    {
        public int activeCooldown = 0;
        public int ultimateCooldown = 0;

        //record nt2
        public float storedPhysDmg = 0f;
        public float storedMageDmg = 0f;
        public bool isEnhancedA = false; // form A 
        public bool isEnhancedB = false; // form B

        public int laserTurretTurnsLeft = 0;

        public Herb() : base(new Data.HeroStats())
        {
            HeroName = "Herb";

            Stats.MaxHP = 1300f;
            Stats.PhysicalDamage = 75f;
            Stats.MagicalDamage = 75f;
            Stats.Defense = 20f;
            Stats.HPRegen = 180f;
            Stats.MaxMana = 200f;
            Stats.CurrentMana = 10f;
            Stats.ManaRegen = 20f;
            Stats.CritRate = 0.05f;
            Stats.CritDamage = 1.5f;
            Stats.LifeSteal = 0f;
            Stats.SpellVamp = 0f;
            Stats.CooldownReduction = 0f;
            Stats.EffectResistance = 0f;
            Stats.DamageAmplification = 0f;
            Stats.DamageReduction = 0f;

            Stats.CurrentHP = Stats.MaxHP;
        }

        public void OnTurnStart(Managers.BoardManager board)
        {
            if (activeCooldown > 0) activeCooldown--;
            if (ultimateCooldown > 0) ultimateCooldown--;

            //NT1: Shield & Heal
            float missingHP = Stats.MaxHP - Stats.CurrentHP;

            if (Stats.Shield > 0)
            {
                Stats.Shield = 0;
                float heal = missingHP * 0.08f;
                Stats.CurrentHP = Mathf.Min(Stats.CurrentHP + heal, Stats.MaxHP);
            }

            float newShield = missingHP * 0.2f;
            Stats.Shield = newShield;
            Debug.Log($"[Herb] Passive 1: Gain {newShield} Shield");

            if (laserTurretTurnsLeft > 0)
            {
                laserTurretTurnsLeft--;
                Debug.Log($"[Herb] Laser turret {laserTurretTurnsLeft} turns remaining.");

                int isRow = Random.Range(0, 2);
                int randomLine = Random.Range(0, board.Width);

                List<CellData> targetCells = new List<CellData>();
                if (isRow == 0)
                {
                    randomLine = Random.Range(0, board.Height);
                    for (int x = 0; x < board.Width; x++)
                    {

                        targetCells.Add(board.GetCell(x, randomLine));
                    }
                }
                else
                {
                    for (int y = 0; y < board.Height; y++)
                    {
                        targetCells.Add(board.GetCell(randomLine, y));
                    }
                }

                board.DesTroyAreaAndRefill(targetCells, new List<CellData>(), 1f);
            }
        }

        public void RecordDamageForPassive(float dmg, DamageType type)
        {
            if (isEnhancedA || isEnhancedB) return;
            if (type == DamageType.Physical && !isEnhancedA)
            {
                storedPhysDmg += dmg;
                if (storedPhysDmg >= 1000f)
                {
                    isEnhancedA = true;
                    Debug.Log("[Herb] Enhance: Form A (Physical)");

                }
            }
            else if (type == DamageType.Magical && !isEnhancedB)
            {
                storedMageDmg += dmg;
                if (storedMageDmg >= 1000f)
                {
                    isEnhancedB = true;
                    Debug.Log("[Herb] Enhance: Form B (Magical)");
                }
            }
        }

        public void CastActiveSkill(Managers.BoardManager board, CellData targetCell)
        {
            if (Stats.CurrentMana < 20f || activeCooldown > 0 || targetCell == null)
            {
                return;
            }

            int rangeX = 1, rangeY = 1;
            float manaCost = 20f;

            if (Stats.CurrentMana >= 80f)
            {
                rangeX = 2;
                rangeY = 2;
                manaCost = 80f;
            }

            Stats.CurrentMana -= manaCost;
            activeCooldown = 2;

            Debug.Log($"[Herb] Use Active Skill, cost {manaCost} Mana.");
            List<CellData> collectCells = board.GetCellsInRange(targetCell, rangeX, rangeY);
            board.DesTroyAreaAndRefill(collectCells, new List<CellData>(), 1f);
        }

        public void CastUltimateSkill(Managers.BoardManager board)
        {
            if (Stats.CurrentMana < 150f || ultimateCooldown > 0) return;

            Stats.CurrentMana -= 150f;
            ultimateCooldown = 6;
            laserTurretTurnsLeft = 5;
            Debug.Log("[Herb] Use Ultimate Skill");
        }
    }
}
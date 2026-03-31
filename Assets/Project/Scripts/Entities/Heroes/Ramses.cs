using System.Collections.Generic;
using Match3Game.Data;
using UnityEngine;

namespace Match3Game.Entities.Heroes
{
    public enum RamsesForm
    {
        Normal,
        Burning
    }

    public class Ramses : BaseHero
    {
        public RamsesForm CurrentForm = RamsesForm.Normal;
        public int burningTurnLeft = 0;
        public int ultimateCoolDown = 0;
        public int activeCoolDown = 0;

        //Noitai 2 
        private bool hasHealedFormBurnThisTurn = false;
        public Ramses() : base(new Data.HeroStats())
        {
            HeroName = "Ramses";

            //basic stat
            Stats.MaxHP = 1350f;
            Stats.PhysicalDamage = 120f;
            Stats.MagicalDamage = 20f;
            Stats.Defense = 15f;
            Stats.HPRegen = 150f;
            Stats.MaxMana = 240f;
            Stats.CurrentMana = 20f;
            Stats.ManaRegen = 30f;
            Stats.CritRate = 0.10f;
            Stats.CritDamage = 1.5f;
            Stats.LifeSteal = 0.1f;
            Stats.SpellVamp = 0f;
            Stats.CooldownReduction = 0f;
            Stats.EffectResistance = 0f;

            Stats.CurrentHP = Stats.MaxHP;

            ApplyPassive1();
        }

        public RamsesForm GetForm()
        {
            return CurrentForm;
        }

        //Noi tai 1
        public void ApplyPassive1()
        {
            Stats.ManaRegen = 30f;
            Stats.DamageReduction = 0f;
            Stats.Defense = 15f;
            Stats.LifeSteal = 0.1f;
            Stats.EffectResistance = 0f;

            if (CurrentForm == RamsesForm.Normal)
            {
                Stats.ManaRegen += 30f * 0.5f;
                Stats.DamageReduction += 10f;
            }
            else if (CurrentForm == RamsesForm.Burning)
            {
                Stats.Defense += 15f;
                Stats.LifeSteal += 0.1f;

                if (Stats.CurrentHP < Stats.MaxHP * 0.5f)
                {
                    Stats.Defense += 10f;
                    Stats.EffectResistance += 40f;
                }
            }
        }

        public void TakeDamage(float incomingDmg, DamageType type)
        {
            base.TakeDamage(incomingDmg, type);
            ApplyPassive1();
        }

        public void OnTurnStart()
        {
            hasHealedFormBurnThisTurn = false;

            //reduce Burning turn
            if (CurrentForm == RamsesForm.Burning)
            {
                burningTurnLeft--;
                if (burningTurnLeft <= 0)
                {
                    CurrentForm = RamsesForm.Normal;
                    ultimateCoolDown = 3;
                    ApplyPassive1();
                    Debug.Log("End Ultimate, return to normal");
                }
            }

            if (activeCoolDown > 0) activeCoolDown--;
            if (ultimateCoolDown > 0) ultimateCoolDown--;
        }

        public void CheckBurnHealPassive()
        {
            if (!hasHealedFormBurnThisTurn)
            {
                float missingHP = Stats.MaxHP - Stats.CurrentHP;
                float healAmount = missingHP * 0.1f;
                Stats.CurrentHP = Mathf.Min(Stats.CurrentHP + healAmount, Stats.MaxHP);
                hasHealedFormBurnThisTurn = true;
                Debug.Log($"Ramses healed {healAmount} HP in Passive 2");
            }
        }

        public void CastActiveSkill(Managers.BoardManager board, CellData targetCell)
        {
            // Kĩ năng chủ động tiêu hao 40 Mana, có 2 lượt hồi chiêu[cite: 399].
            if (Stats.CurrentMana < 40f || activeCoolDown > 0 || targetCell == null) return;

            Stats.CurrentMana -= 40f;
            activeCoolDown = 2;

            List<CellData> collectCells = new List<CellData>();
            List<CellData> destroyCells = new List<CellData>();

            if (CurrentForm == RamsesForm.Normal)
            {
                Debug.Log($"Ramses soi đèn 3x3 tại ({targetCell.X}, {targetCell.Y}).");
                collectCells = board.GetCellsInRange(targetCell, 1, 1);

                // Truyền multiplier 2.0f vì tăng 100% hiệu quả đá mang lại.
                board.DesTroyAreaAndRefill(collectCells, destroyCells, 2.0f);
            }
            else
            {
                Debug.Log($"Ramses đập 5x5, thu thập 3x3 giữa tại ({targetCell.X}, {targetCell.Y}).");

                List<CellData> area5x5 = board.GetCellsInRange(targetCell, 2, 2);
                List<CellData> area3x3 = board.GetCellsInRange(targetCell, 1, 1);

                foreach (var cell in area5x5)
                {
                    if (area3x3.Contains(cell)) collectCells.Add(cell);
                    else destroyCells.Add(cell);
                }

                board.DesTroyAreaAndRefill(collectCells, destroyCells, 1.0f);

                // Ghi chú: Cần bổ sung cờ hiệu ứng để đá mới rơi vào khu vực 3x3 chính giữa sẽ gán Cháy[cite: 403].
            }
        }

        public void CastUltimateSkill(Managers.BoardManager board, CellData targetCell)
        {
            // Kĩ năng tối thượng tiêu hao 120 Mana, có 1 lượt hồi chiêu[cite: 404].
            if (Stats.CurrentMana < 120f || ultimateCoolDown > 0) return;

            Stats.CurrentMana -= 120f;

            if (CurrentForm == RamsesForm.Normal)
            {
                Debug.Log("Ramses dùng Ultimate: Hoá Bùng cháy.");

                // Rút 10% HP hiện tại[cite: 405].
                Stats.CurrentHP -= (Stats.CurrentHP * 0.10f);

                // Tiến vào dạng Bùng cháy trong 4 lượt[cite: 405].
                CurrentForm = RamsesForm.Burning;
                burningTurnLeft = 4;

                ApplyPassive1();

                // Nhận 10% Miễn giảm sát thương trong 2 lượt đầu[cite: 405].
                Stats.DamageReduction += 10f;

                // Ghi chú: Khi trạng thái kết thúc, tiến vào 3 lượt hồi chiêu[cite: 406].
            }
            else
            {
                if (targetCell == null) return;
                Debug.Log($"Ramses trảm 3 cột kề nhau từ cột {targetCell.X}.");

                List<CellData> collectCells = new List<CellData>();
                List<CellData> destroyCells = new List<CellData>();

                // Quét 3 cột kề nhau dựa trên ô trung tâm được click
                for (int x = targetCell.X - 1; x <= targetCell.X + 1; x++)
                {
                    for (int y = 0; y < board.Height; y++)
                    {
                        CellData cell = board.GetCell(x, y);
                        if (cell != null) collectCells.Add(cell);
                    }
                }

                board.DesTroyAreaAndRefill(collectCells, destroyCells, 1.0f);

                // Ghi chú: Sau nhát trảm, hàng hoặc cột chính giữa nhát trảm sẽ gán Cháy[cite: 408].
                ultimateCoolDown = 1;
            }
        }
    }
}
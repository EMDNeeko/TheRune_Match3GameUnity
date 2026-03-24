using UnityEngine;
using Match3Game.Data;

namespace Match3Game.Managers
{
    public class TestCombatManager : MonoBehaviour
    {
        [Header("Chỉ số Hero tạm thời")]
        public float SMVL = 100f;
        public float SMPT = 100f;
        public float maxHP = 1000f;
        public float CurrentHP = 500f;
        public float HPRegen = 50f;
        public float ManaRegen = 20f;
        public float LifeStealPercent = 10f;
        public float SpellVampPercent = 10f;
        public float HPEnemy = 10000f;
        public float critRate = 30f;

        [Header("Trackers")]
        public float totalPhysDmg = 0f;
        public float totalMageDmg = 0f;
        public float totalTrueDmg = 0f;
        public float totalHPHealed = 0f;
        public float totalHPHealedByVamp = 0f;
        public float totalMana = 0f;
        public float totalSupportRegen = 0f;
        public int purpleCnt = 0;
        public int extraTurn = 0;
        public int countTurn = 0;

        private bool hasGainedExtraTurnThisAction = false;
        private static System.Random s_random = new System.Random();

        //Khi bat dau thao tac
        public void StartAction()
        {
            hasGainedExtraTurnThisAction = false;
        }

        //Xu ly to hop
        public void ProcessMatchResult(MatchResult match)
        {
            int count = match.MatchedCells.Count;
            MatchType type = match.TypeOfMatch;
            RuneType rune = match.MatchedRuneType;

            float physDmg = 0, trueDmg = 0, mageDmg = 0, heal = 0, healVamp = 0, mana = 0, support = 0;
            bool triggersExtraTurn = false;

            if (rune == RuneType.Purple)
            {
                purpleCnt += count;
                return;
            }

            if (type == MatchType.Match3Line)
            {
                if (rune == RuneType.Red) physDmg += SMVL * 1.0f;
                else if (rune == RuneType.Blue) mageDmg += SMPT * 1.0f;
                else if (rune == RuneType.Green) heal += HPRegen * 1.0f;
                else if (rune == RuneType.Yellow) mana += ManaRegen * 1.0f;
                else if (rune == RuneType.Orange) support += 10f;
            }
            else if (type == MatchType.Match4Line || type == MatchType.Match4Square || type == MatchType.Match5Cross)
            {
                if (rune == RuneType.Red) physDmg += SMVL * 1.2f;
                else if (rune == RuneType.Blue) mageDmg += SMPT * 1.2f;
                else if (rune == RuneType.Green) heal += HPRegen * 1.5f;
                else if (rune == RuneType.Yellow) mana += ManaRegen * 1.5f;
                else if (rune == RuneType.Orange) support += 15f;
            }
            else if (type == MatchType.Match5Line || type == MatchType.Match6Plus)
            {
                if (rune == RuneType.Red) trueDmg += SMVL * 1.0f;
                else if (rune == RuneType.Blue) mageDmg += SMPT * 1.5f;
                else if (rune == RuneType.Green) heal += HPRegen * 2.0f;
                else if (rune == RuneType.Yellow) mana += ManaRegen * 2.0f;
                else if (rune == RuneType.Orange) support += 20f;

                triggersExtraTurn = true;
            }

            if (physDmg > 0)
            {
                int randomNum = s_random.Next(0, 100);
                if (randomNum <= critRate)
                {
                    physDmg *= 2;
                    Debug.Log("Crit!");
                }
                healVamp += physDmg * (LifeStealPercent / 100f);
                Debug.Log($"Gay ra {physDmg} STVL");
            }
            if (mageDmg > 0)
            {
                healVamp += mageDmg * (SpellVampPercent / 100f);
                Debug.Log($"Gay ra {mageDmg} STPT");
            }

            if (healVamp > 0)
            {
                totalHPHealedByVamp += healVamp;
                CurrentHP = Mathf.Min(CurrentHP + healVamp, maxHP);
            }
            totalPhysDmg += physDmg;
            totalMageDmg += mageDmg;
            totalTrueDmg += trueDmg;

            HPEnemy = HPEnemy - physDmg - mageDmg - trueDmg;

            if (heal > 0)
            {
                totalHPHealed += heal;
                CurrentHP = Mathf.Min(CurrentHP + heal, maxHP);
                Debug.Log($"Hoi lai {heal} HP");
            }
            if (mana > 0)
            {
                totalMana += mana;
                Debug.Log($"Da hoi {mana} Mana");
            }

            totalSupportRegen += support;

            if (triggersExtraTurn && !hasGainedExtraTurnThisAction)
            {
                extraTurn += 1;
                hasGainedExtraTurnThisAction = true;
            }
        }

        //single rune
        public void ProcessingSingleRune(RuneType rune)
        {
            float physDmg = 0;
            float mageDmg = 0;
            float heal = 0;
            float healVamp = 0;
            float mana = 0;

            if (rune == RuneType.Red)
            {
                physDmg += SMVL * 0.2f;
            }
            else if (rune == RuneType.Blue)
            {
                mageDmg += SMPT * 0.2f;
            }
            else if (rune == RuneType.Green)
            {
                heal += HPRegen * 0.2f;
            }
            else if (rune == RuneType.Yellow)
            {
                mana += ManaRegen * 0.2f;
            }
            else if (rune == RuneType.Orange)
            {
                totalSupportRegen += 2f;
            }
            else if (rune == RuneType.Purple)
            {
                purpleCnt += 1;
            }

            if (physDmg > 0)
            {
                int randomNum = s_random.Next(0, 100);
                if (randomNum <= critRate)
                {
                    physDmg *= 2;
                    Debug.Log("Crit!");
                }
                healVamp += physDmg * (LifeStealPercent / 100f);
                Debug.Log($"Gay ra {physDmg} STVL");
            }
            if (mageDmg > 0)
            {
                healVamp += mageDmg * (SpellVampPercent / 100f);
                Debug.Log($"Gay ra {SMPT} STPT");
            }
            if (healVamp > 0)
            {
                totalHPHealedByVamp += healVamp;
                CurrentHP = Mathf.Min(CurrentHP + healVamp, maxHP);
            }
            totalPhysDmg += physDmg;
            totalMageDmg += mageDmg;

            HPEnemy = HPEnemy - physDmg - mageDmg;

            if (heal > 0)
            {
                totalHPHealed += heal;
                CurrentHP = Mathf.Min(CurrentHP + heal, maxHP);
                Debug.Log($"Hoi lai {heal} HP");
            }
            if (mana > 0)
            {
                totalMana += mana;
                Debug.Log($"Da hoi {mana} Mana");
            }
        }

        //het luot
        public void EndTurn()
        {
            countTurn++;
        }

        //GUI tam thoi
        void OnGUI()
        {
            GUI.color = Color.white;
            GUI.skin.label.fontSize = 35;
            GUI.skin.label.fontStyle = FontStyle.Bold;

            GUILayout.BeginArea(new Rect(30, 30, 600, 800));

            GUILayout.Label($"HP Dummy: <color=red>{HPEnemy}</color>");
            GUILayout.Label($"[Test Hero]");
            GUILayout.Label($"HP: <color=green>{CurrentHP} / {maxHP}</color>");
            GUILayout.Label($"Phys Dmg Dealed: <color=red>{totalPhysDmg}</color>");
            GUILayout.Label($"Mage Dmg Dealed: <color=cyan>{totalMageDmg}</color>");
            GUILayout.Label($"True Dmg Dealed: <color=white>{totalTrueDmg}</color>");
            GUILayout.Label($"Healed: <color=lime>{totalHPHealed}|{totalHPHealedByVamp}</color>");
            GUILayout.Label($"Mana Regened: <color=yellow>{totalMana}</color>");
            GUILayout.Label($"Energy Regened: <color=orange>{totalSupportRegen}</color>");
            GUILayout.Label($"Purple Rune Collected: <color=magenta>{purpleCnt}</color>");
            GUILayout.Label($"Gained Extra Turn: <color=white>{extraTurn}</color>");

            GUILayout.EndArea();

            GUILayout.BeginArea(new Rect(600, 160, 600, 800));
            GUILayout.Label($"Turn {countTurn}");
            GUILayout.EndArea();


        }
    }
}
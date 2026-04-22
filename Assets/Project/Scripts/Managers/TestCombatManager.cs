using UnityEngine;
using UnityEngine.UI;
using Match3Game.Data;
using Match3Game.Entities.Heroes;
using Match3Game.Mechanics;
using Match3Game.Assets.Project.Scripts.Data;

namespace Match3Game.Managers
{
    public class TestCombatManager : MonoBehaviour
    {

        // [Header("Chỉ số Hero tạm thời")]
        // public float SMVL = 100f;
        // public float SMPT = 100f;
        // public float maxHP = 1000f;
        // public float CurrentHP = 500f;
        // public float HPRegen = 50f;
        // public float ManaRegen = 20f;
        // public float LifeStealPercent = 10f;
        // public float SpellVampPercent = 10f;
        // public float HPEnemy = 10000f;
        // public float critRate = 30f;
        [Header("References")]
        public BoardManager boardManager;
        public BaseHero playerHero;
        public BaseHero enemyBoss;

        [Header("Turn Management")]
        public bool isPlayerTurn = true;
        public int baseSwipeAction = 2;
        public int currentSwipeAction = 2;
        public int extraActions = 0;
        private bool hasEarnedExtraActionThisTurn = false;

        [Header("Trackers")]
        public float totalPhysDmg = 0f;
        public float totalMageDmg = 0f;
        public float totalTrueDmg = 0f;
        public float totalHPHealed = 0f;

        public int countTurn = 0;

        [Header("UI Elements (Hero)")]
        public Text heroName;
        public Text heroHPText;
        public Text heroShieldText;
        public Text heroManaText;
        public Text trackerText;
        public Text bossHPText;
        public Text currentTurn;
        public Button btnActiveSkill;
        public Button btnUltimateSkill;
        public Button btnConfirmSkill;
        public Text actionCountText;

        [Header("Boss Config")]
        private float bossBaseDmg = 100f;
        private float bossDmgIncrement = 10f;

        [Header("VFX - Floating Number")]
        public GameObject floatingTextPrefab;
        public Transform canvasTransform;

        [Header("UI Elements - Bars")]
        public Image heroHPFill;
        public Image heroManaFill;
        public Image heroShieldFill;
        public Image bossHPFill;


        private bool hasGainedExtraTurnThisAction = false;
        private static System.Random s_random = new System.Random();

        void Start()
        {
            if (GameSession.selectedHero == "Ramses")
            {
                playerHero = new Ramses();
            }
            else
            {
                playerHero = new Ramses();
            }


            enemyBoss = new Boss();

            enemyBoss.Stats = new HeroStats() { MaxHP = 5000, CurrentHP = 5000, Defense = 10 };

            btnConfirmSkill.gameObject.SetActive(false);
            UpdateUI();
        }
        public class Boss : BaseHero { public Boss() : base(new HeroStats()) { } }
        public void UpdateUI()
        {
            if (playerHero != null)
            {
                // Bar
                heroHPFill.fillAmount = playerHero.Stats.CurrentHP / playerHero.Stats.MaxHP;
                heroManaFill.fillAmount = playerHero.Stats.CurrentMana / playerHero.Stats.MaxMana;
                heroShieldFill.fillAmount = playerHero.Stats.Shield / playerHero.Stats.MaxHP;


                heroName.text = $"{playerHero.HeroName} F: {((Ramses)playerHero).CurrentForm} in {((Ramses)playerHero).burningTurnLeft}";
                heroHPText.text = $"{Mathf.RoundToInt(playerHero.Stats.CurrentHP)} / {playerHero.Stats.MaxHP}";
                heroShieldText.text = $"{Mathf.RoundToInt(playerHero.Stats.Shield)}";
                heroManaText.text = $"{Mathf.RoundToInt(playerHero.Stats.CurrentMana)} / {playerHero.Stats.MaxMana}";
                // --- CẬP NHẬT TRẠNG THÁI NÚT SKILL ---
                if (playerHero is Ramses ramses && btnActiveSkill != null && btnUltimateSkill != null)
                {
                    Text activeText = btnActiveSkill.GetComponentInChildren<Text>();
                    Text ultiText = btnUltimateSkill.GetComponentInChildren<Text>();

                    // 1. Kiểm tra Active Skill (Cần 40 Mana)
                    bool canUseActive = true;
                    if (ramses.activeCoolDown > 0)
                    {
                        if (activeText != null) activeText.text = $"Active (Hồi: {ramses.activeCoolDown})";
                        canUseActive = false;
                    }
                    else if (playerHero.Stats.CurrentMana < 40f)
                    {
                        if (activeText != null) activeText.text = "Active (Thiếu Mana)";
                        canUseActive = false;
                    }
                    else
                    {
                        if (activeText != null) activeText.text = "Active Skill";
                    }
                    // Chỉ sáng lên khi đủ điều kiện VÀ đang là lượt của người chơi
                    btnActiveSkill.interactable = canUseActive && isPlayerTurn;

                    // 2. Kiểm tra Ultimate Skill (Cần 120 Mana)
                    bool canUseUlti = true;
                    if (ramses.ultimateCoolDown > 0)
                    {
                        if (ultiText != null) ultiText.text = $"Ultimate (Hồi: {ramses.ultimateCoolDown})";
                        canUseUlti = false;
                    }
                    else if (playerHero.Stats.CurrentMana < 120f)
                    {
                        if (ultiText != null) ultiText.text = "Ultimate (Thiếu Mana)";
                        canUseUlti = false;
                    }
                    else
                    {
                        if (ultiText != null) ultiText.text = "Ultimate Skill";
                    }
                    // Chỉ sáng lên khi đủ điều kiện VÀ đang là lượt của người chơi
                    btnUltimateSkill.interactable = canUseUlti && isPlayerTurn;
                }

                if (actionCountText != null)
                {
                    actionCountText.text = $"Action: {currentSwipeAction + extraActions}" + (extraActions > 0 ? $"(Extra: {extraActions})" : "");
                }
            }
            trackerText.text = $"STVL: {totalPhysDmg} | STPT: {totalMageDmg} | STC: {totalTrueDmg} | Healed: {totalHPHealed}";
            currentTurn.text = $"Current turn: {countTurn}";
            bossHPFill.fillAmount = enemyBoss.Stats.CurrentHP / enemyBoss.Stats.MaxHP;
            bossHPText.text = $"Boss HP: {Mathf.RoundToInt(enemyBoss.Stats.CurrentHP)} / {enemyBoss.Stats.MaxHP}";
        }
        //Khi bat dau thao tac
        public void StartAction()
        {
            hasEarnedExtraActionThisTurn = false;
        }
        public void OnActiveSkillClicked()
        {
            if (playerHero == null || playerHero.Stats == null || boardManager == null || btnConfirmSkill == null)
            {
                Debug.LogError("Thiếu reference trong TestCombatManager! Hãy kiểm tra lại Inspector.");
                return;
            }
            if (!isPlayerTurn || playerHero.Stats.CurrentMana < 40f) return;

            if (boardManager.pendingSkillType == SkillType.Active && btnConfirmSkill.gameObject.activeSelf)
            {
                CancelSkillTargeting();
                return;
            }
            Debug.Log("Chose Active Skill, choose a place in board!");
            boardManager.EnterSkillTargetingMode(SkillType.Active);
            btnConfirmSkill.gameObject.SetActive(true);
        }
        public void OnUltimateSkillClicked()
        {
            if (playerHero == null || playerHero.Stats == null || boardManager == null || btnConfirmSkill == null)
            {
                Debug.LogError("Thiếu reference trong TestCombatManager! Hãy kiểm tra lại Inspector.");
                return;
            }
            if (!isPlayerTurn || playerHero.Stats.CurrentMana < 120f) return;

            if (((Ramses)playerHero).CurrentForm == RamsesForm.Normal)
            {
                ((Ramses)playerHero).CastUltimateSkill(boardManager, null);
                UpdateUI();
            }
            else
            {
                if (boardManager.pendingSkillType == SkillType.Ultimate && btnConfirmSkill.gameObject.activeSelf)
                {
                    CancelSkillTargeting();
                    return;
                }
                Debug.Log("Chose Ultimate (Burning), choose target row / column");
                boardManager.EnterSkillTargetingMode(SkillType.Ultimate);
                btnConfirmSkill.gameObject.SetActive(true);
            }
        }

        public void OnConfirmSkillClicked()
        {
            CellData targetCell = boardManager.GetTargetedCell();
            if (targetCell == null) return;

            SkillType pendingSkill = boardManager.pendingSkillType;
            boardManager.ExitSkillTargetingMode();
            btnConfirmSkill.gameObject.SetActive(false);

            if (pendingSkill == SkillType.Active)
            {
                ((Ramses)playerHero).CastActiveSkill(boardManager, targetCell);
            }
            else if (pendingSkill == SkillType.Ultimate)
            {
                ((Ramses)playerHero).CastUltimateSkill(boardManager, targetCell);
            }
            UpdateUI();
        }

        //huy skill
        public void CancelSkillTargeting()
        {
            Debug.Log("Da huy skill");
            boardManager.ExitSkillTargetingMode();
            btnConfirmSkill.gameObject.SetActive(false);
        }

        public void RecordDamage(float amount, DamageType type)
        {
            if (type == DamageType.Physical) totalPhysDmg += amount;
            else if (type == DamageType.Magical) totalMageDmg += amount;
            else if (type == DamageType.TrueDamage) totalTrueDmg += amount;
            UpdateUI();
        }
        public void RecordHeal(float amount)
        {
            totalHPHealed += amount;
            UpdateUI();
        }

        // public void OnPlayerUseActiveSkill()
        // {
        //     if (!isPlayerTurn) return;
        //     if (playerHero is Ramses ramses)
        //     {
        //         ramses.CastActiveSkill(boardManager);
        //     }
        // }
        // public void OnPlayerUseUltimateSkill()
        // {
        //     if (!isPlayerTurn) return;
        //     if (playerHero is Ramses ramses)
        //     {
        //         ramses.CastUltimateSkill(boardManager);
        //     }
        // }
        public void ProcessMatchResult(MatchResult match, Vector2 pos)
        {
            if (playerHero == null || enemyBoss == null) return;
            float comboEfficiency = CalculateMatchEfficiency(match);

            if (match.TypeOfMatch == MatchType.Match4Line || match.TypeOfMatch == MatchType.Match4Square || match.TypeOfMatch == MatchType.Match5Line || match.TypeOfMatch == MatchType.Match5Cross || match.TypeOfMatch == MatchType.Match6Plus)
            {
                GrantExtraAction();
            }

            RuneType effectiveType = match.MatchedRuneType;
            if (effectiveType == RuneType.Purple)
            {
                effectiveType = GetPriorityRuneType();
            }

            switch (match.MatchedRuneType)
            {
                case RuneType.Red:
                case RuneType.Purple:
                    ProcessRedRune(match.TypeOfMatch, comboEfficiency, pos);
                    break;
                case RuneType.Blue:
                    ProcessBlueRune(match.TypeOfMatch, comboEfficiency, pos);
                    break;
                case RuneType.Green:
                    ProcessGreenRune(match.TypeOfMatch, comboEfficiency, pos);
                    break;
                case RuneType.Yellow:
                    ProcessYellowRune(match.TypeOfMatch, comboEfficiency, pos);
                    break;
                case RuneType.Orange:
                    ProcessOrangeRune(match.TypeOfMatch, comboEfficiency, pos);
                    break;
            }
            UpdateUI();
        }

        public void ProcessingSingleRune(RuneType type, Vector2 pos, float efficiency = 1f)
        {
            if (playerHero == null || enemyBoss == null) return;
            float val = 0;
            Color color = Color.white;

            RuneType effectiveType = type;
            if (effectiveType == RuneType.Purple)
            {
                effectiveType = GetPriorityRuneType();
            }

            switch (type)
            {
                case RuneType.Red:
                case RuneType.Purple:
                    float dmg = (playerHero.Stats.PhysicalDamage * 0.2f) * efficiency;
                    val = dmg; color = Color.red;
                    enemyBoss.TakeDamage(dmg, DamageType.Physical);
                    RecordDamage(dmg, DamageType.Physical);
                    break;
                case RuneType.Blue:
                    float magDmg = (playerHero.Stats.MagicalDamage * 0.2f) * efficiency;
                    val = magDmg; color = Color.cyan;
                    enemyBoss.TakeDamage(magDmg, DamageType.Magical);
                    RecordDamage(magDmg, DamageType.Magical);
                    break;
                case RuneType.Green:
                    float heal = (playerHero.Stats.HPRegen * 0.2f) * efficiency;
                    val = heal; color = Color.green;
                    playerHero.Stats.CurrentHP = Mathf.Min(playerHero.Stats.CurrentHP + heal, playerHero.Stats.MaxHP);
                    break;
                case RuneType.Yellow:
                    float mana = playerHero.Stats.ManaRegen * 0.2f * efficiency;
                    val = mana; color = Color.yellow;
                    playerHero.Stats.CurrentMana = Mathf.Min(playerHero.Stats.CurrentMana + mana, playerHero.Stats.MaxMana);
                    break;
                case RuneType.Orange: // Đá cam nổ lẻ
                    // Tính 20% hiệu quả của mốc 5% HP tối đa
                    float singleShield = (playerHero.Stats.MaxHP * 0.05f * 0.20f) * efficiency;
                    val = singleShield; color = Color.orange;
                    playerHero.Stats.Shield += singleShield;
                    break;
            }
            SpawnPopUp(pos, val, color);
            UpdateUI();
        }

        private RuneType GetPriorityRuneType()
        {
            switch (GameSession.selectedPriorityStat)
            {
                case PriorityStat.PhysicalAttack:
                    return RuneType.Red;
                case PriorityStat.MagicalAttack:
                    return RuneType.Blue;
                case PriorityStat.HealthAndHPRegen:
                    return RuneType.Green;
                case PriorityStat.ManaAndManaRegen:
                    return RuneType.Yellow;
                default: return RuneType.Red;
            }
        }

        //het luot
        public void EndTurn()
        {
            Debug.Log("Change turn");
            isPlayerTurn = false;

            TriggerOverTimeEffects();
            Invoke(nameof(ExecuteBossTurn), 1.5f);
        }
        private void ExecuteBossTurn()
        {
            float currentDmg = bossBaseDmg + ((countTurn - 1) * bossDmgIncrement);
            Debug.Log($"Boss attacked, dealing {currentDmg} PhysDmg");
            playerHero.TakeDamage(currentDmg, DamageType.Physical);

            boardManager.ApplyRandomEffectToBasicRune();
            countTurn++;

            Debug.Log("Boss ended turn, change to player turn");

            currentSwipeAction = baseSwipeAction;

            isPlayerTurn = true;
            if (playerHero is Ramses ramses)
            {
                ramses.OnTurnStart();
            }
            extraActions = 0;
            UpdateUI();
        }
        private void TriggerOverTimeEffects()
        {
            //kich hoat dmg duy tri 
        }

        private void GrantExtraAction()
        {
            if (!hasEarnedExtraActionThisTurn)
            {
                extraActions++;
                hasEarnedExtraActionThisTurn = true;
                Debug.Log("Receive 1 extra action");
            }
        }

        private float CalculateMatchEfficiency(MatchResult match)
        {
            float totalEfficiency = 1f;
            int frozenCount = 0;

            foreach (var cell in match.MatchedCells)
            {
                if (cell.CurrentRune != null)
                {
                    if (cell.CurrentRune.CurrentEffect == RuneEffect.Empowered)
                    {
                        totalEfficiency += cell.CurrentRune.empowerMultiplier;
                    }
                    if (cell.CurrentRune.CurrentEffect == RuneEffect.Frozen && cell.CurrentRune.effectStacks > 0)
                    {
                        frozenCount++;

                    }
                }
            }
            totalEfficiency -= frozenCount * 0.3f;
            return Mathf.Max(0f, totalEfficiency);
        }

        private void ProcessRedRune(MatchType matchType, float comboEfficiency, Vector2 pos)
        {
            float baseDmg = playerHero.Stats.PhysicalDamage;
            float finalDmg = 0f;
            DamageType damageType = DamageType.Physical;
            float multiplier = 1.0f;

            switch (matchType)
            {
                case MatchType.Match3Line:
                    finalDmg = baseDmg * 1.0f;
                    break;
                case MatchType.Match4Line:
                case MatchType.Match4Square:
                    finalDmg = baseDmg * 1.2f;
                    break;
                case MatchType.Match5Cross:
                    finalDmg = baseDmg * 1.5f;
                    break;
                case MatchType.Match5Line:
                case MatchType.Match6Plus:
                    finalDmg = baseDmg * 1.2f;
                    damageType = DamageType.TrueDamage;
                    break;
            }

            finalDmg *= comboEfficiency;
            if (Random.value < playerHero.Stats.CritRate)
            {
                finalDmg *= playerHero.Stats.CritDamage;
                Debug.Log("Chí mạng!");
            }

            SpawnPopUp(pos, finalDmg, damageType == DamageType.TrueDamage ? Color.white : Color.red);
            enemyBoss.TakeDamage(finalDmg, damageType);
            RecordDamage(finalDmg, damageType);
            if (playerHero.Stats.LifeSteal > 0)
            {
                float heal = finalDmg * playerHero.Stats.LifeSteal;
                playerHero.Stats.CurrentHP = Mathf.Min(playerHero.Stats.CurrentHP + heal, playerHero.Stats.MaxHP);
                RecordHeal(heal);
                Debug.Log($"Hút máu hồi {heal} HP.");
            }
            Debug.Log($"[Phys Dmg] Deal {finalDmg} Dmg ({damageType})");
        }

        private void ProcessBlueRune(MatchType matchType, float comboEfficiency, Vector2 pos)
        {
            float baseMagicDmg = playerHero.Stats.MagicalDamage;
            float finalDmg = 0f;

            switch (matchType)
            {
                case MatchType.Match3Line:
                    finalDmg = baseMagicDmg * 1.0f; // 100% SMPT
                    break;
                case MatchType.Match4Line:
                case MatchType.Match4Square:
                    finalDmg = baseMagicDmg * 1.3f; // 130% SMPT
                    break;
                case MatchType.Match5Cross:
                    finalDmg = baseMagicDmg * 1.6f; // 160% SMPT
                    break;
                case MatchType.Match5Line:
                case MatchType.Match6Plus:
                    finalDmg = baseMagicDmg * 2.0f; // 200% SMPT
                    // TODO: Gọi hàm gây duy trì 50% SMPT trong 2 hiệp
                    break;
            }

            finalDmg *= comboEfficiency;

            SpawnPopUp(pos, finalDmg, Color.cyan);
            enemyBoss.TakeDamage(finalDmg, DamageType.Magical);
            RecordDamage(finalDmg, DamageType.Magical);
            if (playerHero.Stats.SpellVamp > 0)
            {
                float heal = finalDmg * playerHero.Stats.LifeSteal;
                playerHero.Stats.CurrentHP = Mathf.Min(playerHero.Stats.CurrentHP + heal, playerHero.Stats.MaxHP);
                RecordHeal(heal);
                Debug.Log($"Hút máu hồi {heal} HP.");
            }
            Debug.Log($"[Blue Rune] Gây {finalDmg} sát thương phép thuật.");
        }

        private void ProcessGreenRune(MatchType matchType, float comboEfficiency, Vector2 pos)
        {
            float baseHeal = playerHero.Stats.HPRegen;
            float finalHeal = 0f;

            switch (matchType)
            {
                case MatchType.Match3Line: finalHeal = baseHeal * 1.0f; break;
                case MatchType.Match4Line:
                case MatchType.Match4Square: finalHeal = baseHeal * 1.5f; break;
                case MatchType.Match5Cross: finalHeal = baseHeal * 1.8f; break;
                case MatchType.Match5Line:
                case MatchType.Match6Plus:
                    finalHeal = baseHeal * 2.0f;
                    // TODO: Tăng 10% Giới hạn HP trong 2 hiệp
                    break;
            }

            finalHeal *= comboEfficiency;

            SpawnPopUp(pos, finalHeal, Color.green);
            RecordHeal(finalHeal);
            playerHero.Stats.CurrentHP = Mathf.Min(playerHero.Stats.CurrentHP + finalHeal, playerHero.Stats.MaxHP);
            Debug.Log($"[Green Rune] Hồi {finalHeal} HP.");
        }

        private void ProcessYellowRune(MatchType matchType, float comboEfficiency, Vector2 pos)
        {
            float baseMana = playerHero.Stats.ManaRegen;
            float finalMana = 0f;

            switch (matchType)
            {
                case MatchType.Match3Line: finalMana = baseMana * 1.0f; break;
                case MatchType.Match4Line:
                case MatchType.Match4Square: finalMana = baseMana * 1.5f; break;
                case MatchType.Match5Cross: finalMana = baseMana * 1.8f; break;
                case MatchType.Match5Line:
                case MatchType.Match6Plus:
                    finalMana = baseMana * 2.0f;
                    // TODO: Giảm 1 hiệp hồi chiêu cho mọi kĩ năng đang cooldown
                    break;
            }

            finalMana *= comboEfficiency;
            SpawnPopUp(pos, finalMana, Color.yellow);
            playerHero.Stats.CurrentMana = Mathf.Min(playerHero.Stats.CurrentMana + finalMana, playerHero.Stats.MaxMana);
            Debug.Log($"[Yellow Rune] Hồi {finalMana} Mana.");
        }

        private void ProcessOrangeRune(MatchType matchType, float comboEfficiency, Vector2 pos)
        {
            float maxHP = playerHero.Stats.MaxHP;
            float shieldPercent = 0f;

            switch (matchType)
            {
                case MatchType.Match3Line:
                    shieldPercent = 0.05f; // 5% HP tối đa
                    break;
                case MatchType.Match4Line:
                case MatchType.Match4Square:
                    shieldPercent = 0.06f; // 6% HP tối đa
                    break;
                case MatchType.Match5Cross:
                    shieldPercent = 0.08f; // 8% HP tối đa
                    break;
                case MatchType.Match5Line:
                case MatchType.Match6Plus:
                    shieldPercent = 0.10f; // 10% HP tối đa
                    break;
            }

            // Tính toán lượng Shield nhận được và cộng dồn vào Shield hiện tại
            float shieldAmount = maxHP * shieldPercent * comboEfficiency;
            playerHero.Stats.Shield += shieldAmount;

            SpawnPopUp(pos, shieldAmount, Color.orange);

            Debug.Log($"[Đá Cam] Nhận được lá chắn trị giá {shieldAmount} HP.");
            UpdateUI(); // Cập nhật lại giao diện để hiển thị thanh Shield nếu có
        }

        //Nay so
        public void SpawnPopUp(Vector2 position, float value, Color color)
        {
            if (value <= 0 || floatingTextPrefab == null)
            {
                if (floatingTextPrefab == null) Debug.LogWarning("Chưa gán floatingTextPrefab trong Inspector!");
                return;
            }
            if (value <= 0) return;
            // 1. Tạo Popup là con của Canvas
            GameObject popUp = Instantiate(floatingTextPrefab, canvasTransform);

            // 2. Chuyển đổi vị trí từ Thế giới sang Màn hình
            // "position" từ BoardManager là tọa độ (x*Spacing, y*Spacing)
            Vector3 worldPos = new Vector3(position.x, position.y, 0);
            Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);

            // Gán trực tiếp vào position của UI
            popUp.transform.position = screenPos;

            // 3. Quan trọng: Đảm bảo Scale không bị nhảy
            popUp.transform.localScale = Vector3.one;

            // 4. Gọi Setup
            FloatingText ft = popUp.GetComponent<FloatingText>();
            if (ft != null)
            {
                ft.Setup(Mathf.RoundToInt(value).ToString(), color);
            }
        }

        //Rune Effect
        public void ApplyRuneNegativeEffect(RuneEffect effect)
        {
            if (playerHero == null) return;
            if (effect == RuneEffect.Burn)
            {
                //deal Burn dmg
                float burnDmg = playerHero.Stats.MaxHP * 0.02f;
                playerHero.TakeDamage(burnDmg, DamageType.Magical);

                Debug.Log($"Received Burn Dmg: {burnDmg}");
                SpawnPopUp(Vector2.zero, burnDmg, Color.violetRed);
            }
            else if (effect == RuneEffect.Poison || effect == RuneEffect.PoisonSpread)
            {
                //deal Poison dmg
                float poisonDmg = Random.Range(10f, 20f);
                playerHero.TakeDamage(poisonDmg, DamageType.TrueDamage);

                Debug.Log($"Received Poison Dmg: {poisonDmg}");
                SpawnPopUp(Vector2.zero, poisonDmg, Color.violet);
            }

            UpdateUI();
        }

        //Quan li action
        public void DeductSwipeAction()
        {
            if (extraActions > 0)
            {
                extraActions--;
            }
            else
            {
                currentSwipeAction--;
            }
            UpdateUI();
        }
        public void CheckTurnEnd()
        {
            if (currentSwipeAction <= 0 && extraActions <= 0)
            {
                EndTurn();
            }
        }

        //GUI tam thoi

    }
}
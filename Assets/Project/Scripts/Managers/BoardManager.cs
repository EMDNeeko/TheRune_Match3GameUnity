using System.Collections.Generic;
using System.Collections;
using System.Linq;
using Match3Game.Board;
using Match3Game.Data;
using Match3Game.Mechanics;
using UnityEngine;

namespace Match3Game.Managers
{
    public class BoardManager : MonoBehaviour
    {
        [Header("Board Configuration")]
        public int Width = 8;
        public int Height = 8;
        public float Spacing = 1.2f; //distance between cells

        [Header("Prefabs & Assets")]
        public GameObject RunePrefab;
        public Sprite[] RuneSprites;
        public Sprite[] SpecialRuneSprites; //0: Bomb, 1: LB, 2:RB, 3:Meteor

        [Header("Managers")]
        public TestCombatManager combatManager;

        [Header("Skill Targeting")]
        public SkillType pendingSkillType;
        private CellData targetedCell;
        public GameObject highlightPrefab;
        private GameObject currentHighlight;

        [Header("Rune Effect Prefabs")]
        public GameObject burnEffectPrefab;
        public GameObject poisonEffectPrefab;
        public GameObject freezeEffectPrefab;

        private BoardData boardData;
        private MatchDetector matchDetector;
        private RuneView[,] runeViews; //show object

        //xu ly input
        private Vector2 startTouchPosition;
        private Vector2 endTouchPosition;
        private CellData selectedCell;
        private float swipeThreshold = 50f;

        void Start()
        {
            InitializeBoard();
        }
        private void InitializeBoard()
        {
            boardData = new BoardData();
            boardData.Width = Width;
            boardData.Height = Height;
            boardData.initBoard();

            matchDetector = new MatchDetector(boardData);
            runeViews = new RuneView[Width, Height];

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    SpawnRuneAt(x, y, true);
                }
            }
        }

        private void SpawnRuneAt(int x, int y, bool isInitialSpawn)
        {
            List<RuneType> availableTypes = new List<RuneType>
            {
                RuneType.Red, RuneType.Blue, RuneType.Green, RuneType.Yellow, RuneType.Orange, RuneType.Purple
            };

            if (isInitialSpawn)
            {
                if (x >= 2 && boardData.Grid[x - 1, y].CurrentRune.BaseType == boardData.Grid[x - 2, y].CurrentRune.BaseType)
                {
                    availableTypes.Remove(boardData.Grid[x - 1, y].CurrentRune.BaseType);
                }
                if (y >= 2 && boardData.Grid[x, y - 1].CurrentRune.BaseType == boardData.Grid[x, y - 2].CurrentRune.BaseType)
                {
                    availableTypes.Remove(boardData.Grid[x, y - 1].CurrentRune.BaseType);
                }
            }

            RuneType chosenType = availableTypes[Random.Range(0, availableTypes.Count)];
            RuneData newRune = new RuneData(chosenType);
            boardData.Grid[x, y].SetRune(newRune);

            Vector2 spawnPos = new Vector2(x * Spacing, (y + 5) * Spacing);
            GameObject runeObj = Instantiate(RunePrefab, spawnPos, Quaternion.identity, this.transform);

            RuneView view = runeObj.GetComponent<RuneView>();
            view.Initialize(RuneSprites[(int)chosenType], spawnPos);
            view.MoveToPosition(new Vector2(x * Spacing, y * Spacing));

            runeViews[x, y] = view;
        }
        public CellData GetCell(int x, int y)
        {
            return boardData.GetCell(x, y);
        }
        public void EnterSkillTargetingMode(SkillType skill)
        {
            boardData.CurrentState = BoardState.SkillTargeting;
            pendingSkillType = skill;
            targetedCell = null;
        }
        public void ExitSkillTargetingMode()
        {
            boardData.CurrentState = BoardState.Idle;

            pendingSkillType = SkillType.None;
            targetedCell = null;

            if (currentHighlight != null)
            {
                Destroy(currentHighlight);
                currentHighlight = null;
            }
        }
        public CellData GetTargetedCell()
        {
            return targetedCell;
        }

        void Update()
        {
            // Nếu CombatManager tồn tại và ĐANG KHÔNG PHẢI LƯỢT CỦA NGƯỜI CHƠI -> Khóa bảng (bỏ qua mọi thao tác)
            if (combatManager != null && !combatManager.isPlayerTurn)
            {
                return;
            }
            if (boardData.CurrentState == BoardState.SkillTargeting)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    CellData clickedCell = GetCellFromMousePos();
                    if (clickedCell != null)
                    {
                        targetedCell = clickedCell;

                        // Lấy form hiện tại của Ramses từ CombatManager
                        var ramses = GetComponent<TestCombatManager>().playerHero as Entities.Heroes.Ramses;
                        var form = ramses != null ? ramses.CurrentForm : Entities.Heroes.RamsesForm.Normal;

                        ShowHighlightArea(clickedCell, pendingSkillType, form);
                    }
                }
                return;
            }
            if (boardData.CurrentState != BoardState.Idle)
            {
                return;
            }
            if (Input.GetMouseButtonDown(0))
            {
                startTouchPosition = Input.mousePosition;
                selectedCell = GetCellFromMousePos();
            }
            else if (Input.GetMouseButtonUp(0) && selectedCell != null)
            {
                endTouchPosition = Input.mousePosition;
                CalculateSwipe();
            }
        }
        public List<CellData> GetCellsInRange(CellData center, int rangeX, int rangeY)
        {
            List<CellData> res = new List<CellData>();
            for (int x = center.X - rangeX; x <= center.X + rangeX; x++)
            {
                for (int y = center.Y - rangeY; y <= center.Y + rangeY; y++)
                {
                    CellData cell = boardData.GetCell(x, y);
                    if (cell != null) res.Add(cell);
                }
            }
            return res;
        }
        private void ShowHighlightArea(CellData centerCell, SkillType skill, Entities.Heroes.RamsesForm currentForm)
        {
            if (currentHighlight != null) Destroy(currentHighlight);

            Vector2 pos = new Vector2(centerCell.X * Spacing, centerCell.Y * Spacing);
            currentHighlight = Instantiate(highlightPrefab, pos, Quaternion.identity);

            //scale vung chon theo skill
            if (skill == SkillType.Active)
            {
                if (currentForm == Entities.Heroes.RamsesForm.Normal)
                {
                    currentHighlight.transform.localScale = new Vector3(3 * Spacing, 3 * Spacing, 1);
                }
                else
                {
                    currentHighlight.transform.localScale = new Vector3(5 * Spacing, 5 * Spacing, 1);
                }

            }
            else if (skill == SkillType.Ultimate && currentForm == Entities.Heroes.RamsesForm.Burning)
            {
                // Highlight 3 cột dọc chạy dài hết bảng
                currentHighlight.transform.localScale = new Vector3(3 * Spacing, Height * Spacing, 1);

                // Cần dời vị trí tâm (Y) ra giữa bảng để nó bao trọn từ dưới lên trên
                // Trừ đi 0.5f Spacing để highlight không bị lố lên trên cùng
                float centerY = ((Height - 1) / 2f) * Spacing;
                currentHighlight.transform.position = new Vector2(centerCell.X * Spacing, centerY);
            }

        }
        public void DesTroyAreaAndRefill(List<CellData> cellsToCollect, List<CellData> cellsToDestroy, float efficiencyMultiplier = 1f)
        {
            boardData.CurrentState = BoardState.Executing;
            TestCombatManager combatManager = GetComponent<TestCombatManager>();

            // 1. Xử lý nhóm Thu Thập (Collect)
            foreach (var cell in cellsToCollect)
            {
                if (cell.CurrentRune != null)
                {
                    Vector2 worldPos = new Vector2(cell.X * Spacing, cell.Y * Spacing);
                    if (cell.CurrentRune.SpecialType != SpecialRuneType.None)
                    {
                        // Kích hoạt đá đặc biệt
                        StartCoroutine(ExplodeSpecialRune(cell));
                    }
                    else
                    {
                        // Chỉ thu thập (tính điểm) nếu đá không bị đóng băng
                        if (!(cell.CurrentRune.CurrentEffect == RuneEffect.Frozen && cell.CurrentRune.effectStacks > 0))
                        {
                            combatManager?.ProcessingSingleRune(cell.CurrentRune.OriginalColor, worldPos, cell.CurrentRune.GetEfficiency() * efficiencyMultiplier);
                        }

                        // Xử lý nổ/phá băng/kích hoạt hiệu ứng
                        ProcessRuneDestruction(cell);
                    }
                }
            }

            // 2. Xử lý nhóm Phá Huỷ (Destroy) - Không kích hoạt sát thương hay hiệu ứng
            foreach (var cell in cellsToDestroy)
            {
                if (cell.CurrentRune != null && !cellsToCollect.Contains(cell))
                {
                    ProcessRuneDestruction(cell);
                }
            }

            // 3. Chờ đá rơi xuống
            StartCoroutine(WaitAndApplyGravity());
        }
        private void ClearCell(CellData cell)
        {
            cell.ClearRune();
            if (runeViews[cell.X, cell.Y] != null)
            {
                Destroy(runeViews[cell.X, cell.Y].gameObject);
                runeViews[cell.X, cell.Y] = null;
            }
        }

        private IEnumerator WaitAndApplyGravity()
        {
            yield return new WaitForSeconds(0.3f);
            ApplyGravity();
            yield return new WaitForSeconds(0.4f);

            List<MatchResult> newMatches = matchDetector.FindAllMatches();
            if (newMatches.Count > 0)
            {
                StartCoroutine(ProcessMatchesAndRefill(newMatches));
            }
            else
            {
                boardData.CurrentState = BoardState.Idle;

                if (combatManager != null && combatManager.isPlayerTurn)
                {
                    combatManager.CheckTurnEnd();
                }
            }
        }

        private CellData GetCellFromMousePos()
        {
            Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            int x = Mathf.RoundToInt(mouseWorldPos.x / Spacing);
            int y = Mathf.RoundToInt(mouseWorldPos.y / Spacing);
            return boardData.GetCell(x, y);
        }

        private void CalculateSwipe()
        {
            Vector2 swipeDelta = endTouchPosition - startTouchPosition;

            //Swipe
            if (swipeDelta.magnitude > swipeThreshold)
            {
                if (!selectedCell.CurrentRune.isMovable())
                {
                    Debug.Log("Da bi dong bang, khong the thao tac");
                    selectedCell = null;
                    return;
                }

                swipeDelta.Normalize();
                int dirX = Mathf.RoundToInt(swipeDelta.x);
                int dirY = Mathf.RoundToInt(swipeDelta.y);

                //vuot 1 huong
                if (Mathf.Abs(dirX) > Mathf.Abs(dirY))
                {
                    dirY = 0;
                }
                else
                {
                    dirX = 0;
                }

                CellData targetCell = boardData.GetCell(selectedCell.X + dirX, selectedCell.Y + dirY);

                //Active special LB & Bomb
                if (targetCell != null)
                {
                    if (!targetCell.CurrentRune.isMovable())
                    {
                        Debug.Log("Bi dong bang, khong the doi cho");
                        selectedCell = null;
                        return;
                    }
                    SpecialRuneType selectedType = selectedCell.CurrentRune.SpecialType;
                    SpecialRuneType targetType = targetCell.CurrentRune.SpecialType;

                    // Nếu người chơi cầm đá LineBlast hoặc Bomb và vuốt
                    if (selectedType == SpecialRuneType.LineBlast || selectedType == SpecialRuneType.Bomb)
                    {
                        StartCoroutine(SwipeAndExplodeSpecial(selectedCell, targetCell, selectedCell));
                    }
                    // Nếu người chơi cầm đá thường vuốt VÀO đá LineBlast hoặc Bomb
                    else if (targetType == SpecialRuneType.LineBlast || targetType == SpecialRuneType.Bomb)
                    {
                        StartCoroutine(SwipeAndExplodeSpecial(selectedCell, targetCell, targetCell));
                    }
                    // Các trường hợp còn lại (bao gồm cả vuốt Meteor/Rainbow) thì kiểm tra tổ hợp bình thường
                    else
                    {
                        StartCoroutine(SwapAndCheck(selectedCell, targetCell));
                    }
                }
            }
            else
            {
                if (selectedCell != null && selectedCell.CurrentRune != null)
                {
                    SpecialRuneType spType = selectedCell.CurrentRune.SpecialType;

                    // Chỉ kích hoạt khi bấm vào Meteor hoặc Rainbow
                    if (spType == SpecialRuneType.Meteor || spType == SpecialRuneType.Rainbow)
                    {
                        if (combatManager != null)
                        {
                            combatManager.DeductSwipeAction();
                        }
                        StartCoroutine(ExplodeSpecialRune(selectedCell));
                    }
                }
            }
            selectedCell = null;
        }
        //swap + to hop
        private IEnumerator SwapAndCheck(CellData cellA, CellData cellB)
        {
            boardData.CurrentState = BoardState.Executing;

            SwapVisualsAndData(cellA, cellB);
            yield return new WaitForSeconds(0.3f); // anim swap

            //goi luot bat dau
            GetComponent<TestCombatManager>()?.StartAction();

            List<MatchResult> matches = matchDetector.FindAllMatches();

            if (matches.Count > 0)
            {
                if (combatManager != null)
                {
                    combatManager.DeductSwipeAction();
                }
                foreach (var match in matches)
                {
                    // Check which swapped cell is actually part of this specific match
                    bool involvesCellB = match.MatchedCells.Any(c => c.X == cellB.X && c.Y == cellB.Y);
                    bool involvesCellA = match.MatchedCells.Any(c => c.X == cellA.X && c.Y == cellA.Y);

                    if (involvesCellB)
                    {
                        match.SpawnCell = cellB;
                    }
                    else if (involvesCellA)
                    {
                        match.SpawnCell = cellA;
                    }
                }
                StartCoroutine(ProcessMatchesAndRefill(matches));
            }
            else
            {
                SwapVisualsAndData(cellA, cellB); //dat lai vi tri
                yield return new WaitForSeconds(0.3f);
                boardData.CurrentState = BoardState.Idle;
            }
        }

        private IEnumerator SwipeAndExplodeSpecial(CellData cellA, CellData cellB, CellData originalSpecialCell)
        {
            boardData.CurrentState = BoardState.Executing;

            // 1. Đổi vị trí 2 viên đá (Cả Data và Visual)
            SwapVisualsAndData(cellA, cellB);
            yield return new WaitForSeconds(0.3f); // Đợi đá trượt sang ô mới

            // 2. Xác định vị trí MỚI của đá đặc biệt sau khi đã hoán đổi
            CellData explosionCenter = (originalSpecialCell == cellA) ? cellB : cellA;

            // 3. Gọi hàm kích nổ mượn lại logic của ExplodeSpecialRune
            if (combatManager != null)
            {
                combatManager.DeductSwipeAction();
            }
            yield return StartCoroutine(ExplodeSpecialRune(explosionCenter));
        }

        private void SwapVisualsAndData(CellData cellA, CellData cellB)
        {
            RuneData tmpRune = cellA.CurrentRune;
            cellA.SetRune(cellB.CurrentRune);
            cellB.SetRune(tmpRune);

            RuneView tmpView = runeViews[cellA.X, cellA.Y];
            runeViews[cellA.X, cellA.Y] = runeViews[cellB.X, cellB.Y];
            runeViews[cellB.X, cellB.Y] = tmpView;

            //anim
            runeViews[cellA.X, cellA.Y].MoveToPosition(new Vector2(cellA.X * Spacing, cellA.Y * Spacing));
            runeViews[cellB.X, cellB.Y].MoveToPosition(new Vector2(cellB.X * Spacing, cellB.Y * Spacing));
        }

        private IEnumerator ProcessMatchesAndRefill(List<MatchResult> matches)
        {
            // 1. Gộp tất cả các ô cần phá hủy vào một danh sách
            List<CellData> cellsToDestroy = new List<CellData>();

            foreach (var match in matches)
            {
                CellData targetCell = match.SpawnCell != null ? match.SpawnCell : match.MatchedCells[0];
                Vector2 popupPos = new Vector2(targetCell.X * Spacing, targetCell.Y * Spacing);
                // Truyền data tổ hợp gốc cho tracker 
                if (combatManager != null)
                {
                    combatManager.ProcessMatchResult(match, popupPos);
                }

                foreach (var cell in match.MatchedCells)
                {
                    if (!cellsToDestroy.Contains(cell))
                    {
                        cellsToDestroy.Add(cell);
                    }
                }
            }

            // 2. TÌM VÀ KÍCH HOẠT ĐÁ ĐẶC BIỆT NẰM TRONG TỔ HỢP (Phản ứng dây chuyền)
            // Dùng vòng lặp for thay vì foreach vì danh sách cellsToDestroy có thể dài ra khi nổ lây
            // for (int i = 0; i < cellsToDestroy.Count; i++)
            // {
            //     CellData currentCell = cellsToDestroy[i];

            //     // Nếu ô này có chứa đá đặc biệt (Bomb, Line Blast, Meteor...)
            //     if (currentCell.CurrentRune != null && currentCell.CurrentRune.SpecialType != SpecialRuneType.None && currentCell.CurrentRune.SpecialType != SpecialRuneType.Rainbow)
            //     {
            //         // Lấy danh sách các ô bị nổ lây bởi viên đá đặc biệt này
            //         List<CellData> affectedCells = GetSpecialRuneAffectedCells(currentCell, currentCell.CurrentRune.SpecialType);

            //         foreach (var affected in affectedCells)
            //         {
            //             // Thêm ô nổ lây vào danh sách phá hủy chung
            //             if (!cellsToDestroy.Contains(affected) && affected.CurrentRune != null)
            //             {
            //                 cellsToDestroy.Add(affected);

            //                 // Báo cho Combat Manager cộng điểm viên đá bị nổ lây (True Damage, Hồi máu...)
            //                 if (combatManager != null)
            //                 {
            //                     combatManager.ProcessingSingleRune(affected.CurrentRune.BaseType);
            //                 }
            //             }
            //         }
            //     }
            // }

            // 3. Thực hiện phá hủy toàn bộ danh sách (cả tổ hợp gốc + các ô nổ lây)
            // List<CellData> actualDestroyList = new List<CellData>();
            // foreach (var cell in cellsToDestroy)
            // {
            //     if (cell.CurrentRune != null)
            //     {
            //         // break ice
            //         if (cell.CurrentRune.CurrentEffect == RuneEffect.Frozen && cell.CurrentRune.effectStacks > 0)
            //         {
            //             cell.CurrentRune.effectStacks--;
            //             if (cell.CurrentRune.effectStacks <= 0)
            //             {
            //                 cell.CurrentRune.CurrentEffect = RuneEffect.None;
            //             }

            //             if (combatManager != null)
            //             {
            //                 Vector2 worldPos = new Vector2(cell.X * Spacing, cell.Y * Spacing);
            //                 combatManager.ProcessingSingleRune(cell.CurrentRune.OriginalColor, worldPos);
            //             }
            //         }
            //         else
            //         {
            //             TriggerDestroyEffects(cell.CurrentRune);
            //             actualDestroyList.Add(cell);
            //         }
            //     }
            // }

            // foreach (var cell in actualDestroyList)
            // {
            //     if (cell.CurrentRune != null)
            //     {
            //         ProcessRuneDestruction(cell);
            //     }
            // }

            // 3. Thực hiện phá hủy toàn bộ danh sách (cả tổ hợp gốc + các ô nổ lây)
            foreach (var cell in cellsToDestroy)
            {
                if (cell.CurrentRune != null)
                {
                    // Hàm này đã bao gồm: trừ stack, cập nhật text UI, gỡ hình ảnh băng và kích hoạt hiệu ứng xấu
                    ProcessRuneDestruction(cell);
                }
            }

            // 4. Tạo đá đặc biệt mới (nếu tổ hợp gốc thỏa mãn điều kiện tạo Bomb/Line/...)
            HashSet<CellData> usedSpawnCells = new HashSet<CellData>();
            foreach (var match in matches)
            {
                if (match.ResultingSpecialRune != SpecialRuneType.None)
                {
                    CellData targetSpawnCell = match.SpawnCell;

                    // Nếu SpawnCell chưa được chỉ định, hoặc Ô ĐÓ ĐÃ BỊ CHIẾM bởi một đá đặc biệt khác trong combo này
                    if (targetSpawnCell == null || usedSpawnCells.Contains(targetSpawnCell))
                    {
                        targetSpawnCell = null; // Reset lại để tìm nhà mới

                        // Lục tìm một ô khác còn trống trong cùng tổ hợp đó
                        foreach (var cell in match.MatchedCells)
                        {
                            if (!usedSpawnCells.Contains(cell))
                            {
                                targetSpawnCell = cell;
                                break;
                            }
                        }
                    }

                    // Nếu đã tìm được ô an toàn thì mới tiến hành tạo đá
                    if (targetSpawnCell != null)
                    {
                        usedSpawnCells.Add(targetSpawnCell); // Đánh dấu ô này đã có chủ
                        SpawnSpecialRuneAt(targetSpawnCell.X, targetSpawnCell.Y, match.ResultingSpecialRune, match.MatchedRuneType);
                    }
                }
            }

            yield return new WaitForSeconds(0.2f);

            // 5. Rớt đá
            ApplyGravity();
            yield return new WaitForSeconds(0.4f);

            // 6. Tìm match mới (Cascades)
            List<MatchResult> newMatches = matchDetector.FindAllMatches();
            if (newMatches.Count > 0)
            {
                StartCoroutine(ProcessMatchesAndRefill(newMatches));
            }
            else
            {
                boardData.CurrentState = BoardState.Idle;
                if (combatManager != null && combatManager.isPlayerTurn)
                {
                    combatManager.CheckTurnEnd();
                }
            }
        }
        private void TriggerDestroyEffects(RuneData rune)
        {
            if (rune == null) return;
            if (rune.CurrentEffect == RuneEffect.Burn)
            {
                Debug.Log("Da chay no, gay dmg va gan thieu dot");
            }
            else if (rune.CurrentEffect == RuneEffect.Poison || rune.CurrentEffect == RuneEffect.PoisonSpread)
            {
                Debug.Log("Da nhiem doc, gay dmg chuan");
            }
        }

        private IEnumerator ExplodeSpecialRune(CellData originCell)
        {
            boardData.CurrentState = BoardState.Executing;
            TestCombatManager combatManager = GetComponent<TestCombatManager>();
            combatManager?.StartAction();

            //Hang doi no day chuyen
            Queue<CellData> specialRunesToExplode = new Queue<CellData>();
            specialRunesToExplode.Enqueue(originCell);

            List<CellData> cellsToDestroy = new List<CellData>();
            HashSet<CellData> processedSpecials = new HashSet<CellData>();

            // 2. Quét toàn bộ vùng ảnh hưởng của chuỗi nổ
            while (specialRunesToExplode.Count > 0)
            {
                CellData currentSpecial = specialRunesToExplode.Dequeue();

                // Bỏ qua nếu viên đá đặc biệt này đã nổ rồi
                if (processedSpecials.Contains(currentSpecial)) continue;
                processedSpecials.Add(currentSpecial);

                // Đảm bảo viên đá gốc cũng nằm trong danh sách xoá
                if (!cellsToDestroy.Contains(currentSpecial))
                {
                    cellsToDestroy.Add(currentSpecial);
                }

                // CHỈ GỌI HÀM NÀY ĐÚNG 1 LẦN CHO MỖI VIÊN ĐÁ: Vùng Random của Meteor sẽ được chốt tại đây
                List<CellData> affectedCells = GetSpecialRuneAffectedCells(currentSpecial, currentSpecial.CurrentRune.SpecialType);

                foreach (var cell in affectedCells)
                {
                    if (cell.CurrentRune != null)
                    {
                        // Thêm ô bị ảnh hưởng vào danh sách xoá tổng
                        if (!cellsToDestroy.Contains(cell))
                        {
                            cellsToDestroy.Add(cell);
                        }

                        // NẾU Ô BỊ LAN TỚI LÀ ĐÁ ĐẶC BIỆT -> Đưa vào hàng đợi để nổ tiếp
                        if (cell.CurrentRune.SpecialType != SpecialRuneType.None &&
                            !processedSpecials.Contains(cell) &&
                            !specialRunesToExplode.Contains(cell))
                        {
                            specialRunesToExplode.Enqueue(cell);
                        }
                    }
                }
            }

            foreach (var cell in cellsToDestroy)
            {
                if (cell.CurrentRune != null)
                {
                    Vector2 worldPos = new Vector2(cell.X * Spacing, cell.Y * Spacing);

                    // Đá dính băng sẽ không sinh ra điểm thu thập
                    if (!(cell.CurrentRune.CurrentEffect == RuneEffect.Frozen && cell.CurrentRune.effectStacks > 0))
                    {
                        combatManager?.ProcessingSingleRune(cell.CurrentRune.OriginalColor, worldPos);
                    }

                    // Gọi hàm xử lý tổng hợp
                    ProcessRuneDestruction(cell);
                }
            }

            yield return new WaitForSeconds(0.3f);
            ApplyGravity();
            yield return new WaitForSeconds(0.4f);

            List<MatchResult> newMatches = matchDetector.FindAllMatches();
            if (newMatches.Count > 0)
            {
                StartCoroutine(ProcessMatchesAndRefill(newMatches));
            }
            else
            {
                boardData.CurrentState = BoardState.Idle;
                if (combatManager != null && combatManager.isPlayerTurn)
                {
                    combatManager.CheckTurnEnd();
                }
            }
        }

        private List<CellData> GetSpecialRuneAffectedCells(CellData origin, SpecialRuneType type)
        {
            List<CellData> affected = new List<CellData>();
            int ox = origin.X;
            int oy = origin.Y;

            if (type == SpecialRuneType.LineBlast)
            {
                for (int x = 0; x < Width; x++)
                {
                    if (boardData.Grid[x, oy].CurrentRune != null)
                    {
                        affected.Add(boardData.Grid[x, oy]);
                    }
                }
                for (int y = 0; y < Height; y++)
                {
                    if (boardData.Grid[ox, y].CurrentRune != null && !affected.Contains(boardData.Grid[ox, y]))
                    {
                        affected.Add(boardData.Grid[ox, y]);
                    }
                }
            }

            else if (type == SpecialRuneType.Bomb)
            {
                // Vùng nổ 3x3 cơ bản
                for (int x = ox - 1; x <= ox + 1; x++)
                {
                    for (int y = oy - 1; y <= oy + 1; y++)
                    {
                        if (x >= 0 && x < Width && y >= 0 && y < Height && boardData.Grid[x, y].CurrentRune != null)
                        {
                            affected.Add(boardData.Grid[x, y]);
                        }
                    }
                }

                // Các tia nổ chữ thập mở rộng (+2)
                if (ox - 2 >= 0 && boardData.Grid[ox - 2, oy].CurrentRune != null)
                    affected.Add(boardData.Grid[ox - 2, oy]);

                // ---> ĐÃ SỬA DÒNG NÀY: Đổi (ox + 2 >= 0) thành (ox + 2 < Width) <---
                if (ox + 2 < Width && boardData.Grid[ox + 2, oy].CurrentRune != null)
                    affected.Add(boardData.Grid[ox + 2, oy]);

                if (oy - 2 >= 0 && boardData.Grid[ox, oy - 2].CurrentRune != null)
                    affected.Add(boardData.Grid[ox, oy - 2]);

                if (oy + 2 < Height && boardData.Grid[ox, oy + 2].CurrentRune != null)
                    affected.Add(boardData.Grid[ox, oy + 2]);
            }

            else if (type == SpecialRuneType.Meteor)
            {
                //2 Random 2x2
                for (int i = 0; i < 2; i++)
                {
                    int rx = Random.Range(0, Width - 1);
                    int ry = Random.Range(0, Height - 1);
                    for (int x = rx; x <= rx + 1; x++)
                    {
                        for (int y = ry; y <= ry + 1; y++)
                        {
                            if (boardData.Grid[x, y].CurrentRune != null && !affected.Contains(boardData.Grid[x, y]))
                            {
                                affected.Add(boardData.Grid[x, y]);
                            }
                        }
                    }
                }
                if (!affected.Contains(origin))
                {
                    affected.Add(origin);
                }
            }

            else if (type == SpecialRuneType.Rainbow)
            {
                // Chọn ngẫu nhiên một màu (Giả sử bạn có 4 màu cơ bản từ 0 đến 3)
                RuneType randomColor = (RuneType)UnityEngine.Random.Range(0, 4);

                // Thu thập toàn bộ đá có màu ngẫu nhiên vừa chọn
                for (int x = 0; x < Width; x++)
                {
                    for (int y = 0; y < Height; y++)
                    {
                        if (boardData.Grid[x, y].CurrentRune != null && boardData.Grid[x, y].CurrentRune.BaseType == randomColor)
                        {
                            affected.Add(boardData.Grid[x, y]);
                        }
                    }
                }
                if (!affected.Contains(origin))
                {
                    affected.Add(origin);
                }
            }

            return affected;
        }
        private void SpawnSpecialRuneAt(int x, int y, SpecialRuneType specialType, RuneType baseType)
        {
            // Nếu tại vị trí này đang có một hình ảnh chưa bị hủy, hãy hủy nó trước khi ghi đè!
            if (runeViews[x, y] != null)
            {
                Destroy(runeViews[x, y].gameObject);
                runeViews[x, y] = null;
            }
            // 1. Ánh xạ sang BaseType mới để tránh bị match nhầm
            RuneType newBaseType = RuneType.None;
            if (specialType == SpecialRuneType.LineBlast) newBaseType = RuneType.SpecialLineBlast;
            else if (specialType == SpecialRuneType.Bomb) newBaseType = RuneType.SpecialBomb;
            else if (specialType == SpecialRuneType.Meteor) newBaseType = RuneType.SpecialMeteor;
            else if (specialType == SpecialRuneType.Rainbow) newBaseType = RuneType.SpecialRainbow;

            // 2. Khởi tạo dữ liệu đá
            RuneData specialRune = new RuneData(newBaseType);
            specialRune.SpecialType = specialType;
            specialRune.OriginalColor = baseType; // Lưu lại màu gốc
            boardData.Grid[x, y].SetRune(specialRune);

            Vector2 pos = new Vector2(x * Spacing, y * Spacing);
            GameObject runeObj = Instantiate(RunePrefab, pos, Quaternion.identity, this.transform);
            RuneView view = runeObj.GetComponent<RuneView>();

            int specialIndex = (int)specialType - 1; // Vì enum SpecialRuneType có None ở vị trí 0
            if (SpecialRuneSprites != null && SpecialRuneSprites.Length > specialIndex && SpecialRuneSprites[specialIndex] != null)
            {
                view.Initialize(SpecialRuneSprites[specialIndex], pos);
            }
            else
            {
                view.Initialize(RuneSprites[(int)baseType], pos); // Fallback
            }

            runeViews[x, y] = view;

            Debug.Log($"Created Special Rune Type: {specialType} in ({x}, {y})");
        }

        private void ApplyGravity()
        {
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    if (boardData.Grid[x, y].isEmpty())
                    {
                        for (int upperY = y + 1; upperY < Height; upperY++)
                        {
                            if (!boardData.Grid[x, upperY].isEmpty() && boardData.Grid[x, upperY].CurrentRune.isMovable())
                            {
                                boardData.Grid[x, y].SetRune(boardData.Grid[x, upperY].CurrentRune);
                                boardData.Grid[x, upperY].ClearRune();

                                runeViews[x, y] = runeViews[x, upperY];
                                runeViews[x, upperY] = null;
                                runeViews[x, y].MoveToPosition(new Vector2(x * Spacing, y * Spacing));
                                break;
                            }
                        }
                    }
                }
            }
            for (int x = 0; x < Width; x++)
            {
                for (int y = Height - 1; y >= 0; y--)
                {
                    if (boardData.Grid[x, y].isEmpty())
                    {
                        SpawnRuneAt(x, y, false);
                    }
                }
            }
        }

        //rune effect
        public void ApplyRandomEffectToBasicRune()
        {
            List<CellData> validRunes = new List<CellData>();

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    CellData cell = boardData.GetCell(x, y);
                    if (cell != null && !cell.isEmpty())
                    {
                        RuneData rune = cell.CurrentRune;
                        if (rune.SpecialType == SpecialRuneType.None && rune.CurrentEffect == RuneEffect.None)
                        {
                            validRunes.Add(cell);
                        }
                    }
                }
            }

            if (validRunes.Count > 0)
            {
                CellData targetCell = validRunes[Random.Range(0, validRunes.Count)];

                // 1. Burn, 2. Poison, 3. Freeze
                int randomEffect = Random.Range(1, 4);
                GameObject prefabToSpawn = null;
                switch (randomEffect)
                {
                    case 1:
                        targetCell.CurrentRune.CurrentEffect = RuneEffect.Burn;
                        prefabToSpawn = burnEffectPrefab;
                        Debug.Log($"Burn in {targetCell.X}, {targetCell.Y}");
                        break;
                    case 2:
                        targetCell.CurrentRune.CurrentEffect = RuneEffect.Poison;
                        prefabToSpawn = poisonEffectPrefab;
                        Debug.Log($"Poison in {targetCell.X}, {targetCell.Y}");
                        break;
                    case 3:
                        targetCell.CurrentRune.CurrentEffect = RuneEffect.Frozen;
                        targetCell.CurrentRune.effectStacks = 1;
                        prefabToSpawn = freezeEffectPrefab;
                        Debug.Log($"Freeze in {targetCell.X}, {targetCell.Y}");
                        break;
                }

                if (runeViews[targetCell.X, targetCell.Y] != null && prefabToSpawn != null)
                {
                    runeViews[targetCell.X, targetCell.Y].ApplyEffectVisual(prefabToSpawn, targetCell.CurrentRune.effectStacks);
                }
            }
            else
            {
                Debug.Log("No more Basic Rune");
            }
        }

        //destroy effect rune
        private void ProcessRuneDestruction(CellData cell)
        {
            if (cell == null || cell.CurrentRune == null) return;
            RuneData rune = cell.CurrentRune;

            //Shattle Freeze
            if (rune.CurrentEffect == RuneEffect.Frozen && rune.effectStacks > 0)
            {
                rune.effectStacks--;

                if (runeViews[cell.X, cell.Y] != null)
                {
                    runeViews[cell.X, cell.Y].UpdateStackText(rune.effectStacks);
                }

                if (rune.effectStacks <= 0)
                {
                    rune.CurrentEffect = RuneEffect.None;
                    if (runeViews[cell.X, cell.Y] != null)
                    {
                        runeViews[cell.X, cell.Y].ClearEffectVisual();
                    }
                }
                return;
            }
            if (rune.CurrentEffect == RuneEffect.Burn || rune.CurrentEffect == RuneEffect.Poison || rune.CurrentEffect == RuneEffect.PoisonSpread)
            {
                if (combatManager != null)
                {
                    combatManager.ApplyRuneNegativeEffect(rune.CurrentEffect);
                }

            }

            cell.ClearRune();
            if (runeViews[cell.X, cell.Y] != null)
            {
                Destroy(runeViews[cell.X, cell.Y].gameObject);
                runeViews[cell.X, cell.Y] = null;
            }
        }

    }
}
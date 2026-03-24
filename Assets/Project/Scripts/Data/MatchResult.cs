using System.Collections.Generic;

namespace Match3Game.Data
{
    [System.Serializable]
    public class MatchResult
    {
        public List<CellData> MatchedCells; //List cell trong to hop
        public MatchType TypeOfMatch;
        public RuneType MatchedRuneType;

        public SpecialRuneType ResultingSpecialRune;
        public CellData SpawnCell;

        public MatchResult()
        {
            MatchedCells = new List<CellData>();
            TypeOfMatch = MatchType.None;
            MatchedRuneType = RuneType.None;
        }

        //add cell vao to hop
        public void AddCell(CellData cell)
        {
            if (!MatchedCells.Contains(cell))
            {
                MatchedCells.Add(cell);
            }
        }
    }
}
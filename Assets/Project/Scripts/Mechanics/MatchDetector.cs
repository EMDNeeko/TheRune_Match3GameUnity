using Match3Game.Data;
using Match3Game.Board;
using System.Collections.Generic;
using System.Linq;

namespace Match3Game.Mechanics
{
    public class MatchDetector
    {
        private BoardData board;
        public MatchDetector(BoardData currentBoard)
        {
            board = currentBoard;
        }

        //call after swap or rune fall
        public List<MatchResult> FindAllMatches()
        {
            List<MatchResult> allMatches = new List<MatchResult>();

            //line col & row
            List<MatchResult> lineMatches = ScanLine();

            List<MatchResult> squareMatches = ScanSquare();
            allMatches = MergeIntersections(lineMatches);

            allMatches.AddRange(squareMatches);
            ClassifyMatches(allMatches);

            return allMatches;
        }

        private List<MatchResult> ScanLine()
        {
            List<MatchResult> results = new List<MatchResult>();

            //horizontal
            for (int y = 0; y < board.Height; y++)
            {
                for (int x = 0; x < board.Width - 2; x++)
                {
                    CellData cell1 = board.GetCell(x, y);
                    CellData cell2 = board.GetCell(x + 1, y);
                    CellData cell3 = board.GetCell(x + 2, y);

                    if (IsSameRune(cell1, cell2, cell3))
                    {
                        MatchResult match = new MatchResult();
                        match.MatchedRuneType = cell1.CurrentRune.BaseType;
                        match.AddCell(cell1);
                        match.AddCell(cell2);
                        match.AddCell(cell3);

                        //Continue expand left
                        int expandX = x + 3;
                        while (expandX < board.Width && IsSameRune(cell1, board.GetCell(expandX, y)))
                        {
                            match.AddCell(board.GetCell(expandX, y));
                            expandX++;
                        }
                        results.Add(match);
                        x = expandX - 1; // Skip scanned 
                    }
                }
            }

            //vertical
            for (int x = 0; x < board.Width; x++)
            {
                for (int y = 0; y < board.Height - 2; y++)
                {
                    CellData cell1 = board.GetCell(x, y);
                    CellData cell2 = board.GetCell(x, y + 1);
                    CellData cell3 = board.GetCell(x, y + 2);

                    if (IsSameRune(cell1, cell2, cell3))
                    {
                        MatchResult match = new MatchResult();
                        match.MatchedRuneType = cell1.CurrentRune.BaseType;
                        match.AddCell(cell1);
                        match.AddCell(cell2);
                        match.AddCell(cell3);

                        //Continue expand bottom
                        int expandY = y + 3;
                        while (expandY < board.Height && IsSameRune(cell1, board.GetCell(x, expandY)))
                        {
                            match.AddCell(board.GetCell(x, expandY));
                            expandY++;
                        }
                        results.Add(match);
                        y = expandY - 1; // Skip scanned 
                    }
                }
            }
            return results;
        }

        private List<MatchResult> ScanSquare()
        {
            List<MatchResult> results = new List<MatchResult>();

            for (int x = 0; x < board.Width - 1; x++)
            {
                for (int y = 0; y < board.Height - 1; y++)
                {
                    CellData topLeft = board.GetCell(x, y);
                    CellData topRight = board.GetCell(x + 1, y);
                    CellData botLeft = board.GetCell(x, y + 1);
                    CellData botRight = board.GetCell(x + 1, y + 1);
                    if (IsSameRune(topLeft, topRight, botLeft, botRight))
                    {
                        MatchResult match = new MatchResult();
                        match.MatchedRuneType = topLeft.CurrentRune.BaseType;
                        match.TypeOfMatch = MatchType.Match4Square;
                        match.AddCell(topLeft);
                        match.AddCell(topRight);
                        match.AddCell(botLeft);
                        match.AddCell(botRight);
                        results.Add(match);
                    }
                }
            }
            return results;
        }

        private List<MatchResult> MergeIntersections(List<MatchResult> lineMatches)
        {
            List<MatchResult> mergedMatches = new List<MatchResult>();
            List<MatchResult> handled = new List<MatchResult>();

            for (int i = 0; i < lineMatches.Count; i++)
            {
                if (handled.Contains(lineMatches[i])) continue;

                MatchResult current = lineMatches[i];
                bool hasMerged = false;

                for (int j = i + 1; j < lineMatches.Count; j++)
                {
                    if (handled.Contains(lineMatches[j])) continue;

                    //same rune, having at least 1 cell
                    if (current.MatchedRuneType == lineMatches[j].MatchedRuneType)
                    {
                        bool sharesCell = false;
                        foreach (var cellA in current.MatchedCells)
                        {
                            foreach (var cellB in lineMatches[j].MatchedCells)
                            {
                                if (cellA == cellB)
                                {
                                    sharesCell = true;
                                    break;
                                }
                            }
                            if (sharesCell) break;
                        }

                        if (sharesCell)
                        {
                            //Gop j vao current
                            foreach (var cell in lineMatches[j].MatchedCells)
                            {
                                current.AddCell(cell);
                                handled.Add(lineMatches[j]);
                                hasMerged = true;
                            }
                        }
                    }
                }
                mergedMatches.Add(current);
            }
            return mergedMatches;
        }

        private void ClassifyMatches(List<MatchResult> matches)
        {
            foreach (var match in matches)
            {
                int count = match.MatchedCells.Count;

                if (match.TypeOfMatch == MatchType.Match4Square)
                {
                    match.ResultingSpecialRune = SpecialRuneType.Meteor;
                    continue;
                }
                if (count == 3)
                {
                    match.TypeOfMatch = MatchType.Match3Line;
                    match.ResultingSpecialRune = SpecialRuneType.None;
                }
                else if (count == 4)
                {
                    match.TypeOfMatch = MatchType.Match4Line;
                    match.ResultingSpecialRune = SpecialRuneType.LineBlast;
                }
                else if (count >= 5)
                {
                    bool isStraightLine = CheckIfStraightLine(match.MatchedCells);

                    if (isStraightLine)
                    {
                        match.TypeOfMatch = MatchType.Match5Line;
                        match.ResultingSpecialRune = SpecialRuneType.Rainbow;
                    }
                    else
                    {
                        match.TypeOfMatch = MatchType.Match5Cross;
                        match.ResultingSpecialRune = SpecialRuneType.Bomb;
                    }
                }
            }
        }

        //check if all matched rune in a line
        private bool CheckIfStraightLine(List<CellData> cells)
        {
            if (cells.Count <= 1) return true;
            bool sameX = true;
            bool sameY = true;

            int firstX = cells[0].X;
            int firstY = cells[0].Y;

            foreach (var cell in cells)
            {
                if (cell.X != firstX) sameX = false;
                if (cell.Y != firstY) sameY = false;
            }

            return sameX || sameY;
        }
        //check if same rune
        private bool IsSameRune(params CellData[] cells)
        {
            if (cells == null || cells.Length == 0 || cells[0].isEmpty())
            {
                return false;
            }
            RuneType targetType = cells[0].CurrentRune.BaseType;

            foreach (var cell in cells)
            {
                if (cell.isEmpty() || cell.CurrentRune.BaseType != targetType)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
using Match3Game.Data;

namespace Match3Game.Board
{
    public class BoardData
    {
        public int Width = 10;
        public int Height = 10;
        public CellData[,] Grid;
        public BoardState CurrentState;

        public void initBoard()
        {
            Grid = new CellData[Width, Height];
            CurrentState = BoardState.Idle;

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    Grid[x, y] = new CellData(x, y);
                }
            }
        }

        //get CellData
        public CellData GetCell(int x, int y)
        {
            if (x >= 0 && x < Width && y >= 0 && y < Height)
            {
                return Grid[x, y];
            }
            return null;
        }

        // Swap 2 rune
        public void SwapRunes(CellData cellA, CellData cellB)
        {
            if (cellA.CurrentRune != null && cellA.CurrentRune.isMovable() && cellB.CurrentRune != null && cellB.CurrentRune.isMovable())
            {
                RuneData tmp = cellA.CurrentRune;
                cellA.SetRune(cellB.CurrentRune);
                cellB.SetRune(tmp);

                CurrentState = BoardState.Executing;
            }
        }
    }
}
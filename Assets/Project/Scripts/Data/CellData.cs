using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Match3Game.Data;

namespace Match3Game.Data
{
    [System.Serializable]
    public class CellData
    {
        public int X;
        public int Y;
        public RuneData CurrentRune; // can be rune if no rune here
        public CellData(int x, int y)
        {
            X = x;
            Y = y;
            CurrentRune = null;
        }

        public bool isEmpty()
        {
            return CurrentRune == null;
        }
        public void SetRune(RuneData newRune)
        {
            CurrentRune = newRune;
        }
        public void ClearRune()
        {
            CurrentRune = null;
        }
    }
}
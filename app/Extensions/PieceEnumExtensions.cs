using System;
using System.Diagnostics;
using app.Models;

namespace app.Services
{
    public static class PieceEnumExtensions
    {
        public static char ToChar(this PieceEnum e)
        {
            return e switch
            {
                PieceEnum.Bishop => 'B',
                PieceEnum.King => 'K',
                PieceEnum.Knight => 'N',
                PieceEnum.Pawn => 'P',
                PieceEnum.Queen => 'Q',
                PieceEnum.Rook => 'R',
                _ => throw new ArgumentOutOfRangeException(nameof(e), e, null)
            };
        }
    }
}
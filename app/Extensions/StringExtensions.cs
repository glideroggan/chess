using System;
using System.Text.RegularExpressions;
using app.Models;

namespace app.Services
{
    public static class StringExtensions
    {
        public static ColorEnum GetColor(this char str) => str.IsUpper() ? ColorEnum.White : ColorEnum.Black;

        public static Position ToPos(this string str) =>
            new Position(int.Parse(str[0].ToString()), int.Parse(str[1].ToString()));

        public static PieceEnum ToEnum(this char chr) => chr switch
        {
            'q' => PieceEnum.Queen,
            'Q' => PieceEnum.Queen,
            'k' => PieceEnum.King,
            'K' => PieceEnum.King,
            'r' => PieceEnum.Rook,
            'R' => PieceEnum.Rook,
            'b' => PieceEnum.Bishop,
            'B' => PieceEnum.Bishop,
            'n' => PieceEnum.Knight,
            'N' => PieceEnum.Knight,
            'p' => PieceEnum.Pawn,
            'P' => PieceEnum.Pawn,
        };
    }
}
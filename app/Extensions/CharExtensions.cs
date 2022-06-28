using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text.RegularExpressions;
using app.Models;

namespace app.Services
{
    public static class CharExtensions
    {
        public static bool IsAlpha(this char chr)
        {
            return Regex.IsMatch(chr.ToString(), "[1-8]");
        }

        public static bool IsUpper(this char c) => char.IsUpper(c);
        public static bool IsLower(this char c) => char.IsLower(c);

        private static List<Position> QueenOffsets = new()
        {
            new(-1, 0), new(-1, -1), new(0, -1), new(1, -1), new(1, 0),
            new(1, 1), new(0, 1), new(-1, 1)
        };
        public static ColorEnum ToColorFromTurn(this char c)
        {
            return c switch
            {
                'w' => ColorEnum.White,
                'b' => ColorEnum.Black,
            };
        }
        
        public static ColorEnum ToColorFromPiece(this char c)
        {
            return c switch
            {
                var a when char.IsUpper(a) => ColorEnum.White,
                _ => ColorEnum.Black
            };
        }
    }
}
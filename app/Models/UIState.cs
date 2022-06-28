using System.Collections.Generic;

namespace app.Models
{
    public enum ColorEnum {White, Black}
    public enum PieceEnum
    {
        Pawn, Rook, Knight, Bishop, King, Queen
    }
    public class UIState
    {
        public Position StartPlace { get; set; }
        public List<Move> LegalMoves { get; set; } = new();
    }
}
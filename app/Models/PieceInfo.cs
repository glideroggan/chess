namespace app.Models
{
    public record PieceInfo(char Piece, Position Pos)
    {
        // public void Deconstruct(out char piece, out Position pos)
        // {
        //     piece = Piece;
        //     pos = Pos;
        // }
    }
}
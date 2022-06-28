namespace app.Models
{
    public record Position(int Col, int Row)
    {
        private static readonly Position empty = new(0, 0);
        public static Position operator +(Position a, Position b)
        {
            return new(a.Col + b.Col, a.Row + b.Row);
        }

        public static Position Empty => empty;
    }
}
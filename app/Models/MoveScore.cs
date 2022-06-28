namespace app.Models
{
    public class MoveScore
    {
        public int Score;
        public Move Move { get; set; }
        public MoveScore NextMove { get; set; }
        public int TotalScore { get; set; }
    }
}
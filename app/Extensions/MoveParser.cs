using app.Models;

namespace app.Services
{
    public static class MoveParser
    {
        public static Move Parse(string moveStr)
        {
            var splits = moveStr.Split(' ');
            var from = new Position(
                int.Parse(splits[0][0].ToString()), int.Parse(splits[0][1].ToString()));
            var to = new Position(
                int.Parse(splits[1][0].ToString()), int.Parse(splits[1][1].ToString()));
            return new Move(from, to);
        }
    }
}
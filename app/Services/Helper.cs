using app.Models;

namespace app.Services
{
    public static class Helper
    {
        public static bool OutOfBounds(Position pos) => pos.Col < 1 || pos.Col > 8 || pos.Row < 1 || pos.Row > 8;
    }
}
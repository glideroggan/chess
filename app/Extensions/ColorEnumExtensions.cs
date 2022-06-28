using app.Models;

namespace app.Services
{
    public static class ColorEnumExtensions
    {
        public static char ToKingChar(this ColorEnum color) => color == ColorEnum.Black ? 'k' : 'K';
    }
}
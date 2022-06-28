using System.Text;

namespace app.Services
{
    public static class StringBuilderExtensions
    {
        public static int IndexOf(this StringBuilder builder, char chr)
        {
            var res = -1;
            for (var i = 0; i < builder.Length; i++)
            {
                if (builder[i] == chr) return i;
            }

            return res;
        }
    }
}
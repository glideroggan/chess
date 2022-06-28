using app.Models;
using Microsoft.Extensions.Logging;

namespace app.Services
{
    public static class MoveScoreExtensions
    {
        public static void LogThinking(this MoveScore score, ILogger logger)
        {
            var current = score;
            while (current != null)
            {
                logger.Log(LogLevel.Warning, $"{current.Move.From} -> {current.Move.To} S[{score.Score}] T[{score.TotalScore}]");
                current = current.NextMove;
            }
        }
    }
}
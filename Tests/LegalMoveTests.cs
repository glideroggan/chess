using app.Models;
using app.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Tests
{
    public class LegalMoveTests
    {
        private MoveService _target;

        public LegalMoveTests()
        {
            var perfService = new Mock<IPerfService>();
            var cacher = new Cacher(new NullLogger<Cacher>(), perfService.Object);
            _target = new MoveService(cacher, new NullLogger<MoveService>(), perfService.Object);
        }

        [Theory]
        [InlineData("rnbqkbnr/ppppp1pp/8/4Pp/8/8/PPP2PPP/RNBQKBNR w", "54 63", "62 64")]
        [InlineData("k/8/8/8/3Pp/8/8/K w", "55 46", "47 45")]
        public void PassantShouldBeLegal(string startState, string moveStr, string previousMoveStr)
        {
            var move = MoveParser.Parse(moveStr);
            var previousMove = MoveParser.Parse(previousMoveStr);
            var state = new BoardState(startState);
            _target._moveHistory.Add(previousMove);
            
            var actual = _target.GetLegalMoves(state, move.From);

            Assert.Contains(actual, x => x.From == move.From && x.To == move.To);
        }

        [Theory]
        [InlineData("K/7k/8/8/8/8/8/P w", "18", 1)] // pawn white  
        [InlineData("K/7k/8/8/8/8/P/P w", "18", 0)]
        [InlineData("K/7k/8/8/8/8/P/8 w", "17", 2)]
        [InlineData("K/7k/8/8/8/8/p/P w", "18", 0)]
        [InlineData("K/7k/8/8/8/8/pp/P w", "18", 1)]
        [InlineData("K/7k/8/8/8/8/p1p/1P w", "28", 3)]
        [InlineData("K/7k/8/8/8/p1p/1P/8 w", "27", 4)]
        [InlineData("8/7k/8/8/8/8/8/K w", "18", 3)] // king white
        [InlineData("8/7k/8/8/8/8/P/K w", "18", 2)] // error
        [InlineData("8/7k/8/8/8/8/K/8 w", "17", 5)]
        [InlineData("8/7k/8/8/8/8/p/K w", "18", 2)]
        [InlineData("8/7k/8/8/8/8/pp/K w", "18", 2)]
        [InlineData("8/7k/8/8/8/8/p1p/1K w", "28", 5)]
        [InlineData("8/7k/8/8/8/p1p/1K/8 w", "27", 8)]
        [InlineData("K/7k/8/8/8/8/8/R w", "18", 13)] // rook white
        [InlineData("K/7k/8/8/8/8/P/R w", "18", 7)]
        [InlineData("K/7k/8/8/8/8/R/8 w", "17", 13)]
        [InlineData("K/7k/8/8/8/8/p/R w", "18", 8)]
        [InlineData("K/7k/8/8/8/8/pp/R w", "18", 8)]
        [InlineData("K/7k/8/8/8/8/p1p/1R w", "28", 14)]
        [InlineData("K/7k/8/8/8/p1p/1R/8 w", "27", 14)]
        [InlineData("K/7k/8/8/8/8/8/Q w", "18", 20)] // queen white
        [InlineData("K/7k/8/8/8/8/P/Q w", "18", 14)]
        [InlineData("K/7k/8/8/8/8/Q/8 w", "17", 20)]
        [InlineData("K/7k/8/8/8/8/p/Q w", "18", 15)]
        [InlineData("K/7k/8/8/8/8/pp/Q w", "18", 9)]
        [InlineData("K/7k/8/8/8/8/p1p/1Q w", "28", 16)]
        [InlineData("K/7k/8/8/8/p1p/1Q/8 w", "27", 18)]
        [InlineData("K/7k/8/8/8/8/8/B w", "18", 7)] // bishop white
        [InlineData("K/7k/8/8/8/8/P/B w", "18", 7)]
        [InlineData("K/7k/8/8/8/8/B/8 w", "17", 7)]
        [InlineData("K/7k/8/8/8/8/p/B w", "18", 7)]
        [InlineData("K/7k/8/8/8/8/pp/B w", "18", 1)]
        [InlineData("K/7k/8/8/8/8/p1p/1B w", "28", 2)]
        [InlineData("K/7k/8/8/8/p1p/1B/8 w", "27", 4)]
        [InlineData("K/7k/8/8/8/8/8/N w", "18", 2)] // knight white
        [InlineData("K/7k/8/8/8/8/P/N w", "18", 2)]
        [InlineData("K/7k/8/8/8/8/N/8 w", "17", 3)]
        [InlineData("K/7k/8/8/8/8/p/N w", "18", 2)]
        [InlineData("K/7k/8/8/8/8/pp/N w", "18", 2)]
        [InlineData("K/7k/8/8/8/8/p1p/1N w", "28", 3)]
        [InlineData("K/7k/8/8/8/p1p/1N/8 w", "27", 4)]
        [InlineData("R/2B/1P4Pr/4P/B2RP/2k1K/P6P/8 b", "36", 1)]
        [InlineData("8/p4ppp/2k/3n/4P1PP/Q2K1P/3q3r/8 w", "46", 1)] 
        
        
        [InlineData("k/p4p/8/1pP3P/8/8/8/K w", "74", 1)]
        
        [InlineData("1rbq1b1r/pppp3p/3kp/6NQ/1P2R/P1N4B/2P/3RK b", "12", 0)] // chess
        [InlineData("1rbq1b1r/pppp3p/3kp/6NQ/1P2R/P1N4B/2P/3RK b", "43", 2)] // chess
        public void FilterOutIllegalMoves(string startState, string fromStr, int expected)
        {
            var state = new BoardState(startState);
            var actual = _target.GetLegalMoves(state, fromStr.ToPos());

            Assert.Equal(expected, actual.Count);
        }
    }
}
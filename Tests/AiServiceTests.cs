using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using app.Models;
using app.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;
using static app.Models.ColorEnum;

namespace Tests
{
    public class AiServiceTests
    {
        private AiService _target;
        private Cacher _cacher;

        public AiServiceTests()
        {
            var perfService = new Mock<IPerfService>();
            _cacher = new Cacher(new NullLogger<Cacher>(), perfService.Object);
            var moveService = new MoveService(_cacher, new NullLogger<MoveService>(), perfService.Object);
            var manager = new Manager(moveService, _cacher, new NullLogger<Manager>(),
                perfService.Object);

            _target = new AiService(moveService, _cacher, new NullLogger<AiService>(),
                manager, perfService.Object);
        }

        // [Theory]
        // [InlineData("p6k/1P/8/8/8/8/8/6K b", 1, 2)]
        // [InlineData("r1bqkbnr/pppppppp/2n/3P/4P/2P/PP3PPP/RNBQKBNR b", 1, 0)]
        // [InlineData("8/1k1K/8/1Q/1p/p/8/8 b", 2,  -2)]
        
        [InlineData("1rbqkbnr/pppppppp/2n/8/3PP/2P/PP3PPP/RNBQKBNR b", 2, 0)]
        public async Task GetScoreTest(string startingState, int maxDepth, int expected)
        {
            var state = new BoardState(startingState);
            var scores = await _target.InternalCalculateMove(state, maxDepth);

            var actual = scores.Moves.OrderByDescending(x => x.Score).First();
            
            Assert.Equal(expected, actual.Score);
        }

        // [Theory]
        // [InlineData("r1bqkbnr/pppppppp/2n/3P/4P/2P/PP3PPP/RNBQKBNR b", 1, "33")]
        // public async Task CalculateMove(string startState, int maxDepth, string expectedStr)
        // {
        //     _cacher.Clear();
        //     var state = new BoardState(startState);
        //     var moves = await _target.InternalCalculateMove(state, maxDepth);
        //
        //     var ordered = moves.Moves.OrderByDescending(x => x.Score);
        //
        //     var actual = ordered.Select(x => x.Move).First();
        //
        //     Assert.Equal(expectedStr.ToPos(), actual.From);
        // }

        // [Fact]
        // public async Task CalculateMove(string startState)
        // {
        //     var perfService = new Mock<IPerfService>();
        //     var cacher = new Mock<ICacher>();
        //     var moveService = new MoveService(cacher.Object, new NullLogger<MoveService>(), perfService.Object);
        //     var state = new BoardState(startState);
        //         
        //     var target = new AiService(cacher.Object, new NullLogger<AiService>(),
        //         moveService, perfService.Object);
        //     
        //     var list = new List<AiService.MoveScore>
        //     {
        //         new AiService.MoveScore {Move = new Move()}
        //     }
        //     target.ChooseStrategy(list)
        // }

        // public async Task GetScoresDepth(string startState)
        // {
        //     var state = new BoardState(startState);
        //     var actual = _target.GetScores(state, 0, 0, () => false, new Dictionary<int, int>(), null);
        // }
        //
        // // [Theory]
        // // [InlineData("rn1q1b1r/p1p1kppp/7n/1p2Q/3pP1P/3P3P/PPP2P/RNB1KBNR b")]
        // // [InlineData("1rbqkbnr/p1pppppp/1pn/1N/3PP1Q/2P/PP3PPP/R1B1KBNR b")]
        // [InlineData("2b1k/1p1p2R/1B/8/2P/1P/3K/3R3r b", "88 87", 2)]
        // public async Task InternalCalculateMove(string startState, string expectedMoveStr, int maxDepth)
        // {
        //     var state = new BoardState(startState);
        //     var expected = MoveParser.Parse(expectedMoveStr);
        //
        //     var info = await _target.InternalCalculateMove(state, maxDepth);
        //     var actual = _target.ChooseStrategy(info);
        //
        //     Assert.Equal(expected.To, actual.Move.To);
        // }
        //
        // // [Theory]
        // [InlineData("P6K/8/8/q/8/8/8/7k b", 0, 2)]      // queen favors the pawn
        // [InlineData("P2R3K/8/8/q/8/8/8/7k b", 0, 8)] // queen favors the rook
        // [InlineData("R2R3K/8/8/q/8/8/8/7k b", 1, 0)]    // queen favors to run away 
        // [InlineData("2b1k/1p1p2R/1B/8/2P/1P/3K/3R3r b", 1, 0)]
        // // [InlineData("2b1k/1p1p2R/1B/8/2P/1P/3K/3R3r b", 2, 0)] // investigate for (no favor?) b -> 88 78
        // public async Task GetScoresPoints(string startState, int depth, int expected)
        // {
        //     var state = new BoardState(startState);
        //     var scores = await _target.GetScores(state, 0, depth,
        //         () => false, new Dictionary<int, int>(), null);
        //     
        //     Assert.Equal(expected, scores.Max(x => x.Score));
        // }
        //
        // [Theory]
        // // [InlineData("k/p/8/8/8/8/P/K b", 0, 1)]
        // // [InlineData("k/p/8/8/8/8/P/K b", 1, 5)]
        // // [InlineData("k/p/8/8/8/8/P/K b", 2, 21)]
        // // [InlineData("k/p/8/8/8/8/P/K b", 3, 112)]
        // [InlineData("rn1q1bnr/pbp1pppp/1pk/1B/1P2PQ/2N/P1PP2PP/R1B1K1NR b", 3, 2)]
        // public void GetScoresRecursions(string startState, int depth, int expected)
        // {
        //     var state = new BoardState(startState);
        //     var scores = _target.GetScores(state, depth, 0, 
        //         () => false, new Dictionary<int, int>(), null);
        //     
        //     // perfService.Verify(mock => 
        //     //     mock.TimeStart(It.Is<string>(x => x == "GetScores")), Times.Exactly(expected));
        // }
    }
}
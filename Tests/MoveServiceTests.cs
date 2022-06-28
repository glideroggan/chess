using System.Collections.Generic;
using app.Models;
using app.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Tests
{
    public class MoveServiceTests
    {
        private MoveService _target;
        private Cacher _cacher;

        public MoveServiceTests()
        {
            var perfService = new Mock<IPerfService>();
            _cacher = new Cacher(new NullLogger<Cacher>(), perfService.Object);
            _target = new MoveService(_cacher, new NullLogger<MoveService>(), perfService.Object);
        }

        [Theory]
        [InlineData("1rbqkbnr/ppp1ppp/3P/nB2PN/5PQP/2P/PP5P/RNB2RK b", "51", true)]
        [InlineData("1rbqkbnr/pp2ppp/3p/nB2PN/5PQP/2P/PP5P/RNB2RK b", "51", true)]
        [InlineData("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w", "51", false)]
        public void CanBeCaptured(string startState, string posStr, bool expected)
        {
            var state = new BoardState(startState);
            var actual = _target.CanBeCaptured(state, posStr.ToPos());
            
            Assert.Equal(expected, actual);
        }

        // [Theory]
        // [InlineData("kn/p1K/P3p/7p/8/5b/q/8 b", "32", false)]
        // [InlineData("8/p4ppp/2k/3n/4P1PP/Q2K1P/3q3r/8 w", "46", true)]
        // public void CanBeCaptured(string startState, string posStr, bool expected)
        // {
        //     var state = new BoardState("k/p1K/P1n1p/7p/8/5b/q/8 b");
        //     state = _target.Move(state, new(3, 3), new(2, 1));
        //     state = _target.Move(state, new(3, 2), new(3, 1));
        //     state = _target.Move(state, new(2, 1), new(3, 3));
        //     state = _target.Move(state, new(3, 1), new(3, 2));
        //     state = _target.Move(state, new(3, 3), new(2, 1));
        //     var actual = _target.CanBeCaptured(state, posStr.ToPos(), ColorEnum.White);
        //     
        //     Assert.Equal(expected, actual);
        // }

        [Theory]
        [InlineData("kQ/RR/8/8/8/8/8/7K b", ColorEnum.Black, true)]
        [InlineData("kn3R/8/Q/8/8/8/8/7K b", ColorEnum.Black, true)]
        [InlineData("3r/pp/k4Q/pp/8/8/8/7K b", ColorEnum.Black, false)]
        [InlineData("3b1q1q/1N2PRQ1/rR3KBr/B4PP1/2Pk1r1b/1P2P1N1/2P2P2/8 b", ColorEnum.Black, true)]
        [InlineData("kn/p1K/P3p/7p/8/5b/q/8 w", ColorEnum.White, false)]
        public void CheckChessMateTests(string startState, ColorEnum color, bool expected)
        {
            var state = new BoardState(startState);
            var actual = _target.CheckChessMate(state, color);
            
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("1n3rk/NpQ1pppp/8/8/8/8/8/R3K2R w", "32 21", false)]
        [InlineData("rnbqkbnr/1ppppppp/8/pP/8/8/8/RNBQKBNR w", "24 13", true)]
        [InlineData("rnbqkbnr/ppppp1pp/8/4Pp/8/8/PPP2PPP/RNBQKBNR w", "54 63", true)]
        // [InlineData("rnbqkbnr/pppppppp/8/8/8/4PP/8/RNBQKBNR w", "24 13", true)]
        public void IsPassant(string startState, string moveStr, bool expected)
        {
            var move = MoveParser.Parse(moveStr);
            var state = new BoardState(startState);
            
            var actual = _target.IsPassant(state, move.From, move.To);
            
            Assert.Equal(expected, actual);
        }
        
        [Theory]
        [InlineData("k/8/8/8/PP/8/8/K w", "15 14", false)]
        public void IsPassant_FALSE(string startState, string moveStr, bool expected)
        {
            var move = MoveParser.Parse(moveStr);
            var state = new BoardState(startState);
            
            var actual = _target.IsPassant(state, move.From, move.To);
            
            Assert.Equal(expected, actual);
        }

        
        
        // [Theory]
        // [InlineData("r1bqkbnr/ppppppp/2n4p/8/4P/5N/pppp1ppp/RNBQKB1R w", "68 57",
        //     "r1bqkbnr/ppppppp/2n4p/8/4P/5N/ppppBppp/RNBQK2R b")]
        // public void ValidMove(string startState, string move, string expected)
        // {
        //     var state = new BoardState(startState);
        //
        //     var p = MoveParser.Parse(move);
        //     var actual = _target.InternalMove(p.From, p.To, state);
        //     
        //     Assert.Equal(expected, actual.Fen);
        // }
        //
        //
        // [Theory]
        // [InlineData("k/p/8/8/8/8/P/K b", "12", 'p', 2)]
        // [InlineData("k/p/8/8/8/8/P/K b", "11", 'k', 2)]
        //
        // public void ValidMoves(string startState, string posStr, char p, int expected)
        // {
        //     var state = new BoardState(startState);
        //
        //     var actual = _target.GetValidMoves(state, new PieceInfo(p, posStr.ToPos()));
        //
        //     Assert.Equal(expected, actual.Count);
        // }
        //
        // [Theory]
        // [InlineData("k/8/8/8/8/8/Q/K b", true)]
        // public void IsChess(string startState, bool expected)
        // {
        //     var state = new BoardState(startState);
        //     var actual = _target.IsChess(state);
        //     
        //     Assert.Equal(expected, actual);
        // }
        //
        //
        // [Theory]
        // [InlineData("r1b1kb1R/pppp1pp/n/4p1B/4P/3P1p/PPP/RN1QKBN w", false)]
        // public void IsChessMate(string startState, bool expected)
        // {
        //     var state = new BoardState(startState);
        //
        //     var actual = _target.IsChessMate(state, ColorEnum.White);
        //     
        //     Assert.Equal(expected, actual);
        //
        // }
        
    }
}
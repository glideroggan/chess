using System.Linq;
using app.Models;
using app.Services;
using Moq;
using Xunit;

namespace Tests
{
    public class BoardStateTests
    {
        [Theory]
        [InlineData("8/8/8/8/8/8/8/8 w", "rnbqkbnrppppppppPPPPPPPPRNBQKBNR")]
        [InlineData("q/8/8/8/8/8/8/8 w", "rnbkbnrppppppppPPPPPPPPRNBQKBNR")]
        [InlineData("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNB1KBNR w", "Q")]
        [InlineData("2P/p3pk1p/6p/4B/r2N1Kn/5b/P/3q w", "bnrppppPPPPPPRQBNR")]
        public void CaptureTests(string startState, string expected)
        {
            var state = new BoardState(startState);
            var actual = string.Concat(state.Captures);
            
            Assert.Equal(expected, actual);
        }
        // TODO: test promotions

        [Theory]
        [InlineData("8/8/8/8/8/8/8/8 w", 'w', 0)]
        [InlineData("8/8/8/8/8/8/8/8 w", 'b', 0)]
        [InlineData("pp/8/8/8/8/8/8/PP w", 'w', 2)]
        [InlineData("pp/8/8/8/8/8/8/PP w", 'b', 2)]
        public void GetPieces(string startState, char who, int expected)
        {
            var state = new BoardState(startState);
            var actual = state.GetPieces(who.ToColorFromTurn());
            
            Assert.Equal(expected, actual.Count);
        }
        
        [Theory]
        [InlineData(
            "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w",
            "12 13", // down 1
            "rnbqkbnr/1ppppppp/p/8/8/8/PPPPPPPP/RNBQKBNR b")]
        [InlineData(
            "rnbqkbnr/p/8/8/8/8/PPPPPPPP/RNBQKBNR w",
            "12 22", // right 1
            "rnbqkbnr/1p/8/8/8/8/PPPPPPPP/RNBQKBNR b")]
        [InlineData(
            "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w",
            "11 18", // full down
            "1nbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/rNBQKBNR b")]
        public void InternalMove(string startState, string moveStr, string expectedState)
        {
            var target = new BoardState(startState);

            var move = MoveParser.Parse(moveStr);
            var actual = target.Move(move.From, move.To);
            
            Assert.Equal(expectedState, actual.Fen);
        }

        [Theory]
        [InlineData("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w",
            'R', "18")]
        [InlineData("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w",
            'Q', "48")]
        [InlineData("rnbqk/2pppppp/8/8/8/8/PPPP1KPP/RNBQ1BNR w",
            'K', "67")]
        public void GetPosition(string startState, char piece, string expectedStr)
        {
            var target = new BoardState(startState);
            var actual = target.GetPosition(piece);
            var expected = expectedStr.ToPos();
            
            Assert.Equal(expected, actual);

        }

        [Theory]
        [InlineData("1p"      , "_p______")]
        [InlineData("pppppppp", "pppppppp")]
        [InlineData("ppppppp" , "ppppppp_")]
        [InlineData("1ppppppp", "_ppppppp")]
        [InlineData("2pppppp" , "__pppppp")]
        [InlineData("2p3p"    , "__p___p_")]
        public void Parse(string state, string expected)
        {
            var startState = "r1bqkbnr/pppppppp/2n/3P/4P/2P/PP3PPP/RNBQKBNR b";
            var target = new BoardState(startState);
            var actual = target.Expand(state);
            
            Assert.Equal(expected, actual);
        }
        
        [Theory]
        [InlineData("___p____", "3p")]
        [InlineData("_K_p____", "1K1p")]
        [InlineData("_ppppppp", "1ppppppp")]
        public void Serialize(string line, string expected)
        {
            var startState = "r1bqkbnr/pppppppp/2n/3P/4P/2P/PP3PPP/RNBQKBNR b";
            var target = new BoardState(startState);
            var actual = target.Serialize(line);
            
            Assert.Equal(expected, actual);
        }
    }
}
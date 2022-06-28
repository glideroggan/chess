using app.Models;
using app.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Tests
{
    public class ManagerTests
    {
        private Manager _target;
        private Mock<IMoveService> _moveService;

        public ManagerTests()
        {
            _moveService = new Mock<IMoveService>();
            var cacher = new Mock<ICacher>();
            var perfService = new Mock<IPerfService>();
            _target = new Manager(_moveService.Object,
                cacher.Object, new NullLogger<Manager>(), perfService.Object);
        }

        // [Theory]
        // [InlineData("8/8/8/8/8/8/8/8 w", ColorEnum.White, )]
        public void CheckStatus(string startState, ColorEnum color, string expected)
        {
            var state = new BoardState(startState);
            var actual = _target.CheckStatus(state, color);
            
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("P/8/8/8/8/8/8/8 w", ColorEnum.White)]
        [InlineData("P/8/8/8/8/8/8/8 b", ColorEnum.Black)]
        [InlineData("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w", ColorEnum.White)]
        [InlineData("rnbqkbnr/pppppppp/8/8/8/4P/PPPP1PPP/RNBQKBNR b", ColorEnum.Black)]
        public void WhosTurnTest(string startState, ColorEnum expected)
        {
            var state = new BoardState(startState);
            var actual = _target.InternalWhosTurn(state);
            
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("2P/p3pk1p/6p/4B/r2N1Kn/5b/P/3q w", 'Q')]
        public void GetCaptures(string startState, char expected)
        {
            var state = new BoardState(startState);
            var p = _target.GetCaptures(state, ColorEnum.White);

            Assert.Contains(p, (d) => d == expected);
        }

        [Theory]
        // The states are after the turn, as we then check for promotions
        [InlineData("P/8/8/8/8/8/8/8 b", "11")]
        [InlineData("1P/8/8/8/8/8/8/8 b", "21")]
        
        [InlineData("Rp3P/8/8/8/8/8/8/8 b", "61")]
        [InlineData("Rp3P/8/8/8/8/8/8/p w", "18")]
        [InlineData("R3P/8/8/8/8/8/8/p b", "51")]
        [InlineData("k6P/9/8/8/8/8/8/7K/ b", "81")]
        public void CanPromote(string startState, string expectedStr)
        {
            var expected = expectedStr.ToPos();
            var state = new BoardState(startState);
            var p = _target.CanPromote(state);
            
            Assert.Equal(expected, p?.Pos);
        }
        
        [Theory]
        // The states are after the turn, as we then check for promotions
        [InlineData("P/8/8/8/8/8/8/8 b", true)]
        [InlineData("p/8/8/8/8/8/8/8 w", false)]
        public void CanPromote_FALSE(string startState, bool expected)
        {
            var state = new BoardState(startState);
            var p = _target.CanPromote(state);
            
            Assert.Equal(expected, p != null);
        }
        
        // [Theory]
        [InlineData("8/8/8/8/8/8/8/1p w", "28", "27")]
        public void MoveTests(string startState, string fromStr, string toStr)
        {
            // TODO: this is more testing boardstate class, we should test specific details in the manager
            var state = new BoardState(startState);
            var actual = _target.Move(state, fromStr.ToPos(), toStr.ToPos());
            
            
        }
        
        // [Theory]
        // [InlineData("8/8/8/8/8/8/8/1p w", "18", "17")]
        // public void GetLegalMovesTests(string startState, string posStr, string validPosArray)
        // {
        //     
        //     var expected = validPosArray.Split(); 
        //     var state = new BoardState(startState);
        //     _moveService.Setup(x => x.GetLegalMoves(state, ))
        //     var actual = _target.GetLegalMoves(state, posStr.ToPos());
        //
        //     Assert.True(actual.Count == expected.Length);
        //     foreach (var move in actual)
        //     {
        //         
        //     }
        // }
    }
}
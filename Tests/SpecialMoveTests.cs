using app.Models;
using app.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;
using static app.Models.ColorEnum;
using static app.Models.PieceEnum;

namespace Tests
{
    public class SpecialMoveTests
    {
        private MoveService _target;

        public SpecialMoveTests()
        {
            var perfService = new Mock<IPerfService>();
            var cacher = new Cacher(new NullLogger<Cacher>(), perfService.Object);
            _target = new MoveService(cacher, new NullLogger<MoveService>(), perfService.Object);
        }

        // [Theory]
        [InlineData("1n3rk/NpQ1pppp/8/8/8/8/8/R3K2R w", "32 21", Pawn)]
        [InlineData("k/p4p/8/1pP3P/8/8/8/K w", "34", 2)]    // en passant, move to special
        [InlineData("k/p/8/1pP/8/8/8/K w", "34", 2)]
        public void EnPassant(string startingState, string moveStr, PieceEnum piece)
        {
            var move = MoveParser.Parse(moveStr);
            var state = new BoardState(startingState);
            _target._moveHistory.Add(new Move(new(2, 2), new(2, 4)));
            var moves = _target.GetLegalMoves(state, move.From);
            
            Assert.Contains(moves, x => x.To == move.To);
        }
        
        [Theory]
        [InlineData("k/8/8/8/8/8/8/R3K2R w", "58 78", "88", false)]
        [InlineData("k/8/8/8/8/8/8/R3K2R w", "58 38", "18", false)]
        [InlineData("r3k2r/8/8/8/8/8/8/R3K2R b", "51 71", "81", false)]
        [InlineData("r3k2r/8/8/8/8/8/8/R3K2R b", "51 31", "11", false )]
        [InlineData("rnbqkbnr/8/8/8/8/8/8/RNBQK2R w", "58 78", "88", false )]
        [InlineData("rnb1kbnr/8/8/q/8/8/8/RNBQK2R w", "58 78", "88", true )]
        public void CastleKing(string startingState, string moveStr, string rookPosStr, bool isNegative)
        {
            var move = MoveParser.Parse(moveStr);
            var state = new BoardState(startingState);
            var moves = _target.GetLegalMoves(state, move.From);


            if (isNegative)
            {
                Assert.DoesNotContain(moves, x => x.To == move.To);
            }
            else
            {
                Assert.Contains(moves, x => x.To == move.To);
            }
            
            Assert.True(state[rookPosStr.ToPos()].Value.ToEnum() == Rook);
        }
    }
}
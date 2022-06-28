using app.Models;
using app.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Tests
{
    
    public class ChessTests
    {
        
        // [Theory]
        // public void ValidMoves()
        // {
        //     var target = 
        // }
        // [Fact]
        // public void BlackInChess()
        // {
        //     var logger = new Mock<ILogger<MoveService>>();
        //     
        //     var fenService = new FenService(new NullLogger<FenService>());
        //     var startState = "3k/3r/8/8/8/8/8/3RK b";
        //     var service = new MoveService(fenService, new NullLogger<MoveService>());
        //     service.SetStartState = startState;
        //
        //     var res = service.Move(new(4, 2), new(5, 2));
        //     
        //     Assert.False(res.Ok);
        // }
        //
        // [Fact]
        // public void WhiteInChess()
        // {
        //     var logger = new Mock<ILogger<MoveService>>();
        //     var fenService = new FenService(new NullLogger<FenService>());
        //     var startState = "3K/3R/8/8/8/8/8/3rk w";
        //     var service = new MoveService(fenService, new NullLogger<MoveService>());
        //     service.SetStartState = startState;
        //
        //     var res = service.Move(new(4, 2), new(5, 2));
        //
        //     Assert.False(res.Ok);
        // }
        //
        // [Fact]
        // public void WhiteInChessMate()
        // {
        //     var logger = new Mock<ILogger<MoveService>>();
        //     var fenService = new FenService(new NullLogger<FenService>());
        //     var startState = "7r/2b/8/1K/r3q/1n/3k2b/8 b";
        //     var service = new MoveService(fenService, new NullLogger<MoveService>());
        //     service.SetStartState = startState;
        //     
        //     var res = service.Move(new(8, 1), new(7, 1));
        //     var actual = service.IsChessMateForCurrentState(ColorEnum.White);
        //
        //     Assert.True(actual);
        // }
    }
}
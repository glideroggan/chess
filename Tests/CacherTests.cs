using System.Threading.Tasks;
using app.Models;
using app.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Tests
{
    public class CacherTests
    {
        private Mock<ICacher> _cacher;
        private AiService _target;
    
        public CacherTests()
        {
            var perfService = new Mock<IPerfService>();
            _cacher = new Mock<ICacher>(MockBehavior.Strict);
            var moveService = new MoveService(_cacher.Object, new NullLogger<MoveService>(), perfService.Object);
            var manager = new Manager(moveService, _cacher.Object, new NullLogger<Manager>(),
                perfService.Object);

            _target = new AiService(moveService, _cacher.Object, new NullLogger<AiService>(),
                manager, perfService.Object);
        }

        // [Theory]
        [InlineData("k/8/8/8/8/8/8/K w", 3)]
        public async Task TestCaching(string startState, int maxDepth)
        {
            // TODO: do tests on the cacher
            var state = new BoardState(startState);
            var res = await _target.InternalCalculateMove(state, maxDepth);
        }
    }
}
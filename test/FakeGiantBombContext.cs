using System.Threading.Tasks;
using GBLive.GiantBomb;
using GBLive.GiantBomb.Interfaces;

namespace GBLive.Tests
{
    public class FakeGiantBombContext : IGiantBombContext
    {
        public IResponse Response { get; set; } = new Response();

        public FakeGiantBombContext() { }

        public Task<IResponse> UpdateAsync() => Task.FromResult(Response);

        public void Dispose() { }
    }
}

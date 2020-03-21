using System;
using System.Threading.Tasks;

namespace GBLive.GiantBomb.Interfaces
{
    public interface IGiantBombContext : IDisposable
    {
        Task<IResponse> UpdateAsync();
    }
}

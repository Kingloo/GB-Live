using System.Threading.Tasks;

namespace GBLive.GiantBomb.Interfaces
{
	public interface IGiantBombContext
	{
		Task<IResponse> UpdateAsync();
	}
}

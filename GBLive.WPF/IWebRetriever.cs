using System.Net;
using System.Threading.Tasks;

namespace GBLive.WPF
{
    public interface IWebRetriever
    {
        Task<(string, HttpStatusCode)> GetAsync();
    }
}

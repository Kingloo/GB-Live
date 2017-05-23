using System.Net;
using System.Threading.Tasks;
using GBLive.WPF;

namespace GBLive.Tests
{
    public class TestWebRetriever : IWebRetriever
    {
        private readonly string text = string.Empty;
        private readonly HttpStatusCode status = HttpStatusCode.OK;

        public TestWebRetriever() { }

        public TestWebRetriever(string text) => this.text = text;

        public TestWebRetriever(HttpStatusCode status) => this.status = status;

        public TestWebRetriever(string text, HttpStatusCode status)
        {
            this.text = text;
            this.status = status;
        }

        public Task<(string, HttpStatusCode)> GetAsync()
        {
            return Task.Run(() => (text, status));
        }
    }
}

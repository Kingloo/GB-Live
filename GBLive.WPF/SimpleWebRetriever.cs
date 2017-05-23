using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using GBLive.WPF.Common;

namespace GBLive.WPF
{
    public class SimpleWebRetriever : IWebRetriever
    {
        #region Fields
        private readonly Uri uri = default(Uri);
        #endregion

        #region Properties
        public string UserAgent { get; set; } = string.Empty;
        #endregion

        public SimpleWebRetriever(Uri uri)
        {
            this.uri = uri ?? throw new ArgumentNullException(nameof(uri));
        }

        public async Task<(string, HttpStatusCode)> GetAsync()
        {
            string text = string.Empty;
            HttpStatusCode status = default(HttpStatusCode);
            
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("User-Agent", UserAgent);

                try
                {
                    var response = await client.GetAsync(uri)
                        .ConfigureAwait(false);

                    status = response.StatusCode;

                    if (status != HttpStatusCode.OK)
                    {
                        return (text, status);
                    }

                    text = await response.Content.ReadAsStringAsync()
                        .ConfigureAwait(false);
                }
                catch (TaskCanceledException) { }
                catch (HttpRequestException ex)
                {
                    Log.LogException(ex);
                }
            }

            return (text, status);
        }
    }
}

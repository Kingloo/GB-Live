using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GBLive.WPF.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;

namespace GBLive.WPF.GiantBomb
{
    public class GiantBombService : IDisposable
    {
        #region Fields
        private static readonly HttpClientHandler defaultHandler = new HttpClientHandler
        {
            AllowAutoRedirect = true,
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
            MaxAutomaticRedirections = 2
        };

        private HttpClient client = new HttpClient(defaultHandler)
        {
            Timeout = TimeSpan.FromSeconds(5d)
        };
        #endregion

        public JSchema Schema { get; } = null;

        public GiantBombService(HttpClient client, JSchema schema) : this(schema)
        {
            this.client = client ?? throw new ArgumentNullException(nameof(client));
        }

        public GiantBombService(JSchema schema)
        {
            Schema = schema ?? throw new ArgumentNullException(nameof(schema));

            SetUserAgent();
        }

        private void SetUserAgent()
        {
            client.DefaultRequestHeaders.UserAgent.Clear();

            if (!client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", Settings.UserAgent))
            {
                throw new ArgumentException(
                    "attempted User-Agent could not be added",
                    nameof(Settings.UserAgent));
            }
        }

        public async Task<UpcomingResponse> UpdateAsync()
        {
            (string text, HttpStatusCode statusCode) = await DownloadUpcomingJsonAsync().ConfigureAwait(false);

            if (statusCode != HttpStatusCode.OK)
            {
                return new UpcomingResponse(isSuccessful: false, Reason.ErrorCode, isLive: false);
            }

            if (String.IsNullOrEmpty(text))
            {
                return new UpcomingResponse(isSuccessful: false, Reason.StringEmpty, isLive: false);
            }

            if (!(Parse(text) is JObject json))
            {
                return new UpcomingResponse(isSuccessful: false, Reason.ParseFailed, isLive: false);
            }

            return ProcessJson(json);
        }

        private async Task<(string, HttpStatusCode)> DownloadUpcomingJsonAsync()
        {
            string text = string.Empty;
            HttpStatusCode statusCode = HttpStatusCode.Unused;

            var request = new HttpRequestMessage(HttpMethod.Get, Settings.Upcoming);

            try
            {
                using (var response = await client.SendAsync(request, CancellationToken.None).ConfigureAwait(false))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        if (response.Content is HttpContent content)
                        {
                            text = await content.ReadAsStringAsync().ConfigureAwait(false);

                            content.Dispose();
                        }
                    }

                    statusCode = response.StatusCode;
                }
            }
            catch (HttpRequestException ex)
            {
                await Log.LogExceptionAsync(ex).ConfigureAwait(false);
            }
            finally
            {
                request.Dispose();
            }

            return (text, statusCode);
        }

        private JObject Parse(string raw)
        {
            try
            {
                JObject json = JObject.Parse(raw);

                if (json.IsValid(Schema, out IList<ValidationError> errors))
                {
                    return json;
                }
                else
                {
                    var sb = new StringBuilder();

                    if (errors.Count > 0)
                    {
                        foreach (ValidationError error in errors)
                        {
                            sb.AppendLine($"{error.ErrorType}: {error.Message}");
                        }
                    }
                    else
                    {
                        sb.AppendLine("no errors");
                    }

                    Log.LogMessage(sb.ToString());

                    return null;
                }
            }
            catch (JsonReaderException)
            {
                return null;
            }
        }

        private static UpcomingResponse ProcessJson(JObject json)
        {
            return new UpcomingResponse(isSuccessful: true, Reason.Success, isLive: true);
        }


        #region IDisposable Support
        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    client?.Dispose();
                    defaultHandler?.Dispose();
                }

                disposedValue = true;
            }
        }

        public void Dispose() => Dispose(true);
        #endregion
    }
}

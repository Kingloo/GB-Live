﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GBLive.WPF.Common;
using GBLive.WPF.Extensions;
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

        private readonly JSchema schema = null;
        #endregion

        public GiantBombService(HttpClient client, JSchema schema)
            : this(schema)
        {
            this.client = client ?? throw new ArgumentNullException(nameof(client));
        }

        public GiantBombService(JSchema schema)
            : this()
        {
            this.schema = schema ?? throw new ArgumentNullException(nameof(schema));
        }

        public GiantBombService()
        {
            SetUserAgent(Settings.UserAgent);
        }

        private void SetUserAgent(string userAgent)
        {
            client.DefaultRequestHeaders.UserAgent.Clear();

            if (!client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", userAgent))
            {
                throw new ArgumentException(
                    "attempted User-Agent could not be added",
                    nameof(userAgent));
            }
        }

        public static void GoToHome() => Utils.OpenUriInBrowser(Settings.Home);

        public static void GoToChat() => Utils.OpenUriInBrowser(Settings.Chat);

        public async Task<UpcomingResponse> UpdateAsync()
        {
            (string text, HttpStatusCode statusCode) = await DownloadUpcomingJsonAsync().ConfigureAwait(false);

            if (statusCode != HttpStatusCode.OK)
            {
                return new UpcomingResponse(isSuccessful: false, Reason.InternetError, isLive: false);
            }

            if (String.IsNullOrEmpty(text))
            {
                return new UpcomingResponse(isSuccessful: false, Reason.StringEmpty, isLive: false);
            }

            if (!(text.ParseCatch<JsonReaderException>() is JObject json))
            {
                return new UpcomingResponse(isSuccessful: false, Reason.ParseFailed, isLive: false);
            }

            if (!Validate(json))
            {
                return new UpcomingResponse(isSuccessful: false, Reason.ValidateFailed, isLive: false);
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
            catch (HttpRequestException) { }
            catch (TaskCanceledException ex) when (ex.InnerException is HttpRequestException) { }
            catch (TaskCanceledException ex) when (ex.InnerException is Exception exc)
            {
                await Log.LogExceptionAsync(exc).ConfigureAwait(false);
            }
            finally
            {
                request.Dispose();
            }

            return (text, statusCode);
        }

        private static void LogValidationErrors(IList<ValidationError> errors)
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
                sb.AppendLine("validation failed but no validation errors supplied");
            }

            Log.LogMessage(sb.ToString());
        }

        private bool Validate(JObject json)
        {
            if (schema == null)
            {
                return true;
            }

            if (!json.IsValid(schema, out IList<ValidationError> errors))
            {
                LogValidationErrors(errors);

                return false;
            }

            return true;
        }

        private static UpcomingResponse ProcessJson(JObject json)
        {
            bool isLive = false;

            if (json.TryGetValue("liveNow", StringComparison.Ordinal, out JToken liveNow))
            {
                isLive = liveNow.HasValues;
            }

            var response = new UpcomingResponse(isSuccessful: true, Reason.Success, isLive);

            if (isLive)
            {
                response.LiveShowName = (string)liveNow["title"] ?? Settings.NameOfUntitledLiveShow;
            }

            if (TryGetEvents(json, out IReadOnlyList<UpcomingEvent> events))
            {
                response.Events = events;
            }

            return response;
        }

        private static bool TryGetEvents(JObject json, out IReadOnlyList<UpcomingEvent> events)
        {
            if (!json.TryGetValue("upcoming", StringComparison.Ordinal, out JToken upcoming))
            {
                events = null;
                return false;
            }

            var upcomingEvents = new List<UpcomingEvent>();

            foreach (JObject each in upcoming)
            {
                if (UpcomingEvent.TryCreate(each, out UpcomingEvent ue))
                {
                    upcomingEvents.Add(ue);
                }
            }

            events = upcomingEvents.AsReadOnly();
            return true;
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

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
﻿using System;
using System.Net;
using System.Threading.Tasks;

namespace GB_Live.Extensions
{
    public static class WebRequestExtensions
    {
        public static WebResponse GetResponseExt(this WebRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            WebResponse webResp = null;

            try
            {
                webResp = request.GetResponse();
            }
            catch (WebException e)
            {
                if (e.Response != null)
                {
                    webResp = e.Response;
                }
            }

            return webResp;
        }

        public static async Task<WebResponse> GetResponseAsyncExt(this WebRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            WebResponse webResp = null;

            try
            {
                webResp = await request.GetResponseAsync().ConfigureAwait(false);
            }
            catch (WebException e)
            {
                if (e.Response != null)
                {
                    webResp = e.Response;
                }
            }

            return webResp;
        }
    }
}
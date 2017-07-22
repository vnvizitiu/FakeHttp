﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Linq;

using Newtonsoft.Json;

namespace BingGeoCoder.Client
{
    static class Factory
    {
        public static HttpClient CreateClient(HttpMessageHandler handler, string user_agent)
        {
            var client = new HttpClient(handler, false);

            client.BaseAddress = new Uri("http://dev.virtualearth.net/REST/v1/", UriKind.Absolute);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            ProductInfoHeaderValue productHeader = null;
            if (!string.IsNullOrEmpty(user_agent) && ProductInfoHeaderValue.TryParse(user_agent, out productHeader))
            {
                client.DefaultRequestHeaders.UserAgent.Clear();
                client.DefaultRequestHeaders.UserAgent.Add(productHeader);
            }

            return client;
        }

        public static string CreateDefaultParameters(string key, string culture, UserContext context)
        {
            var d = new Dictionary<string, object>();

            d.Add("key", key);

            if (!string.IsNullOrEmpty(culture))
            {
                d.Add("c", culture);
            }

            if (context != null)
            {
                if (!string.IsNullOrEmpty(context.IPAddress))
                {
                    d.Add("ip", context.IPAddress);
                }

                if (context.Location != null)
                {
                    d.Add("ul", string.Format("{0},{1}", context.Location.Item1, context.Location.Item2));
                }

                if (context.MapView != null)
                {
                    d.Add("umv", string.Format("{0},{1},{2},{3}", context.MapView.Item1, context.MapView.Item2, context.MapView.Item3, context.MapView.Item4));
                }
            }

            return d.AsQueryString();
        }
    }
}

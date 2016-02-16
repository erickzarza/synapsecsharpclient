﻿using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Synapse.RestClient
{
    public static class ApiHelper
    {
        public static bool IsHttpOk(this IRestResponse r)
        {
            return r.StatusCode == System.Net.HttpStatusCode.OK;
        }

        [Obsolete("This method does not work, as synapse is not sending us the timestamp in UTC or epoch. IT's some weird shit.")]
        public static DateTime UnixTimestampInSecondsToUtc(long timestamp)
        {
            var dt = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dt = dt.AddSeconds(timestamp);
            return dt;
        }
        public static DateTime UnixTimestampInMillisecondsToUtc(long timestamp)
        {
            var dt = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dt = dt.AddMilliseconds(timestamp);
            return dt;
        }
        public static async Task<T> Execute<T>(this IRestClient client, IRestRequest req, Func<dynamic,T> onHttpOk, Func<dynamic,T> onHttpErr)
        {
            var resp = await client.ExecuteTaskAsync(req);
            dynamic data = SimpleJson.DeserializeObject(resp.Content);
            if (resp.IsHttpOk())
            {
                return onHttpOk(data);
            }
            else
            {
                return onHttpErr(data);
            }
        }

        public static string TryGetMessage(dynamic response)
        {
            if (!PropertyExists(response, "message")) return String.Empty;
            return response.message.en;
        }

        public static string TryGetError(dynamic response)
        {
            if (!PropertyExists(response, "error")) return String.Empty;
            return response.error.en;
        }

        public static bool PropertyExists(dynamic settings, string name)
        {
            return settings.ContainsKey(name);
        }

    }
}

﻿using System.Reflection;

namespace Mvkt.Api.Services.Cache
{
    public class StateHeadersMiddleware
    {
        readonly static string Version = Assembly.GetExecutingAssembly().GetName().Version.ToString();

        public const string MVKT_VERSION = "Mvkt-Version";
        public const string MVKT_LEVEL = "Mvkt-Level";
        public const string MVKT_KNOWN_LEVEL = "Mvkt-Known-Level";
        public const string MVKT_SYNCED_AT = "Mvkt-Synced-At";

        readonly RequestDelegate Next;
        public StateHeadersMiddleware(RequestDelegate next) => Next = next;

        public Task InvokeAsync(HttpContext context, StateCache stateCache)
        {
            var state = stateCache.Current;
            context.Response.Headers.Add(MVKT_VERSION, Version);
            context.Response.Headers.Add(MVKT_LEVEL, state.Level.ToString());
            context.Response.Headers.Add(MVKT_KNOWN_LEVEL, state.KnownHead.ToString());
            context.Response.Headers.Add(MVKT_SYNCED_AT, state.LastSync.ToString("yyyy-MM-ddTHH:mm:ssZ"));
            return Next(context);
        }
    }
}

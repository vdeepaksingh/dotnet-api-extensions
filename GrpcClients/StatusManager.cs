﻿using Grpc.Core;

using System.Net;

namespace dotnet_api_extensions.GrpcClients
{
    internal static class StatusManager
    {
        internal static StatusCode? GetStatusCode(HttpResponseMessage response)
        {
            var headers = response.Headers;

            if (!headers.Contains("grpc-status") && response.StatusCode == HttpStatusCode.OK)
                return StatusCode.OK;

            if (headers.Contains("grpc-status"))
                return (StatusCode)int.Parse(headers.GetValues("grpc-status").First());

            return null;
        }
    }
}